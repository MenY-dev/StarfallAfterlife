namespace StarfallAfterlife.Bridge.Server
{
    public enum SfaClientState : byte
    {
        //None = 0,
        //WaitGame = 1,
        //Game = 2,
        //InMainMenu = 3,
        //RankedMode = 4,
        //EnterToGalaxy = 5,
        //InGalaxy = 6,


        PendingLogin = 10,
        PendingGame = 11,
        InRankedMode = 13,
        InDiscoveryMod = 14,
    }
}