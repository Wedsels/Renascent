using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Renascent.content.code;

internal static class Mimic {
	internal static int Upgrades => 5;

	internal static UI UI => UI.Display[ typeof( MimicUI ) ];

	internal static void Digest() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) )
			return;
	
		Main.LocalPlayer.trashItem.SetDefaults();

		foreach ( var i in tp.Trash ) {
			tp.MimicUpgrade = ( tp.MimicUpgrade + 1 ) % Upgrades;
			Console.WriteLine( ( 1.0 + i.value ) * ( 1.0 + Math.Abs( i.rare ) ) * i.stack + " :experience points - " + i );
		}

		tp.Trash.Clear();

		Speak( "Eat" );
	}

	internal static void Speak( string key ) {
		UICommon.Text( Language.SelectRandom( ( x, _ ) => x.Contains( "Mimic." + key ) ).Value, UI.Dim );

		Terraria.Audio.SoundEngine.PlaySound( SoundID.Zombie30 );
	}
}