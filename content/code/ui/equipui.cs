using Terraria;
using Microsoft.Xna.Framework;

using Renascent.content.code.mimic;

namespace Renascent.content.code.ui;

internal class EquipUI : UI {
	internal override bool Show => Mimic.UI.Condition;

	private float w;
	private float h;
	internal override float Width => w;
	internal override float Height => h;
	protected override float Left => Mimic.UI.Dim.Right + ItemSlot.Spacing;
	protected override float Top => ScreenHeight;

	internal override bool Drag => true;

	internal override void Hide() => DragMouse = Vector2.Zero;

	internal override void Update() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out MimicPlayer MP ) )
			return;

		if ( Show )
			foreach ( ItemSlot i in MP.Baubles )
				i?.Update();
	}

	internal override void Draw() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out MimicPlayer MP ) )
			return;

		Rectangle r;

		r = ItemSlot.DrawItemGrid( MP.Baubles, Dim.Left + ItemSlot.Spacing, Dim.Top, 4, MP.UnlockedSlotCount );
		w = r.Width;
		h = r.Height;

		r = ItemSlot.DrawItemGrid( MP.UniqueBaubles, Dim.Left + ItemSlot.Spacing, Dim.Bottom - ItemSlot.InventorySlotSize, 1, MimicPlayer.UniqueBaubleMaxSlots );
		w = System.Math.Max( w, r.Width );
		h += r.Height;
	}
}