using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace XearONE.UI.Helpers;

internal class ToolsHelper
{
	public static int GetRegistryDataInt(string lpSubKey, string lpValueName)
	{
		RegistryKey key = Registry.CurrentUser.OpenSubKey(lpSubKey);
		if (key != null)
		{
			object value = key.GetValue(lpValueName);
			if (value == null)
			{
				return -1;
			}
			return Convert.ToInt32(value);
		}
		return -2;
	}

	public static int SetRegistryDataInt(string lpSubKey, string lpValueName, object data)
	{
		RegistryKey key = Registry.CurrentUser.OpenSubKey(lpSubKey, writable: true);
		if (key != null)
		{
			key.SetValue(lpValueName, data, RegistryValueKind.DWord);
		}
		else
		{
			key = Registry.CurrentUser.CreateSubKey(lpSubKey, writable: true);
			if (key == null)
			{
				return -1;
			}
			key.SetValue(lpValueName, data, RegistryValueKind.DWord);
		}
		return 0;
	}

	public static string GetRegistryDataString(string lpSubKey, string lpValueName)
	{
		RegistryKey key = Registry.CurrentUser.OpenSubKey(lpSubKey);
		if (key != null)
		{
			object value = key.GetValue(lpValueName);
			if (value == null)
			{
				return "";
			}
			return value.ToString();
		}
		return "";
	}

	public static void CheckAnotherInstanceOpen(string mutexName)
	{
		bool mutexCreated = false;
		Mutex mutex = new Mutex(initiallyOwned: true, mutexName, out mutexCreated);
		if (!mutexCreated)
		{
			MessageBox.Show("Tuning Tool is already running.", "Tuning Tool");
			mutex.Close();
			Application.Current.Shutdown();
		}
	}
}
