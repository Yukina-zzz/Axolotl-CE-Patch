using Axolotl;
using CombatExtended;
using RigorMortis;
using System;
using System.Linq;
using Verse;

namespace AxolotlCE;

public class RMCE_AntiZombieBullet : BulletCE
{

    /*
    public Pawn GetPawn
    {
        get
        {
            return this.ParentHolder is Pawn_EquipmentTracker parentHolder ? parentHolder.pawn : (Pawn) null;
        }
    }
    */
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
    }
}