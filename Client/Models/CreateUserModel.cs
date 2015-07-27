using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Client.Models
{
    public class CreateUserModel
    {
        public User User { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }
    }
}