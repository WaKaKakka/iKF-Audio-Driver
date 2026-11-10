using System;
using System.Runtime.InteropServices;

namespace Cmedia;

public static class XearSDK
{
	public enum CmErr
	{
		CE_OK,
		CE_UnknownError,
		CE_LicenseError,
		CE_UnsupportedError,
		CE_DeviceError,
		CE_ParameterError,
		CE_PropertyError,
		CE_EventError
	}

	public enum CmCallbackType
	{
		CCT_Unknown,
		CCT_Log,
		CCT_DeviceState,
		CCT_DefaultDevice,
		CCT_DefaultFormat,
		CCT_DeviceVolume,
		CCT_EffectValue,
		CCT_AudioSession,
		CCT_SystemDynamicLighting,
		CCT_Foreground
	}

	public enum CmDataType
	{
		CDT_None,
		CDT_ArrayedGUID,
		CDT_ArrayedInt32,
		CDT_ArrayedFloat,
		CDT_ArrayedByte,
		CDT_ArrayedCmDataWString,
		CDT_ArrayedCmDataLogSettings,
		CDT_ArrayedCmDataEventSettings,
		CDT_ArrayedCmDataAudioFormat,
		CDT_ArrayedCmDataRangeFloat,
		CDT_ArrayedCmDataChannelVolume,
		CDT_ArrayedCmDataProcessInfo,
		CDT_ArrayedCmDataVigilanceMode
	}

	public enum CmLogType
	{
		CLT_None,
		CLT_Console,
		CLT_File,
		CLT_Callback
	}

	public enum CmDataFlow
	{
		CDF_Unknown,
		CDF_Render,
		CDF_Capture
	}

	public enum CmJackType
	{
		CJT_Unknown,
		CJT_Speaker,
		CJT_Microphone,
		CJT_Line,
		CJT_SPDIF,
		CJT_AUX,
		CJT_CDPlayer,
		CJT_Headphone,
		CJT_StereoMix,
		CJT_HDMI,
		CJT_DesktopMicrophone,
		CJT_SPDIFInRCA,
		CJT_SpeakerQuarter,
		CJT_Headset,
		CJT_DesktopSpeaker,
		CJT_FrontMic,
		CJT_FrontHeadphone,
		CJT_EchoSuppressingSpeakerphone,
		CJT_SpeakerPhone,
		CJT_MicrophoneArray,
		CJT_CommunicationSpeaker
	}

	public enum CmDefaultRole
	{
		CDR_None,
		CDR_Multimedia,
		CDR_Communication
	}

	public enum CmLicenseCategory
	{
		CLC_Public,
		CLC_NoiseReduction,
		CLC_HDNoiseReduction,
		CLC_AINoiseReduction,
		CLC_EnvironmentEffects,
		CLC_Equalizer10Bands,
		CLC_SpeakerShifter,
		CLC_SurroundMax,
		CLC_Compressor,
		CLC_NoiseGate,
		CLC_LDR,
		CLC_XearAudioBrilliant,
		CLC_XearDynamicBass,
		CLC_XearSingFX_Echo,
		CLC_XearSingFX_MagicVoice,
		CLC_XearSingFX_KeyShifting,
		CLC_XearSingFX_VocalFading,
		CLC_XearSmartVolume,
		CLC_XearSurroundHeadphone,
		CLC_XearVoiceClarity,
		CLC_HeadTracking,
		CLC_AIMagicVoice,
		CLC_MultiChannelEQ
	}

	public enum CmDeviceState
	{
		CDS_Unknown,
		CDS_Active,
		CDS_Disabled,
		CDS_NotPresent,
		CDS_Unplugged
	}

	public enum CmVigilanceMode
	{
		CVT_Unknown,
		CVT_Omni,
		CVT_Rear
	}

	public delegate void CmFuncPtrCallback(CmCallbackType cbType, Guid audEndptUID, CmDataType datType, int arrLen, IntPtr arrDat, IntPtr usrDat);

