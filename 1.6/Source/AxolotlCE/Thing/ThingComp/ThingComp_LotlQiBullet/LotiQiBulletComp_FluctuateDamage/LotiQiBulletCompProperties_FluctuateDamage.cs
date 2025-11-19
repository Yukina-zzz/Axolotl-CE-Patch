using RimWorld;
using Verse;

using UnityEngine;

using System;

namespace Axolotl
{
    public class LotiQiBulletCompProperties_FluctuateDamageCE : LotiQiBulletCompProperties_FluctuateDamage
    {
        //计算之前的原本伤害
        //在实装这个功能之前的，原本这个剑气的伤害
        //public float baseDamageAmount;

        //角色技能平均值
        //用作与角色的射击，近战两个skill的平均值对比
        //public int averageSkill;

        //相关的技能
        //基础值为1到5级
        //public HediffDef effectHediffDef;

        //public MoeLotlQiSkillDef effectSkillDef;

        public LotiQiBulletCompProperties_FluctuateDamageCE()
        {
            this.compClass = typeof(LotiQiBulletComp_FluctuateDamageCE);
        }
    }
}
