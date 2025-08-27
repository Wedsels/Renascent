using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Humanizer;
using Microsoft.Xna.Framework;

using Renascent.content.code.ui;
using Renascent.content.code.mimic;

namespace Renascent.content.code.bauble;

// HAVE THE MIMIC UPGRADES BE BAUBLES, YOU OPEN UP A "UNIQUE" SLOT WHICH ALLOWS YOU TO SELECT WHICH "UNIQUE" BAUBLE TO GO AFTER ONCE UNLOCKED, THEN STARTS A MIMIC QUEST
// AFTER LEVEL ~5? MIMIC APPEARANCE AND PASSIVES ARE TIED TO THE UNIQUES SLOTTED, ONE FOR BODY, ONE FOR LEGS, ONE FOR HEAD

internal abstract class Bauble : ModItem {
	internal static HashSet< Bauble > Instances = [];

	internal static readonly Dictionary< int, List< int > > Baubles = [];
    public override void SetStaticDefaults() {
		int r = Math.Abs( Rarity );
		if ( !Baubles.ContainsKey( r ) )
			Baubles[ r ] = [];
		Baubles[ r ].Add( Type );

		Instances.Add( this );
    }

	internal static Player Player => Main.LocalPlayer;

	public override bool CanRightClick() => Player.TryGetModPlayer( out MimicPlayer MP ) && MP.UnlockedSlots.Any( x => x.IsAir );
	public override void RightClick( Player p ) {
		if ( !p.TryGetModPlayer( out MimicPlayer MP ) )
			return;

		for ( int i = 0; i < MimicPlayer.BaubleMaxSlots; i++ )
			if ( MP.Baubles[ i ].Item.IsAir ) {
				MP.Baubles[ i ].Item = Item.Clone();
				break;
			}
	}

	internal static bool IsPowered( int index ) => Player.TryGetModPlayer( out MimicPlayer MP ) && MP.UnlockedSlotCount - MP.PoweredSlotCount <= index;

	internal static double Round( double value ) => Main.keyState.IsKeyDown( Microsoft.Xna.Framework.Input.Keys.LeftAlt ) ? value : Math.Round( value, 2, MidpointRounding.AwayFromZero );
	internal static string DisplayValue( double value ) => ( value > 0.0 ? "+" : "" ) + Round( value );

	protected abstract object[] TooltipArgs { get; }

    public override void ModifyTooltips( List< TooltipLine > tooltips ) {
		tooltips.ForEach( x => x.Text = x.Text.FormatWith( TooltipArgs ) );
		tooltips.Add( new( Mod, "BaubleLevel", "-" ) );
	}

    public override void PostDrawTooltip( ReadOnlyCollection< DrawableTooltipLine > lines ) {
		int x = 0, y = 0;
		float width = 0;

		foreach ( var i in lines ) {
			float w = FontAssets.MouseText.Value.MeasureString( i.Text ).X;
			if ( w > width )
				width = w;

			if ( i.Name == "BaubleLevel" ) {
				x = i.X;
				y = i.Y;
			}
		}

		if ( x == 0 && y == 0 )
			return;

		string text = ( int )( 100.0f * Roll ) + " / " + ( int )( 100.0f * ( 1.0f + Power ) );
		Vector2 mes = FontAssets.MouseText.Value.MeasureString( text );

		const int space = 14;

		y += 3;
		x -= space;
		width += space * 2;

		Utils.DrawInvBG( Main.spriteBatch, new( x, y, ( int )width, space * 2 ), Color.Black );
		Utils.DrawInvBG( Main.spriteBatch, new( x, y, ( int )( width * _roll ), space * 2 ), Color.Red * UI.Oscillate * 0.25f );
 
		Utils.DrawBorderString( Main.spriteBatch, text, new( x + width / 2.0f - mes.X / 2.0f, y + space - mes.Y / 2.0f + 4 ), lines[ 0 ].Color );
    }

	protected virtual int CreateNPC => -1;
	protected virtual int CreateTile => -1;
	internal virtual int Rarity => ItemRarityID.Blue;
	internal virtual Func< Player, FishingAttempt, bool > FishingBiome => ( Player p, FishingAttempt a ) => false;
	internal virtual int[] NPC => [];
	internal virtual double SpawnChance => 0.01;

	internal int Stacks;
	internal double Timer;

	internal float Power = 0.0f;

	protected int Negative = Main.rand.NextBool() ? 1 : -1;
	private float _roll = ( float )Math.Pow( Main.rand.NextDouble(), 5.0 ) * 0.0946f;
	protected float Roll => _roll + Power;

	internal void Reset() {
		Power = 0.0f;
		OnReset();
	}
	internal virtual void OnReset() {}
	internal virtual void OnDodge() {}
	internal virtual void OnCrit( int tier ) {}

	internal virtual void Update( ref Boost boost ) {}
	internal virtual void Hit( Item item, NPC.HitInfo hitinfo, NPC npc ) {}
	internal virtual void Hit( Projectile proj, NPC.HitInfo hitinfo, NPC npc, bool minion ) {}
	internal virtual void Hurt( ref Player.HurtModifiers modifier ) {}
	internal virtual void KillNPC( NPC npc ) {}
	internal virtual void ProjectileSpawn( Item item, ref Projectile projectile, bool minion ) {}

	internal virtual bool PreKill( double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource ) => true;
	internal virtual bool CanUseItem( Item item ) => true;
	internal virtual bool CanConsume( Item weapon ) => true;

	internal static IEnumerable< NPC > NearbyEnemy( float range ) => Main.npc.Where( x => x.active && x.damage > 0 && !x.friendly && x. WithinRange( Player.Center, range * 16.0f ) );
	internal static IEnumerable< Player > NearbyPlayer( float range ) => Main.player.Where( x => x.active && x.whoAmI != Player.whoAmI && x.WithinRange( Player.Center, range * 16.0f ) );

    public override ModItem Clone( Item newEntity ) {
		newEntity.rare = Rarity;
		newEntity.value = ( int )( 100000.0f * ( _roll * 2.0f ) * ( 5.0f + Math.Abs( Rarity ) ) );

        return base.Clone( newEntity );
    }

	public override void SaveData( TagCompound tag ) {
		tag[ "Stacks" ] = Stacks;
		tag[ "Timer" ] = Timer;
		tag[ "Negative" ] = Negative;
		tag[ "Roll" ] = _roll;
	}

	public override void LoadData( TagCompound tag ) {
		if ( tag.ContainsKey( "Stacks" ) )
			Stacks = tag.GetInt( "Stacks" );
		if ( tag.ContainsKey( "Timer" ) )
			Timer = tag.GetDouble( "Timer" );
		if ( tag.ContainsKey( "Negative" ) )
			Negative = tag.GetInt( "Negative" );
		if ( tag.ContainsKey( "Roll" ) )
			_roll = tag.GetFloat( "Roll" );
	}

	public override void NetSend( BinaryWriter writer ) {
		writer.Write( Stacks );
		writer.Write( Timer );
		writer.Write( Negative );
		writer.Write( _roll );
	}

	public override void NetReceive( BinaryReader reader ) {
		Stacks = reader.ReadInt32();
		Timer = reader.ReadDouble();
		Negative = reader.ReadInt32();
		_roll = reader.ReadSingle();
	}

	public override void SetDefaults() {
		if ( CreateTile > -1 || CreateNPC > -1 ) {
			Item.width = 18;
			Item.height = 18;
			Item.useTime = 10;
			Item.useTurn = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useAnimation = 15;

			Item.makeNPC = CreateNPC;
			Item.createTile = CreateTile;
		}
    }
}