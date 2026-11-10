using System.Collections.Generic;
using Newtonsoft.Json;

namespace XearONE.UI.Driver.Settings;

public class JsonEQPreset
{
	[JsonProperty("Index")]
	public int index { get; set; }

	[JsonProperty("EQValues")]
	public List<int> eqValues { get; set; }
}
