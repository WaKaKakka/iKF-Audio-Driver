using System;
using System.IO;

namespace XearONE.UI.Driver.Settings;

public class AutoSave<T> where T : class, new()
{
	private readonly string _filePath;

	private readonly Func<string, T> _loader;

	private readonly Action<string, T> _saver;

	private T _instance;

	public T Value => _instance;

	public AutoSave(string filePath, Func<string, T> loader, Action<string, T> saver)
	{
		_filePath = filePath;
		_loader = loader;
		_saver = saver;
		if (!File.Exists(filePath))
		{
			_saver(filePath, new T());
		}
		_instance = loader(filePath);
	}

	public void Update(Action<T> modifyAction)
	{
		modifyAction(_instance);
		_saver(_filePath, _instance);
	}

	public void Save()
	{
		_saver(_filePath, _instance);
	}

	public void Reload()
	{
		_instance = _loader(_filePath);
	}
}
