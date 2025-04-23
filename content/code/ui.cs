using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Renascent.content.code;

internal class UICommon : ModSystem {
	public override void PostSetupContent() {
        IL_Main.DoUpdate += context => {
			var cursor = new ILCursor( context );
			
			if ( cursor.TryGotoNext( i => i.MatchStsfld( typeof( Main ), nameof( Main.hasFocus ) ) ) )
				cursor.EmitDelegate( () => {
					foreach ( var i in UI.Display.Values ) {
						i._frame += 1.0 / i.Slow;

						i.Update();

						if ( !i.dragging && !i.Within )
							continue;
							
						Main.LocalPlayer.mouseInterface = true;

						if ( i.Drag && Main.mouseRight ) {
							i.dragging = true;
							if ( i.StartDrag == Vector2.Zero )
								i.StartDrag = UI.Mouse - i.DragMouse;
							i.DragMouse = UI.Mouse - i.StartDrag;
						} else {
							i.StartDrag = Vector2.Zero;
							i.dragging = false;
						}
					}
				} );
		};

        IL_Main.DrawThickCursor += context => {
			var cursor = new ILCursor( context );

			cursor.EmitDelegate( () => {
				UI.Mouse = Main.MouseScreen;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix );

				DrawText();

				foreach ( var i in UI.Display.Values )
					if ( i.Show )
						i.Draw();
					else
						i.Hide();
			} );
		};
    }
    
    private class TextData( string text, Vector2 position, Color color, int time ) {
		internal readonly string Text = text;
		internal Vector2 Position = position;
		internal float Rotation = Main.rand.NextFloat( 0.5f ) * ( Main.rand.NextBool() ? -1 : 1 );
		internal readonly Vector2 Origin = FontAssets.MouseText.Value.MeasureString( text ) / 2;
		internal int Time = time;
		internal readonly Color Color = color;

		public override int GetHashCode() => Text.GetHashCode();
		public override bool Equals( object obj ) => obj is TextData data && data.Text == Text;
	}
	private static readonly HashSet< TextData > Texts = [];
	
	internal static void Text( string text, Rectangle target, int time = 240, Color color = default ) {
		time += text.Length * 2;

		Vector2 position = target.Center();
		position.Y -= target.Height;
		position.X += target.Width * Main.rand.NextFloat( 2f ) * ( Main.rand.NextBool() ? -1 : 1 );
		float x = FontAssets.MouseText.Value.MeasureString( text ).X / 2;
		if ( position.X < x )
			position.X = x;
		else if ( position.X > Main.maxScreenW / Main.UIScale - x )
			position.X = Main.maxScreenW / Main.UIScale - x;
		if ( position.Y - x < 0f )
			position.Y = 10f + x / 2f;
		else if ( position.Y + x > Main.maxScreenH / Main.UIScale )
			position.Y = Main.maxScreenH / Main.UIScale - 10f - x / 2f;
			
		if ( Texts.TryGetValue( new ( text, Vector2.Zero, color, time ), out var data ) ) {
			data.Time = time;
			data.Position = position;
		} else
			Texts.Add( new( text, position, color == default ? Color.DarkRed : color, time ) );
	}

	private static void DrawText() {
		foreach ( var i in Texts.Reverse() ) {
			ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(
				Main.spriteBatch,
				FontAssets.MouseText.Value,
				i.Text,
				( i.Position -= new Vector2( Main.rand.NextFloat( -0.1f, 0.1f ) ) )  - Vector2.One * 2f,
				Color.Black,
				i.Rotation *= ( Main.rand.NextBool() ? 1.005f : 0.995f ) + 0.005f / i.Text.Length,
				i.Origin,
				1f,
				SpriteEffects.None,
				0f
			);

			ReLogic.Graphics.DynamicSpriteFontExtensionMethods.DrawString(
				Main.spriteBatch,
				FontAssets.MouseText.Value,
				i.Text,
				i.Position,
				i.Color,
				i.Rotation,
				i.Origin,
				1f,
				SpriteEffects.None,
				0f
			);

			if ( --i.Time <= 0 )
				Texts.Remove( i );
		}
	}
}

