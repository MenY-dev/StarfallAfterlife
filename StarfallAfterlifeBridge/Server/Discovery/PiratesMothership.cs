using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class PiratesMothership : StarSystemDungeon
    {
        public float RespawnTimeout { get; set; } = 300;

        public DateTime RespawnTime { get; protected set; }

        public override void SetDungeonVisible(bool isVisible)
        {
            var currentState = IsDungeonVisible;

            base.SetDungeonVisible(isVisible);

            if (isVisible == currentState)
                return;

            if (isVisible == false)
            {
                System?.AddDeferredAction(() =>
                {
                    if (System is null || IsDungeonVisible == true)
                        return;

                    Hex = System.GetRandomFreeHex(1);
                    SetDungeonVisible(true);
                }, TimeSpan.FromSeconds(RespawnTimeout));
            }

            System?.UpdateNavigationMap();
        }
    }
}
