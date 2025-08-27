using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace Renascent.content.code.bauble;

internal class BaubleNPC : GlobalNPC {
    public override void ModifyNPCLoot( NPC npc, NPCLoot npcLoot ) {
        foreach ( var i in Bauble.Instances )
            if ( i.NPC.Contains( npc.type ) )
                npcLoot.Add( ItemDropRule.Common( i.Type, ( int )( 1.0 / i.SpawnChance ) ) );
    }
}