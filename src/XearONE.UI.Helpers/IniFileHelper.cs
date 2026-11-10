using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace XearONE.UI.Helpers;

public class IniFileHelper
{
	private string filePath;

	private string EXE = Assembly.GetExecutingAssembly().GetName().Name;

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	private static extern int WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	private static extern int GetPrivateProfileInt(string Section, string Key, int Default, string FilePath);

	public IniFileHelper(string IniPath = "")
	{
		if (IniPath == "")
		{
			filePath = AppDomain.CurrentDomain.BaseDirectory + "Settings.ini";
			return;
		}
		string folder = Path.GetDirectoryName(IniPath);
		if (folder != "" && !Directory.Exists(folder))
		{
			Directory.CreateDirectory(folder);
		}
		filePath = new FileInfo(IniPath).FullName;
	}

	public string GetINIfilePath()
	{
		return filePath;
	}

	public string ReadString(string Key, string DefaultValue = "", string Section = null)
	{
		StringBuilder RetVal = new StringBuilder(512);
		GetPrivateProfileString(Section ?? EXE, Key, DefaultValue, RetVal, 512, filePath);
		return RetVal.ToString();
	}

	public int ReadInt(string Key, int DefaultValue = 0, string Section = "")
	{
		return GetPrivateProfileInt(Section ?? EXE, Key, DefaultValue, filePath);
	}

	public void Write(string Key, string Value, string Section = null)
	{
		WritePrivateProfileString(Section ?? EXE, Key, Value, filePath);
	}

	public void DeleteKey(string Key, string Section = null)
	{
		Write(Key, null, Section ?? EXE);
	}

	public void DeleteSection(string Section = null)
	{
		Write(null, null, Section ?? EXE);
	}

	public bool KeyExists(string Key, string Section = null)
	{
		return ReadString(Key, "", Section).Length > 0;
	}
}
