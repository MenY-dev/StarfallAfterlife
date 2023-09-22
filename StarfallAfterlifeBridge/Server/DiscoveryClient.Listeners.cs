using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Matchmakers;
using StarfallAfterlife.Bridge.Server.Quests;
using StarfallAfterlife.Bridge.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient : IStarSystemListener,
                                           IStarSystemObjectListener,
                                           IFleetListener,
                                           IObjectStorageListener,
                                           IUserFleetListener,
                                           IObjectScanningListener
                        
    {
        void IStarSystemObjectListener.OnObjectSpawned(StarSystemObject obj) => Invoke(() =>
        {
            if (obj is DiscoveryFleet fleet)
            {
                if (fleet == CurrentCharacter.Fleet)
                {
                    var system = fleet.System?.Id ?? 0;

                    SyncMove(fleet);
                    SynckSessionSystemInfo(system, fleet.Location);
                    SendEnterToStarSystem(system, fleet.Location);
                    SyncMove(fleet);
                    SyncRoute(fleet);

                    foreach (var item in CurrentCharacter?.ActiveQuests ?? new())
                        item?.Update();

                    SendQuestDataUpdate();
                }

                RequestDiscoveryObjectSync(fleet);
                SyncFleetData(fleet);
            }
            else
            {
                RequestDiscoveryObjectSync(obj);
            }
        });

        void IStarSystemObjectListener.OnObjectDestroed(StarSystemObject obj) => Invoke(() =>
        {
            SendDisconnectObject(obj);
        });

        void IFleetListener.OnFleetMoved(DiscoveryFleet fleet) => Invoke(() =>
        {
            if (CurrentCharacter?.Fleet == fleet)
                SynckSessionSystemInfo(fleet?.System?.Id ?? 0, fleet?.Location ?? Vector2.Zero);

            SyncMove(fleet);
        });

        void IFleetListener.OnFleetRouteChanged(DiscoveryFleet fleet) => Invoke(() => SyncRoute(fleet));

        void IFleetListener.OnFleetDataChanged(DiscoveryFleet fleet) => Invoke(() => SyncFleetData(fleet));

        void IFleetListener.OnFleetSharedVisionChanged(DiscoveryFleet fleet) => Invoke(() => SyncSharedVision(fleet));

        void IObjectStorageListener.OnObjectStorageAdded(DiscoveryObject obj, ObjectStorage storage) => Invoke(() =>
        {

        });

        void IObjectStorageListener.OnObjectStorageRemoved(DiscoveryObject obj, ObjectStorage storage) => Invoke(() =>
        {

        });

        void IObjectStorageListener.OnObjectStorageUpdated(DiscoveryObject obj, ObjectStorage storage, StorageItemInfo[] updatedItems) => Invoke(() =>
        {
            SendObjectStockOld(obj as StarSystemObject, storage.Name);
        });

        void IUserFleetListener.OnSystemExplorationChanged(UserFleet fleet, SystemHex newHex, int vision) => Invoke(() =>
        {
            if (fleet is not null &&
                fleet.System is StarSystem system &&
                CurrentCharacter is ServerCharacter character &&
                character.Fleet == fleet)
            {
                UpdateExploration(system.Id, newHex, vision);
            }
        });

        void IObjectScanningListener.OnScanningStarted(DiscoveryFleet fleet, StarSystemObject toScan, float seconds) => Invoke(() =>
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.Fleet == fleet &&
                character.Selection?.GetObjectInfo(toScan) is ObjectSelectionInfo info)
            {
                info.ScanningStarted = true;
                info.ScanningTime = seconds;
                Invoke(() => SendInfoWidgetData(character.Selection));
            }
        });

        void IObjectScanningListener.OnScanningFinished(DiscoveryFleet fleet, StarSystemObject toScan) => Invoke(() =>
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.Fleet == fleet &&
                character.Selection?.GetObjectInfo(toScan) is ObjectSelectionInfo info)
            {
                info.ScanningStarted = false;
                info.ScanningTime = 0;
                info.Scanned = true;
                Invoke(() =>
                {
                    SendInfoWidgetData(character.Selection);

                    if (info.Target is StarSystemObject target)
                        character.Events.Broadcast<IExplorationListener>(l => l.OnObjectScanned(target));
                });
            }
        });

        void IObjectScanningListener.OnScanningCanceled(DiscoveryFleet fleet, StarSystemObject toScan) => Invoke(() =>
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.Fleet == fleet &&
                character.Selection?.GetObjectInfo(toScan) is ObjectSelectionInfo info)
            {
                info.ScanningStarted = false;
                info.ScanningTime = 0;
                Invoke(() => SendInfoWidgetData(character.Selection));
            }
        });
    }
}
