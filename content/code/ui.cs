using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;

namespace Renascent.content.code;

internal class UICommon : ModSystem {
    public override void Load() {
        IL_Main.DoUpdate += context => {
			var cursor = new ILCursor( context );
			
			if ( cursor.TryGotoNext(
			    i => i.MatchLdsfld( typeof( Main ), nameof( Main.timeForVisualEffects ) ),
			    i => i.MatchLdcR8( 1.0 ),
			    i => i.MatchAdd()
			) ) cursor.EmitDelegate( () => {
				foreach ( var i in UI.Display.Values ) {
					if ( i.Show ) {
						i.Update();
						i._frame += 1.0 / i.Slow;

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
					} else {
						i.DragMouse = Vector2.Zero;
						i._frame = 0.0;
					}
				}
			} );
		};
		
        IL_Main.DrawThickCursor += context => {
			var cursor = new ILCursor( context );

			cursor.EmitDelegate( () => {
				UI.Mouse = Main.MouseScreen;
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
				foreach ( var i in UI.Display.Values )
					if ( i.Show )
						i.Draw();
					else
						i.Hide();
			} );
		};
    }
}

internal abstract class UI {
    internal static readonly Dictionary< Type, UI > Display;
	static UI() {
		Display = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where( t => t.IsSubclassOf( typeof( UI ) ) && !t.IsAbstract ).ToDictionary( t => t, t => ( UI )Activator.CreateInstance( t )! );
		Display.Last().Value.Initialize();
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

	protected static bool LClick => Main.mouseLeftRelease && Main.mouseLeft;
	protected static bool RClick => Main.mouseRightRelease && Main.mouseRight;
	protected static bool MClick => Main.mouseMiddleRelease && Main.mouseMiddle;

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

		return new Rectangle( num, num2, ( int )( Terraria.GameContent.TextureAssets.InventoryBack.Width() * Main.inventoryScale ), ( int )( Terraria.GameContent.TextureAssets.InventoryBack.Height() * Main.inventoryScale ) );
	} }
}

internal class ItemSlot {
	private readonly int slot;
	private ref Item Item => ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan( Items )[ slot ];

	private static readonly List< Item > Items = [];
	public ItemSlot() {
		slot = Items.Count;
		Items.Add( new() );
	}

	private const int Size = 45;
	private const float scale = 1f;
	internal int Left, Top;

	internal Func< bool > Check;

	private Rectangle Dim => new( Left, Top, Size, Size );
	private bool Within => Dim.Contains( UI.Mouse.ToPoint() );

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

    private void Grab() {
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