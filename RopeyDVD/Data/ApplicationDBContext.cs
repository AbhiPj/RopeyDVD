using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Models;

namespace RopeyDVD.Data
{
    public class ApplicationDBContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CastMember>()
                .HasKey(b => new { b.DVDNumber, b.ActorNumber });

    //        modelBuilder
    //.Entity<UserRegisterModel>(builder =>
    //{
    //    builder.HasNoKey();
    //    builder.ToTable("UserRegisterModel");
    //});

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<DVDCategory> DVDCategory { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<Studio> Studios { get; set; }
        public DbSet<MembershipCategory> MembershipCategories { get; set; }
        public DbSet<LoanType> LoanTypes { get; set; }
        public DbSet<DVDTitle> DVDTitle { get; set; }
        public DbSet<CastMember> CastMember { get; set; }
        public DbSet<Member> Member { get; set; }
        public DbSet<DVDCopy> DVDCopy { get; set; }
        public DbSet<Loan> Loan { get; set; }
        public DbSet<RopeyDVD.Models.UserRegisterModel> UserRegisterModel { get; set; }


    }
}
