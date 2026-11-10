using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XearONE.UI.Driver.Settings;

public class JsonEffectStatus
{
	[JsonProperty("Enable")]
	public bool enable { get; set; } = true;

	[JsonProperty("Properties")]
	public Dictionary<string, JToken> properties { get; set; }

	public JsonEffectStatus()
	{
		enable = true;
		properties = new Dictionary<string, JToken>();
	}
}
