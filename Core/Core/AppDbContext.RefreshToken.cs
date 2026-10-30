using DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public partial class AppDbContext
{
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.TokenHash, "UQ_RefreshTokens_TokenHash").IsUnique();
            entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "IX_RefreshTokens_User_ExpiresAt");

            entity.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.ExpiresAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RevokedAt).HasPrecision(0);
            entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(128);
            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<LoginOtp>(entity =>
        {
            entity.ToTable("LoginOtps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.CodeHash).HasMaxLength(128).IsRequired();
            entity.Property(e => e.CreatedByIp).HasMaxLength(45).IsUnicode(false);
            entity.Property(e => e.ExpiresAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.ConsumedAt).HasPrecision(0);
            entity.HasIndex(e => new { e.UserName, e.ExpiresAt }, "IX_LoginOtps_UserName_ExpiresAt");
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_LoginOtps_Users");
        });

        modelBuilder.Entity<TripPlan>(entity =>
        {
            entity.ToTable("TripPlans");
            entity.HasKey(e => e.TripPlanId);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
            entity.Property(e => e.CollectedAmount).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId)
                .HasConstraintName("FK_TripPlans_Users");
        });

        modelBuilder.Entity<TripPayment>(entity =>
        {
            entity.ToTable("TripPayments");
            entity.HasKey(e => e.TripPaymentId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PaymentStatus).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ReferenceCode).HasMaxLength(64);
            entity.Property(e => e.PaidAt).HasPrecision(0);
            entity.HasOne(e => e.TripPlan).WithMany(t => t.TripPayments).HasForeignKey(e => e.TripPlanId)
                .HasConstraintName("FK_TripPayments_Trip");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId)
                .HasConstraintName("FK_TripPayments_User");
        });
    }
}
