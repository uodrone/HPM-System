﻿using Microsoft.EntityFrameworkCore;
using HPM_System.UserService.Models;

namespace HPM_System.UserService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка сущности User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id); // Указываем первичный ключ
                entity.Property(u => u.Id).ValueGeneratedOnAdd(); // автоинкремент                

                // Опционально: настройка других полей
                entity.Property(u => u.FirstName).IsRequired();
                entity.Property(u => u.LastName).IsRequired();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PhoneNumber).IsRequired();

                entity.HasIndex(u => u.Email).IsUnique();  // Уникальный индекс на Email
                entity.HasIndex(u => u.PhoneNumber).IsUnique();  // Уникальный индекс на Номер телефона
            });

            // Настройка сущности Car
            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasKey(c => c.Id); // Указываем первичный ключ
                entity.Property(c => c.Id).ValueGeneratedOnAdd(); // автоинкремент

                // Связь один ко многим: User -> Car
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Cars)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // каскадное удаление при удалении User

                // Добавляем уникальный индекс по полю Number
                entity.HasIndex(c => c.Number).IsUnique();
            });
        }
    }
}