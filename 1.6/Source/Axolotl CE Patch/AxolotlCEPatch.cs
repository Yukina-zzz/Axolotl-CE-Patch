using Axolotl;
using HarmonyLib;
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
            // 将护甲穿透值乘以 30
            //默认值0.4 * 30 = 12
            armorPenetration *= 30f;

            return true;
        }
    }

}
