using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Constructor> Constructors => Set<Constructor>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<FantasyTeam> FantasyTeams => Set<FantasyTeam>();
    public DbSet<FantasyTeamDriver> FantasyTeamDrivers => Set<FantasyTeamDriver>();
    public DbSet<FantasyTeamConstructor> FantasyTeamConstructors => Set<FantasyTeamConstructor>();
    public DbSet<RaceResult> RaceResults => Set<RaceResult>();
    public DbSet<RaceResultDriver> RaceResultDrivers => Set<RaceResultDriver>();
    public DbSet<TeamScore> TeamScores => Set<TeamScore>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueMember> LeagueMembers => Set<LeagueMember>();
    public DbSet<Prediction> Predictions => Set<Prediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.Username).IsUnique();

            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();

            entity.HasOne(x => x.FantasyTeam)
                .WithOne(x => x.User)
                .HasForeignKey<FantasyTeam>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Constructor>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();

            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();

            entity.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Price).HasPrecision(10, 2);

            entity.HasOne(x => x.Constructor)
                .WithMany(x => x.Drivers)
                .HasForeignKey(x => x.ConstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Race>(entity =>
        {
            entity.HasIndex(x => x.RoundNumber).IsUnique();

            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CircuitName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Country).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<FantasyTeam>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.BudgetCap).HasPrecision(10, 2);
            entity.Property(x => x.RemainingBudget).HasPrecision(10, 2);
            entity.Property(x => x.LockedInPoints).HasPrecision(10,2);
        });

        modelBuilder.Entity<FantasyTeamDriver>(entity =>
        {
            entity.HasKey(x => new { x.FantasyTeamId, x.DriverId });

            entity.Property(x => x.PointsAtTransfer).HasPrecision(10, 2);

            entity.HasOne(x => x.FantasyTeam)
                .WithMany(x => x.FantasyTeamDrivers)
                .HasForeignKey(x => x.FantasyTeamId);

            entity.HasOne(x => x.Driver)
                .WithMany(x => x.FantasyTeamDrivers)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FantasyTeamConstructor>(entity =>
        {
            entity.HasKey(x => new { x.FantasyTeamId, x.ConstructorId });

            entity.Property(x => x.PointsAtTransfer).HasPrecision(10, 2);

            entity.HasOne(x => x.FantasyTeam)
                .WithMany(x => x.FantasyTeamConstructors)
                .HasForeignKey(x => x.FantasyTeamId);

            entity.HasOne(x => x.Constructor)
                .WithMany(x => x.FantasyTeamConstructors)
                .HasForeignKey(x => x.ConstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RaceResult>(entity =>
        {
            entity.HasOne(x => x.Race)
                .WithOne(x => x.RaceResult)
                .HasForeignKey<RaceResult>(x => x.RaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaceResultDriver>(entity =>
        {
            entity.HasKey(x => new { x.RaceResultId, x.DriverId });

            entity.HasOne(x => x.RaceResult)
                .WithMany(x => x.DriverResults)
                .HasForeignKey(x => x.RaceResultId);

            entity.HasOne(x => x.Driver)
                .WithMany(x => x.RaceResultDrivers)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Constructor)
                .WithMany(x => x.RaceResultDrivers)
                .HasForeignKey(x => x.ConstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeamScore>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.RaceId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.TeamScores)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Race)
                .WithMany(x => x.TeamScores)
                .HasForeignKey(x => x.RaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<League>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.InviteCode).HasMaxLength(6).IsRequired();

            entity.HasIndex(x => x.InviteCode).IsUnique();

            entity.HasOne(x => x.Owner)
                .WithMany(x => x.OwnedLeagues)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeagueMember>(entity =>
        {
            entity.HasKey(x => new { x.LeagueId, x.UserId });

            entity.HasOne(x => x.League)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(x => x.LeagueMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.RaceId }).IsUnique();

            entity.Property(x => x.Score).HasPrecision(10, 2);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Predictions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Race)
                .WithMany(x => x.Predictions)
                .HasForeignKey(x => x.RaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.P1Driver)
               .WithMany()
               .HasForeignKey(x => x.P1DriverId)
               .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.P2Driver)
                .WithMany()
                .HasForeignKey(x => x.P2DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.P3Driver)
                .WithMany()
                .HasForeignKey(x => x.P3DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}