using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class TakUser
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; } 
        public DateTime LastLogon { get; set; }
        public int? PrimaryGroup { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }
        public bool Active { get; set; }
    }
}
