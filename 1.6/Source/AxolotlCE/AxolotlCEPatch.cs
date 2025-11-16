using Axolotl;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Verse.DamageWorker;

namespace AxolotlCE
{
    [StaticConstructorOnStartup]
    public static class AxolotlCEPatch
    {
        static AxolotlCEPatch()
        {
            var harmony = new Harmony("com.AxolotlCEPatch");
            harmony.PatchAll();
        }
    }

    public static class AxolotlCEPatchContext
    {
        //近战螈力模式攻击目标部位
        public static BodyPartRecord LotiQiMeleeAttackCE_HitBodyPart = null;
        //近战螈力模式攻击目标部位
        public static BodyPartRecord LotiQiImpactAttackCE_HitBodyPart = null;


        //fork by LotiQiBulletComp_LotiQiConfusion.GiveMechDamageIfNotHave
        public static void GiveMechDamageIfNotHave(Pawn targetPawn, Pawn instigator, BodyPartRecord bodypart, float DamageAmount, Thing weapon)
        {
            Vector3 direction = (targetPawn.Position - instigator.Position).ToVector3();
            ////控制
            //保底伤害的思路逃过复杂且没用
            //这里替换为实用性较强的眩晕控制
            DamageInfo info_emp = new DamageInfo(DamageDefOf.EMP, DamageAmount, 2.0f, -1f, instigator, bodypart, weapon.def, DamageInfo.SourceCategory.ThingOrUnknown, targetPawn, true, true, weaponQuality: weapon.TryGetQuality(out QualityCategory qc_emp) ? qc_emp : QualityCategory.Normal, true);
            info_emp.SetAngle(direction);
            targetPawn.TakeDamage(info_emp);

            ////机械族特伤
            //机械族心灵敏感度固定为0.5，蜈蚣为0.75，似乎与体型有关
            //没有心灵敏感就算做0.5f
            float severity = targetPawn.GetStatValue(StatDefOf.PsychicSensitivity, true, -1);
            DamageInfo info_damage = new DamageInfo(DamageDefOf.Bomb, (severity == 0 ? 0.5f : severity), 2.0f, -1f, instigator, bodypart, weapon.def, DamageInfo.SourceCategory.ThingOrUnknown, targetPawn, true, true, weaponQuality: weapon.TryGetQuality(out QualityCategory qc_damage) ? qc_damage : QualityCategory.Normal, true);
            info_damage.SetAngle(direction);
            targetPawn.TakeDamage(info_damage);
        }
    }

