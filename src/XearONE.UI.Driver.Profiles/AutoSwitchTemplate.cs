using Newtonsoft.Json;

namespace XearONE.UI.Driver.Profiles;

public class AutoSwitchTemplate
{
	[JsonProperty("WidgetName")]
	public string widgetName { get; set; }

	[JsonProperty("AppPath")]
	public string appPath { get; set; }

	[JsonProperty("BindingProfileName")]
	public string bindingProfileName { get; set; }

	[JsonProperty("Icon")]
	public string icon { get; set; }

	public AutoSwitchTemplate()
	{
		widgetName = string.Empty;
		appPath = string.Empty;
		bindingProfileName = string.Empty;
		icon = string.Empty;
	}
}
