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
                if (CurrentCharacter is ServerCharacter character &&
                    fleet == character.Fleet)
                {
                    var system = fleet.System?.Id ?? 0;

                    SynckSessionSystemInfo(system, fleet.Location);
                    SendEnterToStarSystem(system, fleet.Location);

                    character.UpdateQuestLines();
                    character.Party?.SetMemberStarSystem(character.UniqueId, system);

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
                SyncDiscoveryObject(obj);
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

        public void OnFleetAttackTarget(DiscoveryFleet fleet, StarSystemObject target) => Invoke(c =>
        {
            if (fleet is DiscoveryAiFleet aiFleet &&
                target is UserFleet userFleet)
            {
                c.SendShowAiMessage(aiFleet.Type, aiFleet.Id, "ai_attack_userfleet");
            }
        });

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

        void IObjectScanningListener.OnScanningStateChanged(DiscoveryFleet fleet, ScanInfo info) => Invoke(() =>
        {
            void SetScanning(SelectionInfo selection, bool started, bool finished, float time)
            {
                if (selection is null)
                    return;

                if (info.SectorScanning == true)
                { 
                    selection.ScanningStarted = started;
                    selection.Scanned = finished;
                    selection.ScanningTime = time;
                }
                else if(selection.GetObjectInfo(info.SystemObject) is ObjectSelectionInfo objSelection)
                {
                    objSelection.ScanningStarted = started;
                    objSelection.Scanned = finished;
                    objSelection.ScanningTime = time;
                }
            }

            if (CurrentCharacter is ServerCharacter character &&
                character.Fleet == fleet)
            {
                if (info.State == ScanState.Started)
                {
                    if (character.Selection is SelectionInfo selection)
                    {
                        SetScanning(selection, true, false, info.Time);
                        Invoke(() => SendInfoWidgetData(selection));
                    }

                }
                else if (info.State == ScanState.Cancelled)
                {
                    if (character.Selection is SelectionInfo selection)
                    {
                        SetScanning(selection, false, false, 0);
                        Invoke(() => SendInfoWidgetData(selection));
                    }
                }
                else if(info.State == ScanState.Finished)
                {
                    if (character.Selection is SelectionInfo selection)
                    {
                        SetScanning(selection, false, true, 0);
                        Invoke(() => SendInfoWidgetData(selection));
                    }

                    if (info.SectorScanning == false &&
                        info.SystemObject is StarSystemObject target)
                        Invoke(() => character.Events.Broadcast<IExplorationListener>(l => l.OnObjectScanned(target)));
                }
            }
        });

        void IObjectScanningListener.OnSecretObjectRevealed(DiscoveryFleet fleet, SecretObject secretObject) => Invoke(() =>
        {
            if (fleet is not null &&
                fleet.System is StarSystem system &&
                CurrentCharacter is ServerCharacter character &&
                character.Fleet == fleet)
            {
                SendSecretObjectRevealed(secretObject);
            }
        });
    }
}
