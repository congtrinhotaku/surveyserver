using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SurveyService.Models;

public partial class SurveyDbContext : DbContext
{
    public SurveyDbContext(DbContextOptions<SurveyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<Function> Functions { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<Response> Responses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Survey> Surveys { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answers__3214EC0757D02D1C");

            entity.Property(e => e.AnswerDate).HasColumnType("datetime");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Answers_Question");

            entity.HasOne(d => d.Response).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ResponseId)
                .HasConstraintName("FK_Answers_Response");
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AnswerOp__3214EC0770BEBC1F");

            entity.HasOne(d => d.Answer).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.AnswerId)
                .HasConstraintName("FK_AnswerOptions_Answer");

            entity.HasOne(d => d.Option).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK_AnswerOptions_Option");
        });

        modelBuilder.Entity<Function>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Function__3214EC07FC510D4C");

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Menu__3214EC07905877DF");

            entity.ToTable("Menu");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Path).HasMaxLength(255);

            entity.HasOne(d => d.Function).WithMany(p => p.Menus)
                .HasForeignKey(d => d.FunctionId)
                .HasConstraintName("FK_Menu_Functions");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Options__3214EC0799278EC3");

            entity.Property(e => e.OptionText).HasMaxLength(500);

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Options_Question");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pages__3214EC07FAEEDD8E");

            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Survey).WithMany(p => p.Pages)
                .HasForeignKey(d => d.SurveyId)
                .HasConstraintName("FK_Pages_Survey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC07970B573B");

            entity.HasIndex(e => e.Code, "UQ__Permissi__A25C5AA799D56500").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Function).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.FunctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permissions_Functions");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07FB21C17E");

            entity.Property(e => e.IsRequired).HasDefaultValue(false);

            entity.HasOne(d => d.Page).WithMany(p => p.Questions)
                .HasForeignKey(d => d.PageId)
                .HasConstraintName("FK_Questions_Page");

            entity.HasOne(d => d.QuestionType).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionTypeId)
                .HasConstraintName("FK_Questions_Type");
        });

        modelBuilder.Entity<QuestionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07B3323282");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
        });

        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Response__3214EC072C14B9A2");

            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Survey).WithMany(p => p.Responses)
                .HasForeignKey(d => d.SurveyId)
                .HasConstraintName("FK_Responses_Survey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07EDD18746");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F65A28EB45").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__RolePermi__Permi__7C4F7684"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__RolePermi__RoleI__7B5B524B"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK__RolePerm__6400A1A879EE9ABE");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Surveys__3214EC0708A817F8");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatorPassword)
                .HasMaxLength(255)
                .HasColumnName("creatorPassword");
            entity.Property(e => e.CreatorUser).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0739E13D26");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E43B2C0A77").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__RoleI__787EE5A0"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__UserI__778AC167"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760AD6BCC5A0A");
                        j.ToTable("UserRoles");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
