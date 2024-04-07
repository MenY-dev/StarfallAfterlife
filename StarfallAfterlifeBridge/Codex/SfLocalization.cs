using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public class SfLocalization
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("namespaces")]
        public List<SfLocalizationNamespace> Namespaces { get; set; }

        internal string GetText(string key, string tag = null)
        {
            if (key is null)
                return null;

            var namespaces = Enumerable.Empty<SfLocalizationNamespace>();

            if (tag is null)
            {
                namespaces = Namespaces;
            }
            else
            {
                var target = Namespaces.FirstOrDefault(n => tag.Equals(n.Name, StringComparison.OrdinalIgnoreCase) == true);

                if (target is not null)
                    namespaces = Enumerable.Repeat(target, 1);
            }

            foreach (var page in namespaces)
            {
                if (page?.Strings?.GetValueOrDefault(key) is string text)
                    return text;
            }

            return null;
        }
    }
}
