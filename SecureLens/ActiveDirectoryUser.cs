using System;
using System.Collections.Generic;

namespace SecureLens
{
    public class ActiveDirectoryUser
    {
        public string Title { get; set; }
        public string Department { get; set; }
        public string DistinguishedName { get; set; }
        public DateTime Created { get; set; }
        // This was originally List<ActiveDirectoryGroup> Groups
        public List<ActiveDirectoryGroup> Groups { get; set; }
    }
}