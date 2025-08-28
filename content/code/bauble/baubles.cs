using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;

namespace Renascent.content.code.bauble;

internal class HasteEnchantedAmulet : Bauble {
	private float Percent => 0.12f * Roll * Negative;
	private float Speed => 1.0f + Player.statManaMax2 * Percent * 0.01f;

	protected override object[] TooltipArgs => [ DisplayValue( Percent * 100.0 ), DisplayValue( ( Speed - 1.0f ) * 100.0 ) ];
	
	internal override void Update( ref Boost boost ) => boost.AttackSpeed *= Speed;
}

internal class TrackingSpuds : Bauble {
	private float Percent => 0.16f * Roll * Negative;
	private int Crit => ( int )( Player.statManaMax2 * Percent );

	protected override object[] TooltipArgs => [ DisplayValue( Percent * 100.0 ), DisplayValue( Crit ) ];
	
	internal override void Update( ref Boost boost ) => boost.Crit += Crit;
}

internal class EldritchSphere : Bauble {
	private int Change => ( int )( 100 * Roll );

	protected override object[] TooltipArgs => [ DisplayValue( Change * Negative ), DisplayValue( Change / 2 * -Negative ) ];

	internal override void Update( ref Boost boost ) => boost.LifeToMana += Change * Negative;
}

internal class ManicMagicManifold : Bauble {
	private float Damage => 0.003f * Roll * Negative;

	private float Bonus => Damage * Math.Max( 0, Player.statManaMax2 - Player.statMana );

	protected override object[] TooltipArgs => [ DisplayValue( Damage * 100.0f ), DisplayValue( Bonus * 100.0f ) ];

	internal override void Update( ref Boost boost ) => boost.Damage *= 1.0f + Bonus;
}

internal class HardyBloodvine : Bauble {
	private float Defense => 0.0015f * Roll * Negative;

	private float Bonus => Defense * ( Player.statLifeMax2 - Player.statLife );

	protected override object[] TooltipArgs => [ DisplayValue( Defense * 100.0f ), DisplayValue( Bonus * 100.0f ) ];

	internal override void Update( ref Boost boost ) => boost.Defense *= 1.0f + Bonus;
}

internal class WeaponWeights : Bauble {
	internal override int Rarity => ItemRarityID.Green;

	private float Speed => 0.85f * Roll * -Negative;
	private float Damage => 0.25f * Roll * Negative;

	protected override object[] TooltipArgs => [ DisplayValue( Speed * 100.0f ), DisplayValue( Damage * 100.0f ) ];

	internal override void Update( ref Boost boost ) {
		boost.Velocity *= 1.0f + Speed;
		boost.AttackSpeed *= 1.0f + Speed;
		
		boost.Damage *= 1.0f + Damage;
	}
}

internal class ConfusingHourglass : Bauble {
	internal override int Rarity => ItemRarityID.Green;

	private float Stat => 0.3f * Roll * Negative;

	private double BuffTime => 480.0 * Roll;

	protected override object[] TooltipArgs => [ DisplayValue( Stat * 100.0f ), Round( BuffTime ), Round( Timer ) ];

	internal override void Update( ref Boost boost ) {
		if ( Timer > 0.0 ) {
			boost.Defense *= 1.0f + Stat;
			boost.Damage *= 1.0f + Stat;
		}
	}

	internal override bool PreKill( double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource ) {
		Timer = BuffTime;

		return true;
	}
}

internal class InefficientMitosis : Bauble {
	internal override int Rarity => ItemRarityID.Green;

	private float Damage => 0.75f * Roll;
	private float Multishot => 1.185f * Roll;

	protected override object[] TooltipArgs => [ Round( Damage * 100.0f ), Round( Multishot * 100.0f ) ];

	internal override void Update( ref Boost boost ) {
		boost.Damage *= 1.0f - Damage;
		boost.Multishot += Multishot;
	}
}

