using Axolotl;
using CombatExtended;
using RimWorld;
using Verse;
using RigorMortis;

namespace AxolotlCE;

public class Verb_IncanationCrossbowIntensifyShotCE : Verb_ShootCE
{
    
    public Pawn GetPawn
  {
    get
    {
      return this.EquipmentSource?.ParentHolder is Pawn_EquipmentTracker parentHolder ? parentHolder.pawn : (Pawn) null;
    }
  }

  public CompAxolotlEnergy PawnComp => this.GetPawn.TryGetComp<CompAxolotlEnergy>();

  public CompLotiQiRangedWeapon_ChangeProjectile WeaponComp
  {
    get
    {
      return this.EquipmentSource.HasComp<CompLotiQiRangedWeapon_ChangeProjectile>() ? this.EquipmentSource.TryGetComp<CompLotiQiRangedWeapon_ChangeProjectile>() : (CompLotiQiRangedWeapon_ChangeProjectile) null;
    }
  }

  private bool CanUse
  {
    get
    {
      //return this.GetPawn.RaceProps.body == AxolotlRaceBodyDefOf.Axolotl && this.PawnComp.IsChangeLotiWeaponMode;
      bool canUse = false;
      if (this.PawnComp != null)
      {
        
        canUse = this.CasterPawn.RaceProps.body == AxolotlRaceBodyDefOf.Axolotl && this.PawnComp.IsChangeLotiWeaponMode;  
      }
      
      return canUse;
    }
  }

  public override ThingDef Projectile
  {
    get
    {
      ThingDef projectile = base.Projectile;
      
      if (this.CanUse)
      {
        if ((double) this.PawnComp.Energy >= (double) this.WeaponComp.Props.costPerShoot * (double) this.verbProps.burstShotCount)
        {
          
          projectile = base.Projectile.GetModExtension<ModExt_QiProjectile>()?.projectile ?? base.Projectile;
        }
        else
        {
          
          this.PawnComp.IsChangeLotiWeaponMode = false;
          Messages.Message((string) "Verb_Disable_NotHaveEnoughEnergy".Translate(), (LookTargets) (Thing) this.GetPawn, MessageTypeDefOf.NegativeEvent);
        }
      }
      
      return projectile;
    }
  }

  public override bool TryCastShot()
  {
    if (this.CanUse)
      this.PawnComp.Energy -= this.WeaponComp.Props.costPerShoot;
    bool isHit = base.TryCastShot();
    
    // zombieLevel  1 to 5 max ;
    //这个射击没命中也会扣傻气
    /*
    if (base.OnCastSuccessful() && RMUtility.IsZombie(base.currentTarget.Thing as Pawn, out int zombieLevel))
    {
      Pawn zombie = base.currentTarget.Thing as Pawn;
      RMUtility.ChangeMalevolent(zombie,base.EquipmentSource.def.GetModExtension<ModExtension_TaoistArtifact>().MalevolentDamage);
    }
    */
    return isHit;

  }
    
}