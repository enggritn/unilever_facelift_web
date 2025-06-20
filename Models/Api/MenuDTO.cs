using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class MenuDTO
    {
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuTitle { get; set; }

        public IEnumerable<MenuDTO> menus { get; set; }
    }
}