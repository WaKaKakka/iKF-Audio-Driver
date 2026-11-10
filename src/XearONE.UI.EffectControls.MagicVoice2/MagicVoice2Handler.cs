using System;
using System.Threading.Tasks;
using Cmedia;

namespace XearONE.UI.EffectControls.MagicVoice2;

public class MagicVoice2Handler
{
	private AiMagicVoice aiMagicVoice;

	private TradMagicVoice tradMagicVoice;

	private IMagicVoice curMagicVoice;

	private string endpointID = "";

	public int GetLastErrorCode()
	{
		if (aiMagicVoice != null && aiMagicVoice.GetLastErrorCode() != 0)
		{
			return aiMagicVoice.GetLastErrorCode();
		}
		if (tradMagicVoice != null && tradMagicVoice.GetLastErrorCode() != 0)
		{
			return tradMagicVoice.GetLastErrorCode();
		}
		return 0;
	}

	public string GetLastErrorString()
	{
		if (aiMagicVoice != null && aiMagicVoice.GetLastErrorString() != "")
		{
			return aiMagicVoice.GetLastErrorString();
		}
		if (tradMagicVoice != null && tradMagicVoice.GetLastErrorString() != "")
		{
			return tradMagicVoice.GetLastErrorString();
		}
		return "";
	}

	public MagicVoice2Handler(Guid audioEndpointUID)
	{
		aiMagicVoice = new AiMagicVoice(audioEndpointUID);
		tradMagicVoice = new TradMagicVoice(audioEndpointUID);
		if (aiMagicVoice.GetLastErrorCode() == 0)
		{
			curMagicVoice = aiMagicVoice;
		}
		else
		{
			curMagicVoice = tradMagicVoice;
		}
		curMagicVoice.IsAvailable();
		XearSDK.GetProperty(audioEndpointUID, "EndpointID", out endpointID, out XearSDK.CmErr _);
	}

	public bool IsEnabled()
	{
		bool flag = aiMagicVoice.IsEnabled();
		bool enableTrad = tradMagicVoice.IsEnabled();
		return flag | enableTrad;
	}

	public int SetEnable(bool enable)
	{
		int result = -1;
		if (enable)
		{
			if (curMagicVoice == tradMagicVoice)
			{
				result = aiMagicVoice.SetEnable(enable: false);
			}
			else if (curMagicVoice == aiMagicVoice)
			{
				result = tradMagicVoice.SetEnable(isEnable: false);
			}
			return curMagicVoice.SetEnable(enable: true);
		}
		result = aiMagicVoice.SetEnable(enable: false);
		return tradMagicVoice.SetEnable(isEnable: false);
	}

	public int SetTarget(MagicVoice2Target toTarget)
	{
		int result = -1;
		if ((toTarget & MagicVoice2Target.ManAi) != 0)
		{
			if (toTarget != aiMagicVoice.GetTarget())
			{
				result = aiMagicVoice.SetTarget(toTarget);
			}
			curMagicVoice = aiMagicVoice;
		}
		else
		{
			if (toTarget != tradMagicVoice.GetTarget())
			{
				result = tradMagicVoice.SetTarget(toTarget);
			}
			curMagicVoice = tradMagicVoice;
		}
		return result;
	}

	public MagicVoice2Target GetTarget()
	{
		return curMagicVoice.GetTarget();
	}

	public int SetPitchValue(double pitchValue)
	{
		return aiMagicVoice.SetPitchValue(pitchValue);
	}

	public double GetDefaultAIMagicVoicePitchValue(MagicVoice2Target target)
	{
		return aiMagicVoice.GetDefaultPitchValue((int)(target & (MagicVoice2Target)0x7F), 1);
	}

	public bool IsPitchEstimationDone()
	{
		return aiMagicVoice.IsPitchEstimationDone();
	}

	public async Task StartPitchEstimation(IProgress<string> _asrProgress)
	{
		if (SoundRecorder.IsRecording)
		{
			_asrProgress.Report("Error: Unable start pitch estimation because the microphone is being used.");
			return;
		}
		aiMagicVoice.StartPitchEstimation();
		SoundRecorder.TargetName = "PitchEst";
		SoundRecorder.PitchValue = 0f;
		SoundRecorder.IsListenToMic = false;
		await SoundRecorder.StartRecordingAsync(_asrProgress, endpointID, outputRecordingFile: false, drawWaveForm: true);
		int pitchResult = aiMagicVoice.GetPitchEstimationResult();
		float pitchValueFloat = aiMagicVoice.GetPitchEstimationValue();
		_asrProgress.Report("[PitchEstimationResult] " + pitchResult + " " + pitchValueFloat);
	}

	public void StopPitchEstimation()
	{
		aiMagicVoice.StopPitchEstimation();
		SoundRecorder.StopRecording();
		aiMagicVoice.SetDebugModePropertyDefaultSetting();
	}

	public async Task StartRecording(IProgress<string> _asrProgress)
	{
		if (SoundRecorder.IsRecording)
		{
			_asrProgress.Report("Error: Unable start recording because the microphone is being used.");
		}
		else
		{
			await SoundRecorder.StartRecordingAsync(_asrProgress, endpointID);
		}
	}

	public void StopRecording()
	{
		SoundRecorder.StopRecording();
	}

	public int ClearPitchEstimationResult()
	{
		return aiMagicVoice.ClearPitchEstimationResult();
	}

	public bool IsAIMagicVoiceAvailable()
	{
		return aiMagicVoice.IsAvailable();
	}

	public bool IsSysCheckResultPass()
	{
		int cpuCheckResult = 0;
		int gpuCheckResult = 0;
		int dummyCheckResult = 0;
		aiMagicVoice.GetCheckResults(ref cpuCheckResult, ref gpuCheckResult, ref dummyCheckResult);
		if (dummyCheckResult == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsCpuCheckResultPass()
	{
		int cpuCheckResult = 0;
		int gpuCheckResult = 0;
		int dummyCheckResult = 0;
		aiMagicVoice.GetCheckResults(ref cpuCheckResult, ref gpuCheckResult, ref dummyCheckResult);
		return cpuCheckResult != 0;
	}

	public bool IsGpuCheckResultPass()
	{
		int cpuCheckResult = 0;
		int gpuCheckResult = 0;
		int dummyCheckResult = 0;
		aiMagicVoice.GetCheckResults(ref cpuCheckResult, ref gpuCheckResult, ref dummyCheckResult);
		return gpuCheckResult != 0;
	}

	public bool IsChangeTargetDone()
	{
		return aiMagicVoice.IsChangeTargetDone();
	}
}
