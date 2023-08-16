using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IObjectStorageListener : IDiscoveryListener
    {
        void OnObjectStorageAdded(DiscoveryObject obj, ObjectStorage storage);

        void OnObjectStorageRemoved(DiscoveryObject obj, ObjectStorage storage);

        void OnObjectStorageUpdated(DiscoveryObject obj, ObjectStorage storage, StorageItemInfo[] updatedItems);
    }
}
