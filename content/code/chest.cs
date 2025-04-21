using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria.GameContent;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace Renascent.content.code;

internal class TrashPlayer  : ModPlayer {
	internal readonly List< Item > Trash = [];

    public override void Load() {
        IL_Main.OnCharacterNamed += context => new ILCursor( context ).EmitDelegate( () => { 
				Mimic.Speak( "I could've come up with better." );
		} );
		
        IL_Main.OnWorldNamed += context => new ILCursor( context ).EmitDelegate( () => { 
				Mimic.Speak( "I could've come up with better." );
		} );
		
        IL_WorldGen.CreateNewWorld += context => new ILCursor( context ).EmitDelegate( () => { 
				Mimic.Speak( "This is gonna take a while." );
		} );
		
		Terraria.UI.ItemSlot.OnItemTransferred += info => {
			if ( info.ToContext != 6 || !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) ) return;
			tp.Trash.Add( Main.LocalPlayer.trashItem.Clone() );
		};
    }

    public override void PostUpdate() {
        if ( Trash.Count > 0 && Trash[ ^1 ].IsAir )
			Trash[ ^1 ] = Main.LocalPlayer.trashItem.Clone();
    }

    public override bool OnPickup( Item item ) {
		Mimic.Speak( "Looks Tasty..." );

        return true;
    }
}

internal static class Mimic {
	private static UI Chest => UI.Display[ typeof( Chest ) ];

	internal static void Digest() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) )
			return;
		
		Main.LocalPlayer.trashItem.TurnToAir();
	
		foreach ( var i in tp.Trash ) {
			Renascent.ChestUpgrade = ( Renascent.ChestUpgrade + 1 ) % Renascent.ChestUpgrades;
			Console.WriteLine( i.value * ( 1 + i.rare ) * i.stack + " :experience points - " + i );
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
		if ( !Chest.Show ) return;

		Terraria.Audio.SoundEngine.PlaySound( SoundID.Zombie30 );

		Vector2 position = Chest.Dim.Center();
		position.Y -= Chest.Height;
		position.X += Chest.Width * Main.rand.NextFloat( 2f ) * ( Main.rand.NextBool() ? -1 : 1 );
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

internal class Chest : UI {
    internal override bool Show => Main.playerInventory || Main.LocalPlayer.dead || Main.ingameOptionsWindow || Main.gameMenu;
	private static readonly Texture2D Chests = Texture( "Mimic_Small" );

    internal override float Width => 32;
    internal override float Height => 46;
    protected override float Left => 0;
    protected override float Top => ScreenHeight + 2;

    protected override int Frames => 8;

    internal override void Hide() {
		if ( Condition )
			Terraria.Audio.SoundEngine.PlaySound( SoundID.ChesterClose, Main.LocalPlayer.Center );
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
	
	internal override double Slow => Frame <= 3.0 ? 6.0 : 16.0 - Frame;

	internal override void Update() {
		if ( !greeting )
			Mimic.Speak( "Didn't leave me with much food.." );
		greeting = true;

		if ( !farewell && Main.ingameOptionsWindow )
			Mimic.Speak( "..Leaving?" );
		farewell = Main.ingameOptionsWindow;

		if ( !death && Main.LocalPlayer.dead )
			Mimic.Speak( "Well done.." );
		death = Main.LocalPlayer.dead;

		bool lift = DragMouse.Y < 0;
		if ( dragging )
			_frame = _frame % 2.0 + 6.0;
		else if ( lift )
			_frame = 7.0;
		else momentum = Vector2.Zero;
		
		if ( !dragging && !lift && Within || Condition ) {
			Terraria.ModLoader.UI.UICommon.TooltipMouseText( "Murmer the Mimic" );

			if ( LClick )
				if ( Condition = !Condition ) {
					Terraria.Audio.SoundEngine.PlaySound( SoundID.ChesterOpen, Main.LocalPlayer.Center );
					close = false;
					open = true;
				} else {
					Terraria.Audio.SoundEngine.PlaySound( SoundID.ChesterClose, Main.LocalPlayer.Center );
					open = false;
					close = true;
				}

			close &= _frame > 0.0;

			if ( close )
				_frame -= 2.0 / Slow;
			else if ( !open )
				_frame = 0;
			else if ( _frame > 2.0 )
				_frame = 2.0;
			
			_frame %= 3.0;
		} else if ( Frame < 3.0 )
			_frame += 3.0;

		Condition &= Main.playerInventory && !lift && !dragging;

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
			momentum = ( UI.Mouse - lastmomentum );
			if ( DragMouse.Y < ScreenHeight * -0.75f || Math.Abs( momentum.X ) > 120 || Math.Abs( momentum.Y ) > 120 )
				Terraria.Audio.SoundEngine.PlaySound( Main.rand.Next( 3 ) switch { 0 => SoundID.Zombie126, 1 => SoundID.Zombie127, _ => SoundID.Zombie128 } );
		}
		lastmomentum = UI.Mouse;

		if ( !lift && !dragging && Frame > 5 )
			momentum.X = Frame * 1.125f;
		
		if ( ( DragMouse += momentum ).Y > 0 ) DragMouse.Y = 0;

		if ( Dim.Left - Dim.Width > ScreenWidth )
			DragMouse.X = -Dim.Width;
		else if ( Dim.Right < 0 )
			DragMouse.X = ScreenWidth + Dim.Width;

		if ( lift && DragMouse.Y == 0 && momentum.Y > 120 ) {
			if ( Main.rand.NextBool( Math.Max( 1, 60 - ( int )momentum.Y ) ) )
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
	}

	internal override void Draw() {
		SB.Draw(
			Chests,
			Dim,
			new( ( int )( Width * Renascent.ChestUpgrade ), ( int )( Height * Frame ), ( int )Width, ( int )Height ),
			Color.White,
			0f,
			Vector2.Zero,
			direction,
			0f
		);

		Lighting.AddLight( ( Dim.Center() + Main.screenPosition ), new Vector3( 2f / Math.Max( Dim.Center().Distance( Main.LocalPlayer.Center - Main.screenPosition ), 100f ) * 100f ) );

		Mimic.DrawSpeak();
    }
}