    //百花印爆炸穿透
    //原版就有拼写错误Expolstion/Explosion
    [HarmonyPatch(typeof(Axolotl.AxolotlUtility.ExpolstionUtility), nameof(Axolotl.AxolotlUtility.ExpolstionUtility.DoFlowerExpolsion))]
    public static class DoFlowerExpolsion_ArmorPen_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float armorPenetration)
        {
            //暂时先简单的将护甲穿透值乘以 30
            //默认值0.4 * 30 = 12
            armorPenetration *= 30f;

            return true;
        }
    }

    //让原螈力模式对机械族造成伤害方法为空,换用自己fork过来的
    [HarmonyPatch(typeof(LotiQiBulletComp_LotiQiConfusion), nameof(LotiQiBulletComp_LotiQiConfusion.GiveMechDamageIfNotHave))]
    public static class LotiQiBulletComp_LotiQiConfusion_GiveMechDamageIfNotHave_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn targetPawn, Pawn instigator, ref BodyPartRecord bodypart, float DamageAmount, Thing weapon)
        {
            return false;
        }
    }

    //近战武器螈力模式 (CE 版本)
    // 目标类从原版的 Verb_MeleeAttack 修改为 CE 的 Verb_MeleeAttackCE
    [HarmonyPatch(typeof(Verb_MeleeAttackCE), nameof(Verb_MeleeAttackCE.ApplyMeleeDamageToTarget))]
    public static class Verb_MeleeAttackCE_ApplyMeleeDamageToTarget_Patch
    {
        [HarmonyPostfix]
        // __instance 的类型从 Verb_MeleeAttack 修改为 Verb_MeleeAttackCE
        private static void Postfix(LocalTargetInfo target, Verb_MeleeAttackCE __instance, DamageResult __result)
        {
            // 获取使用者
            Pawn user = __instance.CasterPawn;

            // 时萌螈 && 开启螈力 && 持有近战螈力武器 && 螈力模式启动
            if (user.IsMoeLotl())
            {
                // 获取武器
                Thing weapon = user?.equipment?.Primary;
                if (weapon != null)
                {
                    // 角色螈力comp
                    CompAxolotlEnergy comp = user.TryGetComp<CompAxolotlEnergy>();

                    // 获取武器螈力模式comp
                    Comp_LotiQiCloseCombatWeapon_ProjectileCost comp_weapon = weapon.TryGetComp<Comp_LotiQiCloseCombatWeapon_ProjectileCost>();

                    if (comp != null && comp.PawnHaveHediff && comp.IsChangeLotiWeaponMode && comp_weapon != null)
                    {
                        //获取第一个受击部位给武器的螈力模式
                        //肾 躯干 受击部位列表一般这样排序的
                        AxolotlCEPatchContext.LotiQiMeleeAttackCE_HitBodyPart = __result.parts.FirstOrDefault();

                        //伤害信息
                        float num = __instance.ToolCE.power;
                        float armorPenetration = __instance.verbProps.AdjustedArmorPenetration(__instance, __instance.CasterPawn);

                        //执行效果
                        comp_weapon.ApplyEffectToTarget(target.Thing, user, num, armorPenetration);
                    }
                }
            }
        }
    }

    // 使用Harmony Prefix重写Comp_LotiQiCloseCombatWeapon_ProjectileCost.ApplyEffectToTarget方法
    [HarmonyPatch(typeof(Comp_LotiQiCloseCombatWeapon_ProjectileCost), nameof(Comp_LotiQiCloseCombatWeapon_ProjectileCost.ApplyEffectToTarget))]
    public static class Comp_LotiQiCloseCombatWeapon_ProjectileCost_ApplyEffectToTarget_Patch_CE
    {
        // 使用Prefix并返回false来完全阻止并替换原方法的执行
        [HarmonyPrefix]
        public static bool Prefix(Comp_LotiQiCloseCombatWeapon_ProjectileCost __instance, Thing targetThing, Pawn instigator, float damageAmount, float armorPenetration)
        {
            if (targetThing is Pawn targetPawn)
            {
                if (targetPawn.Dead) return false;

                //如果目标是友方并且使用者未征召（斗殴状态）
                if (targetPawn.Faction == instigator.Faction && !instigator.Drafted) return false;

                ////扣除螈力并自检
                //扣除螈力
                if (!AxolotlUtility.MoelotlEnergyUtility.DoEnergyCost(instigator, __instance.Props.costPerHit))
                {
                    //扣除失败
                    return false;
                }
                //扣除成功执行自动判断程序
                instigator.TryGetComp<CompAxolotlEnergy>().AutoCloseLotlQiWeaponMode();

                //使用特效
                try
                {
                    if (__instance.Props.fleckDefsOnHitPawn.Any())
                    {
                        FleckMaker.Static(targetPawn.DrawPos, targetPawn.Map, __instance.Props.fleckDefsOnHitPawn.RandomElement());
                    }
                }
                catch (Exception e)
                {
                    Axolotl.Debug.WarningInTry("近战螈力模式-绘制集种fleck错误", e);
                }

                //螈力混乱
                //机械|无人机
                if (targetPawn.RaceProps.IsMechanoid || targetPawn.RaceProps.IsDrone)
                {
                    // CE修改: 不再获取大脑, 而是直接使用由Verb_MeleeAttackCE Patch传入的受击部位
                    //不再使用DPS作为伤害,而是使用当前攻击的基础伤害/5
                    AxolotlCEPatchContext.GiveMechDamageIfNotHave(targetPawn, instigator, AxolotlCEPatchContext.LotiQiMeleeAttackCE_HitBodyPart, damageAmount / 5, __instance.parent);
                }
                //非机械
                else
                {
                    // 对于非机械单位, 仍然以大脑为目标
                    // 烧伤且伤害很低,暂且不管
                    BodyPartRecord bodyPart = targetPawn.health.hediffSet.GetBrain();
                    LotiQiBulletComp_LotiQiConfusion.GivePawnHediffIfNotHave(targetPawn, instigator, AxolotlHediffDefOf.Axolotl_LotiQiConfusion, bodyPart, __instance.parent);
                }

                //获取目标对应effect
                CloseCombatWeaponEffect effectParameter = targetPawn.RaceProps.IsMechanoid ? __instance.Props.effectToMech : __instance.Props.effectToPawn;

                if (targetPawn.Dead) return false;

                //给予hediff
                __instance.TryGivePawnHediffs(targetPawn, instigator, effectParameter);

                if (targetPawn.Dead) return false;

                //给予伤害
                __instance.TryGivePawnDamges(targetPawn, instigator, effectParameter);
            }
            else if (targetThing is Building hitBuilding)
            {
                if (hitBuilding.DestroyedOrNull()) return false;

                //砸家具状态
                if (hitBuilding.Faction == instigator.Faction && !instigator.Drafted) return false;

                ////扣除螈力并自检
                //扣除螈力
                if (!AxolotlUtility.MoelotlEnergyUtility.DoEnergyCost(instigator, __instance.Props.costPerHit))
                {
                    //扣除失败
                    return false;
                }
                //扣除成功执行自动判断程序
                instigator.TryGetComp<CompAxolotlEnergy>().AutoCloseLotlQiWeaponMode();

                //使用特效
                try
                {
                    if (__instance.Props.fleckDefsOnHitPawn.Any())
                    {
                        FleckMaker.Static(hitBuilding.DrawPos, hitBuilding.Map, __instance.Props.fleckDefsOnHitPawn.RandomElement());
                    }
                }
                catch (Exception e)
                {
                    Axolotl.Debug.WarningInTry("近战螈力模式-绘制集种fleck错误", e);
                }

                if (hitBuilding.DestroyedOrNull()) return false;

                //给予伤害
                __instance.TryGiveBuildingDamges(hitBuilding, instigator, __instance.Props.effectToMech);
            }

            return false;
        }
    }

    //螈力模式远程武器
    [HarmonyPatch(typeof(LotiQiBulletComp_LotiQiConfusion), nameof(LotiQiBulletComp_LotiQiConfusion.Impact))]
    public static class LotiQiBulletComp_LotiQiConfusion_Impact_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(LotiQiBulletComp_LotiQiConfusion __instance, Thing instigator, Thing hitThing, bool blockedByShield)
        {
            if (!blockedByShield)
                if (hitThing is Pawn hitPawn && hitPawn != null)
                    if (hitPawn.RaceProps.IsMechanoid || hitPawn.RaceProps.IsDrone)
                    {
                        Pawn launcher = instigator as Pawn;
                        Thing weapon = __instance.parent;

                        //BodyPartRecord bodyPart = hitPawn.health.hediffSet.GetBrain();

                        AxolotlCEPatchContext.GiveMechDamageIfNotHave(hitPawn, launcher, AxolotlCEPatchContext.LotiQiImpactAttackCE_HitBodyPart, __instance.parent.def.projectile.GetDamageAmount(null)/5, __instance.parent);
                    }
        }
    }
}
