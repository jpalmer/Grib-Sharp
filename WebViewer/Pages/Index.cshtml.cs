using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebViewer.Pages
{
    public class IndexModel : PageModel
    {
        public List<Core.Point> values { get; set; }
        public void OnGet()
        {
            var t = ReferenceFiles.Properties.Resources.Pacific_wind_7days;
            values = Core.readHeader(new System.IO.MemoryStream(t));
        }
    }
}
