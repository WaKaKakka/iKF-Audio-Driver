using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;

namespace XearONE.UI;

public class SingleInstanceAppHelper : IDisposable
{
	private Mutex _mutex;

	private EventWaitHandle _eventWaitHandle;

	private CancellationTokenSource _cts;

	private readonly string _appId;

	private readonly string _wakeMessage;

	private readonly Action _onWakeRequest;

	public bool IsFirstInstance { get; private set; }

	public SingleInstanceAppHelper(string appId, string wakeMessage, Action onWakeRequest)
	{
		_appId = CreateSafeId(appId);
		_wakeMessage = wakeMessage;
		_onWakeRequest = onWakeRequest;
	}

	public bool TryStart()
	{
		string mutexName = "Global\\" + _appId;
		_mutex = new Mutex(initiallyOwned: true, mutexName, out var createdNew);
		IsFirstInstance = createdNew;
		_eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, _appId);
		if (createdNew)
		{
			_cts = new CancellationTokenSource();
			ThreadPool.QueueUserWorkItem(ListenForMessages, _cts.Token);
			return true;
		}
		return false;
	}

	public void NotifyExistingInstance()
	{
		try
		{
			using NamedPipeClientStream client = new NamedPipeClientStream(_appId);
			client.Connect(1000);
			using StreamWriter writer = new StreamWriter(client);
			writer.AutoFlush = true;
			writer.WriteLine(_wakeMessage);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to notify existing instance: " + ex.Message);
		}
	}

	private void ListenForMessages(object state)
	{
		CancellationToken token = (CancellationToken)state;
		while (!token.IsCancellationRequested)
		{
			using NamedPipeServerStream server = new NamedPipeServerStream(_appId);
			try
			{
				server.WaitForConnection();
				using StreamReader reader = new StreamReader(server);
				if (reader.ReadLine() == _wakeMessage && _onWakeRequest != null)
				{
					_onWakeRequest();
				}
			}
			catch (IOException)
			{
			}
			catch (Exception ex2)
			{
				Console.WriteLine("Pipe error: " + ex2.Message);
			}
		}
	}

	public void Dispose()
	{
		if (_cts != null)
		{
			_cts.Cancel();
			_cts.Dispose();
			_cts = null;
		}
		if (_eventWaitHandle != null)
		{
			_eventWaitHandle.Close();
			_eventWaitHandle = null;
		}
		if (_mutex != null)
		{
			if (IsFirstInstance)
			{
				_mutex.ReleaseMutex();
			}
			_mutex.Dispose();
			_mutex = null;
		}
	}

	public string CreateSafeId(string rawName)
	{
		if (string.IsNullOrWhiteSpace(rawName))
		{
			rawName = Assembly.GetExecutingAssembly().GetName().Name;
		}
		string safe = rawName;
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char c in invalidFileNameChars)
		{
			safe = safe.Replace(c, '_');
		}
		return safe.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
	}
}
