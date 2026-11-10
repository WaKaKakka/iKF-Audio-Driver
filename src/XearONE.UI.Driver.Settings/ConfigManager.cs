using System.Collections.Generic;
using System.IO;
using System.Linq;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.Driver.Profiles;
using XearONE.UI.Helpers;

namespace XearONE.UI.Driver.Settings;

public class ConfigManager
{
	private JsonConfigHelper configHelper;

	private string appConfigPath = string.Empty;

	private string appTempPath = string.Empty;

	public AutoSave<JsonTempConfig> jsonTempConfigAutoSave;

	private JsonHeadTrackingConfig _jsonHeadTrackingConfig;

	private Dictionary<string, string> dictLangDisplay = new Dictionary<string, string>();

	private const string pathFileName = "Path.json";

	private const string appConfigFileName = "Settings.json";

	private const string headTrackingConfigFileName = "HeadTracking.json";

	private const string tempConfigFileName = "Settings.json";

	private const string endpointListOrderFileName = "EndpointListOrder.json";

	private const string endpointListShowFileName = "EndpointListShow.json";

	private const string eqProfileFileName = "EQProfile.json";

	private const string ledFileName = "LED.json";

	private const string langDisplayName = "LanguageDisplayName.json";

	public JsonAppConfig jsonAppConfig { get; set; }

	public JsonTempConfig jsonTempConfig
	{
		get
		{
			return jsonTempConfigAutoSave.Value;
		}
		set
		{
			jsonTempConfigAutoSave.Update((JsonTempConfig tempConfig) =>
			{
				tempConfig.Language = value.Language;
				tempConfig.DisableMainGuide = value.DisableMainGuide;
				tempConfig.CurrentProfileName = value.CurrentProfileName;
			});
		}
	}

	public JsonHeadTrackingConfig jsonHeadTrackingConfig
	{
		get
		{
			if (_jsonHeadTrackingConfig == null)
			{
				_jsonHeadTrackingConfig = configHelper.Load<JsonHeadTrackingConfig>(GetAppConfigPath("HeadTracking.json"));
			}
			return _jsonHeadTrackingConfig;
		}
	}

	public Dictionary<string, string> GetLangDisplay()
	{
		return dictLangDisplay;
	}

	private string GetAppConfigPath(string fileName)
	{
		return Path.Combine(appConfigPath, fileName);
	}

	private string GetTempConfigPath(string fileName)
	{
		return Path.Combine(appTempPath, fileName);
	}

	private string GetLangDisplayNamePath()
	{
		return Path.Combine(appConfigPath, "Resources", "Language", "LanguageDisplayName.json");
	}

	private string GetProfileConfigPath(string fileName)
	{
		return Path.Combine(appTempPath, "Profiles", fileName);
	}

	public string GetProfileConfigPath()
	{
		return Path.Combine(appTempPath, "Profiles.json");
	}

	public string GetAutoSwitchConfigPath()
	{
		return Path.Combine(appTempPath, "AutoSwitches.json");
	}

	public string GetAutoSwitchIconPath()
	{
		return Path.Combine(appTempPath, "AutoSwitchIcon");
	}

	public string GetEQPresetPath()
	{
		return Path.Combine(appConfigPath, "EQPreset.json");
	}

	public string GetEQCustomPath()
	{
		return Path.Combine(appTempPath, "EQCustom.json");
	}

	public string GetEffectStatusPath(JackDeviceInfo devInfo)
	{
		return Path.Combine(appTempPath, "EffectStatus", $"EffectStatus_0x{devInfo.usbVendorID:X4}_0x{devInfo.usbProductID:X4}_{devInfo.jackType}.json");
	}

