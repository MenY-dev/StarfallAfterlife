using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{

    public struct HardpointInfo
    {
        public string Name;
        public TechType Type;
        public int X, Y, Width, Height;

        public override string ToString() =>
            $"{Name}: (X = {X}, Y = {Y}, Width = {Width}, Height = {Height}, Type = {Type})";
    }
}
