using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public partial class DiscoveryFleet
    {
        protected List<FleetEffectInfo> Effects { get; } = new();

        protected List<DiscoveryFleet> SharedVision { get; } = new();

        protected readonly object EffectsLockher = new();

        public void AddEffect(FleetEffectInfo effect)
        {
            lock (EffectsLockher)
            {
                Effects.Add(effect);
                ApplyEffects();
                Broadcast<IFleetListener>(l => l.OnFleetDataChanged(this));
            }
        }

        protected virtual void ApplyEffects()
        {
            if (GetAllEffects(GameplayEffectType.SpeedBoost).Concat(
                GetAllEffects(GameplayEffectType.FuelStationBoost)).
                MaxBy(e => e.Value.EngineBoost) is FleetEffectInfo speedEffect)
            {
                Speed = BaseSpeed * speedEffect.EngineBoost;
            }
            else Speed = BaseSpeed;

            if (GetAllEffects(GameplayEffectType.Vision).
                MaxBy(e => e.Value.Vision) is FleetEffectInfo visionEffect)
            {
                Vision = Math.Max(BaseVision, visionEffect.Vision);
            }
            else Vision = BaseVision;

            if (GetAllEffects(GameplayEffectType.BlindVision).
                MinBy(e => e.Value.Vision) is FleetEffectInfo blindVisionEffect)
            {
                Vision = Math.Min(Vision, blindVisionEffect.Vision);
                NebulaVision = Math.Min(BaseNebulaVision, blindVisionEffect.NebulaVision);
            }
            else NebulaVision = BaseNebulaVision;

            if (GetAllEffects(GameplayEffectType.EngineNullifier).Any() == true)
            {
                EngineEnabled = false;
            }
            else EngineEnabled = true;

            if (GetAllEffects(GameplayEffectType.Stealth).Any() == true)
            {
                Stealth = true;
            }
            else Stealth = false;

            foreach (var sharedVisionEffect in GetAllEffects(GameplayEffectType.SharedVision, true)
                                              .Cast<FleetEffectInfo>())
            {
                if (System.Fleets.FirstOrDefault(f => f.Id == sharedVisionEffect.FleetId) is DiscoveryFleet fleet)
                    fleet.SetSharedVision(this, sharedVisionEffect.Duration > 0);
            }
        }

        protected virtual void ApplyEffectsOld()
        {
            if ((GetEffect(GameplayEffectType.SpeedBoost) ??
                GetEffect(GameplayEffectType.FuelStationBoost)) is FleetEffectInfo speedEffect)
            {
                Speed = BaseSpeed * speedEffect.EngineBoost;
            }
            else Speed = BaseSpeed;

            if (GetEffect(GameplayEffectType.Vision) is FleetEffectInfo visionEffect)
            {
                Vision = Math.Max(BaseVision, visionEffect.Vision);
            }
            else Vision = BaseVision;

            if (GetEffect(GameplayEffectType.BlindVision) is FleetEffectInfo blindVisionEffect)
            {
                Vision = Math.Min(Vision, blindVisionEffect.Vision);
                NebulaVision = Math.Min(BaseNebulaVision, blindVisionEffect.NebulaVision);
            }
            else NebulaVision = BaseNebulaVision;

            if (GetEffect(GameplayEffectType.EngineNullifier) is not null)
            {
                SetEngineEnabled(false);
            }
            else SetEngineEnabled(true);

            if (GetEffect(GameplayEffectType.Stealth) is not null)
            {
                Stealth = true;
            }
            else Stealth = false;

            if (GetEffect(GameplayEffectType.SharedVision) is null)
                SharedVisionTarget = null;
        }

        protected virtual void UpdateEffects()
        {
            lock (EffectsLockher)
            {
                for (int i = 0; i < Effects.Count; i++)
                {
                    var effect = Effects[i];
                    effect.Duration -= DeltaTime;
                    Effects[i] = effect;
                }

                if (Effects.RemoveAll(e => e.Duration <= 0) > 0)
                {
                    ApplyEffects();
                    Broadcast<IFleetListener>(l => l.OnFleetDataChanged(this));
                }

                if (SharedVision.RemoveAll(f =>
                {
                    var effect = f.GetEffect(GameplayEffectType.SharedVision);
                    return effect is null || (effect.Value.FleetId == Id && effect.Value.Duration <= 0);
                }) > 0)
                {
                    Broadcast<IFleetListener>(l => l.OnFleetSharedVisionChanged(this));
                }
            }
        }

        public FleetEffectInfo[] GetEffects()
        {
            lock (EffectsLockher)
                return Effects.ToArray();
        }

        protected IEnumerable<FleetEffectInfo?> GetAllEffects(GameplayEffectType type, bool includeCompleted = false)
        {
            lock (EffectsLockher)
                foreach (var effect in Effects)
                    if (effect.Logic == type && (includeCompleted || effect.Duration > 0))
                        yield return effect;
        }

        public FleetEffectInfo? GetEffect(GameplayEffectType type)
        {
            lock (EffectsLockher)
                foreach (var effect in Effects)
                    if (effect.Logic == type && effect.Duration > 0)
                        return effect;

            return null;
        }

        public DiscoveryFleet[] GetSharedVision()
        {
            lock (EffectsLockher)
                return SharedVision.ToArray();
        }

        public void SetSharedVision(DiscoveryFleet fleet, bool state)
        {
            if (fleet is null)
                return;

            lock (EffectsLockher)
            {
                var changed = false;

                if (state == true)
                {
                    if (SharedVision.Contains(fleet) == false)
                    {
                        SharedVision.Add(fleet);
                        changed = true;
                    }
                }
                else
                {
                    changed = SharedVision.Remove(fleet);
                }

                if (changed == true)
                    Broadcast<IFleetListener>(l => l.OnFleetSharedVisionChanged(this));
            }
        }
    }
}
