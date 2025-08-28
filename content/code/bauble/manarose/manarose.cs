using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Renascent.content.code.bauble.manarose;

internal class ManaRose : Bauble {
    protected override int CreateTile => ModContent.TileType< ManaRosePlant >();
	internal override int Rarity => ItemRarityID.Green;

	private float Crit => 0.004f * Roll * Negative;

	private float Bonus => Crit * Math.Max( 0, Player.statManaMax2 - Player.statMana );

	protected override object[] TooltipArgs => [ DisplayValue( Crit * 100.0f ), DisplayValue( Bonus * 100.0f ) ];

	internal override void Update( ref Boost boost ) => boost.Crit += ( int )( Bonus * 100.0f );
}

internal class ManaRosePlant : BaublePlant {
    internal override int[] AnchorTiles => [ TileID.Grass ];
    internal override short Dust => DustID.BlueFairy;
    internal override ushort Frames => 1;
	internal override double SpawnChance => 0.001;
    internal override string LocalizedItem => "ManaRose";
    internal override Color MapColor => Color.Blue;
}