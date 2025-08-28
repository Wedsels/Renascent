using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

using Renascent.content.code.ui;

namespace Renascent.content.code.mimic;

internal static class Mimic {
	internal static ref int LastUpgrade => ref Renascent.Client.LastMimicUpgrade;

	// if ( evolution < Evolution.Hardmode ) scale *= 0.5
	internal enum Evolution { Wood, Gold, Ice, Demonic, Size }

	// private static readonly int Upgrades = ( int )( MimicUI.MimicLarge.Size().X / UI.UIs[ typeof( MimicUI ) ].Width );

	internal static UI UI => UI.UIs[ typeof( MimicUI ) ];

// switch ( Main.rand.Next( 4 ) ) {
// 	case 0: Sound( SoundID.Zombie84 ); break;
// 	case 1: Sound( SoundID.Zombie85 ); break;
// 	case 2: Sound( SoundID.Zombie86 ); break;
// 	case 3: Sound( SoundID.Zombie87 ); break;
// }

// MP.MimicUpgrade = ( MP.MimicUpgrade + 1 ) % Upgrades;

	internal static double StartConsume;

	internal static void Consume( Item item ) {
		if ( !Main.LocalPlayer.TryGetModPlayer( out MimicPlayer MP ) )
			return;

		if ( item.IsAir )
			return;

		MP.Trash.Add( item.Clone() );

		StartConsume = Main.timeForVisualEffects;
	}

	internal static void Digest() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out MimicPlayer MP ) )
			return;

		StartConsume = 0.0;
		double xp = 0.0f;

		foreach ( var i in MP.Trash )
			xp += 1.0 + ( 1.0 + i.value ) * ( 1.0 + Math.Abs( i.rare ) ) * i.stack * ( 0.5 + Main.rand.NextDouble() * 2.0 );
		MP.Trash.Clear();

		Microsoft.Xna.Framework.Rectangle dest;

		if ( !Main.playerInventory )
		 	dest = Main.LocalPlayer.Hitbox;
		else
			dest = new( UI.Dim.X + ( int )Main.screenPosition.X, UI.Dim.Y + ( int )Main.screenPosition.Y, UI.Dim.Width, UI.Dim.Height );

		CombatText.NewText( dest, Microsoft.Xna.Framework.Color.Aqua, ( int )xp + " XP!", true, true );

		Speak( "Eat" );
	}

	internal static string Localization( string key ) => Language.SelectRandom( ( x, _ ) => x.Contains( "Murmur." + key ) ).Value;

	internal static void Speak( string key ) {
		if ( !Main.playerInventory )
		 	return;

		UICommon.Text( Localization( key ), UI.Dim );
		Sound( SoundID.Zombie30 );
	}

	internal static void Sound( SoundStyle style ) {
		if ( Main.gameInactive ) return;

		style.PitchVariance = 0.5f;
		style.SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest;
		SoundEngine.PlaySound( style );
	}

	internal static void Gore( Func< int > type, Microsoft.Xna.Framework.Vector2 position, int maxcount ) {
		int rgore = Main.rand.Next( maxcount );
		for ( int i = 0; i < rgore; i++ ) {
			int a = Terraria.Gore.NewGore(
				null,
				position * Main.UIScale + Main.screenPosition,
				new( Main.rand.NextFloat( -6.2f, 6.2f ), Main.rand.NextFloat( -6.2f, 0.0f ) ),
				type(),
				Main.rand.NextFloat()
			);

			Main.gore[ a ].sticky = false;
			Main.gore[ a ].light = float.Epsilon;
		}
	}
}