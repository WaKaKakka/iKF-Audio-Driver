#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Helper;

namespace CmDeviceCheck;

internal class Program
{
	private static void DebugMessageWriteLine(string message)
	{
		Trace.WriteLine("[CM][CmDeviceCheck] " + message);
	}

	private static void Main(string[] args)
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		baseDirectory = baseDirectory.Substring(0, baseDirectory.LastIndexOf('\\'));
		baseDirectory = baseDirectory.Substring(0, baseDirectory.LastIndexOf('\\'));
		baseDirectory += "\\ApoDriver\\Driver";
		string text = baseDirectory;
		if (!Directory.Exists(baseDirectory))
		{
			baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			baseDirectory = baseDirectory.Substring(0, baseDirectory.LastIndexOf('\\'));
			baseDirectory = baseDirectory.Substring(0, baseDirectory.LastIndexOf('\\'));
			baseDirectory = ((!Tools.IsWin11OrAbove()) ? (baseDirectory + "\\Driver\\WIN10") : (baseDirectory + "\\Driver\\WIN11"));
			if (!Directory.Exists(baseDirectory))
			{
				DebugMessageWriteLine("Apo Driver Folder Not Found: " + text);
				DebugMessageWriteLine("Win10/Win11 Driver Folder Not Found: " + baseDirectory);
				return;
			}
		}
		else
		{
			DebugMessageWriteLine("Driver Folder Found: " + baseDirectory);
		}
		if (!File.Exists("devcon.exe"))
		{
			DebugMessageWriteLine("devcon.exe Not Found!");
			return;
		}
		string[] array = null;
		try
		{
			array = Directory.GetFiles(baseDirectory, "*.inf", SearchOption.AllDirectories);
		}
		catch (Exception)
		{
			DebugMessageWriteLine("Driver Folder Not Exist: " + baseDirectory);
			return;
		}
		if (array == null || array.Length == 0)
		{
			DebugMessageWriteLine("No driver inf file found. Driver Folder: " + baseDirectory);
			return;
		}
		string[] array2 = File.ReadAllLines(array[0]);
		if (array2 == null || array2.Length == 0)
		{
			DebugMessageWriteLine("Driver file is empty. Driver File: " + array[0]);
			return;
		}
		List<string> list = new List<string>();
		string[] array3 = array2;
		foreach (string text2 in array3)
		{
			if (text2.Contains("USB\\VID_"))
			{
				string item = text2.Substring(text2.IndexOf("USB\\VID_"), 21);
				list.Add(item);
			}
		}
		if (Enumerable.Count(list) == 0)
		{
			DebugMessageWriteLine("Unable to find DeviceID!");
			return;
		}
		list = list.Distinct().ToList();
		bool deviceExist = false;
		IniFile iniFile = new IniFile("CmDeviceCheck.ini");
		foreach (string item2 in list)
		{
			CheckDeviceIDExist(item2, ref deviceExist);
			if (deviceExist)
			{
				iniFile.Write("DEVICE", "1", "SETUP");
				DebugMessageWriteLine("Device Found: " + item2);
				break;
			}
			DebugMessageWriteLine("Device Not Found: " + item2);
		}
		if (!deviceExist)
		{
			iniFile.Write("DEVICE", "0", "SETUP");
		}
	}

	private static int CheckDeviceIDExist(string deviceID, ref bool deviceExist)
	{
		StreamReader outputFromDevcon = GetOutputFromDevcon("find \"" + deviceID + "\"");
		deviceExist = false;
		string text;
		while ((text = outputFromDevcon.ReadLine()) != null)
		{
			if (text.ToUpper().Contains(deviceID.ToUpper()))
			{
				deviceExist = true;
				break;
			}
		}
		return 0;
	}

	private static StreamReader GetOutputFromDevcon(string arguments)
	{
		Process process = new Process();
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.FileName = "devcon.exe";
		process.StartInfo.Arguments = arguments;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.UseShellExecute = false;
		process.Start();
		StreamReader standardOutput = process.StandardOutput;
		process.Close();
		return standardOutput;
	}
}
