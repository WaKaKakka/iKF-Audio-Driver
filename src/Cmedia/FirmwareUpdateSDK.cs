using System;
using System.Runtime.InteropServices;

namespace Cmedia;

public static class FirmwareUpdateSDK
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
		CE_EventError,
		CE_FileError,
		CE_ResourceBusyError,
		CE_UpdateError,
		CE_TimeoutError
	}

	public enum CmCallbackType
	{
		CCT_Unknown,
		CCT_Log,
		CCT_FirmwareUpdatingState,
		CCT_FirmwareDataTransmittedRatio
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
		CDT_ArrayedCmDataEventSettings
	}

	public enum CmLogType
	{
		CLT_None,
		CLT_Console,
		CLT_File,
		CLT_Callback
	}

	public enum CmFirmwareType
	{
		CFT_Unknown,
		CFT_StandardDevice,
		CFT_TXDevice,
		CFT_RXDevice
	}

	public enum CmFirmwareIC
	{
		CFI_Unknown,
		CFI_CM2025,
		CFI_CM2206
	}

	public enum CmFirmwareUpdatingState
	{
		CFUS_Unknown,
		CFUS_Preparing,
		CFUS_Updating,
		CFUS_Restarting,
		CFUS_Finalizing,
		CFUS_Finished
	}

	public enum CmDeviceBatteryStatus
	{
		CDBS_Unknown,
		CDBS_Discharged,
		CDBS_Charging,
		CDBS_Full
	}

	public delegate void CmFuncPtrCallback(CmCallbackType cbType, Guid reserved, CmDataType datType, int arrLen, IntPtr arrDat, IntPtr usrDat);

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

	public const string DLLPath = "CmFirmwareUpdateSDK.dll";

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmFreeData(CmDataType inDatType, int inArrLen, IntPtr inArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetLogSettings(ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmSetLogSettings(CmDataType inDatType, int inArrLen, IntPtr inArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmFirmwareUpdateDeinit();

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmFirmwareUpdateInit();

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetEventSettings(ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmSetEventSettings(CmDataType inDatType, int inArrLen, IntPtr inArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetDevice(int inVID, int inPID, ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetDeviceProperty(Guid inDevUID, [MarshalAs(UnmanagedType.LPWStr)] string inProp, CmDataType inDatType, int inArrLen, IntPtr inArrDat, ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmGetFileProperty([MarshalAs(UnmanagedType.LPWStr)] string inFilePath, [MarshalAs(UnmanagedType.LPWStr)] string inProp, CmDataType inDatType, int inArrLen, IntPtr inArrDat, ref CmDataType outDatType, ref int outArrLen, out IntPtr outArrDat);

	[DllImport("CmFirmwareUpdateSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	private static extern CmErr CmStartUpdating(Guid inDevUID, [MarshalAs(UnmanagedType.LPWStr)] string inFilePath, int inTimeoutMS);

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
		cmErr = CmFirmwareUpdateDeinit();
		return cmErr == CmErr.CE_OK;
	}

	public static bool Initialize(out CmErr cmErr)
	{
		cmErr = CmFirmwareUpdateInit();
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

	public static bool GetDevices(int vid, int pid, out Guid[] devUIDs, out CmErr cmErr)
	{
		bool result = false;
		devUIDs = Array.Empty<Guid>();
		CmDataType cmDatType = CmDataType.CDT_None;
		int arrLen = 0;
		cmErr = CmGetDevice(vid, pid, ref cmDatType, ref arrLen, out var arrDat);
		if (cmErr == CmErr.CE_OK)
		{
			if (ToCSWrappedArray(cmDatType, arrLen, arrDat, out var csArrDat) && csArrDat != null)
			{
				devUIDs = csArrDat;
			}
			CmFreeData(cmDatType, arrLen, arrDat);
			result = true;
		}
		return result;
	}

	private static bool GetProperty(Guid devUID, string prop, CmDataType inDatType, dynamic inDat, out dynamic outCSArr, out CmErr cmErr)
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
			inArrLen = inDat.Length;
			isErr = !FirmwareUpdateSDK.ToIntPtr(inDat, out cmDatIntPtr);
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
			cmErr = CmGetDeviceProperty(devUID, prop, inDatType, inArrLen, cmDatIntPtr, ref outDatType, ref outArrLen, out var outArrDat);
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

	private static bool GetProperty(string filePath, string prop, CmDataType inDatType, dynamic inDat, out dynamic outCSArr, out CmErr cmErr)
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
			inArrLen = inDat.Length;
			isErr = !FirmwareUpdateSDK.ToIntPtr(inDat, out cmDatIntPtr);
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
			cmErr = CmGetFileProperty(filePath, prop, inDatType, inArrLen, cmDatIntPtr, ref outDatType, ref outArrLen, out var outArrDat);
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

	public static bool GetProperty(Guid devUID, string prop, out int outDat, out CmErr cmErr)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((int[])tmp).Length != 0) ? ((int[])tmp)[0] : 0);
		return result;
	}

	public static bool GetProperty(Guid devUID, string prop, out string outDat, out CmErr cmErr)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((string[])tmp).Length != 0) ? ((string[])tmp)[0] : string.Empty);
		return result;
	}

	public static bool GetProperty(Guid devUID, string prop, out Guid outDat, out CmErr cmErr)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((Guid[])tmp).Length != 0) ? ((Guid[])tmp)[0] : Guid.Empty);
		return result;
	}

	public static bool GetProperty(Guid devUID, string prop, out float outDat, out CmErr cmErr)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((float[])tmp).Length != 0) ? ((float[])tmp)[0] : 0f);
		return result;
	}

	public static bool GetProperty(Guid devUID, string prop, out CmDeviceBatteryStatus outDat, out CmErr cmErr)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_None, null, out var tmp, out cmErr);
		outDat = ((cmErr == CmErr.CE_OK && tmp != null && ((CmDeviceBatteryStatus[])tmp).Length != 0) ? ((CmDeviceBatteryStatus[])tmp)[0] : CmDeviceBatteryStatus.CDBS_Unknown);
		return result;
	}

	public static bool GetProperty(Guid devUID, string prop, out int[] outDats, out CmErr cmErr, params CmFirmwareType[] inDats)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_ArrayedInt32, Array.ConvertAll(inDats, (CmFirmwareType x) => (int)x), out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((int[])tmp) : Array.Empty<int>());
		return result;
	}

	public static bool GetProperty(Guid devUID, string prop, out string[] outDats, out CmErr cmErr, params CmFirmwareType[] inDats)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_ArrayedInt32, Array.ConvertAll(inDats, (CmFirmwareType x) => (int)x), out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((string[])tmp) : Array.Empty<string>());
		return result;
	}

	public static bool GetProperty(Guid devUID, string prop, out Guid[] outDats, out CmErr cmErr, params CmFirmwareType[] inDats)
	{
		bool result = GetProperty(devUID, prop, CmDataType.CDT_ArrayedInt32, Array.ConvertAll(inDats, (CmFirmwareType x) => (int)x), out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((Guid[])tmp) : Array.Empty<Guid>());
		return result;
	}

	public static bool GetProperty(string filePath, string prop, out int[] outDats, out CmErr cmErr, params CmFirmwareIC[] inDats)
	{
		bool result = GetProperty(filePath, prop, CmDataType.CDT_ArrayedInt32, Array.ConvertAll(inDats, (CmFirmwareIC x) => (int)x), out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((int[])tmp) : Array.Empty<int>());
		return result;
	}

	public static bool GetProperty(string filePath, string prop, out string[] outDats, out CmErr cmErr, params CmFirmwareIC[] inDats)
	{
		bool result = GetProperty(filePath, prop, CmDataType.CDT_ArrayedInt32, Array.ConvertAll(inDats, (CmFirmwareIC x) => (int)x), out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((string[])tmp) : Array.Empty<string>());
		return result;
	}

	public static bool GetProperty(string filePath, string prop, out Guid[] outDats, out CmErr cmErr, params CmFirmwareIC[] inDats)
	{
		bool result = GetProperty(filePath, prop, CmDataType.CDT_ArrayedInt32, Array.ConvertAll(inDats, (CmFirmwareIC x) => (int)x), out var tmp, out cmErr);
		outDats = ((cmErr == CmErr.CE_OK && tmp != null) ? ((Guid[])tmp) : Array.Empty<Guid>());
		return result;
	}

	public static bool StartUpdating(Guid devUID, string filePath, int timeoutMS, out CmErr cmErr)
	{
		cmErr = CmStartUpdating(devUID, filePath, timeoutMS);
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
			bool result6 = ToManagedArray(arrLen, arrDat, out Guid[] data6);
			cmArrDat = data6;
			return result6;
		}
		case CmDataType.CDT_ArrayedInt32:
		{
			bool result5 = ToManagedArray(arrLen, arrDat, out var data5);
			cmArrDat = data5;
			return result5;
		}
		case CmDataType.CDT_ArrayedFloat:
		{
			bool result4 = ToManagedArray(arrLen, arrDat, out float[] data4);
			cmArrDat = data4;
			return result4;
		}
		case CmDataType.CDT_ArrayedCmDataWString:
		{
			bool result3 = ToManagedArray(arrLen, arrDat, out CmDataWString[] data3);
			cmArrDat = data3;
			return result3;
		}
		case CmDataType.CDT_ArrayedCmDataLogSettings:
		{
			bool result2 = ToManagedArray(arrLen, arrDat, out CmDataLogSettings[] data2);
			cmArrDat = data2;
			return result2;
		}
		case CmDataType.CDT_ArrayedCmDataEventSettings:
		{
			bool result = ToManagedArray(arrLen, arrDat, out CmDataEventSettings[] data);
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
