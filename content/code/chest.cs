using System;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Renascent.content.code;

internal class Chest : UI {
    internal override bool Show => Main.playerInventory;
	internal static Texture2D Chests = Texture( "Chests" );
	internal static Texture2D ChestsSnow = Texture( "Chests_Snow" );

    internal override int Width => 48;
    internal override int Height => 32;
    internal override int Left => ( int )( ( Main.screenWidth / Scale - 19 - Width / 2 ) * Scale );
    internal override int Top => ( int )( ( 405 - Height / 2 ) * Scale );

	protected override int Slow => 10;

    protected override int Frames => 5;

	internal bool close;

    internal override void Hide() {
		if ( Condition )
			Terraria.Audio.SoundEngine.PlaySound( Terraria.ID.SoundID.ChesterClose, Main.LocalPlayer.Center );
		Condition = close = false;
	}

    internal override void Draw() {
		int x = 0;

		if ( Within ) {
			x = Frame;

			if ( LClick ) {
				FrameCount = 0;

				if ( Condition = !Condition )
					Terraria.Audio.SoundEngine.PlaySound( Terraria.ID.SoundID.ChesterOpen, Main.LocalPlayer.Center );
				else {
					Condition = close = true;
					Terraria.Audio.SoundEngine.PlaySound( Terraria.ID.SoundID.ChesterClose, Main.LocalPlayer.Center );
				}
			}

			if ( RClick ) {
				Terraria.Audio.SoundEngine.PlaySound( Terraria.ID.SoundID.AchievementComplete );
				PopupText.NewText( new() {
						Velocity = new( 0f, -5f ),
						Color = Color.Aqua,
						Text = "Chest Upgraded!",
						DurationInFrames = 60
					},
					Main.LocalPlayer.Top
				);

				Renascent.ChestUpgrade = ( Renascent.ChestUpgrade + 1 ) % Renascent.ChestUpgrades;
			}
		}

		if ( close ) {
			if ( ( x = Frames - Frame - 2 ) == 0 )
				Condition = close = false;
		} else if ( Condition && FrameCount >= ( Frames - 1 ) * Slow )
			x = Frames - 1;

		SB.Draw(
			Renascent.ChestUpgrade % 2 == 0 ? Chests : ChestsSnow,
			new Vector2( Left, Top ),
			new Rectangle( x * Width, ( Renascent.ChestUpgrade / 2 * 2 + Condition.ToInt() ) * Height, Width, Height ),
			Color.White,
			0f,
			Vector2.Zero,
			Scale,
			SpriteEffects.None,
			0f
		);
    }
}