using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

using Renascent.content.code.mimic;
using Renascent.content.code.bauble;

namespace Renascent.content.code;

internal readonly struct Modifier() {
	internal readonly float Multiplicative = 1.0f;
	internal readonly int Additive = 0;

	private Modifier( float multiplicative, int additive ):this() {
		Multiplicative = multiplicative;
		Additive = additive;
	}

	public static Modifier operator +( Modifier a, int b ) => new( a.Multiplicative, a.Additive + b );
	public static Modifier operator -( Modifier a, int b ) => new( a.Multiplicative, a.Additive - b );
	public static Modifier operator *( Modifier a, float b ) => new( a.Multiplicative * b, a.Additive );
	public static Modifier operator /( Modifier a, float b ) => new( a.Multiplicative * ( 1.0f / b ), a.Additive );
}

internal struct Boost() {
	internal static readonly List< int > NPCDebuff = [];
	internal static readonly List< int > PlayerDebuff = [];

	static Boost() {
		for ( int i = 0; i < BuffLoader.BuffCount; i++ ) {
			if ( !Main.debuff[ i ] || !BuffID.Sets.LongerExpertDebuff[ i ] )
				continue;

			if ( Main.pvpBuff[ i ] ) PlayerDebuff.Add( i );
				NPCDebuff.Add( i );
		}
	}

	internal Modifier Life = new();
	internal Modifier Mana = new();
	internal Modifier Crit = new();
	internal Modifier Damage = new();
	internal Modifier Defense = new();
	internal Modifier Knockback = new();
	internal float Speed = 1.0f;
	internal float Dodge = 0.0f;
	internal float Velocity = 1.0f;
	internal float ManaCost = 1.0f;
	internal float Multishot = 0.0f;
	internal float LifeSteal = 0.0f;
	internal float ManaSteal = 0.0f;
	internal float CritDamage = 1.0f;
	internal float AttackSpeed = 1.0f;
	internal int LifeToMana = 0;
	internal int BaubleSlots = 0;
	internal int PoweredSlots = 0;
}

internal class TriggerPlayer : ModPlayer {
	internal Boost Boost = new();
	internal List< Bauble > Worn = [];

	public override void ResetEffects() {
		if ( !Player.TryGetModPlayer( out MimicPlayer MP ) )
			return;

		Worn = [];
		for ( int i = 0; i < MimicPlayer.BaubleMaxSlots; i++ )
			if ( !MP.Vanity[ i ] && MP.Baubles[ i ]?.Item.ModItem is Bauble bauble ) {
				bauble.Power = Bauble.IsPowered( i ) ? 0.5f : 0.0f;
				Worn.Add( bauble );
			}
	}

	public override void PostUpdateMiscEffects() {
		if ( !Player.TryGetModPlayer( out MimicPlayer MP ) )
			return;

		Boost = new();

		foreach ( var w in Worn ) {
			w.Timer = Math.Max( w.Timer - 1 / 60.0, 0 );
			w.Update( ref Boost );
		}

		Player.GetAttackSpeed< GenericDamageClass >() *= Boost.AttackSpeed;

		MP.PoweredSlotCount = Boost.PoweredSlots;
		MP.UnlockedSlotCount = Math.Min( MimicPlayer.BaubleMaxSlots, Boost.BaubleSlots + MP.MimicUpgrade + 1 );
	}

	public override void ModifyMaxStats(out StatModifier Life, out StatModifier Mana ) {
		base.ModifyMaxStats( out Life, out Mana );

		Life.Base += Boost.Life.Additive;
		Life *= Boost.Life.Multiplicative;

		Mana.Base += Boost.Mana.Additive;
		Mana *= Boost.Mana.Multiplicative;

		int llife = ( int )( ( Player.statLifeMax + Life.Base + Life.Additive ) * Life.Multiplicative );
		int mmana = ( int )( ( Player.statManaMax + Mana.Base + Mana.Additive ) * Mana.Multiplicative );

		int mul = Boost.LifeToMana > 0 ? 1 : -1;

		Boost.LifeToMana = Math.Abs( Boost.LifeToMana );
		Boost.LifeToMana = mul > 0 ? Math.Min( Boost.LifeToMana / 2, ( mmana - 21 ) * 2 ) : Math.Min( Boost.LifeToMana, llife - 21 );

		Life.Base += Boost.LifeToMana * ( mul > 0 ? 2 : -1 );
		Mana.Base += Boost.LifeToMana / ( mul > 0 ? -1 : 2 );

		Player.statDefense += Boost.Defense.Additive;
		Player.statDefense *= Boost.Defense.Multiplicative;
	}

