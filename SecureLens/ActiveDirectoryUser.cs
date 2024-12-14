using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SecureLens
{
    public class ActiveDirectoryUser
    {
        public string Title { get; set; }
        public string Department { get; set; }
        public string DistinguishedName { get; set; }
        public DateTime Created { get; set; }

        public List<ActiveDirectoryGroup> Groups { get; set; }
    }
}