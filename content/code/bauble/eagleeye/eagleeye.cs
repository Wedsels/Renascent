using System.Linq;
using Terraria;
using Terraria.ID;

namespace Renascent.content.code.bauble.eagleeye;

internal class EagleEye : Bauble {
    internal override double SpawnChance => 0.0005;
    internal override int[] NPC => [ NPCID.Bird, NPCID.BirdBlue, NPCID.BirdRed, NPCID.Duck, NPCID.Duck2, NPCID.DuckWhite, NPCID.DuckWhite2, NPCID.Grebe, NPCID.Grebe2, NPCID.Owl, NPCID.DemonEyeOwl, NPCID.Seagull, NPCID.Seagull2 ];
    internal override int Rarity => ItemRarityID.LightRed;

	private float Crit => Roll * Negative;
	private static float Near => NearbyEnemy( 250.0f ).Select( e => e.Center.Distance( Player.Center ) ).DefaultIfEmpty( 0.0f ).Min() / 16.0f;

	protected override object[] TooltipArgs => [ DisplayValue( Crit ), ( int )( Crit * Near ) ];

	internal override void Update( ref Boost boost ) => boost.Crit += ( int )( Crit * Near );
}