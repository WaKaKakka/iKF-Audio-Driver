using System.Collections.Generic;
using Newtonsoft.Json;

namespace XearONE.UI.Driver.Settings;

public class JsonEQCustom
{
	[JsonProperty("Name")]
	public string name { get; set; } = "";

	[JsonProperty("EQValues")]
	public List<int> eqValues { get; set; }
}
