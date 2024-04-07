using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public class UObject : UObjectBase, ICollection<UProperty>
    {
        public string Name { get; set; }

        public Dictionary<string, UProperty> Properties { get; } = new();

        public int Count => Properties.Count;

        public bool IsReadOnly => false;

        public UProperty this[string propertyName]
        {
            get => GetProperty(propertyName);
            set
            {
                if (propertyName is null)
                    return;

                var property = value;

                if (property.Name != propertyName)
                    property.Name = propertyName;

                SetProperty(property);
            }
        }

        public void SetProperty(string name, object value, string type = null, FPropertyTag tag = default) =>
            SetProperty(new() { Name = name, Value = value, Type = type, Tag = tag});

        public void SetProperty(UProperty property)
        {
            if (property.Name is null)
                return;

            Properties[property.Name] = property;
        }

        public UProperty GetProperty(string name)
        {
            if (TryGetProperty(name, out var property) == true)
                return property;

            return default;
        }

        public bool TryGetProperty(string name, out UProperty property)
        {
            return Properties.TryGetValue(name, out property);
        }

        public bool RemoveProperty(UProperty property) =>
            RemoveProperty(property.Name);

        public bool RemoveProperty(string name)
        {
            return Properties.Remove(name);
        }

        public T GetValue<T>(string name)
        {
            if (TryGetValue(name, out T value) == true)
                return value;

            return default;
        }

        public T GetValue<T>(string name, T defaultValue)
        {
            if (TryGetValue(name, out T value) == true)
                return value;

            return defaultValue;
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            if (Properties.TryGetValue(name, out var prop) == true &&
                prop.TryGetValue(out value) == true)
                return true;

            value = default;
            return false;
        }

        public override void Load(UAssetLoadContext context)
        {
            ReadObjectProperties(context);
        }

        protected override void AddPropertyInternal(UProperty property)
        {
            SetProperty(property.Name, property.Value, property.Type, property.Tag);
        }

        void ICollection<UProperty>.Add(UProperty item) => SetProperty(item);

        public void Clear() => Properties.Clear();

        public bool Contains(UProperty item) => Properties.ContainsValue(item);

        public void CopyTo(UProperty[] array, int arrayIndex) =>
            Properties.Values.CopyTo(array, arrayIndex);

        bool ICollection<UProperty>.Remove(UProperty item) => RemoveProperty(item);

        public IEnumerator<UProperty> GetEnumerator() =>
            Properties.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Properties.Values.GetEnumerator();
    }
}
