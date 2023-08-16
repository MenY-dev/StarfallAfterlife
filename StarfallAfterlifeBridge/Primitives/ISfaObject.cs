using StarfallAfterlife.Bridge.Serialization.Json;

namespace StarfallAfterlife.Bridge.Primitives
{
    public interface ISfaObject
    {
        void Init();
        void LoadFromJson(JsonNode doc);
        JsonNode ToJson();
    }
}