﻿using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Racoonogram.Models
{
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
        public string Status { get; set; }
        [DataType(DataType.MultilineText)]
        public string Additionally { get; set; }
        [DataType(DataType.Url)]
        public string Site { get; set; }
        public virtual ICollection<Image> Images { get; set; }

        public ApplicationUser()
        {
            Images = new List<Image>();
        }
    }

    // добавляем модель Image
    public class Image
    {
        public int ImageId { get; set; }
        public string ApplicationUserId { get; set; }
        public string Category { get; set; }
        public string KeyWords { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string Colors { get; set; }
        public bool IsBlack { get; set; }
        public string Orient { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
    public class Order
    {
        public int OrderId { get; set; }
        public int ImageId { get; set; }
        public string ApplicationUserId { get; set; }
        public string BuyerEmail { get; set; }
        public double Price { get; set; }
        public int Size { get; set; }
        public int IsHide { get; set; }
        public DateTime BuyingDate { get; set; }
    }
    public class Like
    {
        public int LikeId { get; set; }
        public int ImageId { get; set; }
        public string UserId { get; set; }
        public DateTime BuyingDate { get; set; }
    }

    public class QueryHistory
    {
        public int Id { get; set; }
        public string QuerySting { get; set; }
        public DateTime QueryDate { get; set; }
    }

    public class ImgDownload
    {
        public string Id { get; set; }
        public DateTime DateLast { get; set; }
    }

    public class Plan
    {
        public string Id { get; set; }
        public int PlanPrice { get; set; }
        public int ImgCount { get; set; }
    }
    public class PlanBuying
    {
        public int Id { get; set; }
        public string Id_user { get; set; }
        public string Id_plan { get; set; }
        public double MoneyBalance { get; set; }
        public int ImageBalance { get; set; }
        public int isHide { get; set; }
        public DateTime BuyingDate { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<QueryHistory> QueryHistories { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<ImgDownload> ImgDownloads { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanBuying> PlanBuyings { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}