using System;
using Microsoft.Win32;

namespace Helper;

internal static class Tools
{
	public static bool IsWin11OrAbove()
	{
		Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "productName", "").ToString();
		Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ReleaseId", "").ToString();
		Environment.OSVersion.Version.ToString();
		_ = Environment.Is64BitOperatingSystem;
		string value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", "").ToString();
		Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "UBR", "").ToString();
		try
		{
			if (Convert.ToInt32(value) >= 22000)
			{
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static int GetRegistryDataInt(string lpSubKey, string lpValueName)
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(lpSubKey);
		if (registryKey != null)
		{
			object value = registryKey.GetValue(lpValueName);
			if (value == null)
			{
				return -1;
			}
			return Convert.ToInt32(value);
		}
		return -1;
	}

	public static int SetRegistryDataInt(string lpSubKey, string lpValueName, object data)
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(lpSubKey, writable: true);
		if (registryKey != null)
		{
			registryKey.SetValue(lpValueName, data, RegistryValueKind.DWord);
		}
		else
		{
			registryKey = Registry.CurrentUser.CreateSubKey(lpSubKey, writable: true);
			if (registryKey == null)
			{
				return -1;
			}
			registryKey.SetValue(lpValueName, data, RegistryValueKind.DWord);
		}
		return 0;
	}

	public static string GetRegistryDataString(string lpSubKey, string lpValueName)
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(lpSubKey);
		if (registryKey != null)
		{
			object value = registryKey.GetValue(lpValueName);
			if (value == null)
			{
				return "";
			}
			return value.ToString();
		}
		return "";
	}
}
