using MonoMod.Cil;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Renascent.content.code;

internal class TrashPlayer  : ModPlayer {
    internal static int ChestUpgrade;
    internal const int ChestUpgrades = 5;

	internal static readonly Dictionary< string, Item > Items = [];
    
    // internal static TrashPlayer TP => Main.LocalPlayer.GetModPlayer< TrashPlayer >();

	internal static readonly List< Item > Trash = [];

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
			if ( info.ToContext != 6 ) return;
			Trash.Add( Main.LocalPlayer.trashItem.Clone() );
		};
    }

    public override void PostUpdate() {
        if ( Trash.Count > 0 && Trash[ ^1 ].IsAir )
			Trash[ ^1 ] = Main.LocalPlayer.trashItem.Clone();
    }

    public override bool OnPickup( Item item ) {
		Mimic.Speak( "Looks Tasty..." );

        return true;
    }

    public override void LoadData( TagCompound tag ) {
	    foreach ( var i in Items )
			Items[ i.Key ] = tag.Get< Item >( i.Key );
    }
    
    public override void SaveData( TagCompound tag ) {
	    foreach ( var i in Items )
			tag[ i.Key ] = i.Value;
    }
}