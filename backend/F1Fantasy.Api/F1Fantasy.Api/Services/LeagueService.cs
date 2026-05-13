using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs.Leagues;
using F1Fantasy.Api.Interfaces;
using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace F1Fantasy.Api.Services
{
    public class LeagueService : ILeagueService
    {
        private const string InviteCodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        private const int InviteCodeLength = 6;
        private const int MaxInviteCodeAttempts = 10;
        private const int LeaderboardPageSize = 50;

        private readonly AppDbContext _dbContext;
        private readonly IJolpicaService _jolpicaService;
        private readonly ILogger<LeagueService> _logger;

        public LeagueService(
            AppDbContext dbContext,
            IJolpicaService jolpicaService,
            ILogger<LeagueService> logger)
        {
            _dbContext = dbContext;
            _jolpicaService = jolpicaService;
            _logger = logger;
        }

        public async Task<LeagueDto> CreateAsync(
            int userId, CreateLeagueRequestDto request, CancellationToken ct = default)
        {
            var name = (request.Name ?? string.Empty).Trim();
            if (name.Length < 3)
            {
                throw new ArgumentException("League name must be at least 3 cahracters");
            }

            var inviteCode = await GenerateUniqueInviteCodeAsync(ct);

            var league = new League
            {
                Name = name,
                InviteCode = inviteCode,
                OwnerId = userId
            };

            _dbContext.Leagues.Add(league);
            await _dbContext.SaveChangesAsync(ct);

            var ownerMembership = new LeagueMember
            {
                LeagueId = league.Id,
                UserId = userId,
                IsOwner = true,
                JoinedAtUtc = DateTime.UtcNow
            };

            _dbContext.LeagueMembers.Add(ownerMembership);
            await _dbContext.SaveChangesAsync(ct);

            return await BuildLeagueDtoAsync(userId, league.Id, ct);
        }

        public async Task<LeagueDto> JoinAsync(
            int userId, JoinLeagueRequestDto request, CancellationToken ct = default)
        {
            var normalized = NormalizeInviteCode(request.InviteCode);

            var league = await _dbContext.Leagues
                .FirstOrDefaultAsync(l => l.InviteCode == normalized, ct);

            if (league is null)
            {
                throw new KeyNotFoundException("No league found for that invite code");
            }

            var alreadyMember = await _dbContext.LeagueMembers
                .AsNoTracking()
                .AnyAsync(m => m.LeagueId == league.Id && m.UserId == userId, ct);

            if (alreadyMember)
            {
                throw new InvalidOperationException("You are already a member of this league");
            }

            var membership = new LeagueMember
            {
                LeagueId = league.Id,
                UserId = userId,
                IsOwner = false,
                JoinedAtUtc = DateTime.UtcNow
            };

            _dbContext.LeagueMembers.Add(membership);
            await _dbContext.SaveChangesAsync(ct);

            return await BuildLeagueDtoAsync(userId, league.Id, ct);
        }

        public async Task LeaveAsync(int userId, int leagueId, CancellationToken ct = default)
        {
            var membership = await _dbContext.LeagueMembers
                .FirstOrDefaultAsync(m => m.LeagueId == leagueId && m.UserId == userId, ct);

            if (membership is null)
            {
                throw new KeyNotFoundException("You are not a member of this league");
            }

            if (membership.IsOwner)
            {
                throw new InvalidOperationException("League owners cannot leave, delete the league instead");
            }

            _dbContext.LeagueMembers.Remove(membership);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int userId, int leagueId, CancellationToken ct = default)
        {
            var league = await _dbContext.Leagues
                .FirstOrDefaultAsync(l => l.Id == leagueId, ct);

            if (league is null)
            {
                throw new KeyNotFoundException("League not found");
            }

            if (league.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the league owner can delete the league");
            }

            _dbContext.Leagues.Remove(league);
            await _dbContext.SaveChangesAsync(ct);
        }



        //queries

        public async Task<IReadOnlyList<LeagueSummaryDto>> GetMyLeaguesAsync(
            int userId, CancellationToken ct = default)
        {
            return await _dbContext.LeagueMembers
                .AsNoTracking()
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.JoinedAtUtc)
                .Select(m => new LeagueSummaryDto
                {
                    Id = m.League.Id,
                    Name = m.League.Name,
                    IsOwner = m.IsOwner,
                    MemberCount = m.League.Members.Count,
                    CreatedAtUtc = m.League.CreatedAtUtc
                })
                .ToListAsync(ct);
        }

        public Task<LeagueDto> GetByIdAsync(int userId, int leagueId, CancellationToken ct = default)
        {
            return BuildLeagueDtoAsync(userId, leagueId, ct);
        }


        public async Task<IReadOnlyList<LeagueLeaderboardEntryDto>> GetLeaderboardAsync(
            int userId, int leagueId, CancellationToken ct = default)
        {
            await EnsureMembershipAsync(userId, leagueId, ct);

            var memberUserIds = await _dbContext.LeagueMembers
                .AsNoTracking()
                .Where(m => m.LeagueId == leagueId)
                .Select(m => m.UserId)
                .ToListAsync(ct);

            var teams = await _dbContext.FantasyTeams
                .AsNoTracking()
                .Where(team => memberUserIds.Contains(team.UserId))
                .Include(team => team.User)
                    .ThenInclude(u => u.TeamScores)
                .Include(team => team.FantasyTeamDrivers)
                    .ThenInclude(td => td.Driver)
                .Include(team => team.FantasyTeamConstructors)
                    .ThenInclude(tc => tc.Constructor)
                .ToListAsync(ct);

            IReadOnlyDictionary<string, decimal> driverPointsByCode =
               new Dictionary<string, decimal>();
            IReadOnlyDictionary<string, decimal> constructorPointsByJolpicaId =
                new Dictionary<string, decimal>();

            try
            {
                var driversTask = _jolpicaService.GetDriverStandingsAsync(cancellationToken: ct);
                var constructorsTask = _jolpicaService.GetConstructorStandingsAsync(cancellationToken: ct);
                await Task.WhenAll(driversTask, constructorsTask);

                driverPointsByCode = LivePointsCalculatorService
                    .BuildDriverPointsByCode(await driversTask);
                constructorPointsByJolpicaId = LivePointsCalculatorService
                    .BuildConstructorPointsByJolpicaId(await constructorsTask);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch live standings for league {LeagueId} leaderboard", leagueId);
            }

            var hasLiveStandings = driverPointsByCode.Count > 0 || constructorPointsByJolpicaId.Count > 0;

            var entries = new List<LeagueLeaderboardEntryDto>();

            foreach (var team in teams)
            {
                decimal totalPoints;
                if (hasLiveStandings)
                {
                    totalPoints = LivePointsCalculatorService.CalculateTeamPoints(team, driverPointsByCode, constructorPointsByJolpicaId);
                }
                else
                {
                    totalPoints = team.User.TeamScores.Sum(s => s.Points);
                }

                entries.Add(new LeagueLeaderboardEntryDto
                {
                    UserId = team.UserId,
                    Username = team.User.Username,
                    TeamName = team.Name,
                    TotalPoints = totalPoints
                });
            }

            var ranked = entries
                .OrderByDescending(e => e.TotalPoints)
                .ThenBy(e => e.Username)
                .Take(LeaderboardPageSize)
                .ToList();

            for (var i = 0; i <ranked.Count; i++)
            {
                ranked[i].Rank = i + 1;
            }

            return ranked;
        }



        //helper functions


        private async Task EnsureMembershipAsync(int userId, int leagueId, CancellationToken ct)
        {
            var isMember = await _dbContext.LeagueMembers
                .AsNoTracking()
                .AnyAsync(m => m.LeagueId == leagueId && m.UserId == userId, ct);

            if (!isMember)
            {
                throw new KeyNotFoundException("League not found");
            }
        }

        private async Task<LeagueDto> BuildLeagueDtoAsync(
            int userId, int leagueId, CancellationToken ct)
        {
            await EnsureMembershipAsync(userId, leagueId, ct);

            var league = await _dbContext.Leagues
                .AsNoTracking()
                .Include(l => l.Owner)
                .Include(l => l.Members)
                    .ThenInclude(m => m.User)
                    .ThenInclude(u => u.FantasyTeam)
                .FirstOrDefaultAsync(l => l.Id == leagueId, ct);

            if (league is null)
            {
                throw new KeyNotFoundException("League not found");
            }

            var memberDtos = league.Members
                .OrderByDescending(m => m.IsOwner)
                .ThenBy(m => m.JoinedAtUtc)
                .Select(m => new LeagueMemberDto
                {
                    UserId = m.UserId,
                    Username = m.User.Username,
                    TeamName = m.User.FantasyTeam?.Name,
                    IsOwner = m.IsOwner,
                    JoinedAtUtc = m.JoinedAtUtc
                })
                .ToList();

            return new LeagueDto
            {
                Id = league.Id,
                Name = league.Name,
                InviteCode = league.InviteCode,
                OwnerId = league.OwnerId,
                OwnerUsername = league.Owner.Username,
                MemberCount = league.Members.Count,
                IsOwner = league.OwnerId == userId,
                CreatedAtUtc = league.CreatedAtUtc,
                Members = memberDtos
            };
        }

        private async Task<string> GenerateUniqueInviteCodeAsync(CancellationToken ct)
        {
            for (var attempt = 0; attempt < MaxInviteCodeAttempts; attempt++)
            {
                var candidate = GenerateInviteCode();

                var exists = await _dbContext.Leagues
                    .AsNoTracking()
                    .AnyAsync(l => l.InviteCode == candidate, ct);

                if (!exists)
                {
                    return candidate;
                }
            }
                throw new InvalidOperationException("Could not generate a unique invite code. Try again.");
        }

        private static string GenerateInviteCode()
        {
            var bytes = RandomNumberGenerator.GetBytes(InviteCodeLength);
            var chars = new char[InviteCodeLength];

            for (var i = 0; i< InviteCodeLength; i++)
            {
                chars[i] = InviteCodeAlphabet[bytes[i] % InviteCodeAlphabet.Length];
            }

            return new string(chars);
        }

        private static string NormalizeInviteCode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ArgumentException("Invite code is required");
            }

            return raw.Trim().ToUpperInvariant();
        }
    }
}
