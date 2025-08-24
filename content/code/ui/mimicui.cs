using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Renascent.content.code.mimic;

namespace Renascent.content.code.ui;

// LOWER GRAVITY IN SPACE, HEAVIER IN HELL, SCALE GRAVITY BASED ON CURRENT PLAYER Y FROM TOTAL Y

internal class MimicUI : UI {
	internal override bool Show => Main.playerInventory || Main.LocalPlayer.dead || Main.ingameOptionsWindow || Main.gameMenu;
	internal static readonly Texture2D MimicSmall = Textures.Get( "Mimic_Small" );
	internal static readonly Texture2D MimicLarge = Textures.Get( "Mimic_Large" );

    internal override float Width => 60;
    internal override float Height => 60;
    protected override float Left => 0;
    protected override float Top => ScreenHeight + 2;

    protected override float Scale => 0.75f;

    internal override int Frames => 14;

    internal override void Hide() {
		if ( Condition )
			Mimic.Sound( SoundID.ChesterClose );
		Condition = false;

		Main.LocalPlayer.trashItem.SetDefaults();
	}

    internal override bool Drag => true;
	private Vector2 momentum = Vector2.Zero;
	private Vector2 lastmomentum = Vector2.Zero;

	private Vector2 lastdrag = Vector2.Zero;

	private bool greeting, farewell, death, open, close, canhop = true, hop, dragged;

    private enum Moves { Left, Right, Sleep }
    private static ( Moves Any, Moves Walk ) Move => ( ( Moves )Main.rand.Next( 3 ), ( Moves )Main.rand.Next() );
    private Moves movetype = Move.Any;
	
	internal override double Slow => Frame < Frames / 2.0 ? 2.0 : Frame >= Frames - 1 ? 24.0 : 10.0;

