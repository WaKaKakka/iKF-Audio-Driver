using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XearONE.UI.Driver.Profiles;

public class ProfileTemplate
{
	[JsonProperty("InstanceID")]
	public string instanceID { get; set; }

	[JsonProperty("VendorID")]
	public string vendorID { get; set; }

	[JsonProperty("ProductID")]
	public string productID { get; set; }

	[JsonProperty("PlaybackProperties")]
	public Dictionary<string, JToken> playbackProperties { get; set; }

	[JsonProperty("CaptureProperties")]
	public Dictionary<string, JToken> captureProperties { get; set; }

	public ProfileTemplate()
	{
		instanceID = "";
		vendorID = "0000";
		productID = "0000";
		playbackProperties = new Dictionary<string, JToken>
		{
			{ "AINoiseReduction", 0 },
			{ "AINoiseReduction_Level", 0 },
			{ "EnvironmentEffects", 0 },
			{ "EnvironmentEffects_Mode", 0 },
			{ "EnvironmentEffects_Room", 0 },
			{ "Equalizer10Bands", 0 },
			{ "EqBnad1", 0 },
			{ "EqBnad2", 0 },
			{ "EqBnad3", 0 },
			{ "EqBnad4", 0 },
			{ "EqBnad5", 0 },
			{ "EqBnad6", 0 },
			{ "EqBnad7", 0 },
			{ "EqBnad8", 0 },
			{ "EqBnad9", 0 },
			{ "EqBnad10", 0 },
			{ "SpeakerShifter", 0 },
			{ "LF_ANGLE", -45 },
			{ "RF_ANGLE", 45 },
			{ "LS_ANGLE", -90 },
			{ "RS_ANGLE", 90 },
			{ "LB_ANGLE", -135 },
			{ "RB_ANGLE", 135 },
			{ "C_ANGLE", 0 },
			{ "LFE_ANGLE", 15 },
			{ "LF_DIST", -4 },
			{ "RF_DIST", -4 },
			{ "LS_DIST", -4 },
			{ "RS_DIST", -4 },
			{ "LB_DIST", -4 },
			{ "RB_DIST", -4 },
			{ "C_DIST", -4 },
			{ "LFE_DIST", -4 },
			{ "SurroundMax", 0 },
			{ "XearAudioBrilliant", 0 },
			{ "XearAudioBrilliant_Level", 3 },
			{ "XearDynamicBass", 0 },
			{ "XearDynamicBass_Level", 5 },
			{ "XearDynamicBass_Freq", 200 },
			{ "XearSingFX_KeyShifting", 0 },
			{ "XearSingFX_KeyShifting_Level", 0 },
			{ "XearSingFX_VocalFading", 0 },
			{ "XearSingFX_VocalFading_Level", 50 },
			{ "XearSmartVolume", 0 },
			{ "XearSmartVolume_Level", 70 },
			{ "XearSmartVolume_Mode", 0 },
			{ "XearSurroundHeadphone", 0 },
			{ "XearSurroundHeadphone_Mode", 0 },
			{ "XearSurroundHeadphone_Room", 1 },
			{ "XearVoiceClarity", 0 },
			{ "XearVoiceClarity_Level", 3 },
			{ "XearVoiceClarity_NSLevel", 0 },
			{ "HeadTracking", 0 }
		};
		captureProperties = new Dictionary<string, JToken>
		{
			{ "NoiseReduction", 0 },
			{ "NoiseReduction_Level", 15 },
			{ "HDNoiseReduction", 0 },
			{ "AINoiseReduction", 0 },
			{ "AINoiseReduction_Level", 0 },
			{ "Equalizer10Bands", 0 },
			{ "EqBnad1", 0 },
			{ "EqBnad2", 0 },
			{ "EqBnad3", 0 },
			{ "EqBnad4", 0 },
			{ "EqBnad5", 0 },
			{ "EqBnad6", 0 },
			{ "EqBnad7", 0 },
			{ "EqBnad8", 0 },
			{ "EqBnad9", 0 },
			{ "EqBnad10", 0 },
			{ "Compressor", 0 },
			{ "NoiseGate", 0 },
			{ "LDR", 0 },
			{ "XearSingFX_Echo", 0 },
			{ "XearSingFX_Echo_Level", 0 },
			{ "XearSingFX_MagicVoice", 0 },
			{ "XearSingFX_MagicVoice_Mode", 0 },
			{ "AIMagicVoice", 0 }
		};
	}
}
