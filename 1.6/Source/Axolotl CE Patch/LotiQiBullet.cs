using RimWorld;
using Verse;

using UnityEngine;

using System;

namespace Axolotl
{
    public class LotiQiBulletCE : Bullet
    {
        //击中目标
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            //base.Impact(hitThing, blockedByShield);

            //执行comp
            if (this.AllComps != null)
            {
                foreach (LotiQiBulletComp LotlQiComp in GetComps<LotiQiBulletComp>())
                {
                    LotlQiComp?.Impact(this.Launcher, hitThing, blockedByShield);
                }
            }
        }
    }
}
