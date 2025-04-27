using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Renascent.content.code;

internal class Bauble : ModItem {
	internal readonly EffectStore Store = new();
	
	private static readonly Vector2 PotionRange = new Vector2( 0f,  252f );

    private static readonly List< Texture2D > Potion = [];
    private const float PotionSize = 16f;

    private static readonly List< string > FoodNames = [];
    private static readonly List< Texture2D > Food = [];
    private const float FoodSize = 16f;

    private static readonly List< Texture2D > Book = [];
    private const float BookSize = 64f;

    private static readonly List< Texture2D > Egg = [];
    private const float EggSize = 64f;

    public override void Load() {
        for ( int i = 1; i <= 253; i++ )
            Potion.Add( UI.Texture( "potion/potion" + i ) );

        foreach ( var i in Mod.GetFileNames() )
            if ( i.Contains( "content/texture/food/" ) )
                FoodNames.Add( i[ ..i.IndexOf( '.' ) ][ ( i.LastIndexOf( '/' ) + 1 ).. ] );
        FoodNames.Sort();
        foreach ( var i in FoodNames )
            Food.Add( UI.Texture( "food/" + i ) );

        for ( int i = 1; i <= 24; i++ )
            Book.Add( UI.Texture( "book/book" + i ) );

        for ( int i = 1; i <= 20; i++ )
            Egg.Add( UI.Texture( "egg/egg_" + i ) );
    }

    private System.Type type;
    private Texture2D Sprite;

    public override void SetDefaults() {
        Sprite = Main.rand.Next( Food );
        type = Main.rand.Next( EffectStore.Effects ).GetType();
        Store.Bonus[ type ] = Main.rand.NextFloat( 2f );
    }

    private bool Draw( SpriteBatch SpriteBatch, Vector2 Center, float Scale, Color Color ) {
		Scale = ItemSlot.InventorySlotSize / FoodSize * 0.65f;

        Item.width = ( int )( Sprite.Width * Scale );
        Item.height = ( int )( Sprite.Height * Scale );
        SpriteBatch.Draw(
			Sprite,
			Center - new Vector2( FoodSize * Scale / 2f ),
			null,
			Color,
			0f,
			Vector2.Zero,
			Scale,
			SpriteEffects.None,
			0f
		);

        return false;
    }

    public override void ModifyTooltips( List< TooltipLine > tooltips ) {
	    tooltips.Add( new( Mod, "bonus", type.Name + " " + ( Store.Bonus[ type ] - 1f ) + " scaling" ) );
    }

    public override bool PreDrawInInventory( SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale ) => Draw( spriteBatch, position, scale * 3.5f, drawColor );
    public override bool PreDrawInWorld( SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI ) => Draw( spriteBatch, Item.Center - Main.screenPosition, scale *= 1.2f, lightColor );
}