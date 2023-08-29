using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Reflection.Emit;

namespace TAK_Access_Manager.Models
{
    public class TakDbContext : DbContext
    {
        public TakDbContext() { }
        public TakDbContext(DbContextOptions<TakDbContext> options) : base(options) { }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<TakUser> TakUsers { get; set; }
        public virtual DbSet<TakGroup> TakGroups { get; set; }
        public virtual DbSet<TakAgency> TakAgencies { get; set; }
        public virtual DbSet<CfgTypeCodes> CfgTypeCodes { get; set; }
        public virtual DbSet<PkgStatusCode> PkgStatusCodes { get; set; } 
        public virtual DbSet<DataPackage> DataPackages { get; set; } 
        public virtual DbSet<AgencyAdministrator> AgencyAdministrators { get; set; }
        public virtual DbSet<PkgGroupAssignment> PkgGroupAssignments { get; set; }
        public virtual DbSet<UsrGroupAssignment> UsrGroupAssignments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) { }
    }
}
