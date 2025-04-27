using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Renascent.content.code;

internal class Health : Effect {
	internal override void Life( ref StatModifier life ) {
		life += scale;
	}
}

internal class FlatHealth : Effect {
	internal override void Life( ref StatModifier life ) {
		life.Base += 100 * scale;
	}
}

internal class Debuff : Effect {
	internal override void Hit( Projectile proj, NPC.HitInfo hitinfo, NPC npc, bool Minion ) => hit( npc );
	internal override void Hit( Item item, NPC.HitInfo hitinfo, NPC npc ) => hit( npc );
	private void hit( NPC npc ) {
        while ( Main.rand.NextFloat() < scale-- ) {
			int buff = Main.rand.Next( NPCDebuff );
			Console.WriteLine( Main.GetBuffTooltip( Main.LocalPlayer, buff ) );
			npc.AddBuff( buff, 600 );
		}
	}

	internal override void Hurt( ref Player.HurtModifiers modifier ) {
        while ( -Main.rand.NextFloat() > scale++ ) {
			int buff = Main.rand.Next( PlayerDebuff );
			Console.WriteLine( Main.GetBuffTooltip( Main.LocalPlayer, buff ) );
			Main.LocalPlayer.AddBuff( buff, 600 );
		}
	}
}

internal class AttackSpeedHit : Effect {
	private int count;
	
	private void inc() { timerleft = 10 * scale; count++; }

	internal override void Hit( Item item, NPC.HitInfo hitinfo, NPC npc ) => inc();
	internal override void Hit( Projectile proj, NPC.HitInfo hitinfo, NPC npc, bool Minion ) => inc();

	internal override void AttackSpeed( Item item, ref float speed ) {
		if ( timer )
			speed *= 1f + scale / 50f * count;
		else count = 0;
	}
}

internal class Multishot : Effect {
    internal override void ProjectileSpawn( Item item, ref Projectile projectile, bool minion ) {
        if ( minion )
            return;

        scale *= 10f;
        while ( Main.rand.NextFloat() < scale-- ) {
            Projectile proj = Projectile.NewProjectileDirect(
                new Terraria.DataStructures.EntitySource_Parent( projectile ),
                projectile.position,
                projectile.velocity.RotateRandom( MathHelper.ToRadians( 15f ) ) * Main.rand.NextFloat( 0.8f, 1.2f ),
                projectile.type,
                projectile.damage / Main.rand.Next( 1, 5 ),
                projectile.knockBack / Main.rand.Next( 1, 5 ),
                projectile.owner,
                projectile.ai[ 0 ],
                projectile.ai[ 1 ],
                projectile.ai[ 2 ]
            );

			proj.usesIDStaticNPCImmunity = false;
			proj.usesLocalNPCImmunity = true;
			proj.localNPCHitCooldown = 45;
			proj.noDropItem = true;
        }
    }
}