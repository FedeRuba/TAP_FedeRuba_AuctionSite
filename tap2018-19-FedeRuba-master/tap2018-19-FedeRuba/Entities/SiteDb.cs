using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace Entities
{
    public class SiteDb
    {
        [Key]
        [MaxLength(DomainConstraints.MaxSiteName), MinLength(DomainConstraints.MinSiteName)]
        public string SiteName { get; set; }
        [Required]
        [Range(DomainConstraints.MinTimeZone, DomainConstraints.MaxTimeZone)]
        public int Timezone { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int SessionExpirationTimeInSeconds { get; set; }
        [Required]
        [Range(.0, double.MaxValue)]
        public double MinimumBidIncrement { get; set; }
        public virtual ICollection<SessionDb> Sessions { get; set; }
        public virtual ICollection<UserDb> Users { get; set; }
        public virtual ICollection<AuctionDb> Auctions { get; set; }
    }
}