	private struct CmDataWString
	{
		public IntPtr str;
	}

	private struct CmDataLogSettings
	{
		public CmLogType logType;

		public CmDataWString filePath;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public CmFuncPtrCallback callback;

		public IntPtr userData;
	}

	private struct CmDataEventSettings
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public CmFuncPtrCallback callback;

		public IntPtr userData;
	}

	private struct CmDataAudioFormat
	{
		public int channels;

		public int bitDepth;

		public int sampleRate;
	}

	private struct CmDataRangeFloat
	{
		public float min;

		public float max;

		public float step;
	}

	private struct CmDataChannelVolume
	{
		public int channel;

		public float volume;
	}

	private struct CmDataProcessInfo
	{
		public int pid;

		public CmDataWString filePath;
	}

	private struct CmDataVigilanceMode
	{
		public int enable;

		public CmVigilanceMode mode;
	}

	public struct LogSettings
	{
		public CmLogType LogType;

		public string FilePath;

		public CmFuncPtrCallback Callback;

		public IntPtr UserData;
	}

	public struct EventSettings
	{
		public CmFuncPtrCallback Callback;

		public IntPtr UserData;
	}

	public struct AudioFormat
	{
		public int Channels;

		public int BitDepth;

		public int SampleRate;
	}

	public struct RangeFloat
	{
		public float Min;

		public float Max;

		public float Step;
	}

	public struct ChannelVolume
	{
		public int Channel;

		public float Volume;
	}

	public struct ProcessInfo
	{
		public int Pid;

		public string FilePath;
	}

	public struct VigilanceMode
	{
		public bool Enable;

		public CmVigilanceMode Mode;
	}

	public const string DLLPath = "CmXearSDK.dll";

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmFreeData(CmDataType inDatType, int inArrLen, IntPtr inArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetLogSettings(ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmSetLogSettings(CmDataType inDatType, int inArrLen, IntPtr inArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmXearDeinit();

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmXearInit();

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetLicenseSettings(ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmDeactivateLicense(Guid inLicense);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmActivateLicense(Guid inLicense);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetEventSettings(ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmSetEventSettings(CmDataType inDatType, int inArrLen, IntPtr inArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetAudioEndpoint(ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetProperty(Guid inAudEndptUID, [MarshalAs(UnmanagedType.LPWStr)] string inProp, CmDataType inDatType, int inArrLen, IntPtr inArrDat, ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmXearSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmSetProperty(Guid inAudEndptUID, [MarshalAs(UnmanagedType.LPWStr)] string inProp, CmDataType inDatType, int inArrLen, IntPtr inArrDat);

	public static bool GetLogSettings(out LogSettings logSettings, out CmErr cmErr)
	{
		bool result = false;
		logSettings = new LogSettings
		{
			FilePath = string.Empty
		};
		CmDataType cmDatType = CmDataType.CDT_None;
		int arrLen = 0;
		cmErr = CmGetLogSettings(ref cmDatType, ref arrLen, out var cmDatIntPtr);
		if (cmErr == CmErr.CE_OK)
		{
			if (ToCSWrappedArray(cmDatType, arrLen, cmDatIntPtr, out var csArrDat) && csArrDat != null)
			{
				logSettings = ((LogSettings[])csArrDat)[0];
			}
			CmFreeData(cmDatType, arrLen, cmDatIntPtr);
			result = true;
		}
		return result;
	}

	public static bool SetLogSettings(LogSettings logSettings, out CmErr cmErr)
	{
		CmDataLogSettings cmDat = new CmDataLogSettings
		{
			logType = logSettings.LogType,
			filePath = new CmDataWString
			{
				str = Marshal.StringToHGlobalUni(logSettings.FilePath)
			},
			callback = logSettings.Callback,
			userData = logSettings.UserData
		};
		IntPtr cmDatIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CmDataLogSettings)));
		Marshal.StructureToPtr(cmDat, cmDatIntPtr, fDeleteOld: true);
		cmErr = CmSetLogSettings(CmDataType.CDT_ArrayedCmDataLogSettings, 1, cmDatIntPtr);
		if (cmDat.filePath.str != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(cmDat.filePath.str);
		}
		Marshal.FreeHGlobal(cmDatIntPtr);
		return cmErr == CmErr.CE_OK;
	}

	public static bool Deinitialize(out CmErr cmErr)
	{
		cmErr = CmXearDeinit();
		return cmErr == CmErr.CE_OK;
	}

	public static bool Initialize(out CmErr cmErr)
	{
		cmErr = CmXearInit();
		return cmErr == CmErr.CE_OK;
	}

	public static bool GetLicenseSettings(out Guid[] licenses, out CmErr cmErr)
	{
		bool result = false;
		licenses = Array.Empty<Guid>();
		CmDataType cmDatType = CmDataType.CDT_None;
		int arrLen = 0;
		cmErr = CmGetLicenseSettings(ref cmDatType, ref arrLen, out var cmDatIntPtr);
		if (cmErr == CmErr.CE_OK)
		{
			if (ToCSWrappedArray(cmDatType, arrLen, cmDatIntPtr, out var csArrDat) && csArrDat != null)
			{
				licenses = (Guid[])csArrDat;
			}
			CmFreeData(cmDatType, arrLen, cmDatIntPtr);
			result = true;
		}
		return result;
	}

	public static bool DeactivateLicense(Guid license, out CmErr cmErr)
	{
		cmErr = CmDeactivateLicense(license);
		return cmErr == CmErr.CE_OK;
	}

	public static bool ActivateLicense(Guid license, out CmErr cmErr)
	{
		cmErr = CmActivateLicense(license);
		return cmErr == CmErr.CE_OK;
	}

	public static bool GetEventSettings(out EventSettings evtSettings, out CmErr cmErr)
	{
		bool result = false;
		evtSettings = new EventSettings
		{
			Callback = null,
			UserData = IntPtr.Zero
		};
		CmDataType cmDatType = CmDataType.CDT_None;
		int arrLen = 0;
		cmErr = CmGetEventSettings(ref cmDatType, ref arrLen, out var cmDatIntPtr);
		if (cmErr == CmErr.CE_OK)
		{
			if (ToCSWrappedArray(cmDatType, arrLen, cmDatIntPtr, out var csArrDat) && csArrDat != null)
			{
				evtSettings = ((EventSettings[])csArrDat)[0];
			}
			CmFreeData(cmDatType, arrLen, cmDatIntPtr);
			result = true;
		}
		return result;
	}

	public static bool SetEventSettings(EventSettings evtSettings, out CmErr cmErr)
	{
		CmDataEventSettings structure = new CmDataEventSettings
		{
			callback = evtSettings.Callback,
			userData = evtSettings.UserData
		};
		IntPtr cmDatIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CmDataEventSettings)));
		Marshal.StructureToPtr(structure, cmDatIntPtr, fDeleteOld: true);
		cmErr = CmSetEventSettings(CmDataType.CDT_ArrayedCmDataEventSettings, 1, cmDatIntPtr);
		Marshal.FreeHGlobal(cmDatIntPtr);
		return cmErr == CmErr.CE_OK;
	}

	public static bool GetAudioEndpoints(out Guid[] audEndptUIDs, out CmErr cmErr)
	{
		bool result = false;
		audEndptUIDs = Array.Empty<Guid>();
		CmDataType cmDatType = CmDataType.CDT_None;
		int arrLen = 0;
		cmErr = CmGetAudioEndpoint(ref cmDatType, ref arrLen, out var arrDat);
		if (cmErr == CmErr.CE_OK)
		{
			if (ToCSWrappedArray(cmDatType, arrLen, arrDat, out var csArrDat) && csArrDat != null)
			{
				audEndptUIDs = csArrDat;
			}
			CmFreeData(cmDatType, arrLen, arrDat);
			result = true;
		}
		return result;
	}

	private static bool GetProperty(Guid audEndpt, string prop, CmDataType inDatType, dynamic inDat, out dynamic outCSArr, out CmErr cmErr)
	{
		bool result = false;
		outCSArr = null;
		cmErr = CmErr.CE_UnknownError;
		bool isErr = false;
		int inArrLen;
		IntPtr cmDatIntPtr = default;
		switch (inDatType)
		{
		case CmDataType.CDT_ArrayedInt32:
		case CmDataType.CDT_ArrayedCmDataWString:
		case CmDataType.CDT_ArrayedCmDataAudioFormat:
		case CmDataType.CDT_ArrayedCmDataChannelVolume:
			inArrLen = inDat.Length;
			isErr = !XearSDK.ToIntPtr(inDat, out cmDatIntPtr);
			break;
		default:
			inArrLen = 0;
			cmDatIntPtr = IntPtr.Zero;
			break;
		}
		if (!isErr)
		{
			CmDataType outDatType = CmDataType.CDT_None;
			int outArrLen = 0;
			cmErr = CmGetProperty(audEndpt, prop, inDatType, inArrLen, cmDatIntPtr, ref outDatType, ref outArrLen, out var outArrDat);
			if (cmDatIntPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(cmDatIntPtr);
			}
			if (cmErr == CmErr.CE_OK)
			{
				if (ToCSWrappedArray(outDatType, outArrLen, outArrDat, out var csArrDat) && csArrDat != null)
				{
					outCSArr = csArrDat;
				}
				CmFreeData(outDatType, outArrLen, outArrDat);
				result = true;
			}
		}
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out string outDat, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((string[])tmp).Length != 0) ? ((string[])tmp)[0] : string.Empty);
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out int outDat, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((int[])tmp).Length != 0) ? ((int[])tmp)[0] : 0);
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out AudioFormat outDat, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((AudioFormat[])tmp).Length != 0) ? ((AudioFormat[])tmp)[0] : default(AudioFormat));
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out RangeFloat outDat, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((RangeFloat[])tmp).Length != 0) ? ((RangeFloat[])tmp)[0] : default(RangeFloat));
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out Guid outDat, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((Guid[])tmp).Length != 0) ? ((Guid[])tmp)[0] : Guid.Empty);
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out VigilanceMode outDat, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((VigilanceMode[])tmp).Length != 0) ? ((VigilanceMode[])tmp)[0] : default(VigilanceMode));
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out int[] outDats, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((int[])tmp) : Array.Empty<int>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out string[] outDats, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((string[])tmp) : Array.Empty<string>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out CmLicenseCategory[] outDats, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((CmLicenseCategory[])tmp) : Array.Empty<CmLicenseCategory>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out ProcessInfo[] outDats, out CmErr cmErr)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((ProcessInfo[])tmp) : Array.Empty<ProcessInfo>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out int[] outDats, out CmErr cmErr, params int[] inDats)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_ArrayedInt32, inDats, out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((int[])tmp) : Array.Empty<int>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out int[] outDats, out CmErr cmErr, params string[] inDats)
	{
		CmDataWString[] cmDatArr = Array.ConvertAll(inDats, (string x) => new CmDataWString
		{
			str = Marshal.StringToHGlobalUni(x)
		});
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_ArrayedCmDataWString, cmDatArr, out var tmp, out cmErr);
		CmDataWString[] array = cmDatArr;
		for (int num = 0; num < array.Length; num++)
		{
			Marshal.FreeHGlobal(array[num].str);
		}
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((int[])tmp) : Array.Empty<int>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out float[] outDats, out CmErr cmErr, params int[] inDats)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_ArrayedInt32, inDats, out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((float[])tmp) : Array.Empty<float>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out string[] outDats, out CmErr cmErr, params string[] inDats)
	{
		CmDataWString[] cmDatArr = Array.ConvertAll(inDats, (string x) => new CmDataWString
		{
			str = Marshal.StringToHGlobalUni(x)
		});
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_ArrayedCmDataWString, cmDatArr, out var tmp, out cmErr);
		CmDataWString[] array = cmDatArr;
		for (int num = 0; num < array.Length; num++)
		{
			Marshal.FreeHGlobal(array[num].str);
		}
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((string[])tmp) : Array.Empty<string>());
		return result;
	}

	public static bool GetProperty(Guid audEndpt, string prop, out int[] outDats, out CmErr cmErr, params CmDefaultRole[] inDats)
	{
		return GetProperty(audEndpt, prop, out outDats, out cmErr, Array.ConvertAll(inDats, (CmDefaultRole x) => (int)x));
	}

	public static bool GetProperty(Guid audEndpt, string prop, out int[] outDats, out CmErr cmErr, params AudioFormat[] inDats)
	{
		bool result = GetProperty(audEndpt, prop, CmDataType.CDT_ArrayedCmDataAudioFormat, Array.ConvertAll(inDats, (AudioFormat x) => new CmDataAudioFormat
		{
			channels = x.Channels,
			bitDepth = x.BitDepth,
			sampleRate = x.SampleRate
		}), out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((int[])tmp) : Array.Empty<int>());
		return result;
	}

	public static bool SetProperty(Guid audEndpt, string prop, string value, out CmErr cmErr)
	{
		CmDataWString[] array = new CmDataWString[1]
		{
			new CmDataWString
			{
				str = Marshal.StringToHGlobalUni(value)
			}
		};
		ToIntPtr(array, out var cmDatIntPtr);
		cmErr = CmSetProperty(audEndpt, prop, CmDataType.CDT_ArrayedCmDataWString, 1, cmDatIntPtr);
		Marshal.FreeHGlobal(array[0].str);
		Marshal.FreeHGlobal(cmDatIntPtr);
		return cmErr == CmErr.CE_OK;
	}

	public static bool SetProperty(Guid audEndpt, string prop, int value, out CmErr cmErr)
	{
		ToIntPtr(new int[1] { value }, out var cmDatIntPtr);
		cmErr = CmSetProperty(audEndpt, prop, CmDataType.CDT_ArrayedInt32, 1, cmDatIntPtr);
		Marshal.FreeHGlobal(cmDatIntPtr);
		return cmErr == CmErr.CE_OK;
	}

	public static bool SetProperty(Guid audEndpt, string prop, CmDefaultRole value, out CmErr cmErr)
	{
		return SetProperty(audEndpt, prop, (int)value, out cmErr);
	}

	public static bool SetProperty(Guid audEndpt, string prop, AudioFormat value, out CmErr cmErr)
	{
		ToIntPtr(new CmDataAudioFormat[1]
		{
			new CmDataAudioFormat
			{
				channels = value.Channels,
				bitDepth = value.BitDepth,
				sampleRate = value.SampleRate
			}
		}, out var cmDatIntPtr);
		cmErr = CmSetProperty(audEndpt, prop, CmDataType.CDT_ArrayedCmDataAudioFormat, 1, cmDatIntPtr);
		Marshal.FreeHGlobal(cmDatIntPtr);
		return cmErr == CmErr.CE_OK;
	}

	public static bool SetProperty(Guid audEndpt, string prop, ChannelVolume value, out CmErr cmErr)
	{
		ToIntPtr(new CmDataChannelVolume[1]
		{
			new CmDataChannelVolume
			{
				channel = value.Channel,
				volume = value.Volume
			}
		}, out var cmDatIntPtr);
		cmErr = CmSetProperty(audEndpt, prop, CmDataType.CDT_ArrayedCmDataChannelVolume, 1, cmDatIntPtr);
		Marshal.FreeHGlobal(cmDatIntPtr);
		return cmErr == CmErr.CE_OK;
	}

	public static bool SetProperty(Guid audEndpt, string prop, VigilanceMode value, out CmErr cmErr)
	{
		ToIntPtr(new CmDataVigilanceMode[1]
		{
			new CmDataVigilanceMode
			{
				enable = (value.Enable ? 1 : 0),
				mode = value.Mode
			}
		}, out var cmDatIntPtr);
		cmErr = CmSetProperty(audEndpt, prop, CmDataType.CDT_ArrayedCmDataVigilanceMode, 1, cmDatIntPtr);
		Marshal.FreeHGlobal(cmDatIntPtr);
		return cmErr == CmErr.CE_OK;
	}

	private static bool ToIntPtr(int[] arrDat, out IntPtr outPtr)
	{
		outPtr = Marshal.AllocHGlobal(arrDat.Length * 4);
		Marshal.Copy(arrDat, 0, outPtr, arrDat.Length);
		return true;
	}

	private static bool ToIntPtr<T>(T[] arrDat, out IntPtr outPtr)
	{
		int size = Marshal.SizeOf(typeof(T));
		outPtr = Marshal.AllocHGlobal(arrDat.Length * size);
		long arrDatStartPos = outPtr.ToInt64();
		for (int i = 0; i < arrDat.Length; i++)
		{
			Marshal.StructureToPtr(arrDat[i], new IntPtr(arrDatStartPos + i * size), fDeleteOld: true);
		}
		return true;
	}

	private static bool ToManagedArray(int arrLen, IntPtr arrDat, out int[] outArr)
	{
		outArr = new int[arrLen];
		int size = 4;
		long arrDatStartPos = arrDat.ToInt64();
		for (int i = 0; i < arrLen; i++)
		{
			outArr[i] = Marshal.PtrToStructure<int>(new IntPtr(arrDatStartPos + i * size));
		}
		return true;
	}

	private static bool ToManagedArray<T>(int arrLen, IntPtr arrDat, out T[] outArr)
	{
		outArr = new T[arrLen];
		int size = Marshal.SizeOf(typeof(T));
		long arrDatStartPos = arrDat.ToInt64();
		for (int i = 0; i < arrLen; i++)
		{
			outArr[i] = Marshal.PtrToStructure<T>(new IntPtr(arrDatStartPos + i * size));
		}
		return true;
	}

	private static bool ToCmDataArray(CmDataType cmDatType, int arrLen, IntPtr arrDat, out dynamic cmArrDat)
	{
		switch (cmDatType)
		{
		case CmDataType.CDT_None:
			cmArrDat = null;
			return true;
		case CmDataType.CDT_ArrayedGUID:
		{
			bool result11 = ToManagedArray(arrLen, arrDat, out Guid[] data11);
			cmArrDat = data11;
			return result11;
		}
		case CmDataType.CDT_ArrayedInt32:
		{
			bool result10 = ToManagedArray(arrLen, arrDat, out var data10);
			cmArrDat = data10;
			return result10;
		}
		case CmDataType.CDT_ArrayedFloat:
		{
			bool result9 = ToManagedArray(arrLen, arrDat, out float[] data9);
			cmArrDat = data9;
			return result9;
		}
		case CmDataType.CDT_ArrayedCmDataWString:
		{
			bool result8 = ToManagedArray(arrLen, arrDat, out CmDataWString[] data8);
			cmArrDat = data8;
			return result8;
		}
		case CmDataType.CDT_ArrayedCmDataLogSettings:
		{
			bool result7 = ToManagedArray(arrLen, arrDat, out CmDataLogSettings[] data7);
			cmArrDat = data7;
			return result7;
		}
		case CmDataType.CDT_ArrayedCmDataEventSettings:
		{
			bool result6 = ToManagedArray(arrLen, arrDat, out CmDataEventSettings[] data6);
			cmArrDat = data6;
			return result6;
		}
		case CmDataType.CDT_ArrayedCmDataAudioFormat:
		{
			bool result5 = ToManagedArray(arrLen, arrDat, out CmDataAudioFormat[] data5);
			cmArrDat = data5;
			return result5;
		}
		case CmDataType.CDT_ArrayedCmDataRangeFloat:
		{
			bool result4 = ToManagedArray(arrLen, arrDat, out CmDataRangeFloat[] data4);
			cmArrDat = data4;
			return result4;
		}
		case CmDataType.CDT_ArrayedCmDataChannelVolume:
		{
			bool result3 = ToManagedArray(arrLen, arrDat, out CmDataChannelVolume[] data3);
			cmArrDat = data3;
			return result3;
		}
		case CmDataType.CDT_ArrayedCmDataProcessInfo:
		{
			bool result2 = ToManagedArray(arrLen, arrDat, out CmDataProcessInfo[] data2);
			cmArrDat = data2;
			return result2;
		}
		case CmDataType.CDT_ArrayedCmDataVigilanceMode:
		{
			bool result = ToManagedArray(arrLen, arrDat, out CmDataVigilanceMode[] data);
			cmArrDat = data;
			return result;
		}
		default:
			cmArrDat = null;
			return false;
		}
	}

	public static bool ToCSWrappedArray(CmDataType cmDatType, int arrLen, IntPtr arrDat, out dynamic csArrDat)
	{
		bool result = ToCmDataArray(cmDatType, arrLen, arrDat, out var cmArrDat);
		if (result && cmArrDat != null)
		{
			switch (cmDatType)
			{
			case CmDataType.CDT_ArrayedCmDataWString:
				csArrDat = Array.ConvertAll((CmDataWString[])cmArrDat, (CmDataWString x) => Marshal.PtrToStringUni(x.str));
				break;
			case CmDataType.CDT_ArrayedCmDataLogSettings:
				csArrDat = Array.ConvertAll((CmDataLogSettings[])cmArrDat, (CmDataLogSettings x) => new LogSettings
				{
					LogType = x.logType,
					FilePath = Marshal.PtrToStringUni(x.filePath.str),
					Callback = x.callback,
					UserData = x.userData
				});
				break;
			case CmDataType.CDT_ArrayedCmDataEventSettings:
				csArrDat = Array.ConvertAll((CmDataEventSettings[])cmArrDat, (CmDataEventSettings x) => new EventSettings
				{
					Callback = x.callback,
					UserData = x.userData
				});
				break;
			case CmDataType.CDT_ArrayedCmDataAudioFormat:
				csArrDat = Array.ConvertAll((CmDataAudioFormat[])cmArrDat, (CmDataAudioFormat x) => new AudioFormat
				{
					Channels = x.channels,
					BitDepth = x.bitDepth,
					SampleRate = x.sampleRate
				});
				break;
			case CmDataType.CDT_ArrayedCmDataRangeFloat:
				csArrDat = Array.ConvertAll((CmDataRangeFloat[])cmArrDat, (CmDataRangeFloat x) => new RangeFloat
				{
					Min = x.min,
					Max = x.max,
					Step = x.step
				});
				break;
			case CmDataType.CDT_ArrayedCmDataChannelVolume:
				csArrDat = Array.ConvertAll((CmDataChannelVolume[])cmArrDat, (CmDataChannelVolume x) => new ChannelVolume
				{
					Channel = x.channel,
					Volume = x.volume
				});
				break;
			case CmDataType.CDT_ArrayedCmDataProcessInfo:
				csArrDat = Array.ConvertAll((CmDataProcessInfo[])cmArrDat, (CmDataProcessInfo x) => new ProcessInfo
				{
					Pid = x.pid,
					FilePath = Marshal.PtrToStringUni(x.filePath.str)
				});
				break;
			case CmDataType.CDT_ArrayedCmDataVigilanceMode:
				csArrDat = Array.ConvertAll((CmDataVigilanceMode[])cmArrDat, (CmDataVigilanceMode x) => new VigilanceMode
				{
					Enable = (x.enable == 1),
					Mode = x.mode
				});
				break;
			default:
				csArrDat = cmArrDat;
				break;
			}
		}
		else
		{
			csArrDat = null;
		}
		return result;
	}
}
