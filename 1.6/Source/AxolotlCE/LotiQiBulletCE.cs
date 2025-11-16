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

            //fork ce的获取受击部位 
            if (hitThing != null && hitThing is Pawn hitPawn)
            { 
                var damDefCE = def.projectile.damageDef.GetModExtension<DamageDefExtensionCE>() ?? new DamageDefExtensionCE();

                BodyPartHeight partHeight = new CollisionVertical(hitThing).GetCollisionBodyHeight(ExactPosition.y);

                BodyPartDepth partDepth = damDefCE.harmOnlyOutsideLayers ? BodyPartDepth.Outside : BodyPartDepth.Undefined;

                var dinfoForLocating = new DamageInfo(def.projectile.damageDef, 0);
                dinfoForLocating.SetBodyRegion(partHeight, partDepth);

                BodyPartRecord finalHitPart = hitPawn.health.hediffSet.GetRandomNotMissingPart(dinfoForLocating.Def, dinfoForLocating.Height, dinfoForLocating.Depth);

                AxolotlCEPatchContext.LotiQiImpactAttackCE_HitBodyPart = finalHitPart;
            }


            comp.Launcher = this.launcher as Pawn;
            comp.Impact(this.launcher, hitThing, false);

        }
    }
}
