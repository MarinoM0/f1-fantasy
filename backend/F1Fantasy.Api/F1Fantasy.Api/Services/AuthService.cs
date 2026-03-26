using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs.Auth;
using F1Fantasy.Api.Interfaces;
using F1Fantasy.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<AppUser> _passwordHasher;

        public AuthService(
            AppDbContext dbContext,
            ITokenService tokenService,
            IPasswordHasher<AppUser> passwordHasher)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            var username = request.Username?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.");
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Enter a valid email address.");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.");
            }

            if (username.Length < 3)
            {
                throw new ArgumentException("Username must be at least 3 characters.");
            }

            if (username.Length > 20)
            {
                throw new ArgumentException("Username must be at most 20 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters long.");
            }

            var normalizedUsername = username.ToLowerInvariant();

            var emailExists = await _dbContext.Users.AnyAsync(x => x.Email == email);
            if (emailExists)
            {
                throw new InvalidOperationException("Email is already in use.");
            }

            var usernameExists = await _dbContext.Users.AnyAsync(x => x.Username.ToLower() == normalizedUsername);
            if (usernameExists)
            {
                throw new InvalidOperationException("Username is already in use.");
            }

            var user = new AppUser
            {
                Email = email,
                Username = username
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                ExpiresAtUtc = _tokenService.GetTokenExpiryUtc(),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.");
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Enter a valid email address.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Password is required.");
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                ExpiresAtUtc = _tokenService.GetTokenExpiryUtc(),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username
                }
            };
        }


        public async Task<UserDto?> GetCurrentUserAsync (int userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    Email = x.Email,
                    Username = x.Username
                })
                .FirstOrDefaultAsync();
        }

    }
}
