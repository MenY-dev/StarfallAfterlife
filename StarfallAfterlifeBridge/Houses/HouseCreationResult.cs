using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public enum HouseCreationResult
    {
        Success = 0,
        NameAlreadyExists = 1,
        TagAlreadyExists = 2,
        UserAlreadyInTheHouse = 3,
        NameIsTooLong = 4,
        TagIsTooLong = 5,
        Unknown = 254,
    }
}
