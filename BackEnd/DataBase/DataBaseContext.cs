﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models.Equipments;
using Models.Events;
using Models;
using Models.DataBaseLinks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Models.People;
using BackEnd.Models;
using Models.Events.Roles;
using Models.People.Roles;
using Models.People.UserProperties;

namespace BackEnd.DataBase
{
    public class DataBaseContext : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<RegisterTokenPair> RegisterTokenPairs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<EventRole> EventRoles { get; set; }
        public DbSet<UserProperty> UserProperties { get; set; }
        public DbSet<UserPropertyType> UserPropertyTypes { get; set; }


        public DataBaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigurePlaceEquipment(builder);
            ConfigurePlaceUserEventRole(builder);
            ConfigureEquipmentType(builder);
        }


        private static void ConfigurePlaceEquipment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlaceEquipment>()
               .HasKey(t => new { t.EquipmentId, t.PlaceId });

            modelBuilder.Entity<PlaceEquipment>()
                .HasOne(pe => pe.Place)
                .WithMany(pl => pl.PlaceEquipments)
                .HasForeignKey(pl => pl.PlaceId);

            modelBuilder.Entity<PlaceEquipment>()
                .HasOne(pe => pe.Equipment)
                .WithMany(eq => eq.PlaceEquipments)
                .HasForeignKey(pe => pe.EquipmentId);
        }

        private static void ConfigurePlaceUserEventRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlaceUserEventRole>()
                .HasKey(pur => new { pur.UserId, pur.PlaceId });
            modelBuilder.Entity<PlaceUserEventRole>()
                .HasOne(pur => pur.User)
                .WithMany(u => u.PlaceUserEventRoles)
                .HasForeignKey(pur => pur.UserId);

            modelBuilder.Entity<PlaceUserEventRole>()
                .HasOne(pur => pur.Place)
                .WithMany(p => p.PlaceUserEventRoles)
                .HasForeignKey(pur => pur.PlaceId);

            modelBuilder.Entity<PlaceUserEventRole>()
                .HasOne(pur => pur.EventRole)
                .WithMany(er => er.PlaceUserEventRoles)
                .HasForeignKey(pur => pur.EventRoleId);
        }

        private static void ConfigureEquipmentType(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentType>()
                .HasMany(et => et.Children)
                .WithOne(et => et.Parent)
                .HasForeignKey(et => et.ParentId);

            modelBuilder.Entity<EquipmentType>()
                .HasMany(et => et.AllChildren)
                .WithOne(et => et.Root)
                .HasForeignKey(et => et.RootId);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker
                .Entries<Event>()
                .Where(e => e.State != EntityState.Deleted)
                .ToList()
                .ForEach(e => 
                {
                    e.Entity.BeginTime = e.Entity.Shifts?.Min(s => s.BeginTime) ?? e.Entity.BeginTime;
                    e.Entity.EndTime = e.Entity.Shifts?.Max(s => s.EndTime) ?? e.Entity.EndTime;
                });
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
