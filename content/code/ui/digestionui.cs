using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

using Renascent.content.code.mimic;

namespace Renascent.content.code.ui;

internal class DigestionButtonUI : UI {
	internal override bool Show => Mimic.UI.Condition;

	internal override float Width => Textures.Intestine.Width;
	internal override float Height => Textures.Intestine.Height;
	protected override float Left => Mimic.UI.Dim.Left + Mimic.UI.Dim.Width / 2.0f - Width / 2.0f;
	protected override float Top => ScreenHeight + Height / 2.0f - ItemSlot.InventorySlotSize * 2.5f;

	internal override bool Drag => true;

	internal override void Hide() => DragMouse = Vector2.Zero;

	internal override void Draw() {
        if ( Buttons.Texture( Textures.Intestine, Dim.X + 1, Dim.Y - ItemSlot.Spacing, 1.0f, null, "Digestion", Color.White * Oscillate ) == 0 ) {
		    DigestionUI.Active = !DigestionUI.Active;
            ToleranceUI.Active = false;
            Mimic.Sound( SoundID.Zombie20 );
        }
	}
}

internal class DigestionUI : UI {
    internal static bool Active;
    internal override bool Show => Mimic.UI.Condition && Active;

    private static int Columns => Renascent.Client.DigestionColumns;
    private static int Rows => Renascent.Client.DigestionRows;
    private const int BHeight = 32 + 4;
    private const int BWidth = 32 + 4;

	internal override float Width => Columns * BWidth - 4 + 6 * 2;
	internal override float Height => Rows * BHeight + 6 * 2 + 8;
	protected override float Left => Mimic.UI.Dim.Left - Width - ItemSlot.Spacing;
	protected override float Top => ScreenHeight;
    
	internal override bool Drag => true;

	internal override void Hide() {
        DragMouse = Vector2.Zero;
        Active = false;
    }

    private string search = "";

    private int offset;

    private int[] items = [];
    private int[] display = [];

    internal override void Initialize() => display = items = ContentSamples.ItemsByType.Where( x => !x.Value.IsAir ).Select( x => x.Key ).ToArray();

    internal override void Update() {
        Main.LocalPlayer.mouseInterface |= Within;

        Terraria.GameInput.PlayerInput.LockVanillaMouseScroll( "DigestionUI" );
        offset = Math.Clamp( offset - MScroll, 0, Math.Max( 1, ( int )Math.Ceiling( display.Length / ( double )Columns ) - 1 ) );
    }

    internal override void Draw() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out MimicPlayer MP ) )
			return;

        DrawInv( Dim, 0.0f, Colors.Red );

        int searched = Buttons.Search( Dim.Left + 2, Dim.Top, Dim.Width - 4, 10, ref search, Color.BurlyWood * Oscillate );

        if ( searched > 0 ) {
            display = ( searched == 1 ? display : items ).Where( x => {
                Item item = ContentSamples.ItemsByType[ x ];

                if ( item.HoverName.Contains( search, StringComparison.CurrentCultureIgnoreCase ) )
                    return true;

                if ( item.DamageType.ToString().Contains( search, StringComparison.CurrentCultureIgnoreCase ) )
                    return true;

                if ( item.ModItem != null && item.ModItem.ToString().Contains( search, StringComparison.CurrentCultureIgnoreCase ) )
                    return true;

                var tool = Lang.GetTooltip( x );
                for ( int i = 0; i < tool.Lines; i++ )
                    if ( tool.GetLine( i ).Contains( search, StringComparison.CurrentCultureIgnoreCase ) )
                        return true;

                return false;
            } ).ToArray();

            offset = 0;
        }

        for ( int i = 0; i < display.Length; i++ ) {
            int index = i + offset * Columns;

            if ( i >= Columns * Rows || index >= display.Length || !ContentSamples.ItemsByType.TryGetValue( display[ index ], out Item item ) )
                break;

            int x = Dim.Left + 6 + BWidth * ( i % Columns );
            int y = Dim.Top + 8 + 6 + BHeight * ( i / Columns );

            float size = 32.0f;

            if ( Buttons.Item( item, x + size / 2.0f, y + size / 2.0f ) == 0 )
                MP.Digesting[ item.type ] = !MP.Digesting[ item.type ];

            if ( MP.Digesting[ item.type ] ) {
                Buttons.Texture( Textures.Recycle, x, y, size / Math.Min( Textures.Recycle.Width, Textures.Recycle.Height ) );
                Buttons.Texture( Textures.Border, x, y, size / Math.Min( Textures.Border.Width, Textures.Border.Height ) );
            }
        }
    }
}