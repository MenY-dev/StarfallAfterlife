using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public abstract class UObjectBase
    {
        public abstract void Load(UAssetLoadContext context);

        protected abstract void AddPropertyInternal(UProperty property);

        protected void ReadObjectProperties(UAssetLoadContext context)
        {
            var reader = context.Reader;
            var endOffset = context.StartOffset + context.Size;

            while (reader.Position <= endOffset)
            {
                if (TryReadProperty(context, out var prop) == true)
                    AddPropertyInternal(prop);
                else if (prop.Name?.Equals("None") == true)
                    break;
            }
        }

        public static UProperty? ReadProperty(UAssetLoadContext context)
        {
            if (TryReadProperty(context, out var prop) == true)
                return prop;

            return null;
        }

        public static bool TryReadProperty(UAssetLoadContext context, out UProperty property)
        {
            var tag = ReadPropertyTag(context);

            if (tag.Name?.Equals("None") == true)
            {
                property = new()
                {
                    Name = tag.Name,
                    Tag = tag,
                };
                return false;
            }

            if (TryReadProperty(context, tag, out property) == true)
            {
                property.Tag = tag;
                return true;
            }

            return false;
        }

        public static bool TryReadProperty(UAssetLoadContext context, FPropertyTag tag, out UProperty property)
        {
            var uexp = context.Reader;
            var startPos = uexp.Position;
            var result = true;

            void MoveToEnd()
            {
                if (tag.Size < 0)
                    return;

                var endPos = startPos + tag.Size;

                if (uexp.Position != endPos)
                    uexp.Seek(endPos, SeekOrigin.Begin);
            }

            property = new UProperty()
            {
                Name = tag.Name,
                Type = tag.Type,
                Tag = tag,
            };

            if (context.Converters is not null)
            {
                foreach (var converter in context.Converters)
                {
                    if (converter.CanConvert(tag) == false)
                        continue;

                    if (converter.TryRead(context, tag, ref property) == true)
                    {
                        MoveToEnd();
                        return true;
                    }

                    uexp.Seek(startPos, SeekOrigin.Begin);
                }
            }

            if (tag.IsType(UPropertyType.Int)) property.Value = uexp.ReadInt32();
            else if (tag.IsType(UPropertyType.Int8)) property.Value = uexp.ReadInt8();
            else if (tag.IsType(UPropertyType.Int16)) property.Value = uexp.ReadInt16();
            else if (tag.IsType(UPropertyType.Int64)) property.Value = uexp.ReadInt64();
            else if (tag.IsType(UPropertyType.Byte)) property.Value = uexp.ReadByte();
            else if (tag.IsType(UPropertyType.UInt16)) property.Value = uexp.ReadUInt16();
            else if (tag.IsType(UPropertyType.UInt32)) property.Value = uexp.ReadUInt32();
            else if (tag.IsType(UPropertyType.UInt64)) property.Value = uexp.ReadUInt64();
            else if (tag.IsType(UPropertyType.Float)) property.Value = uexp.ReadSingle();
            else if (tag.IsType(UPropertyType.Double)) property.Value = uexp.ReadDouble();
            else if (tag.IsType(UPropertyType.Text))
            {
                var tmp = uexp.ReadUInt32();
                uexp.Skip(1);

                property.Value = new FText
                {
                    Namespace = uexp.ReadString(),
                    Key = uexp.ReadString(),
                    Text = uexp.ReadString(),
                };
            }
            else if (tag.IsType(UPropertyType.String)) property.Value = uexp.ReadString();
            else if (tag.IsType(UPropertyType.Bool)) property.Value = tag.BoolVal > 0;
            else if (tag.IsType(UPropertyType.Name)) property.Value = context.GetName(uexp.ReadName());
            else if (tag.IsType(UPropertyType.Enum)) property.Value = context.GetName(uexp.ReadName());
            else if (tag.IsType(UPropertyType.Struct))
            {
                if (tag.IsStructName("Guid"))
                {
                    property.Value = new Guid(uexp.ReadBytes(16));
                }
                else if (tag.IsStructName(name: "Vector"))
                {
                    property.Value = uexp.Read<FVector>();
                }
                else if (tag.IsStructName(name: "IntPoint"))
                {
                    property.Value = uexp.Read<FIntPoint>();
                }
                else if (tag.IsStructName("Rotator"))
                {
                    property.Value = uexp.Read<FRotator>();
                }
                else if (tag.IsStructName("PointerToUberGraphFrame"))
                {
                    var name = uexp.ReadName();
                    property.Value = context.GetName(name);
                }
                else
                {
                    result = false;
                }
            }
            else if (tag.IsType(UPropertyType.Object))
            {
                property.Value = uexp.ReadInt32();
            }
            else if (tag.IsType(UPropertyType.SoftObject)) property.Value = context.GetName(uexp.ReadSoftClassPtr().Name);
            else if (tag.IsType(UPropertyType.Array))
            {
                var arrayStartPos = uexp.Position;
                var count = uexp.ReadInt32();
                var structInfo = new FPropertyTag();

                if (count > 0)
                {
                    if (tag.IsInnerType(UPropertyType.Struct))
                    {
                        structInfo = ReadPropertyTag(context);
                    }

                    var items = new UProperty[count];
                    var itemSize = (int)((tag.Size - (uexp.Position - arrayStartPos)) / count);

                    if (itemSize > 0)
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            var itemTag = new FPropertyTag
                            {
                                Type = tag.InnerType,
                                Size = itemSize,
                                StructName = structInfo.StructName,
                                StructGuid = structInfo.StructGuid,
                            };

                            if (TryReadProperty(context, itemTag, out var arrayProp))
                            {
                                items[i] = arrayProp;
                            }
                            else
                            {
                                result = false;
                                break;
                            }
                        }

                        if (result == true)
                            property.Value = items;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            else if (tag.IsType(UPropertyType.Map))
            {
                var numKeysToRemove = uexp.ReadInt32();
                var numEntries = uexp.ReadInt32();

                if (numEntries < 0 || numKeysToRemove > 0)
                {
                    result = false;
                }
                else
                {
                    var map = new List<KeyValuePair<UProperty, UProperty>>();

                    for (int i = 0; i < numEntries; i++)
                    {
                        var keyTag = new FPropertyTag { Size = -1, Type = tag.InnerType };
                        var valueTag = new FPropertyTag { Size = -1, Type = tag.ValueType };

                        if (TryReadProperty(context, keyTag, out var key) == true &&
                            TryReadProperty(context, valueTag, out var val) == true)
                        {
                            map.Add(KeyValuePair.Create(key, val));
                        }
                        else
                        {
                            result = false;
                            break;
                        }
                    }

                    if (result == true)
                        property.Value = map;
                }
            }
            else if (tag.IsType(UPropertyType.MulticastSparseDelegate))
            {
                var flags = uexp.ReadInt32();

                if ((flags & 64) == 1) // Net
                    uexp.Skip(2);

                property.Value = flags;
            }
            else
            {
                result = false;
            }

            if (result == false)
            {

            }

            MoveToEnd();
            return result;
        }

        public static FPropertyTag ReadPropertyTag(UAssetLoadContext context)
        {
            var uexp = context.Reader;
            var tag = new FPropertyTag();

            tag.Name = context.GetName(uexp.ReadName());

            if (tag.Name is null ||
                tag.Name.Equals("None"))
            {
                return tag;
            }

            tag.Type = context.GetName(uexp.ReadName());
            tag.Size = uexp.ReadInt32();
            tag.ArrayIndex = uexp.ReadInt32();

            if (tag.IsType(UPropertyType.Struct))
            {
                tag.StructName = context.GetName(uexp.ReadName());
                tag.StructGuid = uexp.ReadBytes(16);
            }
            else if (tag.IsType(UPropertyType.Bool))
            {
                tag.BoolVal = uexp.ReadByte();
            }
            else if (tag.IsType(UPropertyType.Byte) ||
                     tag.IsType(UPropertyType.Enum))
            {
                tag.EnumName = context.GetName(uexp.ReadName());
            }
            else if (tag.IsType(UPropertyType.Array))
            {
                tag.InnerType = context.GetName(uexp.ReadName());
            }
            else if (tag.IsType(UPropertyType.Set))
            {
                tag.InnerType = context.GetName(uexp.ReadName());
            }
            else if (tag.IsType(UPropertyType.Map))
            {
                tag.InnerType = context.GetName(uexp.ReadName());
                tag.ValueType = context.GetName(uexp.ReadName());
            }

            tag.HasPropertyGuid = uexp.ReadByte();

            if (tag.HasPropertyGuid > 0)
                tag.PropertyGuid = uexp.ReadBytes(16);

            return tag;
        }

    }
}
