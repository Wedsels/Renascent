using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;

namespace Renascent.content.code;

internal class MimicUI : UI {
    internal override bool Show => Main.playerInventory || Main.LocalPlayer.dead || Main.ingameOptionsWindow || Main.gameMenu;
	private static readonly Texture2D MimicSmall = Texture( "Mimic_Small" );
	private static readonly Texture2D MimicLarge = Texture( "Mimic_Large" );

    internal override float Width => 32;
    internal override float Height => 46;
    protected override float Left => 0;
    protected override float Top => ScreenHeight + 2;

    protected override int Frames => 10;

    internal override void Hide() {
		if ( Condition )
			Mimic.Sound( SoundID.ChesterClose );
		Condition = false;

		if ( !Main.LocalPlayer.trashItem.IsAir )
			Mimic.Digest();
	}

    internal override bool Drag => true;
	private Vector2 momentum = Vector2.Zero;
	private Vector2 lastmomentum = Vector2.Zero;
	private SpriteEffects direction = SpriteEffects.None;

	private bool greeting, farewell, death, open, close, canhop = true, hop, dragged;
    
    private int movetype = Main.rand.Next( 3 );
	
	internal override double Slow => Frame < Frames / 2.0 ? 2.0 : Frame >= Frames - 1 ? 18.0 : 10.0;

