using System;
using Cmedia;
using XearONE.UI.Driver;
using XearONE.UI.Helpers;

namespace XearONE.UI.EffectControls.RecordingEffect;

internal class RecordingEffectHandler
{
	private Guid AudioEndpointUID = Guid.Empty;

	private int lastErrorCode;

	private string lastErrorString = "";

	private RecordingEffectMode noiseRedunctionMode = RecordingEffectMode.None;

	private IniFileHelper iniRecordingEffect;

	public int GetLastErrorCode()
	{
		return lastErrorCode;
	}

	public string GetLastErrorString()
	{
		return lastErrorString;
	}

	public RecordingEffectHandler(Guid audioEndpointUID)
	{
		AudioEndpointUID = audioEndpointUID;
		iniRecordingEffect = DriverInstance.getInstance().iniRecordingEffect;
	}

	public bool IsEnabled()
	{
		if (iniRecordingEffect.ReadString("IsEnabled", "", "Settings") == "true")
		{
			return true;
		}
		return false;
	}

	public RecordingEffectMode GetLastMode()
	{
		return iniRecordingEffect.ReadString("LastMode", "", "Settings") switch
		{
			"Compressor" => RecordingEffectMode.Compressor, 
			"AINR" => RecordingEffectMode.AINR, 
			"LDR" => RecordingEffectMode.LDR, 
			_ => RecordingEffectMode.None, 
		};
	}

	public int SetMode(RecordingEffectMode mode)
	{
		int inValue = 0;
		switch (mode)
		{
		case RecordingEffectMode.Compressor:
			inValue = 1;
			SetProperty("Compressor_Enable", inValue);
			inValue = 1;
			SetProperty("NR_Enable", inValue);
			inValue = 0;
			SetProperty("HD_NR_Enable", inValue);
			inValue = 0;
			SetProperty("AI_NR_Enable", inValue);
			iniRecordingEffect.Write("LastMode", "Compressor", "Settings");
			iniRecordingEffect.Write("IsEnabled", "true", "Settings");
			break;
		case RecordingEffectMode.LDR:
			inValue = 1;
			SetProperty("Compressor_Enable", inValue);
			inValue = 0;
			SetProperty("NoiseGate_Enable", inValue);
			inValue = 24;
			SetProperty("Compressor_Threshold", inValue);
			inValue = 10;
			SetProperty("Compressor_KneeWidth", inValue);
			inValue = 8;
			SetProperty("Compressor_Ratio", inValue);
			inValue = 21;
			SetProperty("Compressor_MakeUpGain", inValue);
			inValue = 2;
			SetProperty("Compressor_AttackTime", inValue);
			inValue = 100;
			SetProperty("Compressor_ReleaseTime", inValue);
			inValue = 0;
			SetProperty("AI_NR_Enable", inValue);
			inValue = 0;
			SetProperty("NR_Enable", inValue);
			inValue = 1;
			SetProperty("HD_NR_Enable", inValue);
			iniRecordingEffect.Write("LastMode", "LDR", "Settings");
			iniRecordingEffect.Write("IsEnabled", "true", "Settings");
			break;
		case RecordingEffectMode.AINR:
			inValue = 1;
			SetProperty("AI_NR_Enable", inValue);
			inValue = 0;
			SetProperty("Compressor_Enable", inValue);
			inValue = 0;
			SetProperty("NoiseGate_Enable", inValue);
			inValue = 0;
			SetProperty("NR_Enable", inValue);
			inValue = 0;
			SetProperty("HD_NR_Enable", inValue);
			iniRecordingEffect.Write("LastMode", "AINR", "Settings");
			iniRecordingEffect.Write("IsEnabled", "true", "Settings");
			break;
		case RecordingEffectMode.None:
			inValue = 0;
			SetProperty("NoiseGate_Enable", inValue);
			inValue = 0;
			SetProperty("Compressor_Enable", inValue);
			inValue = 0;
			SetProperty("AI_NR_Enable", inValue);
			inValue = 0;
			SetProperty("NR_Enable", inValue);
			inValue = 0;
			SetProperty("HD_NR_Enable", inValue);
			iniRecordingEffect.Write("IsEnabled", "false", "Settings");
			break;
		}
		noiseRedunctionMode = mode;
		return 0;
	}

	public int GetProperty(string key, ref int outValue)
	{
		if (XearSDK.GetProperty(AudioEndpointUID, key, out int outDats, out XearSDK.CmErr cmErr))
		{
			outValue = outDats;
			return (int)cmErr;
		}
		return (int)cmErr;
	}

	public int SetProperty(string key, int inValue)
	{
		XearSDK.SetProperty(AudioEndpointUID, key, inValue, out var cmErr);
		return (int)cmErr;
	}

	public void SaveCompressorSettings()
	{
		int outValue = 0;
		outValue = 0;
		if (GetProperty("Compressor_Threshold", ref outValue) == 0)
		{
			iniRecordingEffect.Write("Compressor_Threshold", outValue.ToString(), "CompressorSettings");
		}
		outValue = 0;
		if (GetProperty("Compressor_Ratio", ref outValue) == 0)
		{
			iniRecordingEffect.Write("Compressor_Ratio", outValue.ToString(), "CompressorSettings");
		}
		outValue = 0;
		if (GetProperty("Compressor_KneeWidth", ref outValue) == 0)
		{
			iniRecordingEffect.Write("Compressor_KneeWidth", outValue.ToString(), "CompressorSettings");
		}
		outValue = 0;
		if (GetProperty("Compressor_AttackTime", ref outValue) == 0)
		{
			iniRecordingEffect.Write("Compressor_AttackTime", outValue.ToString(), "CompressorSettings");
		}
		outValue = 0;
		if (GetProperty("Compressor_ReleaseTime", ref outValue) == 0)
		{
			iniRecordingEffect.Write("Compressor_ReleaseTime", outValue.ToString(), "CompressorSettings");
		}
		outValue = 0;
		if (GetProperty("Compressor_MakeUpGain", ref outValue) == 0)
		{
			iniRecordingEffect.Write("Compressor_MakeUpGain", outValue.ToString(), "CompressorSettings");
		}
	}

	public void RestoreCompressorSettings()
	{
		string value = "";
		try
		{
			value = iniRecordingEffect.ReadString("Compressor_Threshold", "", "CompressorSettings");
			if (value != "")
			{
				SetProperty("Compressor_Threshold", Convert.ToInt32(value));
			}
			value = iniRecordingEffect.ReadString("Compressor_Ratio", "", "CompressorSettings");
			if (value != "")
			{
				SetProperty("Compressor_Ratio", Convert.ToInt32(value));
			}
			value = iniRecordingEffect.ReadString("Compressor_KneeWidth", "", "CompressorSettings");
			if (value != "")
			{
				SetProperty("Compressor_KneeWidth", Convert.ToInt32(value));
			}
			value = iniRecordingEffect.ReadString("Compressor_AttackTime", "", "CompressorSettings");
			if (value != "")
			{
				SetProperty("Compressor_AttackTime", Convert.ToInt32(value));
			}
			value = iniRecordingEffect.ReadString("Compressor_ReleaseTime", "", "CompressorSettings");
			if (value != "")
			{
				SetProperty("Compressor_ReleaseTime", Convert.ToInt32(value));
			}
			value = iniRecordingEffect.ReadString("Compressor_MakeUpGain", "", "CompressorSettings");
			if (value != "")
			{
				SetProperty("Compressor_MakeUpGain", Convert.ToInt32(value));
			}
		}
		catch
		{
		}
	}
}
