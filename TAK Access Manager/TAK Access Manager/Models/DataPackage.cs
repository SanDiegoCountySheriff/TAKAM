using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class DataPackage
    {
        [Key]
        public int PackageId { get; set; }
        public string? PackageName { get; set; }
        public string? GroupIds { get; set; }
        public string? Server { get; set; }
        public string? Notes { get; set; }
        public string? BlobPath { get; set; }
        public int? StatusCid { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ConfigureDt { get; set; }
        public int? ParentPkgId { get; set; }
        public bool? Renewed { get; set; }
    }
}
