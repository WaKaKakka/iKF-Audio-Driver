using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Cmedia;
using XearONE.UI.Driver;

namespace XearONE.UI.Keyboard;

internal class RegisterHotKeyHelper
{
	private const uint MOD_ALT = 1u;

	private const uint MOD_CONTROL = 2u;

	private const uint MOD_SHIFT = 4u;

	private const uint MOD_WIN = 8u;

	private const int HOTKEY_ID_3DAudio_Enable = 1;

	private const int HOTKEY_ID_3DAudio_AutoReset = 2;

	private const uint VK_OEM_4 = 219u;

	private const uint VK_OEM_6 = 221u;

	[DllImport("user32.dll")]
	private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll")]
	private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

	public void RegisterHotKey(IntPtr hWnd)
	{
		RegisterHotKey(hWnd, 1, 6u, 219u);
		RegisterHotKey(hWnd, 2, 6u, 221u);
		HwndSource.FromHwnd(hWnd)?.AddHook(HwndHook);
	}

	public void UnregisterHotKey(IntPtr hWnd)
	{
		UnregisterHotKey(hWnd, 1);
		UnregisterHotKey(hWnd, 2);
	}

	private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == 786 && wParam.ToInt32() == 1)
		{
			Console.WriteLine("HOTKEY_ID_3DAudio_Enable Pressed.");
			ThreeDAudioEnableHandling();
			handled = true;
		}
		if (msg == 786 && wParam.ToInt32() == 2)
		{
			Console.WriteLine("HOTKEY_ID_3DAudio_AutoReset Pressed.");
			ThreeDAudioAutoResetHandling();
			handled = true;
		}
		return IntPtr.Zero;
	}

	private void ThreeDAudioEnableHandling()
	{
		int enable = 0;
		XearSDK.GetProperty(DriverInstance.getInstance().headtrackingDevice.endpointUID, "HeadTracking_Enable", out enable, out XearSDK.CmErr cmErr);
		int inValue = ((enable != 1) ? 1 : 0);
		XearSDK.SetProperty(DriverInstance.getInstance().headtrackingDevice.endpointUID, "HeadTracking_Enable", inValue, out cmErr);
	}

	private void ThreeDAudioAutoResetHandling()
	{
		DriverInstance.getInstance().HeadTrackingReset();
	}
}
