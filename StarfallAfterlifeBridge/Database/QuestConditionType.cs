using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum QuestConditionType : byte
    {
        None = 0,
        DeliverItems = 1,
        ExploreSystem = 2,
        KillShip = 3,
        KillFleet = 4,
        KillUniquePirateShip = 5,
        InterceptShip = 6,
        ScanUnknownPlanet = 7,
        CompleteTask = 8,
        CompleteSpecialInstance = 9,
        ExploreObject = 10,
        StatTracking = 11,
        Client = 12,
        ExploreRelictShip = 13,
        DeliverMobDrop = 14,
        KillUniquePersonalPirateShip = 15,
        KillMobShip = 16,
        KillFactionGroupShip = 17,
        ExploreSecretLocation = 18,
        KillPiratesOutpost = 19,
        KillPiratesStation = 20,
        KillBoss = 21,
        DeliverQuestItem = 22,
        ScanSystemObject = 23,
        KillBossOfAnyFactiont = 24,
        ResearchProject = 25,
        ReachCharacterLevel = 26,
        InstanceEvent = 27,
        PickUpAndDeliverQuestItem = 28,
        MineQuestItem = 29,
        DeliverRandomItems = 30,
        InterceptPersonalMob = 31,
    }
}
