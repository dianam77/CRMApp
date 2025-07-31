using CRMApp.Constants;
using CRMApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CRMApp.Data
{
    public class CRMAppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
        IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public CRMAppDbContext(DbContextOptions<CRMAppDbContext> options) : base(options) { }

        public DbSet<CustomerIndividual> CustomerIndividuals { get; set; }
        public DbSet<CustomerCompany> CustomerCompanies { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ContactPhone> ContactPhones { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<CustomerCompanyRelation> CustomerCompanyRelations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<CustomerIndividual>().HasKey(ci => ci.CustomerId);
            modelBuilder.Entity<CustomerCompany>().HasKey(cc => cc.CustomerId);
            modelBuilder.Entity<Address>().HasKey(a => a.AddressId);
            modelBuilder.Entity<ContactPhone>().HasKey(cp => cp.PhoneId);
            modelBuilder.Entity<Email>().HasKey(e => e.EmailId);
            modelBuilder.Entity<CustomerCompanyRelation>().HasKey(r => r.RelationId);

            // Cascade Delete Rules
            modelBuilder.Entity<CustomerCompanyRelation>()
                .HasOne(r => r.IndividualCustomer)
                .WithMany(i => i.CustomerCompanyRelations)
                .HasForeignKey(r => r.IndividualCustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerCompanyRelation>()
                .HasOne(r => r.CompanyCustomer)
                .WithMany(c => c.CustomerCompanyRelations)
                .HasForeignKey(r => r.CompanyCustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.IndividualCustomer)
                .WithMany(i => i.Addresses)
                .HasForeignKey(a => a.IndividualCustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.CompanyCustomer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CompanyCustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContactPhone>()
                .HasOne(cp => cp.IndividualCustomer)
                .WithMany(i => i.ContactPhones)
                .HasForeignKey(cp => cp.IndividualCustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContactPhone>()
                .HasOne(cp => cp.CompanyCustomer)
                .WithMany(c => c.ContactPhones)
                .HasForeignKey(cp => cp.CompanyCustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Email>()
                .HasOne(e => e.IndividualCustomer)
                .WithMany(i => i.Emails)
                .HasForeignKey(e => e.IndividualCustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Email>()
                .HasOne(e => e.CompanyCustomer)
                .WithMany(c => c.Emails)
                .HasForeignKey(e => e.CompanyCustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
