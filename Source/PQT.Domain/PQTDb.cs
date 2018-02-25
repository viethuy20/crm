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
        protected override void OnModelCreating(DbModelBuilder mb)
        {
            mb.Entity<User>().HasMany(u => u.Roles).WithMany().Map(map => map.ToTable("Users_Roles").MapLeftKey("User_Id").MapRightKey("Role_Id"));
            mb.Entity<Role>().HasMany(r => r.Permissions).WithMany().Map(map => map.ToTable("Roles_Permissions").MapLeftKey("Role_Id").MapRightKey("Permission_Id"));
            mb.Entity<Menu>().HasMany(m => m.Roles).WithMany().Map(map => map.ToTable("Menus_Roles").MapLeftKey("Menu_Id").MapRightKey("Role_Id"));
            
        }
    }
}
