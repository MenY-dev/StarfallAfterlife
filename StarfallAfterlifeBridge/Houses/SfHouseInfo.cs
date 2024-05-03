using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public class SfHouseInfo
    {
        public string Location { get; set; }

        public SfHouse House { get; set; }

        public void Save()
        {
            try
            {
                House?.ToJson()?.WriteToFileUnbuffered(
                    Location, new() { WriteIndented = true });
            }
            catch { }
        }

        public bool Load()
        {
            var result = false;

            try
            {
                var doc = JsonHelpers.ParseNodeFromFile(Location);
                result = (House ??= new()).LoadFromJson(doc);
            }
            catch { }

            return result;
        }
    }
}
