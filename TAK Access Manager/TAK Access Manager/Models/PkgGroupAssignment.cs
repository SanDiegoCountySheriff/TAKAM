using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class PkgGroupAssignment
    {
        [Key]
        public int GroupAssignmentId { get; set; }
        public int? Packageid { get; set; }
        public int? GroupId { get; set; }
        public bool? Active { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public DateTime? UnassignDate { get; set; }
    }
}
