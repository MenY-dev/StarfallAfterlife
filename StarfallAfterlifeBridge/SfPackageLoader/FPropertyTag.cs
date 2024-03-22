using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct FPropertyTag
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public int Size { get; set; }

        public int ArrayIndex { get; set; }

        public string StructName { get; set; }

        public byte[] StructGuid { get; set; }

        public byte BoolVal { get; set; }

        public string EnumName { get; set; }

        public string InnerType { get; set; }

        public string ValueType { get; set; }

        public byte HasPropertyGuid { get; set; }

        public byte[] PropertyGuid { get; set; }

        private static readonly StringComparer _nameComparison = StringComparer.OrdinalIgnoreCase;

        public bool IsType(string type) =>
            _nameComparison?.Equals(Type, type) == true;

        public bool IsInnerType(string type) =>
            _nameComparison.Equals(InnerType, type) == true;

        public bool IsStructName(string name) =>
            _nameComparison.Equals(StructName, name) == true;
    }
}
