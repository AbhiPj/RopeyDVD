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

    }
}
