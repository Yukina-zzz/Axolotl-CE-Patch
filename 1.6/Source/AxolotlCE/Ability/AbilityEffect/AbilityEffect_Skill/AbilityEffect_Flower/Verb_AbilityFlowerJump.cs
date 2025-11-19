using RimWorld;
using Verse;

using UnityEngine;

using System;
using static Axolotl.AxolotlUtility;
using System.Collections.Generic;

namespace Axolotl
{
    public class Verb_AbilityFlowerJumpCE : Verb_CastAbilityJump
    {
        private IntVec3 lastTruePoint;

        ThingDef jumpflyerDef = AxolotlThingDefOf.Axolotl_FlowerFlyer;

        public override float EffectiveRange => this.verbProps.range;

        private CompAbilityAxolotlEnergyCost EnergyCostComp
        {
            get
            {
                if (ability.EffectComps.Count > 0)
                {
                    foreach (CompAbilityEffect comps in ability.EffectComps)
                    {
                        if (comps is CompAbilityAxolotlEnergyCost compCost)
                        {
                            return compCost;
                        }
                    }
                }
                return null;
            }
        }

        protected override bool TryCastShot()
        {
            //由于扣除enegry的comp似乎执行在角色变成flyer之后导致无法扣除，所以写在这里
            EnergyCostComp.Apply(this.CasterPawn, this.currentTarget);
            this.ability.StartCooldown(this.ability.def.cooldownTicksRange.TrueMax);

            //跳跃高度
            jumpflyerDef.pawnFlyer.heightFactor = 0f;

            //跳跃速度
            jumpflyerDef.pawnFlyer.flightSpeed = 80.0f;

            //留下坐标
            Vector3 loc = CasterPawn.DrawPos;
            Vector3 useV = Vector3.Lerp(loc, GetEndPointOnLine(loc.ToIntVec3(), lastTruePoint, this.verbProps.range).ToVector3(), 0.5f);

            Thing Core = GenSpawn.Spawn(AxolotlThingDefOf.Axolotl_FlowerSlashCore, useV.ToIntVec3(), CasterPawn.Map);
            Core.TryGetComp<CompFlowerSlashCoreCE>().instigator = CasterPawn;

            return RimWorld.JumpUtility.DoJump(this.CasterPawn, lastTruePoint, base.ReloadableCompSource, this.verbProps, this.ability, base.currentTarget, jumpflyerDef);
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            IntVec3 target = GetEndPointOnLine(root, targ.Cell, this.verbProps.range);

            lastTruePoint = target;

            //绘制基本的点
            if (GenSight.LineOfSight(root, target, this.CasterPawn.Map))
            {
                //途径
                GenDraw.DrawFieldEdges(GenSight.BresenhamCellsBetween(root, target), Color.green);

                //中间点及攻击范围
                Vector3 useV = Vector3.Lerp(CasterPawn.DrawPos, GetEndPointOnLine(CasterPawn.Position, target, this.verbProps.range).ToVector3(), 0.5f);
                GenDraw.DrawRadiusRing(new IntVec3(useV), 11.0f, Color.red);

                //最终点
                GenDraw.DrawTargetHighlightWithLayer(GetEndPointOnLine(CasterPawn.Position, target, this.verbProps.range), AltitudeLayer.MetaOverlays);
            }
            else
            {
                //途径
                GenDraw.DrawFieldEdges(GenSight.BresenhamCellsBetween(root, target), Color.red);
                return false;
            }
            return base.CanHitTargetFrom(root, targ);
        }

        //技能gizmo的hover事件
        public override void DrawHighlight(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(
                caster.Position,
                this.verbProps.range,
                Color.white,
                null);

            //base.DrawHighlight(target);
        }

        //获取直线伤范围内的最后一个点
        private IntVec3 GetEndPointOnLine(IntVec3 startPoint, IntVec3 choosePoint, float maxRange)
        {
            float halfRange;
            Vector3 Vstart, Vend, direction, offset, closestPoint;

            List<IntVec3> result = new List<IntVec3>();

            Vstart = startPoint.ToVector3();
            Vend = choosePoint.ToVector3();

            // 计算起点和终点之间的方向向量
            direction = (Vend - Vstart).normalized;

            // 计算起点偏移方向向量的长度
            halfRange = maxRange;

            // 偏移向量
            offset = direction * halfRange;

            // 计算范围内的最靠近终点的点
            closestPoint = Vend + offset;

            foreach (IntVec3 v in GenSight.PointsOnLineOfSight(Vstart.ToIntVec3(), closestPoint.ToIntVec3()))
            {
                if (v.InHorDistOf(startPoint, this.verbProps.range) && !result.Contains(v))
                {
                    result.Add(v);
                }
            }
            return result[result.Count - 1];
        }
    }
    public class AxolotlPawnFlowerFlyer : PawnFlyer
    {
        protected override void RespawnPawn()
        {
            base.RespawnPawn();
            FallOnGroundDo();
        }

        public void FallOnGroundDo()
        {
            try
            {
            }
            catch (Exception e)
            {
                Debug.WarningInTry("角色跳跃-落地时出错", e);
            }
        }

    }
}
