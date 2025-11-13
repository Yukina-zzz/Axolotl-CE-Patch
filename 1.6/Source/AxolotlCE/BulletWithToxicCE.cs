// Decompiled with JetBrains decompiler
// Type: AxolotlCE.BulletWithToxicCE
// Assembly: AxolotlCE, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6477EAF2-EA7A-4442-9637-ABFFEF655EF8
// Assembly location: C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\3315866314\Assemblies\AxolotlCE.dll

using CombatExtended;
using RimWorld;
using Verse;


namespace AxolotlCE;

public class BulletWithToxicCE : BulletCE
{
  private Hediff GetPawnHediff(Pawn pawn, HediffDef hediff)
  {
    return pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
  }

  public override void Impact(Thing hitThing)
  {
    base.Impact(hitThing);
    if (!(hitThing is Pawn))
      return;
    Pawn pawn = hitThing as Pawn;
    if (pawn.RaceProps.IsMechanoid)
      return;
    if (this.GetPawnHediff(pawn, HediffDefOf.ToxicBuildup) == null)
    {
      pawn.health.AddHediff(HediffDefOf.ToxicBuildup);
      this.GetPawnHediff(pawn, HediffDefOf.ToxicBuildup).Severity = 0.03f;
    }
    this.GetPawnHediff(pawn, HediffDefOf.ToxicBuildup).Severity += 0.03f;
  }
}