	public override void PostUpdateRunSpeeds() {
		Player.maxRunSpeed += Boost.Speed;
		Player.accRunSpeed *= Boost.Speed;
		Player.runAcceleration *= Boost.Speed;
	}

	public override void ModifyHurt( ref Player.HurtModifiers modifiers ) { foreach ( var i in Worn ) i.Hurt( ref modifiers ); }

	public override void ModifyManaCost( Item item, ref float reduce, ref float mult ) => mult *= 1.0f / Boost.ManaCost;
    public override void ModifyShootStats( Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback ) => velocity *= Boost.Velocity;
	public override void ModifyWeaponCrit( Item item, ref float crit ) { crit += Boost.Crit.Additive; crit *= Boost.Crit.Multiplicative; }
	public override void ModifyWeaponDamage( Item item, ref StatModifier damage ) { damage += Boost.Damage.Additive; damage *= Boost.Damage.Multiplicative; }
	public override void ModifyWeaponKnockback( Item item, ref StatModifier knockback ) { knockback += Boost.Knockback.Additive; knockback *= Boost.Knockback.Multiplicative; }

	public override void OnHitNPCWithItem( Item item, NPC target, NPC.HitInfo hit, int damageDone ) => Worn.ForEach( x => x.Hit( item, hit, target ) );
	public override void OnHitNPCWithProj( Projectile proj, NPC target, NPC.HitInfo hit, int damageDone ) => Worn.ForEach( x => x.Hit( proj, hit, target, TriggerProjectile.Minion( proj ) ) );

    public override bool FreeDodge( Player.HurtInfo info ) {
		if ( info.Dodgeable && Main.rand.NextFloat() < Boost.Dodge ) {
			Worn.ForEach( x => x.OnDodge() );
			return true;
		}
		return false;
	}

	public override bool PreKill( double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource ) {
		bool die = true;
		foreach ( var i in Worn )
			die &= i.PreKill( damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource );
			
		return die;
	}

    public override bool CanUseItem( Item item ) {
		bool use = true;
		foreach ( var i in Worn )
			use &= i.CanUseItem( item );
			
		return use;
	}

    public override void OnHitNPC( NPC target, NPC.HitInfo hit, int damageDone ) {
		int lifesteal = ( int )( damageDone * Boost.LifeSteal );
		if ( lifesteal != 0 ) {
			Player.statLife = Math.Clamp( Player.statLife + lifesteal, 0, Player.statLifeMax2 );
			Player.HealEffect( lifesteal );
		}

		int manasteal = ( int )( damageDone * Boost.ManaSteal );
		if ( manasteal != 0 ) {
			Player.statMana = Math.Clamp( Player.statMana + manasteal, 0, Player.statManaMax2 );
			Player.ManaEffect( manasteal );
		}
    }
}

internal class TriggerItem : GlobalItem {
    public override void ModifyTooltips( Item item, List< TooltipLine > tooltips ) {
        foreach ( var i in tooltips )
			if ( i.Name == "Speed" )
				i.Text = Bauble.Round( 60.0f / ( item.useTime / Main.LocalPlayer.GetWeaponAttackSpeed( item ) ) ) + " Attacks per second";
			else if ( i.Name == "Knockback" )
				i.Text = Bauble.Round( Main.LocalPlayer.GetWeaponKnockback( item ) ) + " Knockback";
		
		if ( ( item.DamageType.Type > 1 || item.damage > 0 ) && tooltips.All( x => x.Name != "CritChance" ) )
			tooltips.Insert( 1 + tooltips.IndexOf( tooltips.Where( x => x.Name == "Damage" ).FirstOrDefault() ), new( Mod, "CritChance", Bauble.Round( Main.LocalPlayer.GetWeaponCrit( item ) ) + "% Critical Hit Chance" ) );
    }

    public override void SetDefaults( Item item ) {
        if ( item.damage > 0 && item.crit <= 0 )
            item.crit = 4 + item.mana / 12;
        if ( item.axe > 0 || item.pick > 0 || item.hammer > 0 )
            item.attackSpeedOnlyAffectsWeaponAnimation = false;
    }

