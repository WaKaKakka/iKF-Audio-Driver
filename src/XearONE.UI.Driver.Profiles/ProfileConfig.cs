using System.Collections.Generic;
using Newtonsoft.Json;

namespace XearONE.UI.Driver.Profiles;

public class ProfileConfig
{
	[JsonProperty("ProfileName")]
	public string profileName { get; set; }

	[JsonProperty("Templates")]
	public List<ProfileTemplate> templates { get; set; }

	public ProfileConfig()
	{
		profileName = string.Empty;
		templates = new List<ProfileTemplate>();
	}
}
