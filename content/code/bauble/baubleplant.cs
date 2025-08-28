using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Localization;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Renascent.content.code.bauble;

internal abstract class BaublePlant : ModTile {
    internal static HashSet< BaublePlant > Plants = [];

	private bool Grown( int i, int j ) {
		Tile tile = Framing.GetTileSafely( i, j );
		return Size.X - Width <= tile.TileFrameX;
	}

    internal const short BlockSize = 18;

    internal short TileWidth => ( short )Math.Ceiling( Size.X / BlockSize );
    internal short TileHeight => ( short )Math.Ceiling( Size.Y / BlockSize );
    internal short Width => ( short )( Size.X / Frames );
    internal virtual ushort Frames => 1;
    internal virtual short Dust => DustID.Ambient_DarkBrown;
    internal virtual SoundStyle Sound => SoundID.Grass;

    internal abstract string LocalizedItem { get; }
    internal abstract Color MapColor { get; }

    private Vector2 Size;

    internal enum Stems { TopLeft, TopMiddle, TopRight, BottomLeft, BottomMiddle, BottomRight };
    internal virtual Stems Stem => Stems.BottomMiddle;

    internal virtual int[] AnchorTiles => [];
    internal virtual bool AnchorBottom => true;

	internal virtual double SpawnChance => 0.01;
    
	public override bool CanDrop( int i, int j ) => Grown( i, j );
	public override bool IsTileSpelunkable( int i, int j ) => Grown( i, j );

	public override void RandomUpdate( int i, int j ) {
		if ( !Grown( i, j ) ) {
			Framing.GetTileSafely( i, j ).TileFrameX += Width;

			if ( Main.netMode != NetmodeID.SinglePlayer )
				NetMessage.SendTileSquare( -1, i, j, 1 );
		}
	}

    public override void EmitParticles( int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible ) {
        if ( visible && Main.rand.NextBool( 150 ) )
            Terraria.Dust.NewDust( new( i * 16.0f + 8.0f, j * 16.0f + 8.0f ), 0, 0, Dust);
    }

    private static readonly TileObjectData[][] SizeData = [
        [ TileObjectData.Style1x1, TileObjectData.Style1x2, TileObjectData.Style1xX ],
        [ TileObjectData.Style2x1, TileObjectData.Style2x2, TileObjectData.Style2xX ],
        [ TileObjectData.Style3x2, TileObjectData.Style3x2, TileObjectData.Style3x3 ]
    ];

    public override bool PreDraw( int i, int j, SpriteBatch spriteBatch ) {
        Tile tile = Framing.GetTileSafely( i, j );

        if ( TileObjectData.IsTopLeft( tile ) )
            Main.instance.TilesRenderer.AddSpecialPoint( i, j, ( Terraria.GameContent.Drawing.TileDrawing.TileCounterType )( AnchorBottom ? 4 : 5 ) );

        return false;
    }

	public override void SetStaticDefaults() {
		Main.tileCut[ Type ] = true;
		Main.tileNoFail[ Type ] = true;
		Main.tileLavaDeath[ Type ] = true;
		Main.tileObsidianKill[ Type ] = true;
		Main.tileFrameImportant[ Type ] = true;
        TileID.Sets.MultiTileSway[ Type ] = true;
		TileID.Sets.ReplaceTileBreakUp[ Type ] = true;
		TileID.Sets.IgnoredInHouseScore[ Type ] = true;
		TileID.Sets.IgnoredByGrowingSaplings[ Type ] = true;
		TileMaterials.SetForTileId( Type, TileMaterials._materialsByName[ "Plant" ] );

        Size = ModContent.Request< Texture2D >( Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad ).Size();

		DustType = Dust;
		HitSound = Sound;

        TileObjectData.newTile.CopyFrom( SizeData[ Math.Clamp( TileWidth - 1, 0, SizeData.Length ) ][ Math.Clamp( TileHeight - 1, 0, SizeData.Length ) ] );
        TileObjectData.newTile.DrawYOffset = 2;

        if ( AnchorBottom ) {
            TileObjectData.newTile.AnchorBottom = new AnchorData( AnchorType.SolidTile, TileObjectData.newTile.Width, 0 );
            TileObjectData.newTile.AnchorTop = default;
        } else {
            TileObjectData.newTile.AnchorBottom = default;
            TileObjectData.newTile.AnchorTop = new AnchorData( AnchorType.SolidTile, TileObjectData.newTile.Width, 0 );
        }

        TileObjectData.newTile.AnchorValidTiles = AnchorTiles;
        TileObjectData.addTile( Type );

        Main.tileOreFinderPriority[ Type ] = ( short )( 800 + 800 * SpawnChance );
        AddMapEntry( MapColor, Language.GetText( "Mods." + Mod.Name + ".Items." + LocalizedItem + ".DisplayName" ) );

        Plants.Add( this );
	}
}

internal class BaublePlantTile : GlobalTile {
    public override void RandomUpdate( int i, int j, int type ) {
        if ( !Framing.GetTileSafely( i, j ).HasUnactuatedTile )
            return;

        double roll = Main.rand.NextDouble();

        foreach ( var p in BaublePlant.Plants ) {
            if ( roll >= p.SpawnChance )
                continue;

            Point target = new( i, j + ( p.AnchorBottom ? 1 : -1 ) );

            if ( Framing.GetTileSafely( target ).TileType == p.Type )
                continue;

            WorldGen.PlaceTile( target.X, target.Y, p.Type, true );

            if ( Framing.GetTileSafely( target ).TileType == p.Type ) {
                if ( Main.netMode == NetmodeID.Server )
                    NetMessage.SendTileSquare( -1, target.X, target.Y );

                Console.WriteLine( p.Name + " - " + target );

                break;
            }
        }
    }
}