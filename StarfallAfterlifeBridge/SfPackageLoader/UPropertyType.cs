using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct UPropertyType
    {
        public const string Int8 = "IntProperty8";
        public const string Int = "IntProperty";
        public const string Int16 = "IntProperty16";
        public const string Int64 = "IntProperty64";
        public const string Byte = "ByteProperty";
        public const string UInt16 = "UInt16Property";
        public const string UInt32 = "UInt32Property";
        public const string UInt64 = "UInt64Property";
        public const string Float = "FloatProperty";
        public const string Double = "DoubleProperty";
        public const string Text = "TextProperty";
        public const string String = "StrProperty";
        public const string Bool = "BoolProperty";
        public const string Name = "NameProperty";
        public const string Enum = "EnumProperty";
        public const string Struct = "StructProperty";
        public const string Object = "ObjectProperty";
        public const string SoftObject = "SoftObjectProperty";
        public const string Array = "ArrayProperty";
        public const string Map = "MapProperty";
        public const string Set = "SetProperty";
        public const string MulticastSparseDelegate = "MulticastSparseDelegateProperty";
    }
}