internal abstract class UI {
    internal static readonly Dictionary< Type, UI > Display;
	static UI() {
		Display = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where( t => t.IsSubclassOf( typeof( UI ) ) && !t.IsAbstract ).ToDictionary( t => t, t => ( UI )Activator.CreateInstance( t )! );
		foreach ( var ui in Display.Values ) {
			ui.Initialize();
		}
	}

	protected virtual void Initialize() {}
	internal virtual void Update() {}
	internal virtual void Draw() {}
	internal virtual void Hide() {}
    internal virtual bool Show => false;

	protected static SpriteBatch SB => Main.spriteBatch;
	internal static Texture2D Texture( string name ) => ModContent.Request< Texture2D >( "Renascent/content/texture/" + name, ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;

	protected float ScreenWidth => Main.maxScreenW / Main.UIScale - Width;
	protected float ScreenHeight => Main.maxScreenH / Main.UIScale - Height;

	internal static bool LClick => Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseLeft;
	internal static bool RClick => Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseRight;
	internal static bool MClick => Terraria.GameInput.PlayerInput.Triggers.JustPressed.MouseMiddle;

	internal virtual double Slow => 3;
	protected virtual int Frames => 1;
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
	internal Rectangle Dim => new( ( int )( Left + DragMouse.X ), ( int )( Top + DragMouse.Y ), ( int )Width, ( int )Height );
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
}

internal class ItemSlot {
	private readonly string slot;
	internal static readonly List< string > Items = [];
	private readonly Color Color;
	public ItemSlot( string name, Color color = default ) {
		Items.Add( slot = name );
		Color = color;
	}

	private const int Size = 45;
	private const float scale = 1f;
	internal float Left, Top;

	internal Func< bool > Check;

	private Rectangle Dim => new( ( int )Left, ( int )Top, ( int )( Size * scale ), ( int )( Size * scale ) );
	private bool Within => Dim.Contains( UI.Mouse.ToPoint() );
	
	internal void Update() {
		if ( Within && UI.LClick )
			Grab();
	}

    internal void Draw() {
		Utils.DrawInvBG( Main.spriteBatch, Dim, Color );
		
		if ( !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) )
			return;
			
		ref Item Item = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault( tp.Items, slot, out bool _ );

		if ( Item.IsAir )
			return;

		if ( Within ) {
			Main.LocalPlayer.mouseInterface = true;
			Main.hoverItemName = Item.Name;
			Main.HoverItem = Item.Clone();
		}

		if ( !TextureAssets.Item[ Item.type ].IsLoaded )
			Main.instance.LoadItem( Item.type );

		Terraria.UI.ItemSlot.DrawItemIcon(
			Item,
			Terraria.UI.ItemSlot.Context.InventoryItem,
			Main.spriteBatch,
			new Vector2( Left + Size / 2, Top + Size / 2 ),
			scale,
			Size * 0.65f,
			Color.White
		);

		if ( Item.stack <= 1 )
			return;

		string Content = Item.stack.ToString();

		Vector2 Measure = FontAssets.ItemStack.Value.MeasureString( Content );

		Utils.DrawBorderStringFourWay(
			Main.spriteBatch,
			FontAssets.ItemStack.Value,
			Content,
			Left + Size / 2 - Measure.X / 1.5f / 2,
			Top + Size / 2 + 2,
			Color.White,
			Color.Black,
			Vector2.Zero,
			scale / 1.5f
		);
    }

    private void Grab() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) )
			return;
			
		ref Item Item = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault( tp.Items, slot, out bool _ );

		if ( !Within || Item.IsAir && Main.mouseItem.IsAir || !Check() && !Main.mouseItem.IsAir )
			return;

		Terraria.Audio.SoundEngine.PlaySound( Terraria.ID.SoundID.Grab );

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
			if ( ( Main.mouseItem.stack -= Item.stack ) <= 0 )
				Main.mouseItem.TurnToAir();
		} else if ( Item.stack <= Item.maxStack ) {
			Item back = Item.Clone();
			Item = Main.mouseItem.Clone();
			Main.mouseItem = back.Clone();
		}
	}
}