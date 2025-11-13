// Decompiled with JetBrains decompiler
// Type: AxolotlCE.Verb_LotlQiSuppShootCE
// Assembly: AxolotlCE, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6477EAF2-EA7A-4442-9637-ABFFEF655EF8
// Assembly location: C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\3315866314\Assemblies\AxolotlCE.dll

using Axolotl;
using CombatExtended;
using RimWorld;
using Verse;

#nullable disable
namespace AxolotlCE;

public class Verb_LotlQiSuppShootCE : Verb_ShootCE
{
  private bool IsSupplementShoot = false;

  public CompLotiQiRangedWeapon_SupplementShoot WeaponComp
  {
    get
    {
      return this.EquipmentSource.HasComp<CompLotiQiRangedWeapon_SupplementShoot>() ? this.EquipmentSource.TryGetComp<CompLotiQiRangedWeapon_SupplementShoot>() : (CompLotiQiRangedWeapon_SupplementShoot) null;
    }
  }

  public CompAxolotlEnergy PawnComp
  {
    get
    {
      Pawn casterPawn = this.CasterPawn;
      return casterPawn != null ? casterPawn.TryGetComp<CompAxolotlEnergy>() : (CompAxolotlEnergy) null;
    }
  }

  private bool CanUse
  {
    get
    {
      bool canUse = false;
      if (this.PawnComp != null)
        canUse = this.CasterPawn.RaceProps.body == AxolotlRaceBodyDefOf.Axolotl && this.PawnComp.IsChangeLotiWeaponMode;
      return canUse;
    }
  }

  public override ThingDef Projectile
  {
    get
    {
      ThingDef projectile = base.Projectile;
      if (this.CanUse && this.IsSupplementShoot)
      {
        if ((double) this.PawnComp.Energy >= (double) this.WeaponComp.Props.costPerShoot * (double) this.verbProps.burstShotCount)
        {
          projectile = base.Projectile.GetModExtension<ModExt_QiProjectile>()?.projectile ?? base.Projectile;
        }
        else
        {
          this.PawnComp.IsChangeLotiWeaponMode = false;
          Messages.Message((string) "Verb_Disable_NotHaveEnoughEnergy".Translate(), (LookTargets) (Thing) this.CasterPawn, MessageTypeDefOf.NegativeEvent);
        }
      }
      return projectile;
    }
  }

  public override bool TryCastShot()
  {
    try
    {
      if (this.CanUse && this.IsSupplementShoot)
        this.PawnComp.Energy -= this.WeaponComp.Props.costPerShoot;
      if (this.IsSupplementShoot)
        ++this.CompAmmo.CurMagCount;
      if (this.burstShotsLeft != 1)
        return base.TryCastShot();
      if (base.TryCastShot())
      {
        if (!this.IsSupplementShoot)
        {
          if (this.CanUse)
          {
            this.IsSupplementShoot = true;
            this.burstShotsLeft = this.WeaponComp.Props.burstShotCount + 1;
          }
          return true;
        }
        this.IsSupplementShoot = false;
        return true;
      }
    }
    catch
    {
      return base.TryCastShot();
    }
    return base.TryCastShot();
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<bool>(ref this.IsSupplementShoot, "IsSupplementShoot");
  }
}
