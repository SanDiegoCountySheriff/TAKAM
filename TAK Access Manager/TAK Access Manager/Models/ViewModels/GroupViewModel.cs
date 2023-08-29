namespace TAK_Access_Manager.Models.ViewModels
{
    public class GroupViewModel
    {
        public TakUser user { get; set; }
        public List<DataPackage> fileList { get; set; }
        public TakGroup? groupDetails { get; set; }
        public List<TakGroup> groupList { get; set; }
        public int? AdminType { get; set; }
        public GroupViewModel()
        {
            user = new TakUser();
            groupDetails = new TakGroup();
            fileList = new List<DataPackage>();
            groupList = new List<TakGroup>();
        }
    }
}
