using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public class CharacterInventory : ICharInventoryStorage
    {
        public int ResponseTimeout { get; set; } = 5000;

        private ServerCharacter _character;

        private SfaServerClient Client => _character.DiscoveryClient?.Client;

        InventoryItem ICharInventoryStorage.this[int itemId] => GetItem(itemId).Result;

        public CharacterInventory(ServerCharacter character)
        {
            _character = character;
        }

        public Task<InventoryItem> GetItem(int id)
        {
            return Client?
                .SendRequest(SfaServerAction.CharacterInventory, new JsonObject
                {
                    ["action"] = "get",
                    ["id"] = id,
                }.ToJsonStringUnbuffered(false), ResponseTimeout)
                .ContinueWith(t =>
                {
                    if (t.Result is SfaClientResponse response &&
                        response.IsSuccess == true &&
                        JsonHelpers.ParseNodeUnbuffered(response.Text) is JsonObject doc &&
                        JsonHelpers.DeserializeUnbuffered<InventoryItem>(doc["item"]) is InventoryItem item)
                    {
                        return item;
                    }
                    
                    return null;

                }) ?? Task.FromResult<InventoryItem>(null);
        }

        public Task<int> AddItem(InventoryItem item)
        {
            if (item is null)
                Task.FromResult(0);

            return Client?
                .SendRequest(SfaServerAction.CharacterInventory, new JsonObject
                {
                    ["action"] = "add",
                    ["id"] = item.Id,
                    ["type"] = (byte)item.Type,
                    ["count"] = item.Count,
                }.ToJsonStringUnbuffered(false), ResponseTimeout)
                .ContinueWith(t =>
                {
                    if (t.Result is SfaClientResponse response &&
                        response.IsSuccess == true &&
                        JsonHelpers.ParseNodeUnbuffered(response.Text) is JsonObject doc &&
                        (int?)doc["result"] is int result)
                    {
                        return result;
                    }

                    return 0;

                }) ?? Task.FromResult(0);
        }

        public Task<int> RemoveItem(int id, int count)
        {
            if (count < 1)
                Task.FromResult(0);

            return Client?
                .SendRequest(SfaServerAction.CharacterInventory, new JsonObject
                {
                    ["action"] = "remove",
                    ["id"] = id,
                    ["count"] = count,
                }.ToJsonStringUnbuffered(false), ResponseTimeout)
                .ContinueWith(t =>
                {
                    if (t.Result is SfaClientResponse response &&
                        response.IsSuccess == true &&
                        JsonHelpers.ParseNodeUnbuffered(response.Text) is JsonObject doc &&
                        (int?)doc["result"] is int result)
                    {
                        return result;
                    }

                    return 0;

                }) ?? Task.FromResult(0);
        }

        InventoryItem ICharInventoryStorage.Add(InventoryItem item, int count)
        {
            if (item is null)
                return null;

            item = item.Clone();
            item.Count = count;
            item.Count = AddItem(item).Result;

            return item;
        }

        int ICharInventoryStorage.Remove(int itemId, int count)
        {
            if (count < 0)
                return 0;

            return RemoveItem(itemId, count).Result;
        }
    }
}
