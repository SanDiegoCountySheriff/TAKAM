using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class UsrGroupAssignment
    {
        [Key]
        public int UsrAssignmentId { get; set; }
        public int? Packageid { get; set; }
        public int? GroupId { get; set; }
        public DateTime? AssignmentDate { get; set; }
    }
}
