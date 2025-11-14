using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QUANLYTHUVIEN.Models;

public partial class QlthuvienContext : DbContext
{
    public QlthuvienContext()
    {
    }

    public QlthuvienContext(DbContextOptions<QlthuvienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Rental> Rentals { get; set; }

    public virtual DbSet<RentalDetail> RentalDetails { get; set; }

    public virtual DbSet<RentalPrice> RentalPrices { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<TbBlog> TbBlogs { get; set; }

    public virtual DbSet<TbBlogComment> TbBlogComments { get; set; }

    public virtual DbSet<TbMenu> TbMenus { get; set; }

    public virtual DbSet<TbRole> TbRoles { get; set; }

    public virtual DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Authors__70DAFC148D3779D9");

            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.AuthorName).HasMaxLength(100);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C22736596747");

            entity.HasIndex(e => e.Isbn, "UQ__Books__447D36EA30390A25").IsUnique();

            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CoverImage).HasMaxLength(255);
            entity.Property(e => e.Isbn)
                .HasMaxLength(50)
                .HasColumnName("ISBN");
            entity.Property(e => e.LanguageId).HasColumnName("LanguageID");
            entity.Property(e => e.PublisherId).HasColumnName("PublisherID");
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Books__CategoryI__44FF419A");

            entity.HasOne(d => d.Language).WithMany(p => p.Books)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK__Books__LanguageI__46E78A0C");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("FK__Books__Publisher__45F365D3");

            entity.HasMany(d => d.Authors).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BookAuthor",
                    r => r.HasOne<Author>().WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BookAutho__Autho__4AB81AF0"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BookAutho__BookI__49C3F6B7"),
                    j =>
                    {
                        j.HasKey("BookId", "AuthorId").HasName("PK__BookAuth__6AED6DE66A625CC8");
                        j.ToTable("BookAuthors");
                        j.IndexerProperty<int>("BookId").HasColumnName("BookID");
                        j.IndexerProperty<int>("AuthorId").HasColumnName("AuthorID");
                    });

            
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B79434BFB");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8073A049F");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK__Language__B938558BC487F85E");

            entity.Property(e => e.LanguageId).HasColumnName("LanguageID");
            entity.Property(e => e.LanguageName).HasMaxLength(50);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF491CB3EA");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Orders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__Supplier__619B8048");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__UserID__628FA481");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.BookId }).HasName("PK__OrderDet__A04E578D4F0C4D0D");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Book).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__BookI__66603565");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__656C112C");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("PK__Publishe__4C657E4B77619882");

            entity.Property(e => e.PublisherId).HasColumnName("PublisherID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PublisherName).HasMaxLength(100);
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.RentalId).HasName("PK__Rentals__970059638FAD0FC1");

            entity.Property(e => e.RentalId).HasColumnName("RentalID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.RentalDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Đang thuê");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rentals__Custome__5629CD9C");

            entity.HasOne(d => d.User).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rentals__UserID__571DF1D5");
        });

        modelBuilder.Entity<RentalDetail>(entity =>
        {
            entity.HasKey(e => new { e.RentalId, e.BookId }).HasName("PK__RentalDe__F4DE5541B54FEA0E");

            entity.Property(e => e.RentalId).HasColumnName("RentalID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Book).WithMany(p => p.RentalDetails)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RentalDet__BookI__5BE2A6F2");

            entity.HasOne(d => d.Rental).WithMany(p => p.RentalDetails)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RentalDet__Renta__5AEE82B9");
        });

        modelBuilder.Entity<RentalPrice>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("PK__RentalPr__4957584F78124DF4");

            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.DailyRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EffectiveDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MonthlyRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.WeeklyRate).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Book).WithMany(p => p.RentalPrices)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RentalPri__BookI__4E88ABD4");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE6669405A6ECB7");

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SupplierName).HasMaxLength(100);
        });

        modelBuilder.Entity<TbBlog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__tb_Blogs__54379E301A43CB87");

            entity.ToTable("tb_Blogs");

            entity.Property(e => e.Alias).HasMaxLength(150);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Views).HasDefaultValue(0);

            entity.HasOne(d => d.Author).WithMany(p => p.TbBlogs)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_Blogs_Users");
        });

        modelBuilder.Entity<TbBlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__tb_BlogC__C3B4DFCAD5D69924");

            entity.ToTable("tb_BlogComments");

            entity.Property(e => e.CommentText).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsApproved).HasDefaultValue(true);

            entity.HasOne(d => d.Blog).WithMany(p => p.TbBlogComments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlogComments_Blogs");

            entity.HasOne(d => d.Customer).WithMany(p => p.TbBlogComments)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_BlogComments_Customers");
        });

        modelBuilder.Entity<TbMenu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__tb_Menu__C99ED2304DA7EBAE");

            entity.ToTable("tb_Menu");

            entity.HasIndex(e => e.ParentId, "IX_Menu_ParentId");

            entity.Property(e => e.Alias).HasMaxLength(150);
            entity.Property(e => e.CreatedBy).HasMaxLength(150);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Levels).HasDefaultValue(1);
            entity.Property(e => e.ModifiedBy).HasMaxLength(150);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.Url).HasMaxLength(255);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Menu_Parent");
        });

        modelBuilder.Entity<TbRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tb_Roles__8AFACE1AE4639A4F");

            entity.ToTable("tb_Roles");

            entity.Property(e => e.CreatedBy).HasMaxLength(150);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedBy).HasMaxLength(150);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC7689623B");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E422CEF33C").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
