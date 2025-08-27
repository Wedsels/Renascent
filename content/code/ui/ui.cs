using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;

using Renascent.content.code.bauble;

namespace Renascent.content.code.ui;

internal readonly struct Textures {
	internal static Texture2D Get( string name ) => ModContent.Request< Texture2D >( "Renascent/content/texture/" + name, ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;

    internal static readonly Texture2D Intestine = ModContent.Request< Texture2D >( "Terraria/Images/Gore_315", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D TinyHeart = ModContent.Request< Texture2D >( "Terraria/Images/Gore_331", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D Heart = ModContent.Request< Texture2D >( "Terraria/Images/UI/WorldCreation/IconEvilCrimson", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D Accessory = ModContent.Request< Texture2D >( "Terraria/Images/UI/CharCreation/Randomize", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D Recycle = ModContent.Request< Texture2D >( "Terraria/Images/UI/CharCreation/HairStyle_Arrow", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D Border = ModContent.Request< Texture2D >( "Terraria/Images/UI/CharCreation/CategoryPanelBorder", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D Chest = ModContent.Request< Texture2D >( "Terraria/Images/UI/CharCreation/ColorShirt", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D Head = ModContent.Request< Texture2D >( "Terraria/Images/UI/CharCreation/ColorEyeBack", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
    internal static readonly Texture2D Leg = ModContent.Request< Texture2D >( "Terraria/Images/UI/CharCreation/ColorPants", ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;
};

internal readonly struct Colors {
    internal static readonly Color Default = new Color( 63, 65, 151, 255 ) * 0.785f;
    internal static readonly Color Red = new Color( 151, 65, 63, 255 ) * 0.785f;
    internal static readonly Color Vanity = new Color( 65, 151, 63, 255 ) * 0.785f;
    internal static readonly Color Unique = new Color( 63, 151, 151, 255 ) * 0.785f;
};

internal class UICommon : ModSystem {
	public override void PostSetupContent() {
		foreach ( var i in UI.UIs.Values )
			i.Initialize();

        IL_Main.DoUpdate += context => {
			var cursor = new ILCursor( context );

			if ( cursor.TryGotoNext( i => i.MatchStsfld( typeof( Main ), nameof( Main.hasFocus ) ) ) )
				cursor.EmitDelegate( () => {
					foreach ( var i in UI.UIs.Values ) {
						i._frame = ( i._frame + 1.0 / i.Slow ) % i.Frames;

						i.Update();

						if ( !i.dragging && !i.Within )
							continue;
							
						if ( i.dragging )
							Main.LocalPlayer.mouseInterface = true;

						if ( i.Drag && Main.mouseRight && Main.LocalPlayer.mouseInterface ) {
							i.dragging = true;
							if ( i.StartDrag == Vector2.Zero )
								i.StartDrag = UI.Mouse - i.DragMouse;
							i.DragMouse = UI.Mouse - i.StartDrag;

							if ( i.Dim.Left < 0 )
								i.DragMouse.X -= i.Dim.Left;
							else if ( i.Dim.Left > i.ScreenWidth )
								i.DragMouse.X -= i.Dim.Left - i.ScreenWidth;

							if ( i.Dim.Top < 0 )
								i.DragMouse.Y -= i.Dim.Top;
							else if ( i.Dim.Top > i.ScreenHeight )
								i.DragMouse.Y -= i.Dim.Top - i.ScreenHeight;
						} else {
							i.StartDrag = Vector2.Zero;
							i.dragging = false;
						}
					}
				} );
		};

		IL_Main.DrawInterface_41_InterfaceLogic4 += context => {
			var cursor = new ILCursor( context );

			cursor.Goto( cursor.Instrs.Count - 1 );
			cursor.EmitDelegate( () => {
				if ( UI.ShiftHover )
					Main.cursorOverride = 7;
			} );
		};

        IL_Main.DrawThickCursor += context => {
			var cursor = new ILCursor( context );
  
			cursor.EmitDelegate( () => {
				UI.Mouse = Main.MouseScreen;

				if ( Main.gameMenu ) {
					Main.spriteBatch.End();
					Main.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix );
	
					foreach ( var i in UI.UIs.Values )
						if ( i.Show )
							i.EarlyDraw();
						else
							i.Hide();
				}
  
				DrawText();
			} );
		};

        IL_Main.DrawInterface_36_Cursor += context => {
			var cursor = new ILCursor( context );

			if ( cursor.TryGotoNext(
		        x => x.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_R4 && ( float )x.Operand == 0f,
		        x => x.OpCode.Name.StartsWith( "stloc" )
			) ) cursor.EmitDelegate( () => {
				UI.Mouse = Main.MouseScreen;
				UI.ShiftHover = UI.ShiftHover && Main.keyState.PressingShift();

				if ( Main.gameMenu ) {
					Main.spriteBatch.End();
					Main.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix );

					foreach ( var i in UI.UIs.Values )
						if ( i.Show )
							i.EarlyDraw();
						else
							i.Hide();
				}

				DrawText();
			} );
		};

		IL_Main.DrawGore += context => {
			var cursor = new ILCursor( context );

			cursor.EmitDelegate( () => {
				Main.spriteBatch.End();
				Main.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix );

				foreach ( var i in UI.UIs.Values )
					if ( i.Show )
						i.EarlyDraw();
					else
						i.Hide();

				Main.spriteBatch.End();
				Main.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform );
			} );
		
			int index = -1;
			if ( cursor.TryGotoNext( i => i.MatchLdcI4( 0 ), i => i.MatchStloc( out index ) ) )
				while ( cursor.TryGotoNext( MoveType.After, i => i.MatchCallvirt( typeof( Gore ), nameof( Gore.GetAlpha ) ) ) ) {
					cursor.Emit( Mono.Cecil.Cil.OpCodes.Ldloc, index );
					cursor.EmitDelegate< Func< Color, int, Color > >( ( original, i ) => {
						if ( Main.gore[ i ].light == float.Epsilon )
							return Color.White;
						return original;
					} );
				}
		};

		IL_Main.DrawInventory += context => {
			var cursor = new ILCursor( context );
  
			cursor.EmitDelegate( () => {
				foreach ( var i in UI.UIs.Values )
					if ( i.Show )
						i.Draw();
			} );
		};
    }

    private class TextData( string text, Vector2 position, Color color, int time, bool tip ) {
		internal readonly string Text = text;
		internal Vector2 Position = position;
		internal readonly Vector2 Origin = FontAssets.MouseText.Value.MeasureString( text ) / 2.0f;
		internal int Time = time;
		internal readonly Color Color = color;
		internal readonly bool Tip = tip;

		public override int GetHashCode() => Text.GetHashCode();
		public override bool Equals( object obj ) => obj is TextData data && data.Text == Text;
	}
	private static readonly HashSet< TextData > Texts = [];

	private static Vector2 Bounds( string txt, Vector2 pos ) {
		Vector2 size = FontAssets.MouseText.Value.MeasureString( txt ) / 2.0f;
		if ( pos.X < size.X )
			pos.X = size.X;
		else if ( pos.X > Main.maxScreenW / Main.UIScale - size.X )
			pos.X = Main.maxScreenW / Main.UIScale - size.X;
		if ( pos.Y < size.Y )
			pos.Y = size.Y;
		else if ( pos.Y > Main.maxScreenH / Main.UIScale - size.Y )
			pos.Y = Main.maxScreenH / Main.UIScale - size.Y;
		return pos;
	}

	internal static void Text( string text, Rectangle target, int time = 240, Color color = default, bool tip = false ) {
		if ( time > 0 )
			time += text.Length * 2;
		else time = 10;

		Vector2 position = target.Center();
		position.Y -= target.Height;
		position.X += target.Width * Main.rand.NextFloat( 2f ) * ( Main.rand.NextBool() ? -1 : 1 );
			
		if ( Texts.TryGetValue( new ( text, Vector2.Zero, color, time, tip ), out var data ) ) {
			data.Time = time;
			if ( position.Distance( data.Position ) > 30f )
				data.Position = position;
		} else
			Texts.Add( new( text, position, color == default ? Color.Yellow : color, time, tip ) );
	}

	private static void DrawText() {
		foreach ( var i in Texts.Reverse() ) {
			Vector2 measure = FontAssets.MouseText.Value.MeasureString( i.Text );
			float ran = 1.0f + ( float )Math.Sin( Main.timeForVisualEffects * 0.05f ) * 0.015f * ( 200.0f / measure.X );
			measure *= ran;

			Vector2 position = new( Main.rand.NextFloat( -0.1f, 0.1f ) );
			if ( i.Tip ) {
				position += UI.Mouse;
				position.X += measure.X / 2.0f + 25f;
				position.Y += measure.Y / 2.0f;
			} else
				position += i.Position;
			position = Bounds( i.Text, position );
	
			UI.DrawInv(
				( int )( position.X - measure.X / 2.0f - 8.0f ),
				( int )( position.Y - measure.Y / 2.0f - 2.0f ),
				( int )( measure.X + 8.0f * 2.0f ),
				( int )measure.Y,
				ran - 1.0f
			);

			ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(
				Main.spriteBatch,
				FontAssets.MouseText.Value,
				i.Text,
				position + new Vector2( -2f, 2f ),
				Color.Black,
				ran - 1.0f,
				i.Origin,
				ran,
				SpriteEffects.None,
				0f
			);

			ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(
				Main.spriteBatch,
				FontAssets.MouseText.Value,
				i.Text,
				position,
				i.Color * UI.Oscillate,
				ran - 1.0f,
				i.Origin,
				ran,
				SpriteEffects.None,
				0f
			);

			if ( --i.Time <= 0 )
				Texts.Remove( i );
		}
	}
}

internal abstract class UI : ModType {
	protected sealed override void Register() {
		// ModTypeLookup< UI >.Register( this );
        UIs[ GetType() ] = this;
	}

    internal static readonly Dictionary< Type, UI > UIs = [];

	internal static float Oscillate => Main.mouseTextColor / 255f;

	internal static bool ShiftHover;

	internal virtual void Initialize() {}
	internal virtual void Update() {}
	internal virtual void Draw() {}
	internal virtual void EarlyDraw() {}
	internal virtual void Hide() {}
    internal virtual bool Show => false;

	protected static SpriteBatch SB => Main.spriteBatch;

	internal float ScreenWidth => Main.maxScreenW / Main.UIScale - Width;
	internal float ScreenHeight => Main.maxScreenH / Main.UIScale - Height;

	internal static bool LClick => Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseLeft;
	internal static bool RClick => Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseRight;
	internal static bool MClick => Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseMiddle;
	internal static int MScroll => Math.Clamp( Terraria.GameInput.PlayerInput.ScrollWheelDelta, -1, 1 );

	internal virtual double Slow => 3;
	internal virtual int Frames => 1;
	internal double _frame;
	protected int Frame => ( int )_frame % Frames;

	internal virtual bool Condition { get; set; }

	internal virtual bool Drag => false;
	internal Vector2 DragMouse = Vector2.Zero;
	internal Vector2 StartDrag = Vector2.Zero;
	internal bool dragging;

	internal static Vector2 Mouse;

	internal virtual float Width => 0;
	internal virtual float Height => 0;
	protected virtual float Left => 0;
	protected virtual float Top => 0;
	protected virtual float Scale => 1.0f;
	internal Rectangle Dim => new( ( int )( Left + DragMouse.X + Width - Width * Scale ), ( int )( Top + DragMouse.Y + Height - Height * Scale ), ( int )( Width * Scale ), ( int )( Height * Scale ) );
	internal bool Within => Dim.Contains( Mouse.ToPoint() );

	internal static Rectangle TrashSlot { get {
		int num = 448;
		int num2 = 258;
		if ( ( Main.LocalPlayer.chest != -1 || Main.npcShop > 0 ) && !Main.recBigList ) {
			num2 += 168;
			num += 5;
		} else if ( ( Main.LocalPlayer.chest == -1 || Main.npcShop == -1 ) && Main.trashSlotOffset != Terraria.DataStructures.Point16.Zero ) {
			num += Main.trashSlotOffset.X;
			num2 += Main.trashSlotOffset.Y;
		}

		return new Rectangle( num, num2, ( int )( TextureAssets.InventoryBack.Width() * Main.inventoryScale ), ( int )( TextureAssets.InventoryBack.Height() * Main.inventoryScale ) );
	} }

	internal static void DrawInv( Rectangle r, float rotation = 0.0f, Color c = default ) => DrawInv( r.X, r.Y, r.Width, r.Height, rotation, c );
	internal static void DrawInv( float fx, float fy, float size, float rotation = 0.0f, Color c = default ) => DrawInv( fx, fy, size, size, rotation, c );
	internal static void DrawInv( float fx, float fy, float fw, float fh, float rotation = 0.0f, Color c = default ) {
		const int size = 20;

		int req = size * 2;
		if ( fw < req ) {
			fx -= ( req - fw ) / 2.0f;
			fw = req;
		}
		if ( fh < req ) {
			fy -= ( req - fh ) / 2.0f;
			fh = req;
		}

		int x = ( int )fx;
		int y = ( int )fy;
		int w = ( int )fw;
		int h = ( int )fh;

		if ( c == default )
			c = Colors.Default;

		Texture2D bg = TextureAssets.InventoryBack13.Value;

		Vector2 center = new( x + w / 2f, y + h / 2f );

		for ( int i = 0; i < 9; i++ ) {
			Rectangle src = i switch {
				0 => new( 0, 0, size, size ),
				1 => new( size, 0, bg.Width - size * 2, size ),
				2 => new( bg.Width - size, 0, size, size ),

				3 => new( 0, size, size, bg.Height - size * 2 ),
				4 => new( size, size, bg.Width - size * 2, bg.Height - size * 2 ),
				5 => new( bg.Width - size, size, size, bg.Height - size * 2 ),

				6 => new( 0, bg.Height - size, size, size ),
				7 => new( size, bg.Height - size, bg.Width - size * 2, size),
				8 => new( bg.Width - size, bg.Height - size, size, size ),

				_ => Rectangle.Empty
			};

			Rectangle dst = i switch {
				0 => new( x, y, size, size ),
				1 => new( x + size, y, w - size * 2, size ),
				2 => new( x + w - size, y, size, size ),

				3 => new( x, y + size, size, h - size * 2 ),
				4 => new( x + size, y + size, w - size * 2, h - size * 2 ),
				5 => new( x + w - size, y + size, size, h - size * 2 ),

				6 => new( x, y + h - size, size, size ),
				7 => new( x + size, y + h - size, w - size * 2, size ),
				8 => new( x + w - size, y + h - size, size, size ),

				_ => Rectangle.Empty
			};

			Vector2 offset = new( dst.X + dst.Width / 2f - x - w / 2f, dst.Y + dst.Height / 2f - y - h / 2f );

			float cos = ( float )Math.Cos( rotation );
			float sin = ( float )Math.Sin( rotation );
			Vector2 roff = new( offset.X * cos - offset.Y * sin, offset.X * sin + offset.Y * cos );

			SB.Draw(
				bg,
				center + roff,
				src,
				c,
				rotation,
				new Vector2( src.Width, src.Height ) / 2f,
				new Vector2( dst.Width / ( float )src.Width, dst.Height / ( float )src.Height ),
				SpriteEffects.None,
				0f
			);
		}
	}
}

internal readonly struct Buttons {
	private static int Within( float left, float top, float width, float height, ref Color color, string hover = null ) {
		if ( Main.MouseScreen.Between( new( left, top ), new( left + width, top + height ) ) ) {
			Main.LocalPlayer.mouseInterface = true;

			if ( hover is not null )
				UICommon.Text( hover, new Rectangle( ( int )UI.Mouse.X, ( int )UI.Mouse.Y, 0, 0 ), 0, Color.Yellow, true );

			if ( hover is not null && color != default )
				color = Color.Lerp( color, Color.Black, 0.3f );

			if ( Main.mouseLeft && Main.mouseLeftRelease )
				return 0;
			if ( Main.mouseRight && Main.mouseRightRelease )
				return 1;
			if ( Main.mouseMiddle && Main.mouseMiddleRelease )
				return 2;

			return 3;
		}

		return -1;
	}

	// internal static int TextButton( string text, float left, float top, Color color = default, float scale = 0.75f, string hover = null, bool shift = false ) {
	// 	if ( color == default )
	// 		color = Color.White;

	// 	Vector2 size = Common.Measure( text ) * scale;

	// 	int ret = Within( left, top, size.X, size.Y, ref color, hover );

	// 	Text( text, left, top, color, scale, shift );

	// 	return ret;
	// }

	internal static int Item( Item item, float left, float top, Color color = default, float limit = 32.0f, float scale = 1.0f ) {
		if ( color == default )
			color = Color.White;

		var size = TextureAssets.Item[ item.type ].Value.Size() * scale;
		size = new( Math.Min( size.X, limit ), Math.Min( size.Y, limit ) );
		var ret = Within( left - size.X / 2f, top - size.Y / 2f, size.X, size.Y, ref color );

		Vector2 pos = new( left, top );

		Main.GetItemDrawFrame( item.type, out var itemTexture, out var frame );
		Terraria.UI.ItemSlot.DrawItem_GetColorAndScale( item, item.scale, ref color, limit, ref frame, out var itemLight, out var finalDrawScale );
		Main.spriteBatch.Draw( itemTexture, pos, frame, itemLight, 0f, frame.Size() / 2f, finalDrawScale, SpriteEffects.None, 0f );
		if ( item.color != Color.Transparent )
			Main.spriteBatch.Draw( itemTexture, pos, frame, item.GetColor( color ), 0f, frame.Size() / 2f, finalDrawScale, SpriteEffects.None, 0f );

		// if ( item.stack > 1 ) {
		// 	scale /= 1.45f;
		// 	var mes = Common.Measure( item.stack.ToString() ) * scale;
		// 	Text( item.stack.ToString(), left - mes.X / 2f, top + 2.5f, color, scale );
		// }

		if ( ret != -1 ) {
			Main.hoverItemName = item.Name;
			Main.HoverItem = item.Clone();
		}

		return ret;
	}

	internal static int Rectangle( float left, float top, float width, float height, string hover = null, Color color = default ) {
		if ( color == default )
			color = Color.White;

		int ret = Within( left, top, width, height, ref color, hover );

		if ( width < 20 || height < 20 )
            Main.spriteBatch.Draw( TextureAssets.MagicPixel.Value, new Rectangle( ( int )left, ( int )top, ( int )width, ( int )height ), color );
		else
			UI.DrawInv( left, top, width, height, 0.0f, color );

		return ret;
	}

	internal static int Search( float left, float top, float width, float height, ref string search, Color color = default ) {
		if ( color == default )
			color = Color.White;

		if ( Rectangle( left, top, width, height, search.Length <= 0 ? "Type To Search" : search, color ) == 3 ) {
			Terraria.GameInput.PlayerInput.WritingText = true;
			Main.instance.HandleIME();
			string old = search;
			search = Main.GetInputText( old );

			if ( search.Length > 0 && Main.keyState.IsKeyUp( Microsoft.Xna.Framework.Input.Keys.Back ) && Main.keyState.IsKeyDown( Microsoft.Xna.Framework.Input.Keys.Back ) )
				search = search.Remove( search.Length - 1 );

			if ( search.Length > old.Length )
				return 1;
			if ( search.Length < old.Length )
				return 2;
		}

		return 0;
	}

	internal static int Texture( Texture2D texture, float left, float top, float scale = 1.0f, Texture2D hovertexture = null, string hover = null, Color color = default ) {
		if ( color == default )
			color = Color.White;

		var display = texture;
		var size = display.Size();

		int ret = Within( left, top, size.X, size.Y, ref color, hover );
		if ( ret == 3 && hovertexture is not null )
			display = hovertexture;

		Main.spriteBatch.Draw( display, new Vector2( left, top ), null, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f );

		return ret;
	}
};

internal class ItemSlot( Item item ) {
	internal static float InventorySlotSize => TextureAssets.InventoryBack.Width() * 0.875f;
	internal static int Spacing = 2;

	internal static Rectangle DrawItemGrid( ItemSlot[] slots, float x, float y, int maxrows = 5, int drawcount = -1 ) {
		Rectangle dim = new();

		int size = drawcount > -1 ? drawcount : slots.Length;
		int space = ( int )( InventorySlotSize + 2 );
		int rows = Math.Min( ( int )Math.Sqrt( size ), maxrows );
		int columns = ( int )Math.Ceiling( size / ( double )rows );

		dim.X = ( int )x;
		dim.Y = ( int )y;
		dim.Height = rows * space;
		dim.Width = columns * space;

		int index = 0;

		for ( int iy = 0; iy < rows; iy++ )
			for ( int ix =  0; ix < columns; ix++ )
				if ( index >= size )
					return dim;
				else
					slots[ index++ ].Draw( x + ix * space, y + iy * space );

        return dim;
	}

	private static void DrawItem( Item item, float left, float top, Color color = default ) {
		Terraria.UI.ItemSlot.DrawItemIcon(
			item,
			Terraria.UI.ItemSlot.Context.InventoryItem,
			Main.spriteBatch,
			new Vector2( left + InventorySlotSize / 2f, top + InventorySlotSize / 2f ),
			scale,
			InventorySlotSize * 0.65f,
			color == default ? Color.White : color
		);

		if ( item.stack <= 1 )
			return;

		string Content = item.stack.ToString();

		Vector2 Measure = FontAssets.ItemStack.Value.MeasureString( Content );

		Utils.DrawBorderStringFourWay(
			Main.spriteBatch,
			FontAssets.ItemStack.Value,
			Content,
			left + InventorySlotSize / 2f - Measure.X / 1.5f / 2f,
			top + InventorySlotSize / 2f + 2f,
			Color.White,
			Color.Black,
			Vector2.Zero,
			scale / 1.5f
		);
	}

	private const float scale = 1f;
	private float Left, Top;

	internal Func< bool > Check;
	internal Action Toggle;

	internal Item Item = item.Clone();

	private Rectangle Dim => new( ( int )Left, ( int )Top, ( int )( InventorySlotSize * scale ), ( int )( InventorySlotSize * scale ) );
	private bool Within => Dim.Contains( UI.Mouse.ToPoint() );
	
	internal void Update() {
		if ( Within ) {
			bool shift = Main.keyState.PressingShift();
			UI.ShiftHover = shift && !Item.IsAir;
		
			int open = -1;
			for ( int i = 10; i <= 49; i++ )
				if ( Main.LocalPlayer.inventory[ i ].IsAir )
					open = i;
		
			if ( UI.MClick )
				Toggle?.Invoke();
			else if ( UI.LClick ) {
				if ( Item.ModItem is Bauble bauble )
					bauble.Reset();
		
				if ( shift && open > -1 ) {
					Terraria.Audio.SoundEngine.PlaySound( SoundID.Grab );
					Main.LocalPlayer.inventory[ open ] = Item.Clone();
					Item.SetDefaults();
				} else
					Grab();
			}
		}
	}

	internal Func< Color > ColorCheck = () => default;
	internal Func< Texture2D > BackgroundCheck = () => null;
	internal Func< string > HoverCheck = () => null;

    internal void Draw( float left, float top ) {
		Left = left;
		Top = top;
		
		Color color = ColorCheck();
		Texture2D back = BackgroundCheck();
		string hover = HoverCheck();
		
		Utils.DrawInvBG( Main.spriteBatch, Dim, color );
		
		if ( Within ) {
			Main.LocalPlayer.mouseInterface = true;
			Main.hoverItemName = Item.Name;
			Main.HoverItem = Item.Clone();
		}

		if ( Item.IsAir ) {
			if ( back != null )
				Main.spriteBatch.Draw(
					back,
					Dim.Center() - back.Size() * 0.75f / 2f,
					null,
					Color.White,
					0f,
					Vector2.Zero,
					0.75f,
					SpriteEffects.None,
					0f
				);
			
			if ( Within && hover != null )
				UICommon.Text( hover, new Rectangle( ( int )UI.Mouse.X, ( int )UI.Mouse.Y, 0, 0 ), 0, Color.Yellow, true );
		
			return;
		}
		
		if ( !TextureAssets.Item[ Item.type ].IsLoaded )
			Main.instance.LoadItem( Item.type );
		
		DrawItem( Item, Left, Top );
    }
 
    private void Grab() {
		if ( !Within || Item.IsAir && Main.mouseItem.IsAir || !Check() && !Main.mouseItem.IsAir )
			return;
 
		Terraria.Audio.SoundEngine.PlaySound( SoundID.Grab );
 
		if ( Main.mouseItem.IsAir && !Item.IsAir ) {
			Main.mouseItem = Item.Clone();
			Main.mouseItem.stack = Math.Min( Item.stack, Item.maxStack );
			if ( ( Item.stack -= Main.mouseItem.stack ) <= 0 )
				Item.TurnToAir();
		} else if ( !Main.mouseItem.IsAir && ( Item.IsAir || Main.mouseItem.type == Item.type && Main.mouseItem.stack > 1 ) ) {
			if ( Item.IsAir ) {
				Item = Main.mouseItem.Clone();
				Item.stack = 0;
			}
			Item.stack += Main.mouseItem.stack;
			if ( ( Main.mouseItem.stack -= Item.stack ) <= 0 ) {
				Main.LocalPlayer.inventory[ 58 ].SetDefaults();
				Main.mouseItem.SetDefaults();
			}
		} else if ( Item.stack <= Item.maxStack ) {
			Item back = Item.Clone();
			Item = Main.mouseItem.Clone();
			Main.mouseItem = back.Clone();
		}
	}
}