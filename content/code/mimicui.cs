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
			Terraria.Audio.SoundEngine.PlaySound( SoundID.ChesterClose );
		Condition = false;

		if ( !Main.LocalPlayer.trashItem.IsAir )
			Mimic.Digest();
	    
		Mimic.Speaks.Clear();
	}

    internal override bool Drag => true;
	private Vector2 momentum = Vector2.Zero;
	private Vector2 lastmomentum = Vector2.Zero;
	private SpriteEffects direction = SpriteEffects.None;

	private bool greeting, farewell, death, open, close;
    
    private int movetype = Main.rand.Next( 3 );
	
	internal override double Slow => Frame < Frames / 2.0 ? 2.0 : 16.0 - Frame;

	internal override void Update() {
		if ( Main.timeForVisualEffects % 6000 == 0 )
			movetype = Main.rand.Next( 3 );
	
		if ( !greeting ) {
			DragMouse.X += Main.rand.NextFloat( ScreenWidth );
			Mimic.Speak( "Didn't leave me with much food.." );
		}
		greeting = true;

		if ( !farewell && Main.ingameOptionsWindow )
			Mimic.Speak( "..Leaving?" );
		farewell = Main.ingameOptionsWindow;

		if ( !death && Main.LocalPlayer.dead )
			Mimic.Speak( "Well done.." );
		death = Main.LocalPlayer.dead;

		bool lift = DragMouse.Y < 0;
		
		if ( Main.playerInventory && !dragging && !lift && Within || ( Condition &= Main.playerInventory && !lift && !dragging ) ) {
			Terraria.ModLoader.UI.UICommon.TooltipMouseText( "Murmer the Mimic" );

			if ( LClick && Within )
				if ( Condition = !Condition ) {
					Terraria.Audio.SoundEngine.PlaySound( SoundID.ChesterOpen );
					close = false;
					open = true;
				} else {
					Terraria.Audio.SoundEngine.PlaySound( SoundID.ChesterClose );
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

		if ( dragging )
			_frame = Frames - 2.0;
		else if ( lift )
			_frame = Frames - 1.0;
		else momentum = Vector2.Zero;

		if ( momentum.Y <= 0 && TrashSlot.Top < Dim.Top && !Main.LocalPlayer.trashItem.IsAir )
			if ( Dim.Intersects( TrashSlot ) )
				Mimic.Digest();
			else {
				Vector2 d = Dim.Center();
				Vector2 t = TrashSlot.Center();
				momentum = d.DirectionTo( t ) * Math.Max( 15f, d.Distance( t ) / 8f );
			}
		else if ( !dragging && lift ) {
			momentum.X /= 1.08f;
			if ( momentum.Y < -2f )
				momentum.Y /= 1.15f;
			else momentum.Y = Math.Max( momentum.Y, 3f ) * 1.15f;
		}
	
		if ( DragMouse.Y < 0 && dragging && !Main.mouseRight ) {
			momentum = ( Mouse - lastmomentum );
			if ( DragMouse.Y < ScreenHeight * -0.75f || Math.Abs( momentum.X ) > 120 || Math.Abs( momentum.Y ) > 120 )
				Terraria.Audio.SoundEngine.PlaySound( Main.rand.Next( 3 ) switch { 0 => SoundID.Zombie126, 1 => SoundID.Zombie127, _ => SoundID.Zombie128 } );
		}
		lastmomentum = Mouse;

		if ( !lift && !dragging && Frame == Frames - 1 && movetype < 2 )
			momentum.X = movetype == 0 ? 8f : -8f;
		
		if ( ( DragMouse += momentum ).Y > 0 ) DragMouse.Y = 0;

		if ( Dim.Left - Dim.Width > ScreenWidth )
			DragMouse.X = -Dim.Width;
		else if ( Dim.Right < 0 )
			DragMouse.X = ScreenWidth + Dim.Width;

		if ( !dragging)
			if ( lift && DragMouse.Y == 0 && momentum.Y > 120 ) {
				movetype = Main.rand.Next( 3 );
				if ( Main.rand.NextBool( Math.Max( 1, 240 - ( int )momentum.Y ) ) )
					Mimic.Speak( Main.rand.Next( 6 ) switch {
						0 => "I do not get fed enough for this...",
						1 => "Now I'm hungry again.",
						2 => "Di..zz.zy",
						3 => "I feel sick",
						4 => "Ouch..",
						_ => "Sto..o..o..p..",
					} );
				Terraria.Audio.SoundEngine.PlaySound( SoundID.Item171 );
				for ( int i = 0; i < 50; i++ )
					Dust.NewDust(
						Dim.TopLeft() + Main.screenPosition,
						Dim.Width,
						Dim.Height,
						i % 4 == 0 ? DustID.Blood : DustID.WoodFurniture
					);
			} else if ( lift && DragMouse.Y == 0 )
				Terraria.Audio.SoundEngine.PlaySound( SoundID.Tink );

		if ( momentum.X > 0 )
			direction = SpriteEffects.FlipHorizontally;
		else if ( momentum.X < 0 )
			direction = SpriteEffects.None;
		else if ( dragging && !Main.gameMenu )
			direction = ( SpriteEffects )( Main.LocalPlayer.direction > 0 ? 1 : 0 );
			
		if ( Condition && momentum != Vector2.Zero ) {
			Terraria.Audio.SoundEngine.PlaySound( SoundID.ChesterClose );
			Condition = open = false;
		}
	}

	internal override void Draw() {
		Vector2 rotation = Vector2.Normalize( DragMouse );
		SB.Draw(
			MimicSmall,
			Dim,
			new( ( int )( Width * TrashPlayer.ChestUpgrade ), ( int )( Height * Frame ), ( int )Width, ( int )Height ),
			Color.White,
			dragging ? 0f : ( float )Math.Atan2( rotation.Y, rotation.X ) * ( direction == SpriteEffects.FlipHorizontally ? 1.0f : -1.0f ),
			Vector2.Zero,
			direction,
			0f
		);

		if ( dragging ) {
			Vector2 Zoom = Dim.Center() / Main.GameZoomTarget + Main.Camera.ScaledPosition;
			Lighting.AddLight( Zoom, new Vector3( TrashPlayer.ChestUpgrade * 5f / ( float )( Math.Pow( Math.Max( 500.0f, Zoom.Distance( Main.LocalPlayer.Center ) ), 2.0 ) / 20000.0 ) ) );
		}

		Mimic.DrawSpeak();
    }
}