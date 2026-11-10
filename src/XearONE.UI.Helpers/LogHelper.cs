using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace XearONE.UI.Helpers;

public static class LogHelper
{
	public enum LogLevel
	{
		Trace,
		Debug,
		Info,
		Warning,
		Error,
		Critical
	}

	public static LogLevel CurrentLevel { get; set; }

	private static void Log(LogLevel level, string message)
	{
		if (level >= CurrentLevel)
		{
			Console.WriteLine($"[{level}] {message}");
		}
	}

	private static void Log(LogLevel level, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
	{
		if (level >= CurrentLevel)
		{
			string className = Path.GetFileName(callerFilePath);
			Console.WriteLine($"[{level}] {className}: {callerName}() - {message}");
		}
	}

	public static void Trace(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
	{
		Log(LogLevel.Trace, message, callerName, callerFilePath);
	}

	public static void Debug(string message, bool isPrintCaller = false, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
	{
		if (isPrintCaller)
		{
			Log(LogLevel.Debug, message, callerName, callerFilePath);
		}
		else
		{
			Log(LogLevel.Debug, message);
		}
	}

	public static void Info(string message, bool isPrintCaller = false, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
	{
		if (isPrintCaller)
		{
			Log(LogLevel.Info, message, callerName, callerFilePath);
		}
		else
		{
			Log(LogLevel.Info, message);
		}
	}

	public static void Warning(string message, bool isPrintCaller = false, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
	{
		if (isPrintCaller)
		{
			Log(LogLevel.Warning, message, callerName, callerFilePath);
		}
		else
		{
			Log(LogLevel.Warning, message);
		}
	}

	public static void Error(string message, bool isPrintCaller = false, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
	{
		if (isPrintCaller)
		{
			Log(LogLevel.Error, message, callerName, callerFilePath);
		}
		else
		{
			Log(LogLevel.Error, message);
		}
	}

	public static void Critical(string message, bool isPrintCaller = false, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
	{
		if (isPrintCaller)
		{
			Log(LogLevel.Critical, message, callerName, callerFilePath);
		}
		else
		{
			Log(LogLevel.Critical, message);
		}
	}
}
