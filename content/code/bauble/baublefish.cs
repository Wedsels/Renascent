using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace Renascent.content.code.bauble;

internal class BaubleFish : ModPlayer {
    public override void CatchFish( FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition ) {
        foreach ( var i in Bauble.Instances ) {
            if ( !i.FishingBiome( Player, attempt ) || Main.rand.NextDouble() >= i.SpawnChance * ( 1.0 + attempt.fishingLevel * 0.005 ) )
                continue;

            itemDrop = i.Type;
            npcSpawn = -1;

            sonar.Text = i.DisplayName.Value;
            sonar.Color = Terraria.GameContent.UI.ItemRarity.GetColor( i.Rarity );
            sonar.Velocity = Vector2.Zero;
            sonar.DurationInFrames = 120;
        }
    }
}