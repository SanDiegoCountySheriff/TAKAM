namespace TAK_Access_Manager.Models.ViewModels
{
    public class AdminViewModel
    {
        public TakUser user { get; set; }
        public TakAgency agency { get; set; }
        public TakGroup? groupDetails { get; set; }
        public List<TakGroup> groupList { get; set; }
        public List<TakUser> userList { get; set; }
        public List<DataPackage> fileList { get; set; }
        public AdminViewModel()
        {
            user = new TakUser();
            agency = new TakAgency();
            groupDetails = new TakGroup();
            groupList = new List<TakGroup>();
        }
    }
}
