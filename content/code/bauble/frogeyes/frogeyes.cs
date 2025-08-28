using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;

namespace Renascent.content.code.bauble.frogeyes;

internal class FrogEyes : Bauble {
    internal override double SpawnChance => 0.0002;
    internal override Func< Player, FishingAttempt, bool > FishingBiome => ( Player p, FishingAttempt a ) => p.ZoneBeach || p.ZoneForest || p.ZoneJungle;

    internal override int Rarity => ItemRarityID.LightRed;

	private float Multishot => Roll * 0.0115f * Negative;
	private static float Near => NearbyEnemy( 250.0f ).Select( e => e.Center.Distance( Player.Center ) ).DefaultIfEmpty( 0.0f ).Min() / 16.0f;

	protected override object[] TooltipArgs => [ DisplayValue( Multishot * 100.0f ), DisplayValue( Multishot * Near * 100.0f ) ];

	internal override void Update( ref Boost boost ) => boost.Multishot += Multishot * Near;
}