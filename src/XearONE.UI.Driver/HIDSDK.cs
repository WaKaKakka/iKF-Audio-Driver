using System;
using System.Runtime.InteropServices;

namespace XearONE.UI.Driver;

public static class HIDSDK
{
	public enum CmErr
	{
		CE_OK,
		CE_UnknownError,
		CE_LicensingError,
		CE_Unsupported,
		CE_DeviceNotFound,
		CE_InvalidParameter,
		CE_PropertyGetSetError
	}

	public enum CmHIDEventID
	{
		CHEID_Unknown,
		CHEID_DeviceRemoval,
		CHEID_DeviceArrival,
		CHEID_DataArrival
	}

	public enum CmLogType
	{
		CLT_None,
		CLT_Console,
		CLT_File,
		CLT_Callback
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CmValueString
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		public string value;
	}

	public struct CmValueINT32
	{
		public int value;
	}

	public struct CmValueOperationINT32
	{
		public int value;

		public int operation;
	}

	public struct CmValueVariableSizedArrayBYTE
	{
		public int dataSize;

		public IntPtr data;
	}

	public struct CmValueEventCallbackHIDData
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public fp_NotifyEventHIDData callback;

		public IntPtr userData;
	}

	public struct CmValueEventCallbackHIDRemovalArrival
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public fp_NotifyEventHIDRemovalArrival callback;

		public IntPtr userData;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct CmValueLogSettings
	{
		public int logType;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		public string filePath;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public fp_LogCallback callback;

		public IntPtr userData;
	}

	public delegate void fp_NotifyEventHIDData(IntPtr sdkObj, IntPtr hidEndpointObj, CmHIDEventID eventID, IntPtr data, int dataSize, IntPtr userData);

	public delegate void fp_NotifyEventHIDRemovalArrival(IntPtr sdkObj, CmHIDEventID eventID, IntPtr userData);

	public delegate void fp_LogCallback([MarshalAs(UnmanagedType.LPWStr)] string log, IntPtr userData);

	public const int gStringBufferMaxCount = 512;

	public const string DLLPath = "CmHIDSDK.dll";

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmHIDSDKInit(ref IntPtr sdkObj);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmHIDSDKUninit(IntPtr sdkObj);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmRebuildDeviceList(IntPtr sdkObj);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmGetHIDEndpointCount(IntPtr sdkObj, ref int count);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmGetHIDEndpointByIndex(IntPtr sdkObj, int index, ref IntPtr hidEndpointObj);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmGetProperty(IntPtr sdkObj, IntPtr hidEndpointObj, [MarshalAs(UnmanagedType.LPWStr)] string property, IntPtr value);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmSetProperty(IntPtr sdkObj, IntPtr hidEndpointObj, [MarshalAs(UnmanagedType.LPWStr)] string property, IntPtr value);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmGetHIDEndpointEventCallback(IntPtr sdkObj, IntPtr hidEndpointObj, IntPtr value);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmSetHIDEndpointEventCallback(IntPtr sdkObj, IntPtr hidEndpointObj, IntPtr value);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmGetEventCallbackHIDRemovalArrival(IntPtr sdkObj, IntPtr value);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmSetEventCallbackHIDRemovalArrival(IntPtr sdkObj, IntPtr value);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmGetLogSettings(IntPtr logSettings);

	[DllImport("CmHIDSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern CmErr CmSetLogSettings(IntPtr logSettings);
}
