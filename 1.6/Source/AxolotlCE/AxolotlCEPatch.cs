using Axolotl;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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

    //近战螈力模式 (CE 版本)
    // 目标类从原版的 Verb_MeleeAttack 修改为 CE 的 Verb_MeleeAttackCE
    [HarmonyPatch(typeof(Verb_MeleeAttackCE))]
    [HarmonyPatch("ApplyMeleeDamageToTarget", MethodType.Normal)]
    public static class Verb_MeleeAttackCE_ApplyMeleeDamageToTarget_Patch
    {
        [HarmonyPostfix]
        // __instance 的类型从 Verb_MeleeAttack 修改为 Verb_MeleeAttackCE
        private static void Postfix(LocalTargetInfo target, Verb_MeleeAttackCE __instance)
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
                        //伤害信息
                        float num = __instance.verbProps.AdjustedMeleeDamageAmount(__instance, __instance.CasterPawn);
                        float armorPenetration = __instance.verbProps.AdjustedArmorPenetration(__instance, __instance.CasterPawn);

                        //执行效果
                        comp_weapon.ApplyEffectToTarget(target.Thing, user, num, armorPenetration);
                    }
                }
            }
        }
    }

}
