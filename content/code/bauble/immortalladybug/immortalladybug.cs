using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Utilities;

namespace Renascent.content.code.bauble.immortalladybug;

internal class ImmortalLadyBug : Bauble {
    internal override int Rarity => ItemRarityID.Expert;
    protected override int CreateNPC => ModContent.NPCType< ImmortalLadyBugCritter >();

	private double Cooldowntime => 480.0 - 240.0 * Roll;
	private float Life => 0.5f * Roll;

	protected override object[] TooltipArgs => [ Round( Cooldowntime ), Round( Life * 100.0f ), Round( Timer ) ];

	internal override bool PreKill( double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource ) {
		if ( Timer <= 0.0 ) {
			Timer = Cooldowntime;
			Player.statLife += ( int )( Player.statLifeMax2 * Life );
		}

		return Timer < Cooldowntime - 1;
	}
}

internal class ImmortalLadyBugCritter : BaubleCritter {
    protected override int Parent => NPCID.GoldLadyBug;
    protected override int Dust => DustID.Teleporter;
    protected override int ItemType => ModContent.ItemType< ImmortalLadyBug >();
    protected override float SpawningChance => 0.001f;
    protected override Nets Net => Nets.Golden;
    protected override SpawnCondition Location => SpawnCondition.Underground;
    protected override SpawnConditionBestiaryInfoElement Biome => Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundHallow;

    public override void ModifyIncomingHit( ref NPC.HitModifiers modifiers ) {
        modifiers.HideCombatText();

        NPC.life = 0;
        NPC.active = false;
        NPC.timeLeft = 0;

        int ran = Main.rand.Next( 40 );
        for ( int i = 0; i < ran; i++ ) {
            Dust dust = Terraria.Dust.NewDustDirect( NPC.position, NPC.width, NPC.height, Dust, 2 * modifiers.HitDirection, -2f );
            if ( Main.rand.NextBool( 2 ) ) {
                dust.noGravity = true;
                dust.scale = 1.2f * NPC.scale;
            } else dust.scale = 0.7f * NPC.scale;
        }
    }
}