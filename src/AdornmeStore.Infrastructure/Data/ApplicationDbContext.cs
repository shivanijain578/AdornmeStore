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
            // Product
            // =========================

            modelBuilder.Entity<Product>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(x => x.DiscountPrice)
                .HasPrecision(18, 2);

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
        }
    }
}
