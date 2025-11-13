// Decompiled with JetBrains decompiler
// Type: AxolotlCE.CompQiWeaponStatCE
// Assembly: AxolotlCE, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6477EAF2-EA7A-4442-9637-ABFFEF655EF8
// Assembly location: C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\3315866314\Assemblies\AxolotlCE.dll

using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

#nullable disable
namespace AxolotlCE;

public class CompQiWeaponStatCE : ThingComp
{
  public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
  {
    StringBuilder stringBuilder = new StringBuilder();
    bool Display = false;
    stringBuilder.AppendLine((string) "QiWeaponExplanation".Translate());
    CompAmmoUser ammouser = this.parent.GetComp<CompAmmoUser>();
    if (ammouser != null)
    {
      foreach (AmmoLink ammoPair in ammouser.Props.ammoSet.ammoTypes)
      {
        ModExt_QiProjectile ext = ammoPair.projectile.GetModExtension<ModExt_QiProjectile>();
        if (ext != null)
        {
          Display = true;
          string label = string.IsNullOrEmpty(ammoPair.ammo.ammoClass.LabelCapShort) ? (string) ammoPair.ammo.ammoClass.LabelCap : ammoPair.ammo.ammoClass.LabelCapShort;
          stringBuilder.AppendLine($"{label}:\n{ext.projectile.GetProjectileReadout((Thing) null)}");
          label = (string) null;
        }
        ext = (ModExt_QiProjectile) null;
      }
    }
    if (Display)
      yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, (string) "QiWeaponStatLabel".Translate(), "", stringBuilder.ToString(), 0);
  }
}