    public override bool CanConsumeAmmo( Item weapon, Item ammo, Player player ) {
		if ( !player.TryGetModPlayer( out TriggerPlayer TP ) )
			return true;

		bool consume = true;
		foreach ( var i in TP.Worn )
			consume &= i.CanConsume( weapon );
			
		return consume;
    }

	public override bool ConsumeItem( Item item, Player player ) {
		if ( item.damage <= 0 || !player.TryGetModPlayer( out TriggerPlayer TP ) )
			return true;

		bool consume = true;
		foreach ( var i in TP.Worn )
			consume &= i.CanConsume( item );
			
		return consume;
	}
}

internal class TriggerNPC : GlobalNPC {
	internal static void OverCrit( Player player, Item item, ref NPC.HitModifiers modifiers ) {
		if ( item == null || !player.TryGetModPlayer( out TriggerPlayer TP ) )
			return;

		int tier = 0;

		int crit = player.GetWeaponCrit( item );

		float cdamage = TP.Boost.CritDamage;
		while ( Main.rand.Next( 100 ) <= crit ) {
			modifiers.SetCrit();

			modifiers.CritDamage += cdamage;
			modifiers.Knockback *= cdamage;

			crit -= 100;

			tier++;
		}

		if ( tier <= 0 )
			modifiers.DisableCrit();
		else
			foreach ( var i in TP.Worn )
				i.OnCrit( tier );
	
		modifiers.CritDamage -= 1.0f;
	}

	public override void OnKill( NPC npc ) {
		if ( Main.LocalPlayer.whoAmI == npc.lastInteraction && Main.LocalPlayer.TryGetModPlayer( out TriggerPlayer tp ) )
            tp.Worn.ForEach( x => x.KillNPC( npc ) );
	}

	public override void ModifyHitByItem( NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers ) => OverCrit( player, item, ref modifiers );
    public override void ModifyHitByProjectile( NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers ) => OverCrit( Main.player[ projectile.owner ], projectile.GetGlobalProjectile< TriggerProjectile >().item, ref modifiers );
}

internal class TriggerProjectile : GlobalProjectile {
    public override bool InstancePerEntity => true;
    
    internal static bool Minion( Projectile projectile ) => projectile.minionSlots > 0f || projectile.minion || projectile.sentry;

    internal Item item;
    internal Player player;
    public override void OnSpawn( Projectile projectile, IEntitySource source ) {
		if ( !Main.LocalPlayer.TryGetModPlayer( out TriggerPlayer gp ) )
			return;

		bool minion = Minion( projectile );

        if ( source is EntitySource_ItemUse_WithAmmo use && use.Player == Main.LocalPlayer ) {
            item = use.Item;
            player = use.Player;

            gp.Worn.ForEach( x => x.ProjectileSpawn( item, ref projectile, minion ) );
        } else if ( source is EntitySource_Parent { Entity: Projectile parent } && Minion( parent ) && parent.TryGetGlobalProjectile( out TriggerProjectile tp ) && tp.player == Main.LocalPlayer ) {
            item = tp.item;
            player = tp.player;

            gp.Worn.ForEach( x => x.ProjectileSpawn( item, ref projectile, minion ) );
        }

		if ( item != null && player != null && !projectile.minion && !projectile.sentry && !projectile.usesOwnerMeleeHitCD && !ProjectileID.Sets.NoMeleeSpeedVelocityScaling[ projectile.type ] ) {
			float shots = gp.Boost.Multishot;
			
			bool shot = shots > 0.0f;

	        while ( shot && Main.rand.NextFloat() < shots-- ) {
	            Projectile proj = Projectile.NewProjectileDirect(
	                new EntitySource_Parent( projectile ),
	                projectile.position,
	                projectile.velocity.RotateRandom( MathHelper.ToRadians( 15f ) ) * Main.rand.NextFloat( 0.8f, 1.2f ),
	                projectile.type,
	                projectile.damage,
	                projectile.knockBack,
	                projectile.owner,
	                projectile.ai[ 0 ],
	                projectile.ai[ 1 ],
	                projectile.ai[ 2 ]
	            );

				if ( proj.usesIDStaticNPCImmunity ) {
					proj.usesIDStaticNPCImmunity = false;
					proj.usesLocalNPCImmunity = true;
					proj.localNPCHitCooldown = proj.idStaticNPCHitCooldown;
				}
				proj.noDropItem = true;
	        }

	        if ( !shot && -Main.rand.NextFloat() > shots )
				projectile.SetDefaults( ProjectileID.None );
		}
    }
}