	public ConfigManager(string configPath, string tempPath)
	{
		appConfigPath = configPath;
		appTempPath = tempPath;
		configHelper = new JsonConfigHelper();
		jsonAppConfig = configHelper.Load<JsonAppConfig>(GetAppConfigPath("Settings.json"));
		jsonTempConfigAutoSave = new AutoSave<JsonTempConfig>(GetTempConfigPath("Settings.json"), configHelper.Load<JsonTempConfig>, configHelper.Save);
		dictLangDisplay = LoadLangDisplay();
		if (!File.Exists(GetAppConfigPath("Path.json")))
		{
			JsonTempPathConfig jsonTempPathConfig = new JsonTempPathConfig
			{
				Path = tempPath
			};
			configHelper.Save(GetAppConfigPath("Path.json"), jsonTempPathConfig);
		}
	}

	public void SaveEndpointListOrder(List<string> deviceList)
	{
		Dictionary<string, string> dict = ListToIndexedDict(deviceList);
		configHelper.Save(GetTempConfigPath("EndpointListOrder.json"), dict);
	}

	public List<string> LoadEndpointListOrder()
	{
		Dictionary<string, string> dict = configHelper.Load<Dictionary<string, string>>(GetTempConfigPath("EndpointListOrder.json"));
		return IndexedDictToList(dict);
	}

	public void SaveEndpointListShow(Dictionary<string, int> deviceList)
	{
		configHelper.Save(GetTempConfigPath("EndpointListShow.json"), deviceList);
	}

	public Dictionary<string, int> LoadEndpointListShow()
	{
		return configHelper.Load<Dictionary<string, int>>(GetTempConfigPath("EndpointListShow.json"));
	}

	public void SaveEQPreset(List<JsonEQPreset> eqPreset)
	{
		configHelper.Save(GetEQPresetPath(), eqPreset);
	}

	public List<JsonEQPreset> LoadEQPreset()
	{
		return configHelper.Load<List<JsonEQPreset>>(GetEQPresetPath());
	}

	public void SaveEQCustom(List<JsonEQCustom> eqCustom)
	{
		configHelper.Save(GetEQCustomPath(), eqCustom);
	}

	public List<JsonEQCustom> LoadEQCustom()
	{
		return configHelper.Load<List<JsonEQCustom>>(GetEQCustomPath());
	}

	private Dictionary<string, string> LoadLangDisplay()
	{
		return configHelper.Load<Dictionary<string, string>>(GetLangDisplayNamePath());
	}

	public void SaveProfile(List<ProfileConfig> profileConfigs)
	{
		configHelper.Save(GetProfileConfigPath(), profileConfigs);
	}

	public List<ProfileConfig> LoadProfile()
	{
		return configHelper.Load<List<ProfileConfig>>(GetProfileConfigPath());
	}

	public void SaveAutoSwitch(List<AutoSwitchTemplate> autoSwitchTemplates)
	{
		configHelper.Save(GetAutoSwitchConfigPath(), autoSwitchTemplates);
	}

	public List<AutoSwitchTemplate> LoadAutoSwitch()
	{
		return configHelper.Load<List<AutoSwitchTemplate>>(GetAutoSwitchConfigPath());
	}

	public void SaveEffectStatus(JackDeviceInfo devInfo, JsonEffectStatus effectStatus)
	{
		configHelper.Save(GetEffectStatusPath(devInfo), effectStatus);
	}

	public JsonEffectStatus LoadEffectStatus(JackDeviceInfo devInfo)
	{
		return configHelper.Load<JsonEffectStatus>(GetEffectStatusPath(devInfo));
	}

	private Dictionary<string, string> ListToIndexedDict(List<string> list, string prefix = "Device")
	{
		return list.Select((string item, int index) => new KeyValuePair<string, string>($"{prefix}{index:00}", item)).ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key, (KeyValuePair<string, string> kvp) => kvp.Value);
	}

	private List<string> IndexedDictToList(Dictionary<string, string> dict, string prefix = "Device")
	{
		List<string> list = new List<string>();
		int i = 0;
		while (true)
		{
			string key = $"{prefix}{i:00}";
			if (!dict.ContainsKey(key))
			{
				break;
			}
			list.Add(dict[key]);
			i++;
		}
		return list;
	}
}
