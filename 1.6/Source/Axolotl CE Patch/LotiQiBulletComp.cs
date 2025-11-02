using RimWorld;
using Verse;

using UnityEngine;

using System;

namespace Axolotl
{
    public class LotiQiBulletCompCE : ThingComp
    {
        public Pawn Launcher;

        public virtual void Impact(Thing instigator, Thing hitThing, bool blockedByShield)
        {
            Launcher = instigator as Pawn;
        }
    }
}
