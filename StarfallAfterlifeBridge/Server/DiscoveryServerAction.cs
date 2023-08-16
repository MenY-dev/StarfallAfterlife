using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum DiscoveryServerAction : byte
    {
        Sync = 1,
        Moved = 2,
        FleetWarpedGateway = 3,
        DisconnectObject = 4,
        Attacked = 5,
        Instance = 6,
        SetInBattle = 7,
        PlanetNamingFailed = 8,
        CargoList = 9,
        CargoListForObject = 10,
        RepairstationInfo = 11,
        FuelstationInfo = 12,
        SessionEnded = 13,
        NamingFailed = 14,
        FleetWarpedMothership = 15,
        ObjectInfoUpdated = 16,
        ObjectStockUpdated = 17,
        MoneyUpdated = 18,
        FleetRouteSync = 61,
        NewPlayerJoiningToInstance = 70,
        NewAIJoiningToInstance = 71,
        CharactDropSession = 72,
        InstanceInteractiveEvent = 73,
        FleetRecallStateUpdate = 75,
        NewAchievements = 76,
        ShowAiMessage = 77,
        InfoWidgetData = 78,
        SetVision = 100,
        SharedFleetVision = 101,
        SyncAbility = 102
    }
}
