using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelPeakMeter : UserControl, IComponentConnector
{
	private Guid _endpointUID;

	private bool _isMono;

	private XearSDK.CmDataFlow _dataFlow;

	private Timer timerMonitorPeakMeter;

	private WasapiCapture captureDevice;

	private WaveInEvent waveIn;

	public PanelPeakMeter(XearSDK.CmDataFlow dataFlow, bool isMono, Guid endpointUID)
	{
		InitializeComponent();
		_endpointUID = endpointUID;
		_isMono = isMono;
		_dataFlow = dataFlow;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		resetTimerMonitorPeakMeter(monitor: true);
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
		resetTimerMonitorPeakMeter(monitor: false);
	}

	public void UpdateUI(float MasterValue)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_PeakMeter_L.Value = MasterValue * 100f;
			Slider_PeakMeter_R.Value = MasterValue * 100f;
		}));
	}

	public void UpdateUI(float chanLValue, float chanRValue)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			try
			{
				Slider_PeakMeter_L.Value = chanLValue * 100f;
				Slider_PeakMeter_R.Value = chanRValue * 100f;
			}
			catch
			{
			}
		}));
	}

	private void RefreshPeakMeter()
	{
		if (Visibility != Visibility.Visible)
		{
			return;
		}
		XearSDK.CmErr cmErr;
		float[] outDats2;
		if (_isMono)
		{
			if (XearSDK.GetProperty(_endpointUID, "PeakValue", out float[] outDats, out cmErr, -1))
			{
				Dispatcher.BeginInvoke((Action)(() =>
				{
					UpdateUI(outDats[0], outDats[0]);
				}));
			}
		}
		else if (XearSDK.GetProperty(_endpointUID, "PeakValue", out outDats2, out cmErr, 0, 1))
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				UpdateUI(outDats2[0], outDats2[1]);
			}));
		}
	}

	private void resetTimerMonitorPeakMeter(bool monitor, int updateInterval = 100)
	{
		timerMonitorPeakMeter?.Change(-1, -1);
		timerMonitorPeakMeter = null;
		if (monitor)
		{
			timerMonitorPeakMeter = new Timer((object obj) =>
			{
				RefreshPeakMeter();
			}, this, 0, updateInterval);
		}
		else
		{
			UpdateUI(0f, 0f);
		}
	}

	private MMDevice getCaptureDevivceByEndpointId(string endpointID)
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

	private void StartRecording()
	{
		if (captureDevice != null)
		{
			return;
		}
		string endpointID = "";
		XearSDK.GetProperty(_endpointUID, "EndpointID", out endpointID, out XearSDK.CmErr cmErr);
		XearSDK.GetProperty(_endpointUID, "JackType", out int jackType, out cmErr);
		if (_dataFlow != XearSDK.CmDataFlow.CDF_Capture || 2 != jackType)
		{
			return;
		}
		if (endpointID != "")
		{
			MMDevice device = getCaptureDevivceByEndpointId(endpointID);
			if (device != null)
			{
				captureDevice = new WasapiCapture(device);
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
		captureDevice.WaveFormat = new WaveFormat(48000, 1);
		captureDevice.StartRecording();
	}

	private void StopRecording()
	{
		try
		{
			if (captureDevice != null)
			{
				captureDevice.StopRecording();
				captureDevice.Dispose();
				captureDevice = null;
			}
			if (waveIn != null)
			{
				waveIn.StopRecording();
				waveIn.Dispose();
				waveIn = null;
			}
		}
		catch
		{
			Console.WriteLine("Recording thread stop failed.");
		}
	}
}
