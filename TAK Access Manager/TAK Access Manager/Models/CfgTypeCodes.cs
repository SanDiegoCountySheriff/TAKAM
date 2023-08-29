using System.ComponentModel.DataAnnotations;

namespace TAK_Access_Manager.Models
{
    public class CfgTypeCodes
    {
        [Key]
        public int TypeCode { get; set; }
        public string TypeDescription { get; set; }
        public int Active { get; set; }
    }
}
