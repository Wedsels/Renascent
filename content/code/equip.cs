using Microsoft.Xna.Framework;
using Terraria;

namespace Renascent.content.code;

internal class Equip : UI {
    internal override bool Show => Mimic.UI.Condition;

    private const int size = 45;
    private const int space = 5;

    private float w;
    internal override float Width => w;
    internal override float Height => space * 2.0f + size * 4.0f;
    protected override float Left => ScreenWidth / 2.0f + Width / 2.0f;
    protected override float Top => ScreenHeight / 2.0f + Height / 2.0f;

    internal override bool Drag => true;

    private readonly ItemSlot[] Common = new ItemSlot[ Mimic.Upgrades * 2 ];
    private readonly ItemSlot[] Unique = new ItemSlot[ Mimic.Upgrades / 2 ];

    protected override void Initialize() {
        for ( int i = 0; i < Common.Length; i++ )
            Common[ i ] = new( "Common" + i ) { Check = () => Main.mouseItem.ModItem is Bauble b };
        for ( int i = 0; i < Unique.Length; i++ )
            Unique[ i ] = new( "Unique" + i, Color.BlanchedAlmond ) { Check = () => Main.mouseItem.ModItem is Bauble b };
    }

    internal override void Update() {
        foreach ( ItemSlot i in Common )
            i.Update();
        foreach ( ItemSlot i in Unique )
            i.Update();
    }

    internal override void Draw() {
		Utils.DrawInvBG( SB, Dim, Color.DarkRed * 0.8f );

		if ( !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) )
			return;

        w = space * 2f;

        for ( int i = 0; i < tp.MimicUpgrade * 2; i++ ) {
            Common[ i ].Left = Dim.Left + space + i / 2 * size;
            Common[ i ].Top = Dim.Top + space + i % 2 * size;
            Common[ i ].Draw();
        }
            
        w = ( tp.MimicUpgrade * 2 - 1 ) / 2 * size + size + space * 2.0f;

        for ( int i = 0; i < tp.MimicUpgrade / 2; i++ ) {
            Unique[ i ].Left = Dim.Left + space + i * size;
            Unique[ i ].Top = Dim.Bottom - space - size;
            Unique[ i ].Draw();
        }
    }
}