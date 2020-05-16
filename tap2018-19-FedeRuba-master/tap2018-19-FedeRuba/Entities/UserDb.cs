using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace Entities
{
    public class UserDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, Index("IX_SiteName", 0, IsUnique = true)]
        [MaxLength(DomainConstraints.MaxUserName), MinLength(DomainConstraints.MinUserName)]
        public string Username { get; set; }
        [Required]
        [MinLength(DomainConstraints.MinUserPassword)]
        public string Password { get; set; }
        [Index("IX_SiteName", 1, IsUnique = true)]
        [MaxLength(DomainConstraints.MaxSiteName), MinLength(DomainConstraints.MinSiteName)]
        public string SiteName { get; set; }
    }
}
