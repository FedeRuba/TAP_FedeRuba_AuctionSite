using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class SessionDb
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public DateTime ValidUntil { get; set; }
        [Required]
        public virtual UserDb User { get; set; }
        public string SiteName { get; set; }
    }
}
