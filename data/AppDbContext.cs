using Microsoft.EntityFrameworkCore;
using mmrdaconsent.API.Entities;

namespace mmrdaconsent.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<LandDetail> LandDetails { get; set; }
    public DbSet<LocationDetail> LocationDetails { get; set; }
    public DbSet<OTP> OTPs { get; set; }
    public DbSet<State> States { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Taluka> Talukas { get; set; }
    public DbSet<Village> Villages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ============================================================
        // 1. TABLE & PRIMARY KEY MAPPINGS
        // ============================================================

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollment");
            entity.HasKey(e => e.EnrollmentID);
            entity.Property(e => e.EnrollmentID).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.MobileNo).HasDatabaseName("IX_Enrollment_MobileNo");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Application");
            entity.HasKey(a => a.ApplicationID);
            entity.HasIndex(a => a.ReferenceNo).IsUnique().HasDatabaseName("IX_Application_ReferenceNo");

            // Property/Column Mappings
            entity.Property(a => a.Age).HasColumnName("Age");
            entity.Property(a => a.SelectedDeclaration).HasColumnName("SelectedDeclaration");
            entity.Property(a => a.ConsentSignedAt).HasColumnName("ConsentSignedAt");
            entity.Property(a => a.IsConsentGiven).HasColumnName("IsConsentGiven");
            entity.Property(a => a.IsSubmitted).HasColumnName("IsSubmitted");
            entity.Property(a => a.SubmittedAt).HasColumnName("SubmittedAt");
        });

        modelBuilder.Entity<LandDetail>(entity =>
        {
            entity.ToTable("LandDetaill"); // Note: Double 'l' as per DB schema
            entity.HasKey(l => l.LandDetailID);
            entity.Property(l => l.TotalAreaHecter).HasPrecision(18, 2);
        });

        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("LocationDetail");
            entity.HasKey(l => l.LocationDetailID);
        });

        modelBuilder.Entity<OTP>(entity =>
        {
            entity.ToTable("OTP");
            entity.HasKey(o => o.OTPID);
            entity.Property(o => o.OtpCode).HasColumnName("OTP");
        });

        // ============================================================
        // 2. MASTER DATA MAPPINGS (States, Districts, etc.)
        // ============================================================

        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("State");
            entity.HasKey(s => s.StateID);
            entity.Property(s => s.StateName).HasColumnName("State");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("District");
            entity.HasKey(d => d.DistrictID);
            entity.Property(d => d.DistrictName).HasColumnName("District");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("City");
            entity.HasKey(c => c.CityID);
            entity.Property(c => c.CityName).HasColumnName("City");
        });

        modelBuilder.Entity<Taluka>(entity =>
        {
            entity.ToTable("Taluka");
            entity.HasKey(t => t.TalukaID);
            entity.Property(t => t.TalukaName).HasColumnName("Taluka");
        });

        modelBuilder.Entity<Village>(entity =>
        {
            entity.ToTable("Village");
            entity.HasKey(v => v.VillageID);
            entity.Property(v => v.VillageName).HasColumnName("Village");
        });

        // ============================================================
        // 3. RELATIONSHIPS (Foreign Keys)
        // ============================================================

        // Enrollment <-> Application
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Enrollment)
            .WithMany(e => e.Applications)
            .HasForeignKey(a => a.EnrollmentID)
            .OnDelete(DeleteBehavior.Restrict);

        // Application <-> LandDetails
        modelBuilder.Entity<LandDetail>()
            .HasOne(l => l.Application)
            .WithMany(a => a.LandDetails)
            .HasForeignKey(l => l.ApplicationID)
            .OnDelete(DeleteBehavior.Cascade);

        // Application <-> LocationDetails
        modelBuilder.Entity<LocationDetail>()
            .HasOne(l => l.Application)
            .WithMany(a => a.LocationDetails)
            .HasForeignKey(l => l.ApplicationID)
            .OnDelete(DeleteBehavior.Cascade);

        // Master Data Hierarchy
        modelBuilder.Entity<District>()
            .HasOne(d => d.State).WithMany(s => s.Districts)
            .HasForeignKey(d => d.StateID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<City>()
            .HasOne(c => c.State).WithMany(s => s.Cities)
            .HasForeignKey(c => c.StateID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Taluka>()
            .HasOne(t => t.District).WithMany(d => d.Talukas)
            .HasForeignKey(t => t.DistrictID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Village>()
            .HasOne(v => v.Taluka).WithMany(t => t.Villages)
            .HasForeignKey(v => v.DistrictID).OnDelete(DeleteBehavior.Restrict);
    }
}