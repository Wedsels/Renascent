using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Terraria.DataStructures;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework;

namespace Renascent.content.code.bauble;

internal abstract class BaubleCritter : ModNPC {
    protected enum Nets { Net = ItemID.BugNet, Fireproof = ItemID.FireproofBugNet, Golden = ItemID.GoldenBugNet }
    protected virtual Nets Net => Nets.Net;
    protected virtual int Parent => NPCID.Frog;
    protected abstract int Dust { get; }
    protected abstract int ItemType { get; }
    protected abstract float SpawningChance { get; }
    protected abstract SpawnCondition Location { get; }
    protected abstract SpawnConditionBestiaryInfoElement Biome { get; }

    public override LocalizedText DisplayName => Lang.GetItemName( ItemType );

    private Item Source;

    public override void OnSpawn( IEntitySource source ) {
        if ( source is EntitySource_Parent parent && parent.Entity is Player player )
            Source = player.HeldItem.Clone();
    }

    public override void SetStaticDefaults() {
        Main.npcCatchable[ Type ] = true;
        Main.npcFrameCount[ Type ] = Main.npcFrameCount[ Parent ];

        NPCID.Sets.TownCritter[ Type ] = true;
        NPCID.Sets.CountsAsCritter[ Type ] = true;
        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[ Type ] = true;
        NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert( NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf( Parent ) + 1, Type );
    }

    public override void SetDefaults() {
        Source = new( ItemType );

        NPC.CloneDefaults( Parent );

        NPC.catchItem = ItemType;
        NPC.lavaImmune = true;
        AIType = Parent;
        AnimationType = Parent;
    }

    public override void HitEffect( NPC.HitInfo hit ) {
        int ran = Main.rand.Next( 16 );
        for ( int i = 0; i < ran; i++ ) {
            Dust dust = Terraria.Dust.NewDustDirect( NPC.position, NPC.width, NPC.height, Dust, 2 * hit.HitDirection, -2f );
            if ( Main.rand.NextBool( 2 ) ) {
                dust.noGravity = true;
                dust.scale = 1.2f * NPC.scale;
            } else dust.scale = 0.7f * NPC.scale;
        }
    }

    public override void DrawEffects( ref Color drawColor ) {
        if ( !Main.rand.NextBool( 1000 ) )
            return;

        Dust dust = Terraria.Dust.NewDustDirect( NPC.position, NPC.width, NPC.height, Dust, Main.rand.NextFloat( -2.0f, -2.0f ), Main.rand.NextFloat( -2.0f, -2.0f ) );
        if ( Main.rand.NextBool( 2 ) ) {
            dust.noGravity = true;
            dust.scale = 1.2f * NPC.scale;
        } else dust.scale = 0.7f * NPC.scale;
    }

    public override bool? CanBeCaughtBy( Item item, Player player ) {
        switch ( item.type ) {
            case ( int )Nets.Net: if ( Net != Nets.Net ) return false; break;
            case ( int )Nets.Fireproof: if ( Net == Nets.Golden ) return false; break;
            case ( int )Nets.Golden: break;
            default: return false;
        }

        if ( item.type == ( int )Net ) {
            NPC.life = 0;
            NPC.active = false;
            NPC.timeLeft = 0;

            Item.NewItem( new EntitySource_Caught( player, NPC ), NPC.position, Source.Clone() );
        }

        return false;
    }

    public override void SetBestiary( BestiaryDatabase database, BestiaryEntry bestiaryEntry ) => bestiaryEntry.AddTags( Biome );
    public override float SpawnChance( NPCSpawnInfo spawnInfo ) => Location.Chance * SpawningChance;

    public override void SaveData( TagCompound tag ) => tag[ "Source" ] = Source;
    public override void LoadData( TagCompound tag ) => Source = tag.Get< Item >( "Source" );

    public override void SendExtraAI( BinaryWriter writer ) => ItemIO.Send( Source, writer ); 
    public override void ReceiveExtraAI( BinaryReader reader ) => Source = ItemIO.Receive( reader );
}