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

    // Add this DbSet alongside the others
    public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }
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
        });

        modelBuilder.Entity<LandDetail>(entity =>
        {
            entity.ToTable("LandDetail"); // Double 'l' as per DB
            entity.HasKey(l => l.LandDetailID);
            entity.Property(l => l.TotalAreaHecter).HasPrecision(18, 2);
        });

        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("LocationDetail");
            entity.HasKey(l => l.LocationDetailID);
        });

        modelBuilder.Entity<ApplicationDocument>(entity =>
        {
            entity.ToTable("ApplicationDocument");
            entity.HasKey(d => d.DocumentID);
            entity.Property(d => d.DocumentID).ValueGeneratedOnAdd();

            // Index for fast lookup by application
            entity.HasIndex(d => d.ApplicationID)
                  .HasDatabaseName("IX_ApplicationDocument_ApplicationID");
        });

        modelBuilder.Entity<OTP>(entity =>
        {
            entity.ToTable("OTP");
            entity.HasKey(o => o.OTPID);
            entity.Property(o => o.OtpCode).HasColumnName("OTP");
        });

        // ============================================================
        // 2. MASTER DATA MAPPINGS
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
        // 3. RELATIONSHIPS
        // ============================================================

        modelBuilder.Entity<Application>()
            .HasOne(a => a.Enrollment)
            .WithMany(e => e.Applications)
            .HasForeignKey(a => a.EnrollmentID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LandDetail>()
            .HasOne(l => l.Application)
            .WithMany(a => a.LandDetails)
            .HasForeignKey(l => l.ApplicationID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LocationDetail>()
            .HasOne(l => l.Application)
            .WithMany(a => a.LocationDetails)
            .HasForeignKey(l => l.ApplicationID)
            .OnDelete(DeleteBehavior.Cascade);



        // Relationship: Application → ApplicationDocuments
        modelBuilder.Entity<ApplicationDocument>()
            .HasOne(d => d.Application)
            .WithMany(a => a.Documents)
            .HasForeignKey(d => d.ApplicationID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<District>()
            .HasOne(d => d.State).WithMany(s => s.Districts)
            .HasForeignKey(d => d.StateID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<City>()
            .HasOne(c => c.State).WithMany(s => s.Cities)
            .HasForeignKey(c => c.StateID).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Taluka>()
            .HasOne(t => t.District).WithMany(d => d.Talukas)
            .HasForeignKey(t => t.DistrictID).OnDelete(DeleteBehavior.Restrict);

        // FIXED: Village → District (was Village → Taluka, wrong FK)
        modelBuilder.Entity<Village>()
            .HasOne(v => v.District).WithMany(d => d.Villages)
            .HasForeignKey(v => v.DistrictID).OnDelete(DeleteBehavior.Restrict);
    }
}