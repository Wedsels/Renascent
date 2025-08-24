using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Terraria.ModLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Renascent.content.code;

internal class Renascent : Mod {
	internal static Server Server = ModContent.GetInstance< Server >();
	internal static Client Client = ModContent.GetInstance< Client >();
}

internal class Dumper {
	private class AllFields : DefaultContractResolver {
		protected override IList< JsonProperty > CreateProperties( Type type, MemberSerialization serial ) =>
			type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).Select( f =>
				{ var prop = base.CreateProperty( f, serial ); prop.Readable = true; prop.Writable = true; return prop; } ).ToList();
	}

	private static readonly JsonSerializerSettings Settings = new() {
		ContractResolver = new AllFields(),
		Formatting = Formatting.Indented
	};

	internal static void Dump( object obj ) => Console.WriteLine( JsonConvert.SerializeObject( obj, Settings ) );
}