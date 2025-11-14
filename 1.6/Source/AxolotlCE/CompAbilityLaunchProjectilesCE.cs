// Decompiled with JetBrains decompiler
// Type: AxolotlCE.CompAbilityLaunchProjectilesCE
// Assembly: AxolotlCE, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6477EAF2-EA7A-4442-9637-ABFFEF655EF8
// Assembly location: C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\3315866314\Assemblies\AxolotlCE.dll

using Axolotl;
using CombatExtended;
using RimWorld;
using System;
using UnityEngine;
using Verse;


namespace AxolotlCE;

public class CompAbilityLaunchProjectilesCE : CompAbilityEffect
{
    public new CompProperties_AbilityLaunchProjectiles Props
    {
        get => (CompProperties_AbilityLaunchProjectiles)this.props;
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        this.LaunchProjectile(target);
    }

    private void LaunchProjectile(LocalTargetInfo target)
    {
        if (this.Props.projectileDef == null)
            return;
        Pawn pawn = this.parent.pawn;
        if (this.Props.ShootCount > 0)
        {
            for (int index = 0; index < this.Props.ShootCount; ++index)
            {
                if (this.Props.projectileDef.thingClass.IsSubclassOf(typeof(ProjectileCE)) || this.Props.projectileDef.thingClass == typeof(ProjectileCE))
                {
                    ThingDef projectile1 = this.Props.projectileDef.GetProjectile();
                    if (projectile1.projectile is ProjectilePropertiesCE projectile2)
                    {
                        Vector3 vector3 = pawn.TrueCenter();
                        Vector2 origin = new Vector2();
                        origin.Set(vector3.x, vector3.z);
                        Vector2 vector2_1 = new Vector2();
                        if (target.HasThing)
                            vector2_1.Set(target.Thing.TrueCenter().x, target.Thing.TrueCenter().z);
                        else
                            vector2_1.Set((float)target.Cell.ToIntVec2.x, (float)target.Cell.ToIntVec2.z);
                        Vector2 vector2_2 = vector2_1 - origin;
                        float shotRotation = (float)((57.295780181884766 * (double)Mathf.Atan2(vector2_2.y, vector2_2.x) - 90.0) % 360.0);
                        CollisionVertical collisionVertical = new CollisionVertical(target.Thing);
                        // 1.6 update GetShotAngle been moved from ProjectileCE to CE_Utility
                        float shotAngle = CE_Utility.GetShotAngle(projectile2.speed, (target.Cell - pawn.Position).LengthHorizontal, collisionVertical.HeightRange.Average - 1f, projectile2.flyOverhead, Convert.ToSingle(projectile2.Gravity));
                        CE_Utility.LaunchProjectileCE(projectile1, origin, target, (Thing)pawn, shotAngle, shotRotation, 1f, projectile2.speed);
                    }
                }
                else
                    ((Projectile)GenSpawn.Spawn(this.Props.projectileDef, pawn.Position, pawn.Map)).Launch((Thing)pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
            }
        }
        else
            ((Projectile)GenSpawn.Spawn(this.Props.projectileDef, pawn.Position, pawn.Map)).Launch((Thing)pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
    }
}
