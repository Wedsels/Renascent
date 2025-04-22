using Terraria;

namespace Renascent.content.code;

internal class Equip : UI {
    internal override bool Show => Mimic.UI.Condition;

    private const int size = 45;
    private const int space = 5;
    
    private int upgrade;

    internal override float Width => size + upgrade / 2 * size + space * 2;
    internal override float Height => size * 2 + space * 2;
    protected override float Left => ScreenWidth / 2 + Width / 2;
    protected override float Top => ScreenHeight / 2 + Height / 2;

    internal override bool Drag => true;

    private readonly ItemSlot[] Slots = new ItemSlot[ Mimic.Upgrades ];

    protected override void Initialize() {
        for ( int i = 0; i < Mimic.Upgrades; i++ )
            Slots[ i ] = new( "regularequip" + i ) { Check = () => Main.mouseItem.ModItem is Bauble };
    }

    internal override void Update() {
		if ( !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) )
			return;
		
		upgrade = tp.MimicUpgrade;
    
        for ( int i = 0; i <= upgrade; i++ )
            Slots[ i ].Update();
    }

    internal override void Draw() {
		Utils.DrawInvBG( SB, Dim );

        for ( int i = 0; i <= upgrade; i++ ) {
            Slots[ i ].Left = Dim.Left + space + i / 2 * size;
            Slots[ i ].Top = Dim.Top + space + i % 2 * size;
            Slots[ i ].Draw();
        }
    }
}