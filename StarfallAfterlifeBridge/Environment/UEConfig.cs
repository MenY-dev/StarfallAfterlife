using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Environment
{
    public class UEConfig : List<UEConfigSection>
    {
        public string ToConfigString()
        {
            var doc = new StringBuilder();

            foreach (var section in this)
            {
                if (section is null)
                    continue;

                if (string.IsNullOrWhiteSpace(section.SectionClass) == false)
                {
                    doc.Append('[');
                    doc.Append(section.SectionClass);
                    doc.Append(']');
                    doc.AppendLine();
                }

                foreach (var line in section)
                {
                    if (line is null)
                        continue;

                    doc.AppendLine(line);
                }

                doc.AppendLine();
            }

            return doc.ToString();
        }
    }
}
