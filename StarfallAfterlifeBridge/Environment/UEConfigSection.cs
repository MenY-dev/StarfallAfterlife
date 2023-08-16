using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Environment
{
    public class UEConfigSection : List<string>
    {
        public string SectionClass { get; set; }

        public UEConfigSection(string sectionClass)
        {
            SectionClass = sectionClass;
        }
    }
}
