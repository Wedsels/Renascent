using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Renascent.content.code;

internal class TrashPlayer  : ModPlayer {
	internal readonly List< Item > Trash = [];

	internal readonly Dictionary< string, Item > Items = [];
	internal int MimicUpgrade;

    public override void Load() {
        IL_Main.OnCharacterNamed += context => new ILCursor( context ).EmitDelegate( () => { 
				Mimic.Speak( "I could've come up with better." );
		} );

        IL_Main.OnWorldNamed += context => new ILCursor( context ).EmitDelegate( () => { 
				Mimic.Speak( "I could've come up with better." );
		} );
		
        IL_WorldGen.CreateNewWorld += context => new ILCursor( context ).EmitDelegate( () => { 
				Mimic.Speak( "This is gonna take a while." );
		} );
		
		Terraria.UI.ItemSlot.OnItemTransferred += info => {
			if ( info.ToContext != 6 || !Main.LocalPlayer.TryGetModPlayer( out TrashPlayer tp ) ) return;
			tp.Trash.Add( Main.LocalPlayer.trashItem.Clone() );
		};
    }

    public override void PostUpdate() {
        if ( Trash.Count > 0 && Trash[ ^1 ].IsAir )
			Trash[ ^1 ] = Player.trashItem.Clone();
		
		if ( Renascent.LastUpgrade != MimicUpgrade ) {
			var stream = File.CreateText( Renascent.FileText );
			stream.Write( Renascent.LastUpgrade = MimicUpgrade );
			stream.Close();
		}
    }

    public override bool OnPickup( Item item ) {
		Mimic.Speak( "Looks Tasty..." );

        return true;
    }

    public override void LoadData( TagCompound tag ) {
	    foreach ( var i in ItemSlot.Items )
			Items[ i ] = tag.Get< Item >( i );

		MimicUpgrade = tag.Get< int >( "mimicupgrade" );
    }
    
    public override void SaveData( TagCompound tag ) {
	    foreach ( var i in Items.Keys )
			tag[ i ] = Items[ i ];
			
		tag[ "mimicupgrade" ] = MimicUpgrade;
    }
}