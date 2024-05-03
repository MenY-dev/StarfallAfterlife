using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct HouseRankInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name_en")]
        public string NameEn { get; set; }

        [JsonPropertyName("name_ru")]
        public string NameRu { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("permissions")]
        public int Permissions { get; set; }

        public string GetName(string locale = null) =>
            string.IsNullOrWhiteSpace(locale) ? NameEn ?? NameRu :
            "en".Equals(locale, StringComparison.OrdinalIgnoreCase) ? NameEn ?? NameRu :
            "ru".Equals(locale, StringComparison.OrdinalIgnoreCase) ? NameRu ?? NameEn :
            NameEn ?? NameRu;
    }
}
