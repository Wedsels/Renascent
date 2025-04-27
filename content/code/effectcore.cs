using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Renascent.content.code;

internal readonly struct EffectStore {
	internal static readonly List< Effect > Effects;
	static EffectStore() => Effects = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where( t => t.IsSubclassOf( typeof( Effect ) ) && !t.IsAbstract ).Select( t => ( Effect )Activator.CreateInstance( t )! ).ToList();
	
	internal readonly Dictionary< Type, float > Bonus = [];
	public EffectStore() => Clear();
	
	internal void Clear() {
		foreach ( var i in Effects )
			Bonus[ i.GetType() ] = 1f;
	}
	
	internal void Scale( EffectStore store ) {
		foreach ( var i in Bonus.Keys )
			Bonus[ i ] *= store.Bonus[ i ];
	}
}

internal abstract class Effect {
	protected static readonly List< int > NPCDebuff = [];
	protected static readonly List< int > PlayerDebuff = [];

	static Effect() {
		for ( int i = 0; i < BuffLoader.BuffCount; i++ ) {
			if ( !Main.debuff[ i ] || !BuffID.Sets.LongerExpertDebuff[ i ] ) continue;

			if ( Main.pvpBuff[ i ] ) PlayerDebuff.Add( i );
			NPCDebuff.Add( i );
		}
	}

	internal float scale = 1f;
	protected float timerleft;
	protected bool timer => ( timerleft = Math.Max( timerleft - 1f, 0f ) ) > 0f;

	internal virtual void Update() {}

	internal virtual void Life( ref StatModifier life ) {}
	internal virtual void Mana( ref StatModifier Mana ) {}
	internal virtual void Speed( ref float maxRunSpeed, ref float accRunSpeed, ref float runAcceleration  ) {}
	internal virtual void Defense( ref Player.DefenseStat Defense ) {}

	internal virtual void Hit( Item item, NPC.HitInfo hitinfo, NPC npc ) {}
	internal virtual void Hit( Projectile proj, NPC.HitInfo hitinfo, NPC npc, bool Minion ) {}
	internal virtual void Hurt( ref Player.HurtModifiers modifier ) {}

	internal virtual void Crit( Item item, ref float crit ) {}
	internal virtual void Damage( Item item, ref StatModifier damage ) {}
	internal virtual void ManaCost( Item item, ref float reduce, ref float mult ) {}
	internal virtual void Knockback( Item item, ref StatModifier knockback ) {}
	internal virtual void AttackSpeed( Item item, ref float speed ) {}

	internal virtual void KillNPC( NPC npc ) {}
	internal virtual void ProjectileSpawn( Item item, ref Projectile projectile, bool minion ) {}
}

internal class TriggerPlayer : ModPlayer {
	private readonly EffectStore Store = new();

	public override void PostUpdateBuffs() {
		if ( !Player.TryGetModPlayer( out TrashPlayer TP ) )
			return;

		Store.Clear();

		foreach ( var i in TP.Items.Values )
			if ( i.ModItem is Bauble b )
				Store.Scale( b.Store );

		foreach ( var i in EffectStore.Effects ) {
			i.scale = Store.Bonus[ i.GetType() ] - 1f;
			i.Update();
			i.Defense( ref Player.statDefense );
		}
	}

	public override void ModifyMaxStats( out StatModifier health, out StatModifier mana ) {
		base.ModifyMaxStats( out health, out mana );

		foreach ( var i in EffectStore.Effects ) {
			i.Life( ref health );
			i.Mana( ref mana );
		}
	}

	public override void PostUpdateRunSpeeds() { foreach ( var i in EffectStore.Effects ) i.Speed( ref Player.maxRunSpeed, ref Player.accRunSpeed, ref Player.runAcceleration ); }

	public override void ModifyHurt( ref Player.HurtModifiers modifiers ) { foreach ( var i in EffectStore.Effects ) i.Hurt( ref modifiers ); }

	public override float UseSpeedMultiplier( Item item ) { float ret = 1f; foreach ( var i in EffectStore.Effects ) i.AttackSpeed( item, ref ret ); return ret; }
	public override void ModifyManaCost( Item item, ref float reduce, ref float mult ) { foreach ( var i in EffectStore.Effects ) i.ManaCost( item, ref reduce, ref mult ); }
	public override void ModifyWeaponCrit( Item item, ref float crit ) { foreach ( var i in EffectStore.Effects ) i.Crit( item, ref crit ); }
	public override void ModifyWeaponDamage( Item item, ref StatModifier damage ) { foreach ( var i in EffectStore.Effects ) i.Damage( item, ref damage ); }
	public override void ModifyWeaponKnockback( Item item, ref StatModifier knockback ) { foreach ( var i in EffectStore.Effects ) i.Knockback( item, ref knockback ); }

	public override void OnHitNPCWithItem( Item item, NPC target, NPC.HitInfo hit, int damageDone ) => EffectStore.Effects.ForEach( x => x.Hit( item, hit, target ) );
	public override void OnHitNPCWithProj( Projectile proj, NPC target, NPC.HitInfo hit, int damageDone ) => EffectStore.Effects.ForEach( x => x.Hit( proj, hit, target, proj.minionSlots > 0f || proj.minion || proj.sentry ) );
}

internal class TriggerNPC : GlobalNPC {
	public override void OnKill( NPC npc ) {
		if ( Main.LocalPlayer.whoAmI == npc.lastInteraction )
            EffectStore.Effects.ForEach( x => x.KillNPC( npc ) );
	}
}

internal class TriggerProjectile : GlobalProjectile {
    public override bool InstancePerEntity => true;

    private Item item;
    private Player player;
    public override void OnSpawn( Projectile projectile, IEntitySource source ) {
        if ( source is EntitySource_ItemUse_WithAmmo use && use.Player == Main.LocalPlayer ) {
            item = use.Item;
            player = use.Player;

            EffectStore.Effects.ForEach( x => x.ProjectileSpawn( item, ref projectile, projectile.minionSlots > 0f || projectile.minion || projectile.sentry ) );
        } if ( source is EntitySource_Parent { Entity: Projectile parent } && ( parent.minionSlots > 0f || parent.minion || parent.sentry ) && parent.TryGetGlobalProjectile( out TriggerProjectile tp ) && tp.player == Main.LocalPlayer ) {
            item = tp.item;
            player = tp.player;

            EffectStore.Effects.ForEach( x => x.ProjectileSpawn( item, ref projectile, projectile.minionSlots > 0f || projectile.minion || projectile.sentry ) );
        }
    }
}