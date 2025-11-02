using RimWorld;
using Verse;

using UnityEngine;

using System;
using System.Linq;
using CombatExtended;

namespace Axolotl
{
    public class LotiQiBulletComp_FluctuateDamageCE : LotiQiBulletComp
    {
        public LotiQiBulletCompProperties_FluctuateDamage Props
        {
            get
            {
                return (LotiQiBulletCompProperties_FluctuateDamage)this.props;
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
                float levelEffect = (float)(0.3 + 0.18 * (Props.effectHediffDef != null ? GetHediffLevel : GetSkillLevel));
                float skillEffect = (float)(1 + (GetAverageSkill - this.Props.averageSkill) * 0.0167f);
                return (levelEffect + skillEffect) / 2 * this.Props.baseDamageAmount;
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
                    ProjectilePropertiesCE projectileProps = this.parent.def.projectile as ProjectilePropertiesCE;
                    DamageInfo info = new DamageInfo(projectileProps.damageDef, ApplyDamageAmount, projectileProps.armorPenetrationSharp, instigator: Launcher, weapon:this.parent.def);
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
