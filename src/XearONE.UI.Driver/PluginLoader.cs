using System;
using System.IO;
using System.Linq;
using System.Reflection;
using XearONE.Contracts;

namespace XearONE.UI.Driver;

public static class PluginLoader
{
	private static IGuidProvider _cached;

	public static IGuidProvider LoadGuidProvider()
	{
		if (_cached != null)
		{
			return _cached;
		}
		string baseDir = AppContext.BaseDirectory;
		Type contract = typeof(IGuidProvider);
		foreach (string dll in Directory.EnumerateFiles(baseDir, "*.GuidPlugin.dll", SearchOption.TopDirectoryOnly))
		{
			Assembly asm;
			try
			{
				asm = Assembly.LoadFrom(dll);
			}
			catch (Exception)
			{
				continue;
			}
			Type implType = null;
			try
			{
				implType = asm.GetTypes().FirstOrDefault((Type t) => !t.IsAbstract && contract.IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null);
			}
			catch (ReflectionTypeLoadException ex2)
			{
				string.Join("; ", ex2.LoaderExceptions.Select((Exception e) => e.Message));
				continue;
			}
			if (implType != null)
			{
				_cached = (IGuidProvider)Activator.CreateInstance(implType);
				return _cached;
			}
		}
		throw new InvalidOperationException("No IGuidProvider plugin found in " + baseDir + ".");
	}

	public static void ResetCacheForTests()
	{
		_cached = null;
	}
}
