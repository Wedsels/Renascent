using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

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

		Speak( Main.rand.Next( 5 ) switch {
			0 => "YUM",
			1 => "I've tasted better.",
			2 => "Boring...",
			3 => "MINE",
			_ => "...Thanks",
		} );
	}

	internal class SpeakData( string text, Vector2 position ) {
		internal readonly string Text = text;
		internal Vector2 Position = position;
		internal float Rotation = Main.rand.NextFloat( 0.5f ) * ( Main.rand.NextBool() ? -1 : 1 );
		internal Vector2 Origin = FontAssets.MouseText.Value.MeasureString( text ) / 2;
		internal int Time = 240;
	}
	internal static readonly List< SpeakData > Speaks = [];

	internal static void DrawSpeak() {
		for ( int i = Speaks.Count - 1; i >= 0; i-- ) {
			ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(
				Main.spriteBatch,
				FontAssets.MouseText.Value,
				Speaks[ i ].Text,
				Speaks[ i ].Position - Vector2.One * 2f,
				Color.Black,
				Speaks[ i ].Rotation *= Main.rand.NextBool() ? 1.005f : 0.995f,
				Speaks[ i ].Origin,
				1f,
				SpriteEffects.None,
				0f
			);

			ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(
				Main.spriteBatch,
				FontAssets.MouseText.Value,
				Speaks[ i ].Text,
				Speaks[ i ].Position,
				Color.DarkRed,
				Speaks[ i ].Rotation,
				Speaks[ i ].Origin,
				1f,
				SpriteEffects.None,
				0f
			);

			if ( --Speaks[ i ].Time <= 0 )
				Speaks.RemoveAt( i );
		}
	}

	internal static void Speak( string text ) {
		if ( !UI.Show ) return;

		Terraria.Audio.SoundEngine.PlaySound( SoundID.Zombie30 );

		Vector2 position = UI.Dim.Center();
		position.Y -= UI.Height;
		position.X += UI.Width * Main.rand.NextFloat( 2f ) * ( Main.rand.NextBool() ? -1 : 1 );
		float x = FontAssets.MouseText.Value.MeasureString( text ).X / 2;
		if ( position.X < x )
			position.X = x;
		else if ( position.X > Main.maxScreenW / Main.UIScale - x )
			position.X = Main.maxScreenW / Main.UIScale - x;
		if ( position.Y < 0f )
			position.Y = 10f;
		else if ( position.Y > Main.maxScreenH / Main.UIScale )
			position.Y = Main.maxScreenH / Main.UIScale - 10f;

		Speaks.Add( new( text, position ) );
	}
}