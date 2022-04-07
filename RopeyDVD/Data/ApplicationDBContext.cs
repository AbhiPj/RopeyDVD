using Microsoft.EntityFrameworkCore;
using RopeyDVD.Models;

namespace RopeyDVD.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {


        }
        public DbSet<User> Users { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<DVDCategory> DVDCategory { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<Studio> Studios { get; set; }
        public DbSet<MembershipCategory> MembershipCategories { get; set; }
        public DbSet<LoanType> LoanTypes { get; set; }


    }
}