internal class MasterBelt : Bauble {
	internal override int Rarity => ItemRarityID.Green;

	private float Dodge => 0.02f * Roll;

	protected override object[] TooltipArgs => [ DisplayValue( Dodge * 100.0f ), DisplayValue( Dodge * Stacks * 100.0f ) ];

	internal override void Update( ref Boost boost ) {
		if ( Stacks < 20 && Timer <= 0.0 ) {
			Timer = 1;
			Stacks++;
		}
		
		boost.Dodge += Dodge * Stacks;
	}

	internal override void OnDodge() => Stacks = 0;
}

internal class ManaVacuum : Bauble {
	internal override int Rarity => ItemRarityID.Orange;

	private float Mana => 0.7f * Roll;
	private double Cooldown => 40.0 - 30.0 * Roll;

	protected override object[] TooltipArgs => [ Round( Mana * 100.0f ), Round( Cooldown ), Round( Timer ) ];

	internal override void Update( ref Boost boost ) {
		if ( Timer == 0.0 && Player.statMana < 20 ) {
			Timer = Cooldown;
			Player.statMana += ( int )( Player.statManaMax2 * Mana );
		}
	}
}

internal class MirrorShield : Bauble {
	internal override int Rarity => ItemRarityID.Orange;

	private float Dodge => 0.006f * Roll;
	private int Def => ( int )( Player.statDefense * Roll );

	protected override object[] TooltipArgs => [ DisplayValue( Roll * 100.0f ), DisplayValue( Dodge * 100.0f ), DisplayValue( -Def ), DisplayValue( Dodge * Def * 100.0f ) ];

	internal override void Update( ref Boost boost ) {
		boost.Defense -= Def;
		boost.Dodge += Dodge * Def;
	}
}

internal class BloodsoakedFang : Bauble {
	internal override int Rarity => ItemRarityID.Orange;

	private float Crit => 0.015f * Roll;
	private double Time => 10 * Roll;

	protected override object[] TooltipArgs => [ DisplayValue( Crit * 100.0f ), Round( Time ), Round( Timer ), DisplayValue( Stacks * Crit * 100.0f ) ];

	internal override void Update( ref Boost boost ) {
		if ( Timer > 0.0 )
			boost.Crit *= 1.0f + Crit * Stacks;
		else Stacks = 0;
	}

	internal override void KillNPC( NPC npc ) {
		Timer = Time;
		Stacks++;
	}
}

internal class TomeOfFrenzy : Bauble {
	internal override int Rarity => ItemRarityID.LightRed;

	private readonly int[] Usage = new int[ 60 ];
	private int Index = 0;

	private int MPS => Usage.Sum();
	private int LastMana = 0;
	private float Cost => 0.4f * Roll * Negative;
	private float Damage => 0.009f * Roll * Negative;

	protected override object[] TooltipArgs => [ DisplayValue( Damage * 100.0f ), DisplayValue( Cost * 100.0f ), DisplayValue( MPS * Damage * 100.0f ), Round( MPS ) ];

	internal override void Update( ref Boost boost ) {
		int change = Player.statMana - LastMana;
		LastMana = Player.statMana;
		Usage[ Index ] = change < 0 ? -change : 0;

		Index = ( Index + 1 ) % 60;

		Player.manaCost += Cost;

		boost.Damage *= 1.0f + MPS * Damage;
	}
}

internal class FluffyToughyTeddyBear : Bauble {
	internal override int Rarity => ItemRarityID.LightRed;

	private float Defense => 0.18f * Roll * Negative;
	private double BuffTime => 12.0 * Roll;

	protected override object[] TooltipArgs => [ DisplayValue( Defense ), Round( BuffTime ), DisplayValue( Defense * Stacks * 100.0f ), Round( Timer ) ];

	internal override void Update( ref Boost boost ) {
		if ( Timer <= 0.0 )
			Stacks = 0;
		else
			boost.Defense *= 1.0f + Defense * Stacks;
	}

