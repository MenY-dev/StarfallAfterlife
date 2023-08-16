using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization.Json
{
    public partial class JsonArray : JsonNode, IList<JsonNode>
    {
        public int Count { get { Init(); return Nodes.Count; } }

        public bool IsReadOnly => false;

        protected virtual List<JsonNode> Nodes { get; set; }

        public JsonArray()
        {
            Init();
        }

        public JsonArray(JsonElement element)
        {
            Element = element;
            //Init();
        }

        public JsonArray(params JsonNode[] items) : this(items as IEnumerable<JsonNode>)
        {

        }

        public JsonArray(IEnumerable<object> items) : this(items?.Select(Parse))
        {

        }

        public JsonArray(IEnumerable<JsonNode> items)
        {
            Init();

            if (items is null)
                return;

            foreach (var item in items)
                Add(item);
        }



        public void Add<T>(T value)
        {
            Init();

            if (value == null)
                Nodes.Add(null);
            else
                Nodes.Add(value as JsonNode ?? new JsonValue(value));
        }

        public virtual JsonNode GetItem(int index)
        {
            Init();
            return Nodes[index];
        }

        public virtual void SetItem(int index, JsonNode value)
        {
            Init();
            Nodes[index] = value;
        }

        public override JsonNode Clone()
        {
            Init();
            return new JsonArray(Nodes);
        }

        public override void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options = null)
        {
            if (writer is null)
                return;

            if (Element.ValueKind == JsonValueKind.Array)
            {
                Element.WriteTo(writer);
            }
            else
            {
                Init();

                if (options is null)
                    options = new JsonSerializerOptions();

                writer.WriteStartArray();

                foreach (var item in this)
                {
                    if (item is null)
                        writer.WriteNullValue();
                    else
                        item.WriteTo(writer, options);
                }

                writer.WriteEndArray();
            }
        }

        public void Add(JsonNode item) => Add<JsonNode>(item);

        public void Clear() { Init(); Nodes.Clear(); }

        public bool Contains(JsonNode item) { Init(); return Nodes.Contains(item); }

        public void CopyTo(JsonNode[] array, int arrayIndex) { Init(); Nodes.CopyTo(array, arrayIndex); }

        public IEnumerator<JsonNode> GetEnumerator() { Init(); return Nodes.GetEnumerator(); }

        public int IndexOf(JsonNode item) { Init(); return Nodes.IndexOf(item); }

        public void Insert(int index, JsonNode item) { Init(); Nodes.Insert(index, item); }

        public bool Remove(JsonNode item) { Init(); return Nodes.Remove(item); }

        public void RemoveAt(int index) { Init(); Nodes.RemoveAt(index); }

        IEnumerator IEnumerable.GetEnumerator() { Init(); return Nodes.GetEnumerator(); }

        protected void Init()
        {
            if (Nodes == null)
            {
                List<JsonNode> nodes;

                if (Element.ValueKind != JsonValueKind.Array)
                {
                    nodes = new List<JsonNode>();
                }
                else
                {
                    nodes = new List<JsonNode>(Element.GetArrayLength());

                    foreach (JsonElement element in Element.EnumerateArray())
                    {
                        JsonNode node = Parse(element);
                        nodes.Add(node);
                    }

                    Element = default;
                }

                Nodes = nodes;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (Nodes is not null)
            {
                foreach (var item in Nodes)
                    item?.Dispose();

                Nodes.Clear();
                Nodes.EnsureCapacity(0);
                Nodes = null;
            }
        }
    }
}
