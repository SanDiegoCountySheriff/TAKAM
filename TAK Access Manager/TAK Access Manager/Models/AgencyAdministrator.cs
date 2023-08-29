using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class AgencyAdministrator
    {
        [Key]
        public int AgencyId { get; set; }
        public Guid UserId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? DeactivationDate { get; set; }
        public int? Active { get; set; }
        public bool GroupOnly { get; set; }

    }
}
