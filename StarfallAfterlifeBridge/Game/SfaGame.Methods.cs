using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Game
{
    public partial class SfaGame
    {
        public void UpdateProductionPointsIncome(bool autosave = true)
        {
            Profile?.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    var now = DateTime.Now;

                    if (character.ProductionPoints < character.ProductionCap)
                    {
                        var timeDelta = now - character.LastProductionIncomeTime;
                        character.ProductionPoints += Math.Max(0, (int)(timeDelta.TotalMinutes * character.ProductionIncome));

                        DistributeProductionPoints(false);

                        if (character.ProductionPoints > character.ProductionCap)
                            character.ProductionPoints = character.ProductionCap;
                    }

                    character.LastProductionIncomeTime = now;

                    if (autosave == true)
                        p.SaveGameProfile();
                }
            });

        }

        public virtual void AddProductionPoints(int count, bool autosave = true)
        {
            if (count < 1)
                return;

            Profile?.Use(p =>
            {
                UpdateProductionPointsIncome(false);

                if (p.GameProfile.CurrentCharacter is Character character)
                    character.ProductionPoints += count;

                DistributeProductionPoints(false);

                if (autosave == true)
                    p.SaveGameProfile();
            });
        }

        public virtual void DistributeProductionPoints(bool autosave = true)
        {
            Profile?.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character &&
                    character.Crafting?.ToList() is List<CraftingInfo> craftingQueue &&
                    p.Database is SfaDatabase database)
                {
                    craftingQueue.Sort((a, b) =>
                        a?.QueuePosition.CompareTo(b?.QueuePosition) ?? -1);

                    foreach (var project in craftingQueue)
                    {
                        if (character.ProductionPoints < 1)
                            break;

                        var dtbItem = database.GetItem(project.ProjectEntity);

                        if (dtbItem.ProductionPoints < 0 ||
                            project.ProductionPointsSpent >= dtbItem.ProductionPoints)
                            continue;

                        var remainingPoints = dtbItem.ProductionPoints - project.ProductionPointsSpent;

                        if (remainingPoints < character.ProductionPoints)
                        {
                            project.ProductionPointsSpent = dtbItem.ProductionPoints;
                            character.ProductionPoints -= remainingPoints;
                        }
                        else
                        {
                            project.ProductionPointsSpent += character.ProductionPoints;
                            character.ProductionPoints = 0;
                            break;
                        }
                    }

                    if (autosave == true)
                        p.SaveGameProfile();
                }
            });
        }

        public bool IsProductionPossible(SfaItem item, int count = 1)
        {
            var result = false;

            if (count < 0 ||
                item is null ||
                item.IsDefective == true)
                return result;

            Profile?.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character &&
                    p.Database is SfaDatabase database &&
                    (item.IGCToProduce * count) <= character.IGC &&
                    (item.BGC * count) <= character.BGC)
                {
                    result = true;

                    if (item.Materials is List<MaterialInfo> materials && materials.Count > 0)
                    {
                        foreach (var material in materials)
                        {
                            var inv = character.GetInventoryItem(database.GetItem(material.Id));

                            if (inv.IsEmpty || inv.Count < (material.Count * count))
                            {
                                result = false;
                                return;
                            }
                        }
                    }
                }
            });

            return result;
        }


        public void UpdateShipsRepairProgress(bool autosave = true)
        {
            Profile?.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    var now = DateTime.Now;
                    var totalSeconds = (int)(now - character.LastShipsRepairTime).TotalSeconds;

                    foreach (var ship in character.Ships ?? new())
                    {
                        if (ship is null)
                            continue;

                        ship.TimeToRepair = Math.Max(0, ship.TimeToRepair - totalSeconds);
                    }

                    character.LastShipsRepairTime = now;

                    if (autosave == true)
                        p.SaveGameProfile();
                }
            });

        }
    }
}
