using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization.Json
{
    public partial class JsonObject : JsonNode, IDictionary<string, JsonNode>
    {
        public ICollection<string> Keys
        {
            get
            {
                Init();
                return Children.Keys;
            }
        }

        public ICollection<JsonNode> Values
        {
            get
            {
                Init();
                return Children.Values;
            }
        }

        public int Count
        {
            get
            {
                Init();
                return Children.Count;
            }
        }

        public bool IsReadOnly => false;

        protected Dictionary<string, JsonNode> Children { get; set; }

        public JsonObject()
        {

        }

        public JsonObject(JsonElement element)
        {
            Element = element;
            //Init();
        }

        public override void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options = null)
        {

            if (writer is null)
                return;
            
            if (Element.ValueKind == JsonValueKind.Object)
            {
                Element.WriteTo(writer);
            }
            else
            {
                Init();

                if (options is null)
                    options = new JsonSerializerOptions();

                writer.WriteStartObject();

                foreach (var item in this)
                {
                    writer.WritePropertyName(item.Key);

                    if (item.Value is null)
                        writer.WriteNullValue();
                    else
                        item.Value.WriteTo(writer, options);
                }

                writer.WriteEndObject();
            }
        }

        public override JsonNode Clone()
        {
            if (Element.ValueKind == JsonValueKind.Object)
                return Parse(Element.Clone());

            JsonObject newObject = new ();

            foreach (var item in this)
                newObject[item.Key] = item.Value;

            return base.Clone();
        }

        public void Init()
        {
            if (Children != null)
            {
                return;
            }

            var children = new Dictionary<string, JsonNode>(StringComparer.InvariantCultureIgnoreCase);

            if (Element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in Element.EnumerateObject())
                {
                    JsonNode node = Parse(property.Value);

                    if (node != null)
                    {

                    }

                    children.TryAdd(property.Name, node);
                }

                Element = default;
            }

            Children = children;
        }

        public virtual JsonNode GetItem(string propertyName)
        {
            Init();

            if (TryGetValue(propertyName, out JsonNode value))
                return value;

            return null;
        }

        public virtual void SetItem(string propertyName, JsonNode value)
        {
            Init();

            if (Children.ContainsKey(propertyName))
                Children[propertyName] = value;
            else
                Children.Add(propertyName, value);
        }

        public void Add(string propertyName, JsonNode value) => SetItem(propertyName, value);

        public void Add(KeyValuePair<string, JsonNode> property) => SetItem(property.Key, property.Value);

        public void Clear()
        {
            Element = default;
            Children?.Clear();
        }

        public bool Contains(KeyValuePair<string, JsonNode> item)
        {
            Init();
            return Children.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            Init();
            return Children.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, JsonNode>[] array, int arrayIndex)
        {
            Init();
            ((ICollection<KeyValuePair<string, JsonNode>>)Children).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, JsonNode>> GetEnumerator()
        {
            Init();
            return Children.GetEnumerator();
        }

        public bool Remove(string key)
        {
            Init();
            return Children.Remove(key);
        }

        public bool Remove(KeyValuePair<string, JsonNode> item) => Remove(item.Key);

        public bool TryGetValue(string key,  out JsonNode value)
        {
            Init();
            return Children.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Init();
            return Children.GetEnumerator();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (Children is not null)
            {
                foreach (var item in Children)
                    item.Value?.Dispose();

                Children.Clear();
                Children.TrimExcess(0);
                Children = null;
            }
        }
    }
}
