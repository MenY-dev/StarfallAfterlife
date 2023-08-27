using System.Text.Json.Nodes;

namespace StarfallAfterlife.Bridge.Primitives
{
    public interface ISfaObject
    {
        void Init();
        void LoadFromJson(JsonNode doc);
        JsonNode ToJson();
    }
}