using RimWorld;
using Verse;

using UnityEngine;

using System;
using System.Collections.Generic;
using Verse.Sound;

namespace Axolotl
{
    public class CompFlowerSlashCoreCE : ThingComp
    {
        public Compproperties_FlowerSlashCoreCE Props
        {
            get
            {
                return (Compproperties_FlowerSlashCoreCE)this.props;
            }
        }

        public int ticksUse = 0;
         
        private int range = 11;

        private float amount
        {
            get
            {
                //原4.0f + GetLauncherLevel
                return 5.0f + GetLauncherLevel*1.5f;
            }
        }

        private float scale = 20.0f;

        //施法者
        public Pawn instigator;

        //施法者等级
        public int GetLauncherLevel
        {
            get
            {
                //Hediff hediffFromPawn = this.instigator.health.hediffSet.GetFirstHediffOfDef(AxolotlHediffDefOf.Axolotl_LotlQi_Flower);
                //if (hediffFromPawn != null)
                //{
                //    return (int)hediffFromPawn.Severity;
                //}
                return MoeLotlQiSkillUtility.GetMoeLotlQiSkillLevel(instigator, AxolotlMoelotlQiSkillDefOf.Axolotl_MainSkill_Flower);
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            ticksUse++;

            //20tick
            if (ticksUse == 20)
            {
                FleckMaker.Static(this.parent.Position, this.parent.Map, this.Props.FleckList_FlowerSlash[0], scale);

                TakeDamageForListPawn(GetPawnInRange(this.parent, range), amount);
            }
            //27tick
            if (ticksUse == 27)
            {
                FleckMaker.Static(this.parent.Position, this.parent.Map, this.Props.FleckList_FlowerSlash[1], scale);

                TakeDamageForListPawn(GetPawnInRange(this.parent, range), amount);
            }
            //34tick
            if (ticksUse == 34)
            {
                FleckMaker.Static(this.parent.Position, this.parent.Map, this.Props.FleckList_FlowerSlash[2], scale);

                TakeDamageForListPawn(GetPawnInRange(this.parent, range), amount);
            }
            if (ticksUse == 41)
            {
                FleckMaker.Static(this.parent.Position, this.parent.Map, this.Props.FleckList_FlowerSlash[3], scale);

                TakeDamageForListPawn(GetPawnInRange(this.parent, range), amount);
            }
            if (ticksUse == 48)
            {
                FleckMaker.Static(this.parent.Position, this.parent.Map, this.Props.FleckList_FlowerSlash[4], scale);

                TakeDamageForListPawn(GetPawnInRange(this.parent, range), amount);
            }
            if(ticksUse == 60)
            {
                AxolotlSoundDefOf.Axolotl_Sound_FlowerSlash_Final.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
            }
            if (ticksUse == 75)
            {
                FleckMaker.Static(this.parent.Position, this.parent.Map, this.Props.FleckList_FlowerSlash[5], scale + 2.0f);

                List<Pawn> pawnInRange = GetPawnInRange(this.parent, range);
                for (int i = pawnInRange.Count - 1; i >= 0; i--)
                {
                    Pawn target = pawnInRange[i];
                    if (target != null && target.Spawned && !target.Dead && target.health.hediffSet.HasHediff(AxolotlHediffDefOf.Axolotl_FlowerDebris))
                        target.health.RemoveHediff(target.health.hediffSet.GetFirstHediffOfDef(AxolotlHediffDefOf.Axolotl_FlowerDebris));
                }
            }
            if(ticksUse == 80)
            {
                this.parent.Destroy();
            }
        }

        private void TakeDamageForListPawn(List<Pawn> list, float amount)
        {
            try
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    Pawn target = list[i];
                    
                    AxolotlUtility.HediffUtility.GivePawnLevelHediff(instigator, target, this.parent.def, AxolotlHediffDefOf.Axolotl_FlowerDebris, 600);
                    //原穿透0.4f 
                    target.TakeDamage(new DamageInfo(DamageDefOf.Cut, amount, 12.0f, instigator:instigator, weapon:this.parent.def));
                }
            }
            catch { }
        }

        private List<Pawn> GetPawnInRange(Thing loc, float range)
        {
            List<Pawn> result = new List<Pawn>();
            foreach (Pawn mapPawn in loc.Map.mapPawns.AllPawnsSpawned)
            {
                //范围内
                if (!mapPawn.Position.InHorDistOf(loc.Position, range)) continue;

                //友方保护
                if (AxolotlModSetting.isMoeLotlAbilityNotHitFriend && mapPawn.Faction == instigator.Faction) continue;

                result.Add(mapPawn);
            }
            return result;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref ticksUse, "ticksUse", 0);
        }
    }
}
