using System;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Renascent.content.code;

internal class UICommon : ModSystem {
	internal static bool Details;

	public override void ModifyInterfaceLayers( List< GameInterfaceLayer > layers ) {
		int index = layers.FindIndex( layer => layer.Name.Equals( "Vanilla: Mouse Text" ) );
		if ( index > -1 )
			layers.Insert(
				index,
				new LegacyGameInterfaceLayer(
					"Renascent: UI",
					delegate {
						foreach ( var i in UI.Display.Values )
							if ( i.Show )
								i.Update();
							else
								i.Hide();

						return true;
					},
					InterfaceScaleType.None
				)
			);
	}
}

internal class UI {
    internal static Dictionary< Type, UI > Display = [];
	static UI() {
		Display = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where( t => t.IsSubclassOf( typeof( UI ) ) && !t.IsAbstract ).ToDictionary( t => t, t => ( UI )Activator.CreateInstance( t )! );
		Display.Last().Value.Initialize();
	}

	internal void Update() {
		if ( Within ) {
			Main.LocalPlayer.mouseInterface = true;

			if ( Main.mouseRight ) {
				if ( StartDrag == Point.Zero )
					StartDrag = Main.MouseScreen.ToPoint() - DragMouse;
				DragMouse = Main.MouseScreen.ToPoint() - StartDrag;
			} else StartDrag = Point.Zero;
		} else StartDrag = Point.Zero;

		FrameCount++;
		Draw();
	}

	internal virtual void Initialize() {}
    internal virtual void Draw() {}
	internal virtual void Hide() {}
    internal virtual bool Show => false;

	protected static SpriteBatch SB => Main.spriteBatch;
	internal static Texture2D Texture( string name ) => ModContent.Request< Texture2D >( "Renascent/content/texture/" + name, ReLogic.Content.AssetRequestMode.ImmediateLoad ).Value;

	protected static bool LClick => Main.mouseLeftRelease && Main.mouseLeft;
	protected static bool RClick => Main.mouseRightRelease && Main.mouseRight;
	protected static bool MClick => Main.mouseMiddleRelease && Main.mouseMiddle;

	protected virtual int Slow => 15;
	protected virtual int Frames => 1;
	protected int Frame => FrameCount / Slow % Frames;
	internal int FrameCount = 0;

	internal virtual bool Condition { get; set; }

	protected virtual bool Drag => false;
	protected Point DragMouse = Point.Zero;
	protected Point StartDrag = Point.Zero;

	internal virtual int Width => 0;
	internal virtual int Height => 0;
	internal virtual int Left => 0;
	internal virtual int Top => 0;
	protected Rectangle Dim => new( Left + DragMouse.X, Top + DragMouse.Y, Width, Height );
	protected bool Within => Dim.Contains( Main.MouseScreen.ToPoint() );

	protected static float Scale => Main.UIScale;
}

internal class ItemSlot {
	internal int slot;
	internal ref Item Item => ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan( Items )[ slot ];

	internal static List< Item > Items = [];
	public ItemSlot() {
		slot = Items.Count;
		Items.Add( new() );
	}

	internal const int Size = 45;
	internal float scale = 1f;
	internal int Left, Top;
	internal bool Show = false;

	internal Func< bool > Check;

	internal Rectangle Dim => new( Left, Top, Size, Size );
	internal bool Within => Dim.Contains( Main.MouseScreen.ToPoint() );

    internal void Draw() {
		Utils.DrawInvBG( Main.spriteBatch, Dim );

		if ( Within )
			Main.LocalPlayer.mouseInterface = true;

		if ( !Item.IsAir ) {
			if ( !Terraria.GameContent.TextureAssets.Item[ Item.type ].IsLoaded )
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

			if ( Item.stack > 1 ) {
				string Content = Item.stack.ToString();

				Vector2 Measure = Terraria.GameContent.FontAssets.ItemStack.Value.MeasureString( Content );

				Utils.DrawBorderStringFourWay(
					Main.spriteBatch,
					Terraria.GameContent.FontAssets.ItemStack.Value,
					Content,
					Left + Size / 2 - Measure.X / 1.5f / 2,
					Top + Size / 2 + 2,
					Color.White,
					Color.Black,
					Vector2.Zero,
					scale / 1.5f
				);
			}

			if ( Within ) {
				Main.hoverItemName = Item.Name;
				Main.HoverItem = Item.Clone();
			}
		}

		if ( Within && Main.mouseLeft && Main.mouseLeftRelease )
			Grab();
    }

	protected void Grab() {
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