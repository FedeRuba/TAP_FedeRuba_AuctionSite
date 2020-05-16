using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class SiteContext : DbContext
    {
        public DbSet<UserDb> Users { get; set; }
        public DbSet<SiteDb> Sites { get; set; }
        public DbSet<SessionDb> Sessions { get; set; }
        public DbSet<AuctionDb> Auctions { get; set; }


        public SiteContext(string connectionString) : base(connectionString)
        {
        }
    }
}
