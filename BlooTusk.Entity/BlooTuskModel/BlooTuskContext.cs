using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class BlooTuskContext : DbContext
{
    public BlooTuskContext()
    {
    }

    public BlooTuskContext(DbContextOptions<BlooTuskContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admincoupongraph> Admincoupongraphs { get; set; }

    public virtual DbSet<Adminfrequentusergraph> Adminfrequentusergraphs { get; set; }

    public virtual DbSet<Adminpointgraph> Adminpointgraphs { get; set; }

    public virtual DbSet<Adminusergraph> Adminusergraphs { get; set; }

    public virtual DbSet<Auditlog> Auditlogs { get; set; }

    public virtual DbSet<Categorymaster> Categorymasters { get; set; }

    public virtual DbSet<Countrymaster> Countrymasters { get; set; }

    public virtual DbSet<Couponissuedetail> Couponissuedetails { get; set; }

    public virtual DbSet<Couponissuemaster> Couponissuemasters { get; set; }

    public virtual DbSet<Couponmaster> Couponmasters { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Customermerchantmapper> Customermerchantmappers { get; set; }

    public virtual DbSet<Errorlog> Errorlogs { get; set; }

    public virtual DbSet<Levelmaster> Levelmasters { get; set; }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<Merchantsystemuser> Merchantsystemusers { get; set; }

    public virtual DbSet<Messagetypemaster> Messagetypemasters { get; set; }

    public virtual DbSet<Modetypemaster> Modetypemasters { get; set; }

    public virtual DbSet<Notemaster> Notemasters { get; set; }

    public virtual DbSet<Nudgecoupon> Nudgecoupons { get; set; }

    public virtual DbSet<Otplog> Otplogs { get; set; }

    public virtual DbSet<Po> Pos { get; set; }

    public virtual DbSet<Redeemtion> Redeemtions { get; set; }

    public virtual DbSet<Remarkhistory> Remarkhistories { get; set; }

    public virtual DbSet<Rewardpointmaster> Rewardpointmasters { get; set; }

    public virtual DbSet<Rewardpointtransaction> Rewardpointtransactions { get; set; }

    public virtual DbSet<Rewardtypemaster> Rewardtypemasters { get; set; }

    public virtual DbSet<Signuprequest> Signuprequests { get; set; }

    public virtual DbSet<Smstemplate> Smstemplates { get; set; }

    public virtual DbSet<Statemaster> Statemasters { get; set; }

    public virtual DbSet<Systemusermaster> Systemusermasters { get; set; }

    public virtual DbSet<Trustscore> Trustscores { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("User Id=blootusk;Host=34.29.46.247;Database=BlooTusk;Password=blootusk@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admincoupongraph>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("admincoupongraph");

            entity.Property(e => e.MonthText).HasMaxLength(3);
            entity.Property(e => e.RedeemCount)
                .HasPrecision(64)
                .HasColumnName("redeemCount");
            entity.Property(e => e.TotalCouponCount)
                .HasPrecision(42)
                .HasColumnName("totalCouponCount");
            entity.Property(e => e.Yy).HasColumnName("YY");
        });

        modelBuilder.Entity<Adminfrequentusergraph>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("adminfrequentusergraph");

            entity.Property(e => e.Mm).HasColumnName("MM");
            entity.Property(e => e.MonthText).HasMaxLength(3);
            entity.Property(e => e.Yy).HasColumnName("YY");
        });

        modelBuilder.Entity<Adminpointgraph>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("adminpointgraph");

            entity.Property(e => e.Earned).HasPrecision(32);
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.MonthText).HasMaxLength(3);
            entity.Property(e => e.Redeem).HasPrecision(32);
            entity.Property(e => e.Yy).HasColumnName("YY");
        });

        modelBuilder.Entity<Adminusergraph>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("adminusergraph");

            entity.Property(e => e.MonthText).HasMaxLength(3);
            entity.Property(e => e.ReferralCount).HasPrecision(23);
            entity.Property(e => e.WalkInCount).HasPrecision(24);
            entity.Property(e => e.Yy)
                .HasMaxLength(4)
                .HasColumnName("YY");
        });

        modelBuilder.Entity<Auditlog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PRIMARY");

            entity.ToTable("auditlogs");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.RequestStatus).HasMaxLength(20);
            entity.Property(e => e.RequestedApiname)
                .HasMaxLength(200)
                .HasColumnName("RequestedAPIName");
        });

        modelBuilder.Entity<Categorymaster>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("categorymaster");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.RecStatus).HasMaxLength(1);
        });

        modelBuilder.Entity<Countrymaster>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("PRIMARY");

            entity.ToTable("countrymaster");

            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.CountryName).HasMaxLength(50);
        });

        modelBuilder.Entity<Couponissuedetail>(entity =>
        {
            entity.HasKey(e => e.CouponIssueDetailsId).HasName("PRIMARY");

            entity.ToTable("couponissuedetails");

            entity.Property(e => e.CouponSerialNo).HasMaxLength(45);
            entity.Property(e => e.CreatedBy).HasMaxLength(45);
            entity.Property(e => e.CreatedDate).HasMaxLength(45);
            entity.Property(e => e.IssuedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyBy).HasMaxLength(45);
            entity.Property(e => e.ModifyDate).HasMaxLength(45);
            entity.Property(e => e.Specialoffer).HasColumnName("specialoffer");
        });

        modelBuilder.Entity<Couponissuemaster>(entity =>
        {
            entity.HasKey(e => e.CouponIssuemasterId).HasName("PRIMARY");

            entity.ToTable("couponissuemaster");

            entity.Property(e => e.CouponIssuemasterId).HasColumnName("CouponIssuemasterID");
            entity.Property(e => e.CreatedBy).HasMaxLength(45);
            entity.Property(e => e.CreatedDate).HasMaxLength(45);
            entity.Property(e => e.IssueDate).HasColumnType("date");
            entity.Property(e => e.IssuedTo).HasMaxLength(45);
            entity.Property(e => e.ModifyBy).HasMaxLength(45);
            entity.Property(e => e.ModifyDate).HasMaxLength(45);
        });

        modelBuilder.Entity<Couponmaster>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PRIMARY");

            entity.ToTable("couponmaster");

            entity.Property(e => e.CouponId).HasColumnName("couponId");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(45)
                .HasColumnName("couponCode");
            entity.Property(e => e.CouponDiscerption).HasMaxLength(1000);
            entity.Property(e => e.CouponTitle).HasMaxLength(45);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.RecStatus)
                .HasMaxLength(10)
                .HasColumnName("recStatus");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PRIMARY");

            entity.ToTable("customer");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.CustomerCode).HasMaxLength(10);
            entity.Property(e => e.Lastname)
                .HasMaxLength(255)
                .HasColumnName("lastname");
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.PhoneNumber).HasMaxLength(250);
            entity.Property(e => e.RecStatus).HasMaxLength(1);
        });

        modelBuilder.Entity<Customermerchantmapper>(entity =>
        {
            entity.HasKey(e => e.UserMerchantMapperId).HasName("PRIMARY");

            entity.ToTable("customermerchantmapper");

            entity.HasIndex(e => e.MerchantId, "UMMMerchantID_idx");

            entity.HasIndex(e => e.CustomerId, "UUMCustomerID_idx");

            entity.Property(e => e.UserMerchantMapperId).HasColumnName("UserMerchantMapperID");
            entity.Property(e => e.ApprovlStatus).HasMaxLength(1);
            entity.Property(e => e.Createdate)
                .HasColumnType("datetime")
                .HasColumnName("createdate");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ReferCode).HasMaxLength(50);
            entity.Property(e => e.StopMessage).HasColumnName("stopMessage");

            entity.HasOne(d => d.Customer).WithMany(p => p.Customermerchantmappers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UUMCustomerID");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Customermerchantmappers)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UMMMerchantID");
        });

        modelBuilder.Entity<Errorlog>(entity =>
        {
            entity.HasKey(e => e.ErrorLogId).HasName("PRIMARY");

            entity.ToTable("errorlogs");

            entity.Property(e => e.ErrorLogId).HasColumnName("ErrorLogID");
            entity.Property(e => e.ErrorLog1).HasColumnName("ErrorLog");
            entity.Property(e => e.LogDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Levelmaster>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PRIMARY");

            entity.ToTable("levelmaster");

            entity.Property(e => e.Level).HasMaxLength(45);
        });

        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasKey(e => e.MerchantId).HasName("PRIMARY");

            entity.ToTable("merchant");

            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ApprovalStatus).HasMaxLength(1);
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("city");
            entity.Property(e => e.ContactPersonName).HasMaxLength(250);
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(50)
                .HasColumnName("DeviceID");
            entity.Property(e => e.DeviceOs)
                .HasMaxLength(20)
                .HasColumnName("DeviceOS");
            entity.Property(e => e.MerchantCode).HasMaxLength(10);
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.OrganizationName).HasMaxLength(250);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(250);
            entity.Property(e => e.RecStatus).HasMaxLength(1);
        });

        modelBuilder.Entity<Merchantsystemuser>(entity =>
        {
            entity.HasKey(e => e.MerchantSystemUserId).HasName("PRIMARY");

            entity.ToTable("merchantsystemuser");

            entity.HasIndex(e => e.MerchantId, "MSUMerchantID_idx");

            entity.Property(e => e.MerchantSystemUserId).HasColumnName("MerchantSystemUserID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(50)
                .HasColumnName("DeviceID");
            entity.Property(e => e.DeviceOs)
                .HasMaxLength(20)
                .HasColumnName("DeviceOS");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(250);
            entity.Property(e => e.RecStatus).HasMaxLength(1);
            entity.Property(e => e.UserId)
                .HasMaxLength(12)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Merchantsystemusers)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("MSUMerchantID");
        });

        modelBuilder.Entity<Messagetypemaster>(entity =>
        {
            entity.HasKey(e => e.MessageTypeId).HasName("PRIMARY");

            entity.ToTable("messagetypemaster");

            entity.Property(e => e.MessageTypeId).HasColumnName("MessageTypeID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.MessageType).HasMaxLength(50);
            entity.Property(e => e.ModeType).HasMaxLength(10);
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.RecStatus).HasMaxLength(1);
        });

        modelBuilder.Entity<Modetypemaster>(entity =>
        {
            entity.HasKey(e => e.ModeTypeId).HasName("PRIMARY");

            entity.ToTable("modetypemaster");

            entity.Property(e => e.ModeTypeId).HasColumnName("ModeTypeID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.ModeType).HasMaxLength(50);
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.RecStatus).HasMaxLength(1);
        });

        modelBuilder.Entity<Notemaster>(entity =>
        {
            entity.HasKey(e => e.NotemasterId).HasName("PRIMARY");

            entity.ToTable("notemaster");

            entity.Property(e => e.NotemasterId).HasColumnName("NotemasterID");
            entity.Property(e => e.CouponId).HasColumnName("couponId");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("customerId");
            entity.Property(e => e.ModifyDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(45);
            entity.Property(e => e.RecStatus).HasMaxLength(4);
            entity.Property(e => e.Specialoffer).HasColumnName("specialoffer");
        });

        modelBuilder.Entity<Nudgecoupon>(entity =>
        {
            entity.HasKey(e => new { e.NudgeCouponId, e.CouponId }).HasName("PRIMARY");

            entity.ToTable("nudgecoupon");

            entity.HasIndex(e => e.NudgeCouponId, "NudgeCouponId_UNIQUE").IsUnique();

            entity.Property(e => e.NudgeCouponId).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Otplog>(entity =>
        {
            entity.HasKey(e => e.OtplogId).HasName("PRIMARY");

            entity.ToTable("otplog");

            entity.Property(e => e.OtplogId).HasColumnName("OTPLogID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Otp)
                .HasMaxLength(45)
                .HasColumnName("OTP");
            entity.Property(e => e.Otpstatus)
                .HasMaxLength(45)
                .HasColumnName("OTPStatus");
            entity.Property(e => e.Otptype)
                .HasMaxLength(45)
                .HasColumnName("OTPType");
            entity.Property(e => e.SendTo).HasMaxLength(100);
        });

        modelBuilder.Entity<Po>(entity =>
        {
            entity.HasKey(e => e.Posid).HasName("PRIMARY");

            entity.ToTable("pos");

            entity.HasIndex(e => e.CategoryId, "CategoryID_idx");

            entity.HasIndex(e => e.CountryId, "POSCountryID_idx");

            entity.HasIndex(e => e.MerchantId, "POSMerchantID_idx");

            entity.HasIndex(e => e.StateId, "StateID_idx");

            entity.Property(e => e.Posid).HasColumnName("POSID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.Latitude).HasMaxLength(20);
            entity.Property(e => e.Longitude).HasMaxLength(20);
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.Posaddress).HasColumnName("POSAddress");
            entity.Property(e => e.Poscode)
                .HasMaxLength(20)
                .HasColumnName("POSCode");
            entity.Property(e => e.Posname)
                .HasMaxLength(250)
                .HasColumnName("POSName");
            entity.Property(e => e.RecStatus).HasMaxLength(1);
            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.Zip).HasMaxLength(20);

            entity.HasOne(d => d.Category).WithMany(p => p.Pos)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CategoryID");

            entity.HasOne(d => d.Country).WithMany(p => p.Pos)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("POSCountryID");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Pos)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("POSMerchantID");

            entity.HasOne(d => d.State).WithMany(p => p.Pos)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("StateID");
        });

        modelBuilder.Entity<Redeemtion>(entity =>
        {
            entity.HasKey(e => e.CouponredeemtionId).HasName("PRIMARY");

            entity.ToTable("redeemtion");

            entity.Property(e => e.CouponredeemtionDate).HasColumnType("date");
            entity.Property(e => e.CreatedBy).HasMaxLength(45);
            entity.Property(e => e.CreatedDate).HasMaxLength(45);
            entity.Property(e => e.ModifyBy).HasMaxLength(45);
            entity.Property(e => e.ModifyDate).HasMaxLength(45);
            entity.Property(e => e.RedeembyCustomerId).HasColumnName("redeembyCustomerId");
        });

        modelBuilder.Entity<Remarkhistory>(entity =>
        {
            entity.HasKey(e => e.RemarkHistoryId).HasName("PRIMARY");

            entity.ToTable("remarkhistory");

            entity.HasIndex(e => e.MerchantId, "MerchantID_idx");

            entity.Property(e => e.RemarkHistoryId).HasColumnName("RemarkHistoryID");
            entity.Property(e => e.ApprovalStatus).HasMaxLength(1);
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.RemarkDate).HasColumnType("date");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Remarkhistories)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("MerchantID");
        });

        modelBuilder.Entity<Rewardpointmaster>(entity =>
        {
            entity.HasKey(e => e.RewardPonitId).HasName("PRIMARY");

            entity.ToTable("rewardpointmaster");

            entity.Property(e => e.RewardPonitId).HasColumnName("RewardPonitID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.ModifiedDate).HasColumnType("date");
            entity.Property(e => e.RewardDate).HasMaxLength(20);
            entity.Property(e => e.RewardTypeId).HasColumnName("RewardTypeID");
        });

        modelBuilder.Entity<Rewardpointtransaction>(entity =>
        {
            entity.HasKey(e => e.RewardTransactionId).HasName("PRIMARY");

            entity.ToTable("rewardpointtransaction");

            entity.Property(e => e.RewardTransactionId).HasColumnName("RewardTransactionID");
            entity.Property(e => e.Createddate)
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.Modifieddate)
                .HasColumnType("datetime")
                .HasColumnName("modifieddate");
            entity.Property(e => e.RewardPointId).HasColumnName("RewardPointID");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Rewardtypemaster>(entity =>
        {
            entity.HasKey(e => e.RewardTypeId).HasName("PRIMARY");

            entity.ToTable("rewardtypemaster");

            entity.Property(e => e.RewardTypeId).HasColumnName("RewardTypeID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.ModifiedDate).HasColumnType("date");
            entity.Property(e => e.RewardType).HasMaxLength(45);
            entity.Property(e => e.ShortName).HasMaxLength(20);
        });

        modelBuilder.Entity<Signuprequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("signuprequest");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryName).HasMaxLength(25);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<Smstemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PRIMARY");

            entity.ToTable("smstemplate");

            entity.HasIndex(e => e.MerchantId, "MerchantID_idx");

            entity.HasIndex(e => e.MessageTypeId, "MessageTypeID_idx");

            entity.Property(e => e.TemplateId).HasColumnName("TemplateID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.MerchantId).HasColumnName("MerchantID");
            entity.Property(e => e.MessageTypeId).HasColumnName("MessageTypeID");
            entity.Property(e => e.ModeTypeId).HasColumnName("ModeTypeID");
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.RecStatus).HasMaxLength(1);

            entity.HasOne(d => d.Merchant).WithMany(p => p.Smstemplates)
                .HasForeignKey(d => d.MerchantId)
                .HasConstraintName("TemplateMerchantID");

            entity.HasOne(d => d.MessageType).WithMany(p => p.Smstemplates)
                .HasForeignKey(d => d.MessageTypeId)
                .HasConstraintName("MessageTypeID");
        });

        modelBuilder.Entity<Statemaster>(entity =>
        {
            entity.HasKey(e => e.StateId).HasName("PRIMARY");

            entity.ToTable("statemaster");

            entity.HasIndex(e => e.CountryId, "CountryID_idx");

            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.StateName).HasMaxLength(50);

            entity.HasOne(d => d.Country).WithMany(p => p.Statemasters)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("CountryID");
        });

        modelBuilder.Entity<Systemusermaster>(entity =>
        {
            entity.HasKey(e => e.SystemUserId).HasName("PRIMARY");

            entity.ToTable("systemusermaster");

            entity.Property(e => e.SystemUserId).HasColumnName("SystemUserID");
            entity.Property(e => e.CreatedDate).HasColumnType("date");
            entity.Property(e => e.ModifyDate).HasColumnType("date");
            entity.Property(e => e.Password).HasMaxLength(20);
            entity.Property(e => e.RecStatus).HasMaxLength(1);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Trustscore>(entity =>
        {
            entity.HasKey(e => e.TrustScoreId).HasName("PRIMARY");

            entity.ToTable("trustscore");

            entity.Property(e => e.TrustScore1).HasColumnName("TrustScore");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
