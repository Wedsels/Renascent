using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Renascent.content.code;

public class Server : ModConfig {
    public override ConfigScope Mode => ConfigScope.ServerSide;
}

public class Client : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [ DefaultValue( 0 ) ]
    public int LastMimicUpgrade;

    [ DefaultValue( 6 ) ]
    public int DigestionColumns;
    [ DefaultValue( 10 ) ]
    public int DigestionRows;

    [ DefaultValue( 6 ) ]
    public int ToleranceColumns;
    [ DefaultValue( 10 ) ]
    public int ToleranceRows;
}