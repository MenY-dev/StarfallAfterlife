using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public interface ICharacterInstanceListener
    {
        public void OnInstanceInteractEvent(ServerCharacter character, int systemId, string eventType, string eventData);
    }
}
