using System;
using System.Collections.Generic;

namespace StackOverflowClone.Models
{
    public class User
    {
        public string Id { get; set; } // We are going to need to access it, so while not required, this is very convenient to have
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Reputation { get; set; }
        public IList<string> FavoriteTags { get; set; }
        public bool IsActive { get; set; }
    }
}