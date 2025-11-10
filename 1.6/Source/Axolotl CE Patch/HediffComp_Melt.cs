using RimWorld;
using Verse;

using UnityEngine;

using System;
using System.Collections.Generic;

namespace Axolotl
{
    public class HediffComp_MeltCE : HediffComp
    {
        public HediffCompProperties_MeltCE Props
        {
            get
            {
                return (HediffCompProperties_MeltCE)this.props;
            }
        }

        public Pawn GetPawn
        {
            get
            {
                return this.parent.pawn;
            }
        }

        int tickAfterStart = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (++tickAfterStart >= this.Props.tickForHit)
            {
                //重置计数
                tickAfterStart = 0;

                try
                {
                    if (GetPawn != null && GetPawn.Map != null && GetPawn.Spawned)
                    {
                        //发热
                        HeatTemperature(100.0f);

                        TakeDamageTo(GetPawn);

                        AttachFireTo(GetPawn);
                    }
                }
                catch { }
            }
        }

        private void HeatTemperature(float maxTemperature)
        {
            //产生热量
            if (GetPawn.AmbientTemperature < maxTemperature)
            {
                GenTemperature.PushHeat(GetPawn.Position, GetPawn.Map, this.Props.heatPerTick * Props.tickForHit);
            }
        }

        private void TakeDamageTo(Pawn target)
        {
            if(target != null && !target.Dead && target.Spawned)
            {
                //造成伤害
                int DamageAmount = (int)(this.Props.baseDamage);
                target.TakeDamage(new DamageInfo(DamageDefOf.Burn, DamageAmount, armorPenetration: 2.0f, instigator: GetPawn));

                //显示特效
                FleckMaker.Static(target.DrawPos, target.Map, AxolotlFleckDefOf.Axolotl_FireBurn);
            }
        }

        private void AttachFireTo(Pawn target)
        {
            if (target != null && !target.Dead && target.Spawned)
            {
                //点燃
                float baseFlammability = target.GetStatValue(StatDefOf.Flammability);
                if (baseFlammability > 0)
                {
                    float probability = this.Props.chanceForBurnEveryHit * baseFlammability;

                    if (Verse.Rand.Range(0, 100) < Math.Min(probability * 100, 100))
                    {
                        target.TryAttachFire(Rand.Range(0.25f, 0.35f), GetPawn);
                    }
                }
            }
        }
    }
}
