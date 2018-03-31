using System.Configuration;
using System.Data.Entity;
using System.Web;
using NS.Entity;
using PQT.Domain.Entities;

namespace PQT.Domain
{
    public class PQTDb : DbContext
    {
        public PQTDb()
        {
            // hook up the Migrations configuration
            Database.SetInitializer<PQTDb>(null);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<Menu> MenuItems { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<EmailSetting> EmailSettings { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<SalesGroup> SalesGroups { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        protected override void OnModelCreating(DbModelBuilder mb)
        {
            mb.Entity<User>().HasMany(u => u.Roles).WithMany().Map(map => map.ToTable("Users_Roles").MapLeftKey("User_Id").MapRightKey("Role_Id"));
            mb.Entity<Role>().HasMany(r => r.Permissions).WithMany().Map(map => map.ToTable("Roles_Permissions").MapLeftKey("Role_Id").MapRightKey("Permission_Id"));
            mb.Entity<Menu>().HasMany(m => m.Roles).WithMany().Map(map => map.ToTable("Menus_Roles").MapLeftKey("Menu_Id").MapRightKey("Role_Id"));
            mb.Entity<SalesGroup>().HasMany(m => m.Users).WithMany().Map(map => map.ToTable("SalesGroup_Users").MapLeftKey("SalesGroup_Id").MapRightKey("User_Id"));
            mb.Entity<Event>().HasMany(m => m.SalesGroups).WithMany().Map(map => map.ToTable("Event_SalesGroup").MapLeftKey("Event_Id").MapRightKey("SalesGroup_Id"));
            mb.Entity<Event>().HasMany(m => m.Users).WithMany().Map(map => map.ToTable("Event_Users").MapLeftKey("Event_Id").MapRightKey("User_Id"));
            mb.Entity<Event>().HasMany(m => m.Companies).WithMany().Map(map => map.ToTable("Event_Companies").MapLeftKey("Event_Id").MapRightKey("Company_Id"));
            //mb.Entity<Event>().HasMany(m => m.Trainers).WithMany().Map(map => map.ToTable("Event_Trainers").MapLeftKey("Event_Id").MapRightKey("Trainer_Id"));
            mb.Entity<Registration>().HasMany(m => m.EventSessions).WithMany().Map(map => map.ToTable("Registration_EventSessions").MapLeftKey("Registration_Id").MapRightKey("EventSession_Id"));
        }
    }
}
