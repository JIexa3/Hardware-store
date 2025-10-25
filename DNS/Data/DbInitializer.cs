using DNS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DNS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Проверяем, есть ли уже данные
            if (context.Categories.Any())
            {
                return; // База данных уже заполнена
            }

            // Добавляем категории
            var categories = new Category[]
            {
                new Category { Name = "Смартфоны", Description = "Мобильные телефоны и смартфоны" },
                new Category { Name = "Ноутбуки", Description = "Портативные компьютеры" },
                new Category { Name = "Планшеты", Description = "Планшетные компьютеры" },
                new Category { Name = "Компьютеры", Description = "Настольные компьютеры и моноблоки" },
                new Category { Name = "Аксессуары", Description = "Аксессуары для электроники" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

         
          

            // Проверяем, есть ли уже пользователи
            if (context.Users.Any())
            {
                return;   // База данных уже заполнена
            }

            var users = new User[]
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Password = "admin",
                    IsAdmin = true,
                    IsEmailVerified = true,
                    RegisterDate = DateTime.Now
                },
                new User
                {
                    Username = "user",
                    Email = "user@example.com",
                    Password = "user",
                    IsAdmin = false,
                    IsEmailVerified = true,
                    RegisterDate = DateTime.Now
                }
            };

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();
        }
    }
}
