using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public record struct QuestTileParams (string TileName, Dictionary<string, JsonNode> Params)
    {

    }
}
