using AdornmeStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
        public DbSet<Banner> Banners => Set<Banner>();
        public DbSet<Checkout> Checkouts => Set<Checkout>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        public DbSet<Offer> Offers => Set<Offer>();

        public DbSet<OfferProduct> OfferProducts => Set<OfferProduct>();

        public DbSet<OfferCategory> OfferCategories => Set<OfferCategory>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // User
            // =========================

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            // =========================
            // Order
            // =========================

            modelBuilder.Entity<Order>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            // =========================
            // Order Item
            // =========================

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            // =========================
            // Payment
            // =========================

            modelBuilder.Entity<Payment>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            // =========================
            // Wishlist
            // =========================

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();

            // =========================
            // User -> Addresses
            // =========================

            modelBuilder.Entity<User>()
                .HasMany(x => x.Addresses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // User -> Orders
            // =========================
            // IMPORTANT:
            // Do not delete historical orders
            // when a user is deleted.

            modelBuilder.Entity<Order>()
                .HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // Address -> Orders
            // =========================
            // IMPORTANT:
            // Do not delete orders when an
            // address is deleted.

            modelBuilder.Entity<Order>()
                .HasOne(x => x.Address)
                .WithMany()
                .HasForeignKey(x => x.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // Category -> Products
            // =========================

            modelBuilder.Entity<Category>()
                .HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // Product -> Wishlist
            // =========================

            modelBuilder.Entity<Product>()
                .HasMany(x => x.WishlistItems)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // Product -> Cart
            // =========================

            modelBuilder.Entity<Product>()
                .HasMany(x => x.CartItems)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(x => x.OriginalPrice)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(x => x.SellingPrice)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(x => x.DiscountPercentage)
                    .HasPrecision(5, 2)
                    .IsRequired();
            });

            // =========================
            // Order -> OrderItems
            // =========================

            modelBuilder.Entity<Order>()
                .HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // Order -> Payment
            // =========================

            modelBuilder.Entity<Order>()
                .HasOne(x => x.Payment)
                .WithOne(x => x.Order)
                .HasForeignKey<Payment>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.TokenHash)
                .IsUnique();

            modelBuilder.Entity<WishlistItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.ProductId
                })
                .IsUnique();

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // Cart
            // =========================


            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new
                {
                    x.CartId,
                    x.ProductId
                })
                .IsUnique();

                entity.HasOne(x => x.Cart)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
           
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.HasOne(x => x.Product)
                    .WithMany(x => x.ProductImages)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new
                {
                    x.ProductId,
                    x.DisplayOrder
                });
            });

            // =========================
            // Offer
            // =========================

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.OfferType)
                    .IsRequired();

                entity.Property(x => x.Scope)
                    .IsRequired();

                entity.Property(x => x.DiscountValue)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(x => x.StartDate)
                    .IsRequired();

                entity.Property(x => x.EndDate)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.IsActive,
                    x.StartDate,
                    x.EndDate
                });
            });


            // =========================
            // Offer -> Products
            // =========================

            modelBuilder.Entity<OfferProduct>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.OfferId,
                    x.ProductId
                });

                entity.HasOne(x => x.Offer)
                    .WithMany(x => x.OfferProducts)
                    .HasForeignKey(x => x.OfferId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ProductId);
            });


            // =========================
            // Offer -> Categories
            // =========================

            modelBuilder.Entity<OfferCategory>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.OfferId,
                    x.CategoryId
                });

                entity.HasOne(x => x.Offer)
                    .WithMany(x => x.OfferCategories)
                    .HasForeignKey(x => x.OfferId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.CategoryId);
            });

            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Type)
                    .IsRequired();

                entity.Property(x => x.Quantity)
                    .IsRequired();

                entity.Property(x => x.PreviousStock)
                    .IsRequired();

                entity.Property(x => x.NewStock)
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(500);

                entity.Property(x => x.Reference)
                    .HasMaxLength(200);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasOne(x => x.Product)
                    .WithMany(x => x.InventoryTransactions)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ProductId);

                entity.HasIndex(x => x.CreatedAt);
            });
        }
    }
}
