using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Renascent.content.code;

internal class Bauble : ModItem {
    private static List< Texture2D > Potion = [];

    private static readonly List< string > FoodNames = [];
    private static List< Texture2D > Food = [];

    private static readonly List< Texture2D > Book = [];

    protected static Texture2D Egg = UI.Texture( "Eggs" );

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
    }

    private bool Draw( SpriteBatch SpriteBatch, Vector2 Center, float Scale, Color Color ) {
        SpriteBatch.Draw(
            Sprite,
			Center - new Vector2( Sprite.Width * Scale / 2, Sprite.Height * Scale / 2 ),
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

    private Texture2D Sprite => Book[ 20 ];

    public override void LoadData( TagCompound tag ) {
        Item.width = Sprite.Width;
        Item.height = Sprite.Height;
    }

    public override bool PreDrawInInventory( SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale ) => Draw( spriteBatch, position, scale * 3.5f, drawColor );
    public override bool PreDrawInWorld( SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI ) => Draw( spriteBatch, Item.Center - Main.screenPosition, scale *= 1.2f, lightColor );
}