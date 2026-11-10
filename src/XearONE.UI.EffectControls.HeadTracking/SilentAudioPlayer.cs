using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace XearONE.UI.EffectControls.HeadTracking;

public class SilentAudioPlayer : IDisposable
{
	private WasapiOut _waveOut;

	private BufferedWaveProvider _silentProvider;

	public void PlaySilentAudioToDevice(string endpointID)
	{
		MMDeviceCollection mMDeviceCollection = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
		MMDevice device = null;
		foreach (MMDevice md in mMDeviceCollection)
		{
			if (md.ID.ToLower() == endpointID.ToLower())
			{
				device = md;
				break;
			}
		}
		if (device == null)
		{
			Console.WriteLine("Error: Failed to find the device.");
			return;
		}
		WaveFormat waveFormat = new WaveFormat(48000, 16, 2);
		_silentProvider = new BufferedWaveProvider(waveFormat);
		byte[] silence = new byte[waveFormat.AverageBytesPerSecond];
		_silentProvider.AddSamples(silence, 0, silence.Length);
		_waveOut = new WasapiOut(device, AudioClientShareMode.Shared, useEventSync: true, 100);
		_waveOut.Init(_silentProvider);
		_waveOut.Play();
	}

	public void Dispose()
	{
		_waveOut?.Stop();
		_waveOut?.Dispose();
		_waveOut = null;
	}
}
