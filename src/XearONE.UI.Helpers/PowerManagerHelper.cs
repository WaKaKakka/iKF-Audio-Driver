using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XearONE.UI.Helpers;

public class PowerManagerHelper
{
	public struct PowerStatus
	{
		public byte ACLineStatus;

		public byte BatteryFlag;

		public byte BatteryLifePercent;

		public byte Reserved;

		public int BatteryLifeTime;

		public int BatteryFullLifeTime;
	}

	private readonly PowerPlan MaximumPerformance;

	private readonly PowerPlan Balanced;

	private readonly PowerPlan PowerSourceOptimized;

	public PowerManagerHelper()
	{
		MaximumPerformance = NewPlan("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
		Balanced = NewPlan("381b4222-f694-41f0-9685-ff5bb260df2e");
		PowerSourceOptimized = NewPlan("a1841308-3541-4fab-bc81-f71556f20b4a");
	}

	private PowerPlan NewPlan(string guidString)
	{
		Guid guid = new Guid(guidString);
		return new PowerPlan(GetPowerPlanName(guid), guid);
	}

	public List<PowerPlan> GetPlans()
	{
		return new List<PowerPlan>(new PowerPlan[3] { MaximumPerformance, Balanced, PowerSourceOptimized });
	}

	private unsafe Guid GetActiveGuid()
	{
		Guid ActiveScheme = Guid.Empty;
		IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
		if (PowerGetActiveScheme((IntPtr)(void*)null, out ptr) == 0)
		{
			ActiveScheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
			Marshal.FreeHGlobal(ptr);
		}
		return ActiveScheme;
	}

	public PowerPlan GetCurrentPlan()
	{
		Guid guid = GetActiveGuid();
		return GetPlans().Find((PowerPlan p) => p.guid == guid);
	}

	public bool IsCurrentPlanMaximumPerformance()
	{
		return GetActiveGuid() == MaximumPerformance.guid;
	}

	private unsafe static string GetPowerPlanName(Guid guid)
	{
		string name = string.Empty;
		IntPtr lpszName = (IntPtr)(void*)null;
		uint dwSize = 0u;
		PowerReadFriendlyName((IntPtr)(void*)null, ref guid, (IntPtr)(void*)null, (IntPtr)(void*)null, lpszName, ref dwSize);
		if (dwSize != 0)
		{
			lpszName = Marshal.AllocHGlobal((int)dwSize);
			if (PowerReadFriendlyName((IntPtr)(void*)null, ref guid, (IntPtr)(void*)null, (IntPtr)(void*)null, lpszName, ref dwSize) == 0)
			{
				name = Marshal.PtrToStringUni(lpszName);
			}
			if (lpszName != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(lpszName);
			}
		}
		return name;
	}

	[DllImport("powrprof.dll")]
	public static extern uint PowerGetActiveScheme(IntPtr UserPowerKey, out IntPtr ActivePolicyGuid);

	[DllImport("powrprof.dll")]
	public static extern uint PowerReadFriendlyName(IntPtr RootPowerKey, ref Guid SchemeGuid, IntPtr SubGroupOfPowerSettingsGuid, IntPtr PowerSettingGuid, IntPtr Buffer, ref uint BufferSize);

	[DllImport("kernel32.dll")]
	public static extern bool GetSystemPowerStatus(out PowerStatus BatteryInfo);

	public void GetSystemPowerStatus(ref byte aCLineStatus, ref byte batteryLiftPercent)
	{
		PowerStatus ps = default;
		if (GetSystemPowerStatus(out ps))
		{
			aCLineStatus = ps.ACLineStatus;
			batteryLiftPercent = ps.BatteryLifePercent;
		}
	}

	private bool IsACLineConnected()
	{
		PowerStatus ps = default;
		if (GetSystemPowerStatus(out ps))
		{
			if (ps.ACLineStatus == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
