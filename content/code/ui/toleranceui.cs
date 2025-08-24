using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Microsoft.Xna.Framework;

using Renascent.content.code.mimic;

namespace Renascent.content.code.ui;

internal class ToleranceButtonUI : UI {
	internal override bool Show => Mimic.UI.Condition;

	internal override float Width => Textures.Heart.Width;
	internal override float Height => Textures.Heart.Height;
	protected override float Left => Mimic.UI.Dim.Left + Mimic.UI.Dim.Width / 2.0f - Width / 2.0f;
	protected override float Top => ScreenHeight + Height / 2.0f - ItemSlot.InventorySlotSize * 1.5f;

	internal override bool Drag => true;

	internal override void Hide() => DragMouse = Vector2.Zero;

	internal override void Draw() {
        if ( Buttons.Texture( Textures.Heart, Dim.X + 1, Dim.Y - ItemSlot.Spacing, 1.0f, null, "Tolerance", Color.White * Oscillate ) == 0 ) {
		    ToleranceUI.Active = !ToleranceUI.Active;
            DigestionUI.Active = false;
            Mimic.Sound( SoundID.Zombie118 );
        }
	}
}

internal class ToleranceUI : UI {
    internal static bool Active;
    internal override bool Show => Mimic.UI.Condition && Active;

    private static int Columns => Renascent.Client.ToleranceColumns;
    private static int Rows => Renascent.Client.ToleranceRows;
    private const int BHeight = 32 + 4;
    private const int BWidth = 32 + 4;

	internal override float Width => Columns * BWidth - 4 + 6 * 2;
	internal override float Height => Rows * BHeight + 6 * 2 + 8;
	protected override float Left => Mimic.UI.Dim.Left - Width - ItemSlot.Spacing;
	protected override float Top => ScreenHeight;
    
	internal override bool Drag => true;

    private readonly int[] buffs = BuffID.Search.Names.Select( BuffID.Search.GetId ).ToArray();
    private int[] display = BuffID.Search.Names.Select( BuffID.Search.GetId ).ToArray();

    private string search = "";

	internal override void Hide() {
        DragMouse = Vector2.Zero;
        Active = false;
    }

    private int offset;

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
            display = ( searched == 1 ? display : buffs ).Where( x => {
                if ( Lang.GetBuffName( x ).Contains( search, StringComparison.CurrentCultureIgnoreCase ) )
                    return true;

                if ( Lang.GetBuffDescription( x ).Contains( search, StringComparison.CurrentCultureIgnoreCase ) )
                    return true;

                return false;
            } ).ToArray();

            offset = 0;
        }

        if ( display.Length > 0 )
            for ( int i = 0; i < display.Length; i++ ) {
                int index = i + offset * Columns;

                if ( i >= Columns * Rows || index >= display.Length || display[ index ] >= BuffID.Count )
                    break;

                int buff = display[ index ];

                int x = Dim.Left + 6 + BWidth * ( i % Columns );
                int y = Dim.Top + 8 + 6 + BHeight * ( i / Columns );

                if ( Buttons.Texture(
                    TextureAssets.Buff[ buff ].Value,
                    x,
                    y,
                    1.0f,
                    null,
                    Lang.GetBuffName( buff ) + "\n" + Lang.GetBuffDescription( buff ) + "\n" + bauble.Bauble.Round( 19.0f / 32.0f ) + "%",
                    MP.ActiveTolerance[ buff ] ? Color.White : Color.DarkSlateGray
                ) == 0 ) MP.ActiveTolerance[ buff ] = !MP.ActiveTolerance[ buff ];

                Buttons.Rectangle( x + 1, y + 32 - 1, 32 - 1, 4, null, Color.Black );
                Buttons.Rectangle( x + 1, y + 32 - 1, 19 - 1, 4, null, ( MP.ActiveTolerance[ buff ] ? Color.LawnGreen : Color.Yellow ) * Oscillate );
            }
    }
}