	internal override void Update() {
		if ( Main.timeForVisualEffects % 600 == 0 )
			movetype = Main.rand.Next( 3 );
	
		if ( !greeting ) {
			DragMouse.X += Main.rand.NextFloat( ScreenWidth );
			Mimic.Speak( "Greeting" );
		}
		greeting = true;

		if ( !farewell && Main.ingameOptionsWindow )
			Mimic.Speak( "Parting" );
		farewell = Main.ingameOptionsWindow;

		if ( !death && Main.LocalPlayer.dead )
			Mimic.Speak( "Death" );
		death = Main.LocalPlayer.dead;
		
		dragged |= dragging;

		bool lift = DragMouse.Y < 0;
		
		if ( Frame == Frames / 2.0 + 1.0 )
			canhop = movetype < 2;
			
		if ( dragging && lift && movetype == 2 ) {
			movetype = Main.rand.Next( 2 );
			canhop = true;
		}

		if ( movetype < 2 && !dragged && Show && ( canhop || hop ) && Frame >= Frames - 3 ) {
			canhop = false;
			hop = true;

			if ( Frame > Frames - 2 )
				momentum.Y -= 0.85f;
			
			if ( Frame > Frames - 2 )
				momentum.X -= movetype == 0 ? 1.1f : -1.1f;
		} else hop = false;
		
		if ( Main.playerInventory && !dragged && Within || ( Condition &= Main.playerInventory && !dragged ) ) {
			if ( Within)
				UICommon.Text( Terraria.Localization.Language.GetTextValue( "Mods.Renascent.Mimic.Name" ), new Rectangle( ( int )Mouse.X, ( int )Mouse.Y, 0, 0 ), 0, Color.Yellow );
			
			momentum.X = 0f;

			if ( LClick && Within )
				if ( Condition = !Condition ) {
					Mimic.Sound( SoundID.ChesterOpen );
					close = false;
					open = true;
				} else {
					Mimic.Sound( SoundID.ChesterClose );
					open = false;
					close = true;
				}

			close &= _frame > 0.0;

			if ( close )
				_frame -= 2.0 / Slow;
			else if ( !open )
				_frame = 0.0;
			else if ( _frame > Frames / 2.0 - 1.0 )
				_frame = Frames / 2.0 - 1.0;
			
			_frame %= Frames / 2.0;
		} else if ( movetype == 2 )
			_frame = 0.0;
		else if ( Frame < Frames / 2.0 )
			_frame += Frames / 2.0;
		else if ( dragging ) {
			_frame = Frames - 2.0;
			momentum = Vector2.Zero;
		} else if ( lift && !hop && !canhop && dragged )
			_frame = Frames - 1.0;

		if ( momentum.Y <= 0 && TrashSlot.Top < Dim.Top && !Main.LocalPlayer.trashItem.IsAir )
			if ( Dim.Intersects( TrashSlot ) )
				Mimic.Digest();
			else {
				dragged = true;
				movetype = Main.rand.Next();
				Vector2 d = Dim.Center();
				Vector2 t = TrashSlot.Center();
				momentum = d.DirectionTo( t ) * Math.Max( 15f, d.Distance( t ) / 8f );
			}
			
		momentum.X /= 1.04f;
		if ( Math.Abs( momentum.X ) < 0.5f )
			momentum.X = 0f;

		if ( momentum.Y != 0f ) {
			momentum.Y /= 1.12f;
			momentum.Y += 0.5f;
			if ( momentum.Y > 0f )
				momentum.Y *= 1.33f;
		}
	
		if ( DragMouse.Y < 0 && dragging && !Main.mouseRight ) {
			momentum = ( Mouse - lastmomentum );
			if ( DragMouse.Y < ScreenHeight * -0.75f || Math.Abs( momentum.X ) > 120 || Math.Abs( momentum.Y ) > 120 )
				Mimic.Sound( Main.rand.Next( 3 ) switch { 0 => SoundID.Zombie126, 1 => SoundID.Zombie127, _ => SoundID.Zombie128 } );
		}
		lastmomentum = Mouse;
		
		if ( ( DragMouse += momentum ).Y > 0 ) DragMouse.Y = 0;

		if ( dragging && momentum.Y == 0 )
			momentum.Y = 1f;

		if ( Dim.Left - Dim.Width > ScreenWidth )
			DragMouse.X = -Dim.Width;
		else if ( Dim.Right < 0 )
			DragMouse.X = ScreenWidth + Dim.Width;

		if ( !dragging && lift && DragMouse.Y == 0 ) {
			if ( momentum.Y > 200f ) {
				movetype = Main.rand.Next( 3 );
				if ( Main.rand.NextBool( Math.Max( 1, 240 - ( int )momentum.Y ) ) )
					Mimic.Speak( "Fall" );
				Mimic.Sound( SoundID.Item171 );
				for ( int i = 0; i < 50; i++ )
					Dust.NewDust(
						Dim.TopLeft() + Main.screenPosition,
						Dim.Width,
						Dim.Height,
						i % 4 == 0 ? DustID.Blood : DustID.WoodFurniture
					);
			} else
				Mimic.Sound( SoundID.Tink );

			canhop = movetype < 2;
		}

		if ( DragMouse.Y == 0 ) {
			momentum.Y = 0f;
			dragged = false;
		}

		if ( momentum.X > 0 )
			direction = SpriteEffects.FlipHorizontally;
		else if ( momentum.X < 0 )
			direction = SpriteEffects.None;
		else if ( dragging && !Main.gameMenu )
			direction = ( SpriteEffects )( Main.LocalPlayer.direction > 0 ? 1 : 0 );
			
		if ( Condition && momentum != Vector2.Zero ) {
			Mimic.Sound( SoundID.ChesterClose );
			Condition = open = false;
		}
		
		if ( movetype == 2 && ( Dim.Left <= 0f || Dim.Right >= ScreenWidth ) )
			movetype = Main.rand.Next( 2 );
	}

	internal override void Draw() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) && !Main.gameMenu )
			return;

		Vector2 rotation = Vector2.Normalize( DragMouse );
		SB.Draw(
			MimicSmall,
			Dim,
			new( ( int )( Width * ( Main.gameMenu ? Renascent.LastUpgrade : tp.MimicUpgrade ) ), ( int )( Height * Frame ), ( int )Width, ( int )Height ),
			Color.White,
			dragging || hop ? 0f : ( float )Math.Atan2( rotation.Y, rotation.X ) * ( direction == SpriteEffects.FlipHorizontally ? 1.0f : -1.0f ),
			Vector2.Zero,
			direction,
			0f
		);

		if ( dragging ) {
			Vector2 Zoom = Vector2.Transform( Dim.Center() + Main.screenPosition, Main.GameViewMatrix.ZoomMatrix ) / Main.UIScale;
			Lighting.AddLight( Zoom, new Vector3( Renascent.LastUpgrade * 5f / ( float )( Math.Pow( Math.Max( 500.0f, Zoom.Distance( Main.LocalPlayer.Center ) ), 2.0 ) / 20000.0 ) ) );
		}
    }
}