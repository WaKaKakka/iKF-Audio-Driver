using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace XearONE.UI.EffectControls.HeadTracking;

internal class HeadTrackingPipeServer
{
	private string pipeServerName = "";

	private SilentAudioPlayer _silentPlayer;

	private volatile bool _isRunning;

	public event EventHandler<string> MessageReceived;

	private void OnMessageReceived(string message)
	{
		if (_isRunning)
		{
			MessageReceived?.Invoke(this, message);
		}
	}

	public int Start(string endpointID, int vid, int pid)
	{
		if (_isRunning)
		{
			return 1;
		}
		_isRunning = true;
		pipeServerName = "HeadTrackingPipeServer_" + vid.ToString("X4") + "_" + pid.ToString("X4");
		if (IsPipeServerAlreadyRunning(pipeServerName))
		{
			return -1;
		}
		Thread thread = new Thread(RunPipeServer);
		thread.IsBackground = true;
		thread.Start();
		_silentPlayer = new SilentAudioPlayer();
		_silentPlayer.PlaySilentAudioToDevice(endpointID);
		return 0;
	}

	public void Stop()
	{
		if (!_isRunning)
		{
			return;
		}
		if (_silentPlayer != null)
		{
			_silentPlayer.Dispose();
		}
		_isRunning = false;
		try
		{
			using NamedPipeClientStream dummyClient = new NamedPipeClientStream(".", pipeServerName, PipeDirection.Out);
			dummyClient.Connect(100);
		}
		catch
		{
			Console.WriteLine("Error: Pipe isn't ready.");
		}
		MessageReceived = null;
	}

	private bool IsPipeServerAlreadyRunning(string pipeName)
	{
		try
		{
			using NamedPipeClientStream client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None);
			client.Connect(100);
			return true;
		}
		catch (TimeoutException)
		{
			return false;
		}
		catch (IOException)
		{
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void RunPipeServer()
	{
		while (_isRunning)
		{
			PipeSecurity pipeSecurity = new PipeSecurity();
			pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
			using NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeServerName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, pipeSecurity);
			pipeServer.WaitForConnection();
			byte[] buffer = new byte[256];
			int bytesRead = pipeServer.Read(buffer, 0, buffer.Length);
			string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
			OnMessageReceived(message);
		}
	}
}
