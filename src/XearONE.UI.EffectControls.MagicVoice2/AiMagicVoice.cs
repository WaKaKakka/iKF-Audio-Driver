using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Cmedia;
using Newtonsoft.Json;
using XearONE.UI.EffectControls.RecordingEffect;
using XearONE.UI.Helpers;

namespace XearONE.UI.EffectControls.MagicVoice2;

internal class AiMagicVoice : IMagicVoice
{
	private Guid AudioEndpointUID = Guid.Empty;

	private Root myDeserializedClass;

	private const string RVC_REG_SUB_KEY = "Software\\C-Media\\RVC\\";

	private List<string> targetVoiceFriendlyNames = new List<string>();

	private int lastErrorCode;

	private string lastErrorString = "";

	public int GetLastErrorCode()
	{
		return lastErrorCode;
	}

	public string GetLastErrorString()
	{
		return lastErrorString;
	}

	private void DebugMessageWriteLine(string message)
	{
	}

	public AiMagicVoice(Guid audioEndpointUID)
	{
		AudioEndpointUID = audioEndpointUID;
		string rvcWorkFolder = ToolsHelper.GetRegistryDataString("Software\\C-Media\\RVC\\", "RvcWorkFolder");
		string targetVoicesFilepath = rvcWorkFolder + "\\pretrain_target\\TargetVoices.json";
		if (File.Exists(targetVoicesFilepath))
		{
			string myJsonResponse = File.ReadAllText(targetVoicesFilepath);
			myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
			foreach (TargetVoice target in myDeserializedClass.TargetVoices)
			{
				targetVoiceFriendlyNames.Add(target.friendlyName);
			}
			if (myDeserializedClass.TargetVoices.Count < 4)
			{
				lastErrorCode = -995;
				lastErrorString = "TargetVoices.json does Not have 4 targets.";
				DebugMessageWriteLine("Error: " + lastErrorString);
				return;
			}
			foreach (TargetVoice target2 in myDeserializedClass.TargetVoices)
			{
				if (!File.Exists(rvcWorkFolder + "\\pretrain_target\\" + target2.fileName))
				{
					lastErrorCode = -992;
					lastErrorString = "Unable to find the target voice file: " + target2.fileName;
					return;
				}
			}
			if (Process.GetProcessesByName("CmRVCServerCpp").Length != 0)
			{
				return;
			}
			DebugMessageWriteLine("CmRVCServerCpp is NOT running. Starting CmRVCServerCpp.exe...");
			try
			{
				Process.Start(rvcWorkFolder + "\\CmRVCServerCpp.exe");
				if (Process.GetProcessesByName("CmRVCServerCpp").Length == 0)
				{
					lastErrorCode = -993;
					lastErrorString = "Unable to find CmRVCServerCpp running.";
				}
				return;
			}
			catch
			{
				lastErrorCode = -994;
				lastErrorString = "Unable to run CmRVCServerCpp.";
				return;
			}
		}
		lastErrorCode = -991;
		lastErrorString = "Error: Unable to find TargetVoices.json file in pretrain_target folder!";
	}

	public List<string> GetTargetVoiceFriendlyNames()
	{
		return targetVoiceFriendlyNames;
	}

	public string GetTargetVoiceTargetName(int index)
	{
		return myDeserializedClass.TargetVoices[index].targetName;
	}

	public int SetTarget(MagicVoice2Target target)
	{
		int inValue = (int)(target & (MagicVoice2Target)0x7F);
		ToolsHelper.SetRegistryDataInt("Software\\C-Media\\RVC\\", "TargetVoiceIndex", inValue);
		XearSDK.SetProperty(AudioEndpointUID, "AI_MagicVoice_TargetIndex", inValue, out var cmErr);
		return (int)cmErr;
	}

	public MagicVoice2Target GetTarget()
	{
		XearSDK.GetProperty(AudioEndpointUID, "AI_MagicVoice_TargetIndex", out int outDats, out XearSDK.CmErr cmErr);
		if (cmErr == XearSDK.CmErr.CE_OK)
		{
			return (MagicVoice2Target)(0x80 | outDats);
		}
		lastErrorCode = (int)cmErr;
		lastErrorString = "Unable to get AI_MagicVoice_TargetIndex property.";
		return MagicVoice2Target.Monster;
	}

