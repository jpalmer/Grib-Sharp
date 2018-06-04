using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebViewer.Models
{
    public class Index
    {
        public List<float> ForecastValues { get; set; }
        public void test()
        {
            var t = ReferenceFiles.Properties.Resources.Pacific_wind_7days;
            ForecastValues = Core.readHeader(new System.IO.MemoryStream(t));
        }
    }
}
