using System;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Renascent.content.code;

internal class Equip : UI {
    internal override bool Show => Display[ typeof( Chest ) ].Condition;

    internal const int size = 45;
    internal const int space = 5;

    internal override int Width => size + Renascent.ChestUpgrade / 2 * size + space * 2;
    internal override int Height => size * 2 + space * 2;
    internal override int Left => Main.maxScreenW / 2 - Width / 2;
    internal override int Top => Main.maxScreenH / 2 - Height / 2;

    protected override bool Drag => true;

    internal ItemSlot[] Slots = new ItemSlot[ Renascent.ChestUpgrades ];

    internal override void Initialize() {
        for ( int i = 0; i < Renascent.ChestUpgrades; i++ )
            Slots[ i ] = new() { Check = () => Main.mouseItem.ModItem is Bauble };
    }

    internal override void Draw() {
		Utils.DrawInvBG( SB, Dim );

        for ( int i = 0; i <= Renascent.ChestUpgrade; i++ ) {
            Slots[ i ].scale = Scale;
            Slots[ i ].Left = Dim.Left + space + i / 2 * size;
            Slots[ i ].Top = Dim.Top + space + i % 2 * size;
            Slots[ i ].Draw();
        }
    }
}