	public double GetDefaultPitchValue(int targetVoiceIndex, int speakerGender)
	{
		int pitchEstimationResult = GetPitchEstimationResult();
		string pitchEstAll = ToolsHelper.GetRegistryDataString("Software\\C-Media\\RVC\\", "PitchEstAll");
		if (pitchEstimationResult == 0 && pitchEstAll != "")
		{
			Dictionary<string, int> dictPitch = new Dictionary<string, int>();
			string[] items = pitchEstAll.Split(',');
			if (items.Length % 2 == 0)
			{
				for (int i = 0; i < items.Length; i += 2)
				{
					dictPitch.Add(items[i], Convert.ToInt32(items[i + 1]));
				}
				int num = dictPitch[myDeserializedClass.TargetVoices[targetVoiceIndex].targetName];
				lastErrorCode = 0;
				lastErrorString = "Return default pitch according pitch estimation.";
				return (double)num / 10.0;
			}
			lastErrorCode = -1;
			lastErrorString = "PitchEstAll registry data string is Not valid!";
		}
		else
		{
			lastErrorCode = 0;
			lastErrorString = "Pitch estimation is Not available, just use default pitch according speaker gender and target gender.";
			string targetGender = myDeserializedClass.TargetVoices[targetVoiceIndex].gender;
			if (speakerGender == 1 && targetGender == "Male")
			{
				return 0.0;
			}
			if (speakerGender == 1 && targetGender == "Female")
			{
				return 8.0;
			}
			if (speakerGender == 0 && targetGender == "Male")
			{
				return -8.0;
			}
			if (speakerGender == 0)
			{
				_ = targetGender == "Female";
				return 0.0;
			}
		}
		return 0.0;
	}

	public double GetPitch()
	{
		XearSDK.GetProperty(AudioEndpointUID, "AI_MagicVoice_Pitch", out int outDats, out XearSDK.CmErr cmErr);
		if (cmErr == XearSDK.CmErr.CE_OK)
		{
			return (float)outDats / 10f;
		}
		lastErrorCode = (int)cmErr;
		lastErrorString = "Unable to get AI_MagicVoice_Pitch property.";
		return 0.0;
	}

	public int SetEnable(bool enable)
	{
		int inValue = (enable ? 1 : 0);
		XearSDK.SetProperty(AudioEndpointUID, "AI_MagicVoice_Enable", inValue, out var cmErr);
		if (enable)
		{
			inValue = 1;
			XearSDK.SetProperty(AudioEndpointUID, "AI_NR_Enable", inValue, out cmErr);
		}
		else
		{
			bool AiNrEnable = true;
			RecordingEffectHandler recordingEffectHandler = new RecordingEffectHandler(AudioEndpointUID);
			if (!recordingEffectHandler.IsEnabled())
			{
				AiNrEnable = false;
			}
			else if (recordingEffectHandler.GetLastMode() != RecordingEffectMode.AINR)
			{
				AiNrEnable = false;
			}
			if (!AiNrEnable)
			{
				inValue = 0;
				XearSDK.SetProperty(AudioEndpointUID, "AI_NR_Enable", inValue, out cmErr);
			}
		}
		return (int)cmErr;
	}

	public bool IsEnabled()
	{
		bool enable = false;
		XearSDK.GetProperty(AudioEndpointUID, "AI_MagicVoice_Enable", out int outDats, out XearSDK.CmErr cmErr);
		if (cmErr == XearSDK.CmErr.CE_OK)
		{
			return outDats == 1;
		}
		lastErrorCode = (int)cmErr;
		lastErrorString = "Unable to get AI Magic Voice Enable property.";
		return enable;
	}

