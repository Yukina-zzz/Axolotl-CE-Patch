using RimWorld;
using RimWorld.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

namespace Axolotl
{
    public class Comp_AbilityRockColumnCE : CompAbilityEffect
    {
        public new CompProperties_AbilityRockColumnCE Props
        {
            get
            {
                return (CompProperties_AbilityRockColumnCE)this.props;
            }
        }

        public Pawn GetPawn
        {
            get
            {
                return this.parent.pawn;
            }
        }

        private int _pawnSkillLevel = -1;

        public int PawnHediffLevel
        {
            get
            {
                //Hediff hediffFromPawn = GetPawn.health.hediffSet.GetFirstHediffOfDef(AxolotlHediffDefOf.Axolotl_LotlQi_Rock);
                //if (hediffFromPawn != null)
                //{
                //    return (int)hediffFromPawn.Severity;
                //}
                //如果角色没有等级但是持有技能
                //使用就会导致错误

                if (_pawnSkillLevel == -1)
                {
                    _pawnSkillLevel = Mathf.Max(1, MoeLotlQiSkillUtility.GetMoeLotlQiSkillLevel(GetPawn, AxolotlMoelotlQiSkillDefOf.Axolotl_MainSkill_Rock));
                }
                return _pawnSkillLevel;
            }
        }

        public float GetTrueStunSec
        {
            get
            {
                return PawnHediffLevel * Props.stunSecPerLevel;
            }
        }

        public float GetTrueStunRange
        {
            get
            {
                return PawnHediffLevel * Props.stunRangePerLevel;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            //获取当前格子的物品
            List<Thing> replaceThingList = new List<Thing>();
            replaceThingList.AddRange(from t in target.Cell.GetThingList(GetPawn.Map)
                                      where t.def.category == ThingCategory.Item
                                      select t);
            foreach (Thing thing in replaceThingList)
            {
                thing.DeSpawn(DestroyMode.Vanish);
            }

            //特效与眩晕效果
            Action_RockSkilExpolsion(GetPawn, target.Cell, GetTrueStunRange, GetTrueStunSec, 3, 15.0f);

            //生成柱子
            GenSpawn.Spawn(AxolotlThingDefOf.Axolotl_Skill_RockColumn, target.Cell, GetPawn.Map, WipeMode.Vanish);
            FleckMaker.ThrowDustPuffThick(target.Cell.ToVector3Shifted(), GetPawn.Map, 3.0f, new Color(0.55f, 0.55f, 0.55f, 4f));

            //挤开物品
            foreach (Thing thing in replaceThingList)
            {
                GenPlace.TryPlaceThing(thing, thing.Position, GetPawn.Map, ThingPlaceMode.Near, null, null, default(Rot4));
            }

            //重置
            _pawnSkillLevel = -1;
        }

        //大招一样的效果
        public static void Action_RockSkilExpolsion(Pawn GetPawn, IntVec3 targetPosition, float range, float stunSec, int randomHitPart, float damageAmountHitPart)
        {
            //友方伤害保护
            List<Thing> ignoredThings = new List<Thing>
                {
                    GetPawn
                };
            if (AxolotlModSetting.isMoeLotlAbilityNotHitFriend)
            {
                foreach (Pawn mapPawn in GetPawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (mapPawn.Faction == GetPawn.Faction) ignoredThings.Add(mapPawn);
                }
            }

            //播放特效
            FleckMaker.Static(targetPosition.ToVector3Shifted(), GetPawn.Map, AxolotlFleckDefOf.Axolotl_RockExplosion, range * 2);

            //爆炸
            //注：
            //  这是为了音效
            //  顺便对准敌人砸的话可以眩晕更久
            GenExplosion.DoExplosion(targetPosition, GetPawn.Map, 0.3f, DamageDefOf.Bomb, GetPawn, 4, ignoredThings: ignoredThings);

            //眩晕周围
            List<Pawn> list_attackTargets = (from pawn in GetPawn.Map.mapPawns.AllPawnsSpawned
                                             where pawn.Position.InHorDistOf(targetPosition, range)
                                                && !pawn.Equals(GetPawn)
                                                && !(AxolotlModSetting.isMoeLotlAbilityNotHitFriend && pawn.Faction == GetPawn.Faction)
                                             select pawn).ToList();

            for (int i = list_attackTargets.Count - 1; i >= 0; i--)
            {
                Pawn targetPawn = list_attackTargets[i];

                try
                {
                    //眩晕
                    targetPawn.stances.stunner.StunFor((int)(stunSec * AxolotlUtility.TickUtility.oneSec), GetPawn, true, true, true);

                    //中断job
                    Pawn_JobTracker pawn_JobTracker = targetPawn.jobs;
                    if (((pawn_JobTracker != null) ? pawn_JobTracker.curJob : null) != null)
                    {
                        targetPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                    }

                    //下身伤害
                    IEnumerable<BodyPartRecord> targetBodyPart = targetPawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Bottom, BodyPartDepth.Outside).Where(part => !part.def.conceptual);
                    if (targetBodyPart.Any())
                    {
                        for (int j = 0; j < randomHitPart && !targetPawn.Dead; j++)
                        {
                            targetPawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, damageAmountHitPart, 100.0f, instigator: GetPawn, hitPart: targetBodyPart.RandomElement()));
                        }
                    }
                    else
                    {
                        targetBodyPart = targetPawn.health.hediffSet.GetNotMissingParts().Where(part => !part.def.conceptual);
                        if (targetBodyPart.Any())
                        {
                            for (int j = 0; j < randomHitPart && !targetPawn.Dead; j++)
                            {
                                targetPawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, damageAmountHitPart, 100.0f, instigator: GetPawn, hitPart: targetBodyPart.RandomElement()));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WarningInTry("岩螈技能眩晕错误", e);
                }
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            if (this.Valid(target))
            {
                //绘制高亮
                GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);

                //绘制范围
                GenDraw.DrawRadiusRing(target.Cell, GetTrueStunRange);

                ////绘制范围
                //???这是我哪天喝多了写的逆天代码???
                //List<IntVec3> cells = new List<IntVec3>();
                //foreach (var cell in GetPawn.Map.AllCells)
                //{
                //    if (cell.InHorDistOf(target.Cell, GetTrueStunRange) && !cells.Contains(cell))
                //    {
                //        cells.Add(cell);
                //    }
                //}
                //if (cells.Any())
                //{
                //    GenDraw.DrawFieldEdges(cells);
                //}
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return GetPawn != null && base.Valid(target) && JumpUtility.ValidJumpTarget(GetPawn, this.GetPawn.Map, target.Cell)
                && !target.Cell.GetTerrain(GetPawn.Map).IsWater;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return GetPawn != null && base.CanApplyOn(target, dest) && JumpUtility.ValidJumpTarget(GetPawn, GetPawn.Map, target.Cell)
                && !target.Cell.GetTerrain(GetPawn.Map).IsWater;
        }
    }
}
