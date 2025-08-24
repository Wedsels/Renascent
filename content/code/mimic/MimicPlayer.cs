using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using MonoMod.Cil;
using Microsoft.Xna.Framework;

using Renascent.content.code.ui;
using Renascent.content.code.bauble;

namespace Renascent.content.code.mimic;

internal class MimicPlayer : ModPlayer {
	internal readonly List< Item > Trash = [];
	
	internal const int BaubleMaxSlots = 20;
	internal const int UniqueBaubleMaxSlots = 3;
	internal int PoweredSlotCount = 0;
    internal int UnlockedSlotCount = 0;
	
	internal readonly bool[] Vanity = new bool[ BaubleMaxSlots ];
    internal readonly ItemSlot[] Baubles = new ItemSlot[ BaubleMaxSlots ];
    internal Item[] UnlockedSlots => [ .. Baubles.Select( x => x.Item ).ToList().GetRange( 0, UnlockedSlotCount ) ];

    internal readonly ItemSlot[] UniqueBaubles = new ItemSlot[ UniqueBaubleMaxSlots ];

	internal bool[] Digesting = new bool[ ItemLoader.ItemCount ];
	internal int[] Tolerance = new int[ BuffLoader.BuffCount ];
	internal bool[] ActiveTolerance = new bool[ BuffLoader.BuffCount ];

	internal int MimicUpgrade;

    public override void Load() {
        IL_Main.OnWorldNamed += context => new ILCursor( context ).EmitDelegate( () => Mimic.Speak( "NewName" ) );
        IL_Main.OnCharacterNamed += context => new ILCursor( context ).EmitDelegate( () => Mimic.Speak( "NewName" ) );
        IL_WorldGen.CreateNewWorld += context => new ILCursor( context ).EmitDelegate( () => Mimic.Speak( "Loading" ) );

		Terraria.UI.ItemSlot.OnItemTransferred += info => {
			if ( info.ToContext == 6 )
				Mimic.Consume( Main.LocalPlayer.trashItem.IsAir ? Main.LocalPlayer.HeldItem.Clone() : Main.LocalPlayer.trashItem.Clone() );
		};
    }

    public override void PreUpdateBuffs() {
		for ( int i = 0; i < ActiveTolerance.Length; i++ )
			if ( ActiveTolerance[ i ] )
				Player.AddBuff( i, 2 );
    }

    public override void PostUpdate() {
		for ( int i = 0; i < BaubleMaxSlots; i++ )
			if ( i >= UnlockedSlotCount && !Baubles[ i ].Item.IsAir ) {
				Item.NewItem( new EntitySource_OverfullInventory( Main.LocalPlayer ), Main.LocalPlayer.position, Vector2.Zero, Baubles[ i ].Item );
				Baubles[ i ].Item.SetDefaults();
			}
    
        if ( Trash.Count > 0 && Trash[ ^1 ].IsAir )
			Trash[ ^1 ] = Player.trashItem.Clone();
		
		if ( Mimic.LastUpgrade != MimicUpgrade ) {
			Mimic.LastUpgrade = MimicUpgrade;
			Renascent.Client.SaveChanges();
		}

		if ( Mimic.StartConsume > 0.0 && Math.Abs( Mimic.StartConsume - Main.timeForVisualEffects ) > 60.0 )
			Mimic.Digest();
    }

    public override bool OnPickup( Item item ) {
		if ( Digesting[ item.type ] ) {
			Mimic.Consume( item );
			item.SetDefaults();

			return false;
		}

		if ( Main.rand.NextFloat() <= 0.005f * ( item.rare + 1.0f ) )
			Mimic.Speak( "Pickup" );

        return true;
    }
  
    public override void SaveData( TagCompound tag ) {
	    for ( int i = 0; i < BaubleMaxSlots; i++ )
			tag[ "Bauble" + i ] = Baubles[ i ]?.Item;

	    for ( int i = 0; i < UniqueBaubleMaxSlots; i++ )
			tag[ "UniqueBauble" + i ] = UniqueBaubles[ i ]?.Item;
  
		tag[ "mimicupgrade" ] = MimicUpgrade;

		tag[ "Vanity" ] = Vanity;
		tag[ "Digesting" ] = Digesting;
		tag[ "Tolerance" ] = Tolerance;
		tag[ "ActiveTolerance" ] = ActiveTolerance;
    }

    public override void LoadData( TagCompound tag ) {
	    for ( int i = 0; i < BaubleMaxSlots; i++ ) {
			int index = i;

			Baubles[ i ] = new( tag.Get< Item >( "Bauble" + i ) ) {
				Check = () => Main.mouseItem.ModItem is Bauble,
				Toggle = () => Vanity[ index ] = !Vanity[ index ],
				ColorCheck = () => Vanity[ index ] ? Colors.Default : ( Bauble.IsPowered( index ) ? Colors.Vanity : Colors.Red ),
				BackgroundCheck = () => Textures.Accessory,
				HoverCheck = () => Bauble.IsPowered( index ) ? "Powered Bauble Slot" : "Bauble Slot"
			};
	    }

	    for ( int i = 0; i < UniqueBaubleMaxSlots; i++ ) {
			int index = i;

			UniqueBaubles[ i ] = new( tag.Get< Item >( "UniqueBauble" + i ) ) {
				Check = () => Main.mouseItem.ModItem is Bauble,
				ColorCheck = () => Colors.Unique,
				BackgroundCheck = () => index switch { 0 => Textures.Head, 1 => Textures.Chest, _ => Textures.Leg },
				HoverCheck = () => "Unique Bauble Slot"
			};
	    }
  
		MimicUpgrade = tag.Get< int >( "mimicupgrade" );

		tag.Get< bool[] >( "Vanity" ).CopyTo( Vanity, 0 );
		tag.Get< bool[] >( "Digesting" ).CopyTo( Digesting, 0 );
		tag.Get< int[] >( "Tolerance" ).CopyTo( Tolerance, 0 );
		tag.Get< bool[] >( "ActiveTolerance" ).CopyTo( ActiveTolerance, 0 );
    }
}