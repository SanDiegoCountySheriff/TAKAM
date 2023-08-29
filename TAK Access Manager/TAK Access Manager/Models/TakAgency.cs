using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class TakAgency
    {
        [Key]
        public int AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public string? AgencyDesc { get; set; }
        public string? AgencyAdmin { get; set; }
        public DateTime? CreateDt { get; set; }
        public string? Domain { get; set; }
        public int? DefaultGroupId { get; set; }
    }
}
