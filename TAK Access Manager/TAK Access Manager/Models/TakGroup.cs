using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class TakGroup
    {
        [Key]
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupContactName { get; set; }
        public string? GroupContactNumber { get; set; }
        public string? GroupContactEmail { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDt { get; set; }
        public Guid CreateUserId { get; set; }
        public DateTime? LastModifiedDt { get; set; }
        public int? AgencyId { get; set; }
    }
}
