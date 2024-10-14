using Gluten.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class GlutenDataContext : DbContext
    {
        public GlutenDataContext(DbContextOptions<GlutenDataContext> contextOptions) : base(contextOptions)
        {
        }


        public GlutenDataContext()
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("");
        //}


        public DbSet<Venue> Venues { get; set; }
    }
}
