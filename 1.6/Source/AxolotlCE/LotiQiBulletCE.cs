// Decompiled with JetBrains decompiler
// Type: AxolotlCE.LotiQiBulletCE
// Assembly: AxolotlCE, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6477EAF2-EA7A-4442-9637-ABFFEF655EF8
// Assembly location: C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\3315866314\Assemblies\AxolotlCE.dll

using Axolotl;
using CombatExtended;
using Verse;

#nullable disable
namespace AxolotlCE;

public class LotiQiBulletCE : BulletCE
{
    public override void Impact(Thing hitThing)
    {
        base.Impact(hitThing);
        foreach (LotiQiBulletComp comp in this.GetComps<LotiQiBulletComp>())
        {
            comp.Launcher = this.launcher as Pawn;
            comp.Impact(this.launcher, hitThing, false);
        }
    }
}
