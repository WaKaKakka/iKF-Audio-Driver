#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;

namespace XearONE.UI.EffectControls.MagicVoice2;

internal class SoundRecorder
{
	private static IProgress<string> AsrProgress = null;

	public static string OutputFolder = "";

	public static bool IsRecording = false;

	public static string TargetName = "";

	public static float PitchValue = -99f;

	public static bool IsListenToMic = true;

	private static TaskCompletionSource<int> taskStopRecording = null;

	private static MMDevice GetCaptureAudioEndpointByDeviceId(string targetDeviceID)
	{
		string targetKSNODETYPE_MIC = "{DFF21BE1-F70F-11D0-B917-00A0C9223196}";
		IEnumerable<MMDevice> CaptureDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
		if (CaptureDevices.Count() > 0)
		{
			foreach (MMDevice md in CaptureDevices)
			{
				string deviceID = "";
				string ksNodeType = "";
				PropVariant propDeviceId = md.Properties.GetValue(4);
				PropVariant propKsNodeTypeWin10 = md.Properties.GetValue(11);
				PropVariant propKsNodeTypeWin11 = md.Properties.GetValue(16);
				if (propDeviceId.DataType == VarEnum.VT_LPWSTR)
				{
					deviceID = propDeviceId.Value.ToString();
				}
				if (propKsNodeTypeWin10.DataType == VarEnum.VT_LPWSTR)
				{
					ksNodeType = propKsNodeTypeWin10.Value.ToString();
				}
				else if (propKsNodeTypeWin11.DataType == VarEnum.VT_LPWSTR)
				{
					ksNodeType = propKsNodeTypeWin11.Value.ToString();
				}
				if (deviceID.ToUpper().Contains(targetDeviceID.ToUpper()) && ksNodeType.ToUpper().Contains(targetKSNODETYPE_MIC.ToUpper()))
				{
					return md;
				}
			}
			return null;
		}
		return null;
	}

	public static MMDevice GetCaptureDeviceByEndpointId(string endpointID)
	{
		IEnumerable<MMDevice> CaptureDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
		if (CaptureDevices.Count() > 0)
		{
			foreach (MMDevice md in CaptureDevices)
			{
				if (md.ID.ToLower() == endpointID.ToLower())
				{
					return md;
				}
			}
			return null;
		}
		return null;
	}

	public static async Task StartRecordingAsync(IProgress<string> _asrProgress, string endpointID, bool outputRecordingFile = false, bool drawWaveForm = false)
	{
		IsRecording = true;
		taskStopRecording = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		await Task.Run(() =>
		{
			AsrProgress = _asrProgress;
			IWavePlayer wavePlayer = null;
			WasapiCapture captureDevice = null;
			try
			{
				if (endpointID != "")
				{
					MMDevice captureDeviceByEndpointId = GetCaptureDeviceByEndpointId(endpointID);
					if (captureDeviceByEndpointId != null)
					{
						captureDevice = new WasapiCapture(captureDeviceByEndpointId);
					}
					else
					{
						captureDevice = new WasapiCapture();
					}
				}
				else
				{
					captureDevice = new WasapiCapture();
				}
			}
			catch (Exception)
			{
				AsrProgress.Report("[StartRecordingAsync][Task.Run][OUT]===> Error when creating WasapiCapture!!!");
				IsRecording = false;
				return;
			}
			captureDevice.WaveFormat = new WaveFormat(16000, 1);
			BufferedWaveProvider provider_recording = new BufferedWaveProvider(captureDevice.WaveFormat);
			wavePlayer = new WaveOut();
			wavePlayer.Init(provider_recording);
			WaveFileWriter writer_rvc = null;
			if (outputRecordingFile)
			{
				OutputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIMagicVoice");
				Directory.CreateDirectory(OutputFolder);
				string filename = Path.Combine(OutputFolder, "RVC_" + $"{DateTime.Now:yyy-MM-dd_HH-mm-ss}_" + TargetName + "_Pitch_" + PitchValue.ToString("0.#") + ".wav");
				writer_rvc = new WaveFileWriter(filename, captureDevice.WaveFormat);
			}
			provider_recording.DiscardOnBufferOverflow = true;
			captureDevice.DataAvailable += (object s, WaveInEventArgs e) =>
			{
				if (IsListenToMic)
				{
					provider_recording.AddSamples(e.Buffer, 0, e.BytesRecorded);
				}
				float num = 0f;
				float num2 = 0f;
				if (writer_rvc != null)
				{
					writer_rvc.Write(e.Buffer, 0, e.BytesRecorded);
				}
				if (drawWaveForm)
				{
					int num3 = e.BytesRecorded * 8 / captureDevice.WaveFormat.BitsPerSample;
					float[] array = new float[num3];
					_ = new float[num3];
					for (int i = 0; i < num3; i++)
					{
						short num4 = (short)((e.Buffer[i * 2 + 1] << 8) | e.Buffer[i * 2]);
						array[i] = (float)num4 / 32768f;
						if (array[i] > num)
						{
							num = array[i];
						}
						if (array[i] < num2)
						{
							num2 = array[i];
						}
					}
					AsrProgress.Report("[AudioData] " + num.ToString("0.0") + " " + num2.ToString("0.0"));
				}
			};
			captureDevice.StartRecording();
			wavePlayer?.Play();
			Task[] tasks = new Task<int>[1] { taskStopRecording.Task };
			Task.WaitAny(tasks);
			if (captureDevice != null)
			{
				captureDevice.StopRecording();
				captureDevice.Dispose();
				captureDevice = null;
			}
			if (writer_rvc != null)
			{
				writer_rvc?.Dispose();
				writer_rvc = null;
			}
			if (wavePlayer != null)
			{
				wavePlayer.Stop();
				wavePlayer.Dispose();
				wavePlayer = null;
			}
			IsRecording = false;
		});
	}

	public static void StopRecording()
	{
		if (!IsRecording)
		{
			return;
		}
		taskStopRecording.TrySetResult(0);
		int nTimeoutCount = 0;
		while (IsRecording)
		{
			Thread.Sleep(10);
			nTimeoutCount++;
			if (nTimeoutCount >= 3000)
			{
				Trace.WriteLine("[CM][StopRecordingAsync] Error: 30-second timeout. IsRecording is still true.");
				break;
			}
		}
	}
}
