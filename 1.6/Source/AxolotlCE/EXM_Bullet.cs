using CombatExtended;
using RigorMortis;
using Verse;
namespace AxolotlCE;

public class EXM_Bullet : BulletCE
{
    public override void Impact(Thing hitThing)
    {
        Pawn victim = hitThing as Pawn;
        /*
        if (victim != null && victim.Faction.Name == "Entities")
        {
            this.DamageInfo.SetAmount(this.DamageAmount*2.5f);
        }
        */

        base.Impact(hitThing);
        if (victim != null && RMUtility.IsZombie(hitThing as Pawn, out int zombieLevel))
        {
            RMUtility.ChangeMalevolent(victim, -this.DamageAmount);
        }

    }

    public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        Pawn victim = dinfo.IntendedTarget as Pawn;
        if (victim != null && victim.Faction.Name == "Entities")
        {
            dinfo.SetAmount(dinfo.Amount * 2f);
        }
        base.PreApplyDamage(ref dinfo, out absorbed);
    }
}