    internal override void OnCrit( int tier ) {
		Timer = BuffTime;
		Stacks = Math.Max( Stacks, tier );
	}
}

internal class ShellOfPrey : Bauble {
	internal override int Rarity => ItemRarityID.LightRed;

	private float Defense => 2.0f * Roll;
	private float Range => 15.0f - 10.0f * Roll;
	private int Nearby => NearbyEnemy( Range ).Count();

	protected override object[] TooltipArgs => [ DisplayValue( Defense ), ( int )Range, ( int )( Defense * Nearby ) ];

	internal override void Update( ref Boost boost ) => boost.Defense += ( int )( Defense * Nearby );
}

internal class ChestersBloodVessel : Bauble {
	internal override int Rarity => ItemRarityID.LightRed;

	private float Stat => 0.06f * Roll;
	private float Steal => ( 1.0f - Player.statLife / ( float )Player.statLifeMax2 ) * Stat;

	protected override object[] TooltipArgs => [ DisplayValue( Stat * 100.0f ), DisplayValue( Steal * 100.0f ) ];

	internal override void Update( ref Boost boost ) {
		Player.lifeRegenTime = 0.0f;
		boost.LifeSteal += Steal;
	}
}

internal class IllogicalManaTubes : Bauble {
	internal override int Rarity => ItemRarityID.LightRed;

	private float Stat => 0.04f * Roll;
	private float Steal => ( 1.0f - Player.statMana / ( float )Player.statManaMax2 ) * Stat;

	protected override object[] TooltipArgs => [ DisplayValue( Stat * 100.0f ), DisplayValue( Steal * 100.0f ) ];

	internal override void Update( ref Boost boost ) {
		Player.manaRegenDelay = 90.0f;
		boost.ManaSteal += Steal;
	}
}

internal class TinyCloningMachine : Bauble {
	internal override int Rarity => ItemRarityID.Quest;

	private float Mana => 2.85f - 2.25f * Roll;

	protected override object[] TooltipArgs => [ DisplayValue( Mana * 100.0f ) ];

	internal override bool CanConsume( Item weapon ) => false;
    internal override bool CanUseItem( Item item ) {
		int use = ( int )( ( item.damage + ( item.ammo > 0 ? Player.ChooseAmmo( item ).damage : 0 ) ) * Mana );

		bool e = item.useAmmo > 0 || item.consumable && item.damage > 0;

		if ( e && Player.statMana >= use ) {
			Player.statMana -= use;
			Player.manaRegenDelay = 90.0f;

			return true;
		}

		return !e;
	}
}

internal class OverclockedOvercharger : Bauble {
	internal override int Rarity => ItemRarityID.Expert;

	private int Health => ( int )( 60.0f - 40.0f * Roll );

	protected override object[] TooltipArgs => [ Health ];

    internal override void Update( ref Boost boost ) {
		boost.Life -= Health;
        boost.PoweredSlots += 1;
    }
}

internal class PracticalGuideBook : Bauble {
	internal override int Rarity => ItemRarityID.Master;

	private double Cooldowntime => 8.0 * Roll * -Negative;

	protected override object[] TooltipArgs => [ DisplayValue( Cooldowntime ) ];

	internal override void KillNPC( NPC npc ) {
		if ( !Player.TryGetModPlayer( out TriggerPlayer MP ) )
			return;

		MP.Worn.ForEach( x => x.Timer = Math.Max( x.Timer + Cooldowntime, 0.0 ) );
	}
}

internal class TestBauble : Bauble {
	internal override int Rarity => ItemRarityID.Expert;

	private int Slots => ( int )( 6.0f * Roll * Negative );

	protected override object[] TooltipArgs => [ DisplayValue( Slots ) ];

    internal override void Update( ref Boost boost ) => boost.BaubleSlots += Slots;
}