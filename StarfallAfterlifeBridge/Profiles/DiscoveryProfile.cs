using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class DiscoveryProfile
    {
        [JsonPropertyName("chars")]
        public List<Character> Chars { get; set; } = new List<Character>();

        public Character GetCharById(int id)
        {
            foreach (var character in Chars)
            {
                if (character?.Id == id)
                    return character;
            }

            return null;
        }


        public Character GetCharByUniqueId(int id)
        {
            if (id < 0)
                return null;

            foreach (var character in Chars)
            {
                if (character?.UniqueId == id)
                    return character;
            }

            return null;
        }
    }
}