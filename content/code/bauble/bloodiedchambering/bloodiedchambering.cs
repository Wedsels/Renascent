using Terraria;
using Terraria.ID;

namespace Renascent.content.code.bauble.bloodiedchambering;

internal class BloodiedChambering : Bauble {
    internal override double SpawnChance => 0.05;
    internal override int[] NPC => [ NPCID.ArmsDealer ];

    internal override int Rarity => 5;

	private double BuffTime => 2.0 * Roll;
	private float Multishot => 0.015f * Roll * Negative;

	protected override object[] TooltipArgs => [ DisplayValue( Multishot * 100.0f ), Round( BuffTime ), Stacks, DisplayValue( Multishot * Stacks * 100.0f ), Round( Timer ) ];

	internal override void Hit( Projectile proj, NPC.HitInfo hitinfo, NPC npc, bool minion ) {
		Stacks++;
		Timer = BuffTime;
	}

    internal override void Update( ref Boost boost ) {
        if ( Timer == 0.0 )
			Stacks = 0;
		else
			boost.Multishot += Multishot * Stacks;
    }

    internal override void OnReset() => Stacks = 0;
}