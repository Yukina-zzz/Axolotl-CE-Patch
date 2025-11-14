using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Axolotl
{
    public class Comp_AbilityEffect_InstantChopCE : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect_InstantChopCE Props
        {
            get
            {
                return (CompProperties_AbilityEffect_InstantChopCE)this.props;
            }
        }

        public Pawn GetPawn
        {
            get
            {
                return this.parent.pawn;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            //获取范围
            List<IntVec3> TargetPosition = CompAbilitySwordAuraSuper.GetTrueIntVec3(GetPawn.Position, target.Cell, this.parent.def.verbProperties.range, out IntVec3 TargetLastPosition);

            //特效绘制
            FleckMaker.ConnectingLine(GetPawn.TrueCenter(), TargetLastPosition.ToVector3(), AxolotlFleckDefOf.Axolotl_ChoppingTrajectory, GetPawn.Map, 3.0f);

            //播放音效
            AxolotlSoundDefOf.Axolotl_Sound_FlamebreakerFlash.PlayOneShot(new TargetInfo(Vector3.Lerp(GetPawn.Position.ToVector3(), TargetLastPosition.ToVector3(), 0.5f).ToIntVec3(), GetPawn.Map, false));

            //移动角色
            GetPawn.Position = TargetLastPosition;
            GetPawn.Notify_Teleported(true, true);

            //范围内的角色受伤
            IEnumerable<Pawn> targetPawns = (from x in GetPawn.Map.mapPawns.AllPawnsSpawned
                                             where TargetPosition.Contains(x.Position)
                                                && !x.Equals(GetPawn)
                                                && !(AxolotlModSetting.isMoeLotlAbilityNotHitFriend && x.Faction == GetPawn.Faction)
                                             select x);
            List<FleckDef> list_usefleck = new List<FleckDef>
            {
                AxolotlFleckDefOf.Axolotl_CounterAttack_one,
                AxolotlFleckDefOf.Axolotl_CounterAttack_two,
                AxolotlFleckDefOf.Axolotl_CounterAttack_three
            };

            for (int i = targetPawns.ToList().Count - 1; i >= 0; i--)
            {
                //获取目标角色
                Pawn targetPawn = targetPawns.ToList()[i];

                targetPawn.TryAttachFire(Rand.Range(0.35f, 0.25f), null);
                FleckMaker.Static(targetPawn.Position, targetPawn.Map, list_usefleck.RandomElement(), 2.5f);

                //灼炎一闪穿透修改,原穿透1.0f
                DamageInfo info = new DamageInfo(DamageDefOf.Cut, 40.0f, 50.0f, instigator: GetPawn, weapon: Props.weapon);
                targetPawn.TakeDamage(info);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            CompAbilitySwordAuraSuper.GetTrueIntVec3(GetPawn.Position, target.Cell, this.parent.def.verbProperties.range, out IntVec3 TargetLastPosition);
            if (!GenSight.LineOfSight(GetPawn.Position, TargetLastPosition, GetPawn.Map))
            {
                return false;
            }
            return base.CanApplyOn(target, dest);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            List<IntVec3> targetPosition = CompAbilitySwordAuraSuper.GetTrueIntVec3(GetPawn.Position, target.Cell, this.parent.def.verbProperties.range, out IntVec3 lastPosition);
            if (!GenSight.LineOfSight(GetPawn.Position, lastPosition, GetPawn.Map))
            {
                GenDraw.DrawFieldEdges(targetPosition, Color.red);
            }
            else
            {
                GenDraw.DrawFieldEdges(targetPosition, Color.green);
            }
        }
    }
}
