using Axolotl;
using CombatExtended;
using RigorMortis;
using System;
using System.Linq;
using Verse;

namespace AxolotlCE;

public class RMCE_AntiZombieBullet_Qied : BulletCE
{

    public override void Impact(Thing hitThing)
    {

        base.Impact(hitThing);
        Pawn zombie = hitThing as Pawn;
        if (zombie != null && RMUtility.IsZombie(hitThing as Pawn, out int zombieLevel))
        {
            ModExtension_TaoistArtifact modEx = this.equipmentDef.GetModExtension<ModExtension_TaoistArtifact>();
            if (modEx != null)
            {
                RMUtility.ChangeMalevolent(zombie, modEx.MalevolentDamage);
            }
        }
        foreach (LotiQiBulletComp comp in this.GetComps<LotiQiBulletComp>())
        {
            comp.Launcher = this.launcher as Pawn;
            comp.Impact(this.launcher, hitThing, false);
        }
    }

}