	public bool GetAutoPitchEnable()
	{
		if (ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "AutoF0Enable") == 1)
		{
			return true;
		}
		return false;
	}

	public int SetPitchValue(double pitchValue)
	{
		int value = (int)(pitchValue * 10.0);
		XearSDK.SetProperty(AudioEndpointUID, "AI_MagicVoice_Pitch", value, out var _);
		return ToolsHelper.SetRegistryDataInt("Software\\C-Media\\RVC\\", "Pitch", value);
	}

	private int GetGPUInfo(ref string gpuName, ref string videoMemory, ref string systemMemory, ref string hardwareCheck)
	{
		int gpuInfoCount = ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "GPUInfoCount");
		gpuName = "";
		videoMemory = "";
		systemMemory = "";
		hardwareCheck = "";
		for (int i = 0; i < gpuInfoCount; i++)
		{
			string valueName = "GPUInfo" + i;
			string[] lines = ToolsHelper.GetRegistryDataString("Software\\C-Media\\RVC\\", valueName).Split(',');
			if (lines.Length < 5)
			{
				continue;
			}
			try
			{
				if (Convert.ToInt32(lines[3]) != 0)
				{
					gpuName = lines[0];
					videoMemory = lines[1];
					systemMemory = lines[2];
					hardwareCheck = lines[4];
					int indexVideoMem = videoMemory.IndexOf(".");
					int indexSystemMem = systemMemory.IndexOf(".");
					if (indexVideoMem != -1 && indexSystemMem != -1 && videoMemory.Length >= indexVideoMem + 2 && systemMemory.Length >= indexSystemMem)
					{
						videoMemory = videoMemory.Substring(0, indexVideoMem + 2);
						systemMemory = systemMemory.Substring(0, indexSystemMem + 2);
					}
					break;
				}
			}
			catch
			{
				return -1;
			}
		}
		if (gpuName == "" || videoMemory == "" || systemMemory == "" || hardwareCheck == "")
		{
			return -2;
		}
		return 0;
	}

	public bool IsAvailable()
	{
		IsEnabled();
		if (lastErrorCode != 0)
		{
			return false;
		}
		return ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "dummy_check_result") == 1;
	}

	public float GetAutoPitchValue(ref bool autoPitchFinish)
	{
		int inValue = 4;
		XearSDK.SetProperty(AudioEndpointUID, "AI_MagicVoice_DebugMode", inValue, out var _);
		autoPitchFinish = ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "AutoF0Finish") != 0;
		return (float)ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "AutoF0Value") / 10f;
	}

	public bool IsPitchEstimationDone()
	{
		if (ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "PitchEstResult") == 0)
		{
			return true;
		}
		return false;
	}

	public int StartPitchEstimation()
	{
		SetEnable(enable: true);
		int inValue = 2;
		XearSDK.SetProperty(AudioEndpointUID, "AI_MagicVoice_DebugMode", inValue, out var cmErr);
		return (int)cmErr;
	}

	public int StopPitchEstimation()
	{
		int inValue = 3;
		XearSDK.SetProperty(AudioEndpointUID, "AI_MagicVoice_DebugMode", inValue, out var cmErr);
		return (int)cmErr;
	}

	public int SetDebugModePropertyDefaultSetting()
	{
		int inValue = 0;
		XearSDK.SetProperty(AudioEndpointUID, "AI_MagicVoice_DebugMode", inValue, out var cmErr);
		return (int)cmErr;
	}

	public int GetPitchEstimationResult()
	{
		return ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "PitchEstResult");
	}

	public float GetPitchEstimationValue()
	{
		return (float)ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "PitchEstValue") / 10f;
	}

	public int ClearPitchEstimationResult()
	{
		return ToolsHelper.SetRegistryDataInt("Software\\C-Media\\RVC\\", "PitchEstResult", "1");
	}

	public int SetAiNrRecordingMode(int mode)
	{
		int inValue = 0;
		XearSDK.CmErr cmErr;
		switch (mode)
		{
		case 0:
			inValue = 0;
			XearSDK.SetProperty(AudioEndpointUID, "AI_NR_Enable", inValue, out cmErr);
			inValue = 0;
			XearSDK.SetProperty(AudioEndpointUID, "AI_AEC_Enable", inValue, out cmErr);
			return 0;
		case 1:
			inValue = 1;
			XearSDK.SetProperty(AudioEndpointUID, "AI_NR_Enable", inValue, out cmErr);
			inValue = 0;
			XearSDK.SetProperty(AudioEndpointUID, "AI_AEC_Enable", inValue, out cmErr);
			return 0;
		case 2:
			inValue = 0;
			XearSDK.SetProperty(AudioEndpointUID, "AI_NR_Enable", inValue, out cmErr);
			inValue = 1;
			XearSDK.SetProperty(AudioEndpointUID, "AI_AEC_Enable", inValue, out cmErr);
			break;
		}
		return 0;
	}

	public int GetAiNrRecordingMode()
	{
		int outValueAINR = 0;
		int outValueAEC = 0;
		XearSDK.GetProperty(AudioEndpointUID, "AI_NR_Enable", out int outDats, out XearSDK.CmErr cmErr);
		outValueAINR = outDats;
		if (cmErr != XearSDK.CmErr.CE_OK)
		{
			lastErrorCode = (int)cmErr;
			lastErrorString = "Unable to find AI_NR_Enable property.";
			return 0;
		}
		XearSDK.GetProperty(AudioEndpointUID, "AI_AEC_Enable", out outDats, out cmErr);
		outValueAEC = outDats;
		if (cmErr != XearSDK.CmErr.CE_OK)
		{
			lastErrorCode = (int)cmErr;
			lastErrorString = "Unable to find AI_AEC_Enable property.";
			return 0;
		}
		if (outValueAINR == 0 && outValueAEC == 0)
		{
			return 0;
		}
		if (outValueAINR == 1)
		{
			return 1;
		}
		if (outValueAEC == 1)
		{
			return 2;
		}
		return 0;
	}

	public int GetSpeakerGender()
	{
		return ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "SpeakerGender");
	}

	public int SetSpeakerGender(int gender)
	{
		return ToolsHelper.SetRegistryDataInt("Software\\C-Media\\RVC\\", "SpeakerGender", gender);
	}

	public int GetCheckResults(ref int cpuCheckResult, ref int gpuCheckResult, ref int dummyCheckResult)
	{
		cpuCheckResult = ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "cpu_check_result");
		gpuCheckResult = ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "gpu_check_result");
		dummyCheckResult = ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "dummy_check_result");
		return 0;
	}

	public bool IsChangeTargetDone()
	{
		if (ToolsHelper.GetRegistryDataInt("Software\\C-Media\\RVC\\", "IsChangeTargetDone") == 1)
		{
			return true;
		}
		return false;
	}
}
