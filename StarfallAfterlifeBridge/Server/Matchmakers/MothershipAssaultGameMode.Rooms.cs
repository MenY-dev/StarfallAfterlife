using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public partial class MothershipAssaultGameMode
    {
        protected List<MothershipAssaultRoom> Rooms { get; } = new();

        public MothershipAssaultRoom CreateNewRoom(string name)
        {
            lock (_locker)
            {
                var room = new MothershipAssaultRoom(this) { Name = name };
                Rooms.Add(room);
                return room;
            }
        }

        public void RemoveRoom(MothershipAssaultRoom room)
        {
            lock ( _locker)
            {
                room.Close();
                Rooms.Remove(room);
            }
        }

        public MothershipAssaultRoom[] GetRooms()
        {
            lock (_locker)
                return Rooms.ToArray();
        }
    }
}
