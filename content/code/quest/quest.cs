using System;
using System.Linq;
using System.Collections.Generic;

namespace Renascent.content.code.quest;

internal abstract class Quest {
    internal int ID;

    // internal static readonly Dictionary< int, Quest > Quests = [];
	// static Quest() {
	// 	foreach ( var i in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where( t => t.IsSubclassOf( typeof( Quest ) ) && !t.IsAbstract ).Select( t => ( Quest )Activator.CreateInstance( t )! ).ToList() ) {
    //         i.ID = Renascent.Hash( i.ToString() );
    //         Quests.Add( i.ID, i );
	// 		Quests[ i.ID ].Initialize();
	// 	}
	// }

    internal virtual int QuestStarterItemType => -1;
    internal virtual bool Repeatable => false;

    internal abstract void Reward();

    internal virtual void Initialize() {}
}