namespace SecureLens.Core.Models
{
    public class ActiveDirectoryGroup
    {
        public string Name { get; set; }
        public List<ActiveDirectoryUser> Users { get; set; }

        public ActiveDirectoryGroup()
        {
            Users = new List<ActiveDirectoryUser>();
        }
    }
}