using CombatExtended;
using RimWorld;
using System;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Verse;

namespace Axolotl
{
    public class LotiQiBulletComp_FluctuateDamageCE : LotiQiBulletComp
    {
        public LotiQiBulletCompProperties_FluctuateDamageCE Props
        {
            get
            {
                return (LotiQiBulletCompProperties_FluctuateDamageCE)this.props;
            }
        }

        public int GetHediffLevel
        {
            get
            {
                if(Launcher != null)
                {
                    Hediff getHediffFromPawn = Launcher.health.hediffSet.GetFirstHediffOfDef(this.Props.effectHediffDef);
                    if(getHediffFromPawn != null)
                    {
                        return (int)getHediffFromPawn.Severity;
                    }
                }
                return 0;
            }
        }

        public int GetSkillLevel
        {
            get
            {
                if (Launcher != null)
                {
                    //角色有修炼comp
                    Comp_Cultivation pawnComp_Cultivation = Launcher.TryGetComp<Comp_Cultivation>();
                    if (pawnComp_Cultivation != null && pawnComp_Cultivation.SkillInstallList.Contains(Props.effectSkillDef))
                    {
                        return pawnComp_Cultivation.AllLearnedSkills.First(skill => skill.def == Props.effectSkillDef).Level ?? 0;
                    }
                }
                return 0;
            }
        }

        public int GetAverageSkill
        {
            get
            {
                //最大20
                int skill_shoot = Math.Min(Launcher.skills.GetSkill(SkillDefOf.Shooting).GetLevel(true), 20);
                int skill_melee = Math.Min(Launcher.skills.GetSkill(SkillDefOf.Melee).GetLevel(true), 20);
                return (skill_shoot + skill_melee) / 2;
            }
        }

        public float ApplyDamageAmount
        {
            get
            {
                float levelEffect = (float)(0.18 + 0.34 * (Props.effectHediffDef != null ? GetHediffLevel : GetSkillLevel));
                float skillEffect = (float)(1 + (GetAverageSkill - this.Props.averageSkill) * 0.0167f);
                return (levelEffect + skillEffect) / 2 * this.Props.baseDamageAmount;
            }
        }

        public float ApplyPenetrationAmount
        {
            get
            {
                float pen = 10f;
                if (this.parent.def.projectile is CombatExtended.ProjectilePropertiesCE propsCE)
                {
                    pen = propsCE.armorPenetrationSharp;
                }
                //0.18增加到0.34
                //0.0167f增加到0.0833
                //倍率上限1.2增加到2
                float levelEffect = (float)(0.3 + 0.34 * (Props.effectHediffDef != null ? GetHediffLevel : GetSkillLevel));
                float skillEffect = (float)(1 + (GetAverageSkill - this.Props.averageSkill) * 0.0833f);
                return (levelEffect + skillEffect) / 2 * pen;
            }
        }

        public override void Impact(Thing instigator, Thing hitThing, bool blockedByShield)
        {
            base.Impact(instigator, hitThing, blockedByShield);

            //打中东西
            if (hitThing != null)
            {
                try
                {

                    DamageInfo info = new DamageInfo(this.parent.def.projectile.damageDef, ApplyDamageAmount, ApplyPenetrationAmount, instigator: Launcher, weapon:this.parent.def);
                    info.SetAngle((hitThing.Position - instigator.Position).ToVector3());
                    hitThing.TakeDamage(info);
                }
                catch(Exception e)
                {
                    Debug.WarningInTry("螈力剑气-波动伤害错误", e);
                }
            }
        }
    }
}