	internal override void Update() {
		if ( dragging )
			momentum.X += ( DragMouse - lastdrag ).X * 1.25f;
		lastdrag = DragMouse;

		if ( Within )
			Main.LocalPlayer.mouseInterface = true;

		if ( !open && !dragging && !Within && Main.timeForVisualEffects % 600 == 0 )
			if ( ( movetype = Move.Any ) == Moves.Sleep )
				Mimic.Speak( "Tired" );

		if ( !greeting ) {
			DragMouse.X += Main.rand.NextFloat( ScreenWidth );
			Mimic.Speak( "Greeting" );
			greeting = true;
		}

		if ( !farewell && Main.ingameOptionsWindow && Main.rand.NextFloat() < 0.15f )
			Mimic.Speak( "Parting" );
		farewell = Main.ingameOptionsWindow;

		if ( death && Main.rand.NextFloat() < 0.005f )
			Mimic.Speak( "Loading" );
		if ( !death && Main.LocalPlayer.dead )
			Mimic.Speak( "Death" );
		death = Main.LocalPlayer.dead;
		
		dragged |= dragging;

		bool lift = DragMouse.Y < 0;
		
		if ( Frame == Frames / 2.0 )
			canhop = movetype != Moves.Sleep;
			
		if ( dragging && lift && movetype == Moves.Sleep ) {
			movetype = Move.Walk;
			canhop = true;
		}

		if ( movetype != Moves.Sleep && !dragged && Show && ( canhop || hop ) && Frame >= Frames / 2 + 2 ) {
			canhop = false;
			hop = true;

			float move = 1.1f * Scale + ( Frames - Frame ) / 5f;

			if ( Frame > Frames - 2 )
				momentum.Y -= move;

			if ( Frame > Frames - 2 )
				momentum.X -= movetype == 0 ? move : -move;
		} else hop = false;

		if ( Main.playerInventory && !dragged && Within || Condition ) {
			if ( Within )
				UICommon.Text( Mimic.Localization( "Name" ), new Rectangle( ( int )Mouse.X, ( int )Mouse.Y, 0, 0 ), 0, Color.Yellow, true );

			momentum.X = 0f;

			if ( LClick && Within )
				if ( Condition = !Condition ) {
					Mimic.Sound( SoundID.ChesterOpen );
					close = false;
					open = true;

					Mimic.Gore( () => GoreID.Search.GetId( Main.rand.Next( GoreID.Search.Names.ToList() ) ), Dim.Center(), 10 );
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
		} else if ( movetype == Moves.Sleep )
			_frame = 0.0;
		else if ( Frame < Frames / 2.0 )
			_frame += Frames / 2.0;
		else if ( dragging ) {
			_frame = Frames - 1.0;
			momentum = Vector2.Zero;
		} else if ( lift && !hop && !canhop && dragged )
			_frame = Frames - 1.0;

		if ( momentum.Y <= 0 && TrashSlot.Top < Dim.Top && !Main.LocalPlayer.trashItem.IsAir )
			if ( Dim.Intersects( TrashSlot ) )
				Main.LocalPlayer.trashItem.SetDefaults();
			else {
				dragged = true;
				movetype = Move.Walk;
				Vector2 d = Dim.Center();
				Vector2 t = TrashSlot.Center();
				momentum = d.DirectionTo( t ) * Math.Max( 15f, d.Distance( t ) / 8f );
			}

		momentum.X /= 1.04f;
		if ( Math.Abs( momentum.X ) < 0.5f )
			momentum.X = 0.0f;

		if ( momentum.Y != 0.0f ) {
			momentum.Y /= 1.12f;
			momentum.Y += 0.5f;
			if ( momentum.Y > 0.0f )
				momentum.Y *= 1.33f;
		}

		if ( DragMouse.Y < 0 && dragging && !Main.mouseRight ) {
			momentum = Mouse - lastmomentum;
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
			DragMouse.X = ScreenWidth;

		if ( !dragging && lift && DragMouse.Y == 0 ) {
			if ( momentum.Y > 200f ) {
				movetype = Move.Any;
				if ( Main.rand.NextBool( Math.Max( 1, 180 - ( int )momentum.Y / 4 ) ) )
					Mimic.Speak( "Fall" );
				Mimic.Sound( SoundID.Item171 );
			} else
				Mimic.Sound( SoundID.Tink );

			Mimic.Gore( () => Main.rand.Next( 166, 175 ), new( Dim.Left + Width / 2.0f, Dim.Bottom ), ( int )Math.Abs( momentum.Y ) / 4 );

			canhop = movetype != Moves.Sleep;
		}

		if ( DragMouse.Y == 0 ) {
			momentum.Y = 0f;
			dragged = false;
		}

		if ( Condition && ( momentum != Vector2.Zero || !Main.playerInventory || dragged ) ) {
			Mimic.Sound( SoundID.ChesterClose );
			Condition = false;
			open = false;
			close = true;
		}

		if ( movetype == Moves.Sleep && ( Dim.Left <= 0f || Dim.Right >= ScreenWidth ) )
			movetype = Move.Walk;
	}

	internal override void EarlyDraw() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out MimicPlayer MP ) && !Main.gameMenu )
			return;

		bool right = momentum.X > 0;
		float rotation = dragging ? 0.0f : momentum.Y * 0.1f * MathHelper.PiOver4 * ( right ? 1.0f : -1.0f );

		SB.Draw(
			MimicLarge,
			Dim.Center(),
			new( ( int )( Width * ( Main.gameMenu ? Mimic.LastUpgrade : MP.MimicUpgrade ) ), ( int )( Height * Frame ), ( int )Width, ( int )Height ),
			Color.White,
			rotation,
			new Vector2( Width / 2.0f, Height / 2.0f ),
			Scale,
			right ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
			0f
		);

		if ( dragging ) {
			Vector2 Zoom = Vector2.Transform( Dim.Center() + Main.screenPosition, Main.GameViewMatrix.ZoomMatrix ) / Main.UIScale;
			Lighting.AddLight( Zoom, new Vector3( Mimic.LastUpgrade * 5f / ( float )( Math.Pow( Math.Max( 500.0f, Zoom.Distance( Main.LocalPlayer.Center ) ), 2.0 ) / 20000.0 ) ) );
		}
    }
}