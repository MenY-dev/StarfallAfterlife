using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class StarSystemDungeon : StarSystemObject
    {
        public bool IsDungeonVisible { get; protected set; } = true;

        public virtual void SetDungeonVisible(bool isVisible)
        {
            if (isVisible == IsDungeonVisible)
                return;

            IsDungeonVisible = isVisible;

            if (isVisible == true)
                Broadcast<IStarSystemObjectListener>(l => l.OnObjectSpawned(this));
            else
                Broadcast<IStarSystemObjectListener>(l => l.OnObjectDestroed(System?.Id ?? -1, Type, Id));
        }
    }
}
