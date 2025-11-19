using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;
using static RimWorld.Building_HoldingPlatform;
using static UnityEngine.GraphicsBuffer;

namespace Axolotl
{
    public class CompTargetEffect_ElectromagneticLauncherCE : CompTargetEffect
    {
        public CompProperties_TargetEffect_ElectromagneticLauncherCE Props
        {
            get
            {
                return (CompProperties_TargetEffect_ElectromagneticLauncherCE)this.props;
            }
        }

        public const int raiudRange = 15;

        public override void DoEffectOn(Pawn user, Thing target)
        {
            //目标
            Pawn targetPawn = (Pawn)target;
            if (targetPawn.Dead)
            {
                return;
            }

            //获取目标们
            var Pawns = new List<Pawn>
            {
                targetPawn
            };

            foreach (Pawn otherPawn in targetPawn.Map.mapPawns.AllPawnsSpawned)
            {
                //友伤保护
                if (AxolotlModSetting.isMoeLotlAbilityNotHitFriend && otherPawn.Faction == user.Faction) continue;
                //范围内
                if (!otherPawn.Position.InHorDistOf(targetPawn.Position, raiudRange)) continue;
                //视线内
                if (!GenSight.LineOfSight(targetPawn.Position, otherPawn.Position, targetPawn.Map)) continue;
                //重复
                if (Pawns.Contains(otherPawn)) continue;

                Pawns.Add(otherPawn);
            }

            ////特效绘制
            //对目标
            FleckMaker.ConnectingLine(user.DrawPos, Pawns[0].DrawPos, this.Props.mainTargetFleckDef, user.Map, 1.5f);
            //闪烁特效
            FleckMaker.Static(Pawns[0].Position, Pawns[0].Map, AxolotlFleckDefOf.Axolotl_ThunderExplosion, raiudRange * 2);
            //对其他
            for (int i = Pawns.Count - 1; i >= 0; i--)
            {
                TakeDamgeTo(user, Pawns[i]);
            }

            //  2024/10/04 bug日记：
            //  状态：
            //      癫疯
            //  心情：
            //      郁闷
            //  记录：
            //      为什么这边会出现有特效但是没伤害的情况呢
            //      最终我决定把伤害与特效同时进行
            //      这样就不会出现这种情况了
            //      吧？

            //  2025/02/04 bug日记：
            //  状态：
            //      null
            //  心情：
            //      null
            //  记录：
            //      我去除了旧的特效代码，换上了新的特效
            //      这样你才知道我修改了代码（
            //      好吧这是雷螈的特效，希望他们不要骂我（（（

            //战斗记录
            BattleLogEntry_ItemUsed battleLogEntry_ItemUsed = new BattleLogEntry_ItemUsed(user, target, this.parent.def, RulePackDefOf.Event_ItemUsed);
            /*
            hediff.combatLogEntry = new Verse.WeakReference<LogEntry>(battleLogEntry_ItemUsed);
            hediff.combatLogText = battleLogEntry_ItemUsed.ToGameStringFromPOV(null, false);
            */
            Find.BattleLog.Add(battleLogEntry_ItemUsed);

            ////建筑emp
            try
            {
                var ListTargetThing = GenRadial.RadialDistinctThingsAround(user.Position, user.Map, raiudRange, true);
                //对建筑产生emp效果
                for (int i = 0; i < ListTargetThing.Count() - 1; i++)
                {
                    Thing item = ListTargetThing.ElementAt(i);

                    if (item is Building build)
                    {
                        build.TakeDamage(new DamageInfo(DamageDefOf.EMP, 15f, 2.0f, -1f, user, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WarningInTry("对范围内建筑实行emp效果错误", e);
            }
        }

        private void TakeDamgeTo(Pawn user, Pawn target)
        {
            try
            {
                //机械族
                if (target.RaceProps.IsMechanoid)
                {
                    try
                    {
                        //试图获取大脑
                        BodyPartRecord targetBodyPart = target.health.hediffSet.GetBrain();

                        //部队机械族boss造成伤害
                        if (targetBodyPart != null && !target.kindDef.isBoss)
                        {
                            //获取大脑血量
                            float minDamAmountOnMechBrain = targetBodyPart.def.GetMaxHealth(target);
                            //七分之一的伤害
                            float damAmount = minDamAmountOnMechBrain / Props.DamageToMech.max * Props.DamageToMech.min;
                            //CE轰天雷穿透，原2.0f
                            target.TakeDamage(new DamageInfo(DamageDefOf.Crush, damAmount, 100.0f, -1f, user, targetBodyPart, this.parent.def, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
                        }
                        //机械族眩晕
                        //不计算伤害
                        //固定眩晕数值
                        target.TakeDamage(new DamageInfo(DamageDefOf.EMP, 10f, 2.0f, -1f, user, null, this.parent.def, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
                    }
                    catch (Exception e)
                    {
                        Debug.WarningInTry("轰天雷造成伤害-机械体错误", e);
                    }
                }
                //非机械
                else
                {
                    try
                    {
                        float damAmount = this.Props.DamageToNotMech.RandomInRange;
                        //CE轰天雷穿透，原0.5f，不过这应该是环境伤害,穿透无所谓
                        target.TakeDamage(new DamageInfo(DamageDefOf.Burn, damAmount, 12.0f, -1f, user, null, this.parent.def, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
                        //三分之一的概率点燃对方
                        if (Verse.Rand.Range(0, 3) == 0) target.TryAttachFire(Rand.Range(0.15f, 0.25f), null);
                    }
                    catch (Exception e)
                    {
                        Debug.WarningInTry("轰天雷造成伤害-非机械体单位错误", e);
                    }
                }

                try
                {
                    //掉关系
                    Faction faction = (target != null) ? target.HomeFaction : target.Faction;
                    if (user.Faction == Faction.OfPlayer && faction != null && !faction.HostileTo(user.Faction) && (target == null || !target.IsSlaveOfColony))
                    {
                        Faction.OfPlayer.TryAffectGoodwillWith(faction, -200, true, true, HistoryEventDefOf.UsedHarmfulItem, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.WarningInTry("轰天雷造成伤害-掉派系关系错误", e);
                }
            }
            catch (Exception e)
            {
                Debug.WarningInTry("轰天雷造成伤害错误", e);
            }
        }

        //是否可以选择对象
        public override bool CanApplyOn(Thing target)
        {
            if (target is Pawn pawn && pawn != null)
            {
                if (pawn.kindDef.forceDeathOnDowned)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
