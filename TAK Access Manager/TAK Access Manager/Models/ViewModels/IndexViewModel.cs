namespace TAK_Access_Manager.Models.ViewModels
{
    public class IndexViewModel
    {
        public TakUser user { get; set; }
        public List<TakUser> userList { get; set; }
        public List<TakGroup> groupList { get; set; }
        public IndexViewModel()
        {
            user = new TakUser();
            userList = new List<TakUser>();
            groupList = new List<TakGroup>();
        }
    }
}
