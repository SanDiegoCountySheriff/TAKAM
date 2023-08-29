using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class PkgStatusCode
    {
        [Key]
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public int Active { get; set; }
    }
}
