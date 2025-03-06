using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.API.Models;

public partial class YourDbContext : DbContext
{
    public YourDbContext()
    {
    }

    public YourDbContext(DbContextOptions<YourDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BboChathistory> BboChathistories { get; set; }

    public virtual DbSet<BboChathistoryBk> BboChathistoryBks { get; set; }

    public virtual DbSet<BboCredit> BboCredits { get; set; }

    public virtual DbSet<BboFeedback> BboFeedbacks { get; set; }

    public virtual DbSet<BboFilter> BboFilters { get; set; }

    public virtual DbSet<BboNotification> BboNotifications { get; set; }

    public virtual DbSet<BboQuestionanalyst> BboQuestionanalysts { get; set; }

    public virtual DbSet<BboRole> BboRoles { get; set; }

    public virtual DbSet<BboUser> BboUsers { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseNpgsql("Host=localhost;Database=ChatBotAI;Username=postgres;Password=123456");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BboChathistory>(entity =>
        {
            entity.HasKey(e => e.Chatid).HasName("bbo_chathistory_pkey");

            entity.ToTable("bbo_chathistory");

            entity.Property(e => e.Chatid).HasColumnName("chatid");
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .HasColumnName("language_code");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.Response).HasColumnName("response");
            entity.Property(e => e.Responsetime)
                .HasPrecision(10, 2)
                .HasColumnName("responsetime");
            entity.Property(e => e.Sentat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sentat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.BboChathistories)
                .HasPrincipalKey(p => p.Telegramid)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_user_chathistory");
        });

        modelBuilder.Entity<BboChathistoryBk>(entity =>
        {
            entity.HasKey(e => e.Chatid).HasName("bbo_chathistory_bk_pkey");

            entity.ToTable("bbo_chathistory_bk");

            entity.Property(e => e.Chatid).HasColumnName("chatid");
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .HasColumnName("language_code");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.Response).HasColumnName("response");
            entity.Property(e => e.Responsetime)
                .HasPrecision(10, 2)
                .HasColumnName("responsetime");
            entity.Property(e => e.Sentat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sentat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.BboChathistoryBks)
                .HasPrincipalKey(p => p.Telegramid)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_user_chathistory_bk");
        });

        modelBuilder.Entity<BboCredit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbo_credit_pkey");

            entity.ToTable("bbo_credit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Point).HasColumnName("point");
            entity.Property(e => e.Updateat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updateat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.BboCredits)
                .HasPrincipalKey(p => p.Telegramid)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_user_credit");
        });

        modelBuilder.Entity<BboFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbo_feedback_pkey");

            entity.ToTable("bbo_feedback");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .HasColumnName("comment");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.BboFeedbacks)
                .HasPrincipalKey(p => p.Telegramid)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_user_feedback");
        });

        modelBuilder.Entity<BboFilter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbo_filters_pkey");

            entity.ToTable("bbo_filters");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Question)
                .HasMaxLength(1000)
                .HasColumnName("question");
            entity.Property(e => e.Updateat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updateat");
        });

        modelBuilder.Entity<BboNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbo_notification_pkey");

            entity.ToTable("bbo_notification");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Notification)
                .HasMaxLength(250)
                .HasColumnName("notification");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Updateat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updateat");
        });

        modelBuilder.Entity<BboQuestionanalyst>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbo_questionanalyst_pkey");

            entity.ToTable("bbo_questionanalyst");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Askedcount).HasColumnName("askedcount");
            entity.Property(e => e.Mainkey)
                .HasMaxLength(100)
                .HasColumnName("mainkey");
            entity.Property(e => e.Questionpattern)
                .HasMaxLength(1000)
                .HasColumnName("questionpattern");
        });

        modelBuilder.Entity<BboRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbo_role_pkey");

            entity.ToTable("bbo_role");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<BboUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bbo_user_pkey");

            entity.ToTable("bbo_user");

            entity.HasIndex(e => e.Telegramid, "uq_telegramid").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Allownoti).HasColumnName("allownoti");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Joindate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("joindate");
            entity.Property(e => e.Language)
                .HasMaxLength(10)
                .HasColumnName("language");
            entity.Property(e => e.Lastactive)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastactive");
            entity.Property(e => e.Onchainid)
                .HasMaxLength(250)
                .HasColumnName("onchainid");
            entity.Property(e => e.Password)
                .HasMaxLength(250)
                .HasColumnName("password");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Telegramid)
                .IsRequired()
                .HasColumnName("telegramid");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.BboUsers)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("fk_user_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
