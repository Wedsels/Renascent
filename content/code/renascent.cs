using System.IO;
using Terraria.ModLoader;

namespace Renascent.content.code;

internal class Renascent : Mod {
	internal static string FileText => "RenascentLastUpgrade";
	internal static int LastUpgrade;

	public override void Load() {
		if ( File.Exists( FileText ) )
			int.TryParse( File.ReadAllText( FileText ), out LastUpgrade );
	}
}