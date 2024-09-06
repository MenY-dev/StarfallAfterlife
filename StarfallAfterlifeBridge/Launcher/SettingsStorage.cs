using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public class SettingsStorage
    {
        public JsonNode this[string key]
        {
            get
            {
                lock (_locker)
                    return (_innnerDoc ??= new JsonObject())[key];
            }
            set
            {
                lock (_locker)
                    (_innnerDoc ??= new JsonObject())[key] = value;
            }
        }

        public string Path { get; set; }

        private JsonNode _innnerDoc = new JsonObject();
        private readonly object _locker = new();


        public bool Save()
        {
            lock (_locker)
            {
                return (_innnerDoc ??= new JsonObject()).WriteToFileUnbuffered(
                    Path,
                    new() { WriteIndented = true, TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver });
            }
        }

        public bool Load()
        {
            lock (_locker)
            {
                var doc = JsonHelpers.ParseNodeFromFileUnbuffered(Path)?.AsObjectSelf();

                if (doc is null)
                {
                    _innnerDoc = new JsonObject();
                    return false;
                }

                _innnerDoc = doc;
                return true;
            }
        }
    }
}
