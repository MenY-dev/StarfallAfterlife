using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public static class Ability
    {
        public static void ApplyPassiveEffects(DiscoveryFleet fleet, int abilityId)
        {
            if (fleet is not null &&
                SfaDatabase.Instance.GetAbility(abilityId) is AbilityInfo ability)
            {
                switch (ability.Logic)
                {
                    case AbilityLogic.AgroVision:
                        fleet.AgroVision = (int)ability.AgroVision;
                        break;
                    case AbilityLogic.NebulaViewer:
                        fleet.BaseNebulaVision = (int)ability.NebulaVision;
                        break;
                    case AbilityLogic.WarpJump:
                        break;
                    case AbilityLogic.EffectApplier:
                        break;
                    default:
                        break;
                }
            }
        }

        public static void Use(ServerCharacter character, int abilityId, int systemId, SystemHex hex)
        {
            if (character.Fleet is UserFleet fleet &&
                fleet.System is StarSystem currentSystem &&
                character.DiscoveryClient is DiscoveryClient client &&
                client.Galaxy is DiscoveryGalaxy galaxy &&
                SfaDatabase.Instance.GetAbility(abilityId) is AbilityInfo ability)
            {
                galaxy.BeginPreUpdateAction(g =>
                {
                    if (ability.Logic == AbilityLogic.WarpJump)
                    {
                        if (systemId == currentSystem?.Id)
                        {
                            client.Invoke(client.SendFleetWarpedGateway);

                            client.Invoke(() => galaxy.BeginPreUpdateAction(g =>
                            {
                                if (fleet.System is StarSystem jumpSystem)
                                {
                                    jumpSystem.RemoveFleet(fleet);
                                    jumpSystem.AddFleet(fleet, SystemHexMap.HexToSystemPoint(hex));
                                }
                            }));
                        }
                    }
                    else if (ability.Logic == AbilityLogic.EffectApplier)
                    {
                        if (ability.TargetType == AbilityTargetType.Self)
                        {
                            foreach (var effect in ability.Effects)
                                fleet.AddEffect(effect);
                        }
                        else if (
                            ability.TargetType == AbilityTargetType.Target &&
                            currentSystem.GetObjectsAt<DiscoveryFleet>(hex, true).FirstOrDefault(f => f.Faction.IsEnemy(fleet.Faction)) is DiscoveryFleet targetFleet)
                        {
                            foreach (var effect in ability.Effects)
                            {
                                var newEffect = effect;

                                switch (newEffect.Logic)
                                {
                                    case GameplayEffectType.SharedVision:
                                        newEffect.FleetId = fleet.Id;
                                        break;
                                }

                                targetFleet.AddEffect(newEffect);
                            }
                        }
                    }
                });
            }
        }
    }
}
