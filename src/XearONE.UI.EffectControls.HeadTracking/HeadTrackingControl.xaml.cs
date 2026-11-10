using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.ControlWindow;
using XearONE.UI.Driver;

namespace XearONE.UI.EffectControls.HeadTracking;

public partial class HeadTrackingControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid AudioEndpointUID = Guid.Empty;

	private const string propertyName_HeadTracking_Enable = "HeadTracking_Enable";

	private const string propertyName_HeadTracking_Reverb = "HeadTracking_Reverb_Enable";

	private const string propertyName_HeadTracking_Yaw = "HeadTracking_OrientationYaw";

	private const string propertyName_HeadTracking_Roll = "HeadTracking_OrientationRoll";

	private const string propertyName_HeadTracking_Pitch = "HeadTracking_OrientationPitch";

	private const string propertyName_HeadTracking_DetectTime = "HeadTracking_AutoReset_DetectTime";

	private const string propertyName_HeadTracking_Reset = "HeadTracking_Reset";

	private const string propertyName_HeadTracking_Orientation_Enable = "HeadTracking_Orientation_Enable";

	private const int intervalRedraw = 40;

	private const int intervalCheckConnect = 1000;

	private GuideWindow guideWindow;

	private HeadTrackingPipeServer pipeServer;

	private volatile bool b3DAudioEnable;

	private volatile bool bHeadTrackingOrientationEnable;

	private Timer checkTimer;

	private Timer redrawTimer;

	public List<string> propertyNames { get; set; } = new List<string>();

	public HeadTrackingControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		AudioEndpointUID = audioEndpointUID;
		InitPropertyNames();
		windowsFormsHost.Child = ThreeDViewer.getInstance();
	}

	private void OnPipeMessageReceived(object sender, string message)
	{
		Application.Current.Dispatcher.Invoke(() =>
		{
			string[] array = message.Split(',');
			if (array[0] == "HeadTracking" && array[1] == "Angles")
			{
				if (array.Length >= 5)
				{
					try
					{
						int yaw = (int)double.Parse(array[2]);
						int pitch = (int)double.Parse(array[3]);
						int roll = (int)double.Parse(array[4]);
						SetHeadTrackingangleProperties(yaw, pitch, roll);
						return;
					}
					catch (Exception)
					{
						statusUpdate("Error: message: " + message);
						return;
					}
				}
				statusUpdate(message);
			}
			else if (array[0] == "HeadTracking" && array[1] == "Calibration")
			{
				statusUpdate(message);
			}
			else if ((!(array[0] == "HeadTracking") || !(array[1] == "StartSendingData")) && (!(array[0] == "HeadTracking") || !(array[1] == "StopSendingData")))
			{
				statusUpdate(message);
			}
		});
	}

	private void statusUpdate(string msg)
	{
	}

	private void SetHeadTrackingangleProperties(int yaw, int pitch, int roll)
	{
		int inValue = 0;
		inValue = yaw + 180;
		XearSDK.SetProperty(AudioEndpointUID, "HeadTracking_OrientationYaw", inValue, out var cmErr);
		inValue = pitch + 180;
		XearSDK.SetProperty(AudioEndpointUID, "HeadTracking_OrientationPitch", inValue, out cmErr);
		inValue = roll + 180;
		XearSDK.SetProperty(AudioEndpointUID, "HeadTracking_OrientationRoll", inValue, out cmErr);
	}

	private void HeadTracking_Loaded(object sender, RoutedEventArgs e)
	{
		ThreeDViewer.getInstance().UpdateScaleFactor(Window.GetWindow(this));
		RefreshAllControls();
		resetTimerCheck(monitor: true);
		resetTimerRedraw(monitor: true);
		StartPipeServer_Click(null, null);
	}

	private void HeadTracking_Unloaded(object sender, RoutedEventArgs e)
	{
		if (pipeServer != null)
		{
			pipeServer.MessageReceived -= OnPipeMessageReceived;
			pipeServer.Stop();
		}
		resetTimerCheck(monitor: false);
		resetTimerRedraw(monitor: false);
		StopPipeServer_Click(null, null);
	}

	private void StartPipeServer_Click(object sender, RoutedEventArgs e)
	{
		double detectionTime = 10.0;
		double fadingTime = 1.0;
		double dYawShiftThrd = 3.0;
		double dYawPathThrd = 20.0;
		bool autoReset = false;
		DriverInstance.getInstance().LoadRecenterConfig(ref detectionTime, ref fadingTime, ref dYawShiftThrd, ref dYawPathThrd, ref autoReset);
		if (!autoReset)
		{
			detectionTime = 0.0;
		}
		int inValue = (int)detectionTime;
		XearSDK.SetProperty(AudioEndpointUID, "HeadTracking_AutoReset_DetectTime", inValue, out var cmErr);
		string endpointID = "";
		XearSDK.GetProperty(AudioEndpointUID, "EndpointID", out endpointID, out cmErr);
		XearSDK.GetProperty(AudioEndpointUID, "USBVendorID", out int usbVendorID, out cmErr);
		XearSDK.GetProperty(AudioEndpointUID, "USBProductID", out int usbProductID, out cmErr);
		if (pipeServer == null)
		{
			pipeServer = new HeadTrackingPipeServer();
		}
		pipeServer.MessageReceived += OnPipeMessageReceived;
		pipeServer.Start(endpointID, usbVendorID, usbProductID);
	}

	private void StopPipeServer_Click(object sender, RoutedEventArgs e)
	{
		if (pipeServer != null)
		{
			pipeServer.MessageReceived -= OnPipeMessageReceived;
			pipeServer.Stop();
		}
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(AudioEndpointUID, "HeadTracking_Enable", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
			b3DAudioEnable = Btn_Enable.IsChecked == true;
		}
		if (XearSDK.GetProperty(AudioEndpointUID, "HeadTracking_Orientation_Enable", out enable, out cmErr))
		{
			Btn_EnableHeadTracking.IsChecked = enable == 1;
			bHeadTrackingOrientationEnable = Btn_EnableHeadTracking.IsChecked == true;
		}
		int reverb = 0;
		if (XearSDK.GetProperty(AudioEndpointUID, "HeadTracking_Reverb_Enable", out reverb, out cmErr))
		{
			Btn_EnableReverb.IsChecked = reverb == 1;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("HeadTracking_Enable");
		propertyNames.Add("HeadTracking_Reverb_Enable");
	}

	private void resetTimerCheck(bool monitor, int updateInterval = 1000)
	{
		checkTimer?.Change(-1, -1);
		checkTimer = null;
		if (monitor)
		{
			checkTimer = new Timer((object obj) =>
			{
				OnCheckRxDeviceConnection();
			}, this, 0, updateInterval);
		}
	}

	private void resetTimerRedraw(bool monitor, int updateInterval = 40)
	{
		redrawTimer?.Change(-1, -1);
		redrawTimer = null;
		if (monitor)
		{
			redrawTimer = new Timer((object obj) =>
			{
				OnRedrawTimeOut();
			}, this, 0, updateInterval);
		}
		else
		{
			ThreeDViewer.getInstance().UpdateOrientation(0, 0, 0);
		}
	}

	private void OnCheckRxDeviceConnection()
	{
		Console.WriteLine("[CheckRxDeviceConnection]");
		bool show = false;
		if (Environment.TickCount - DriverInstance.getInstance().lastHIDTime > 3000)
		{
			if (ucWarningDialog.Visibility == Visibility.Visible)
			{
				return;
			}
			show = true;
		}
		else
		{
			if (ucWarningDialog.Visibility == Visibility.Hidden)
			{
				return;
			}
			show = false;
		}
		Dispatcher.BeginInvoke((Action)(() =>
		{
			ucWarningDialog.Visibility = ((!show) ? Visibility.Hidden : Visibility.Visible);
		}));
	}

	private void OnRedrawTimeOut()
	{
		int oriYaw = 0;
		int oriRoll = 0;
		int oriPitch = 0;
		if (b3DAudioEnable && bHeadTrackingOrientationEnable)
		{
			if (!XearSDK.GetProperty(AudioEndpointUID, "HeadTracking_OrientationYaw", out int outData, out XearSDK.CmErr cmErr))
			{
				return;
			}
			oriYaw = outData - 180;
			if (!XearSDK.GetProperty(AudioEndpointUID, "HeadTracking_OrientationRoll", out outData, out cmErr))
			{
				return;
			}
			oriRoll = outData - 180;
			if (!XearSDK.GetProperty(AudioEndpointUID, "HeadTracking_OrientationPitch", out outData, out cmErr))
			{
				return;
			}
			oriPitch = outData - 180;
		}
		else
		{
			oriYaw = 0;
			oriPitch = 0;
			oriRoll = 0;
		}
		ThreeDViewer.getInstance().UpdateOrientation(oriYaw, oriPitch, oriRoll);
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		b3DAudioEnable = Btn_Enable.IsChecked == true;
		XearSDK.SetProperty(AudioEndpointUID, "HeadTracking_Enable", enable, out var _);
		DriverInstance.getInstance().SetHIDStatus(AudioEndpointUID, "HeadTracking_Enable", (byte)enable);
	}

	private void Btn_EnableReverb_Click(object sender, RoutedEventArgs e)
	{
		int reverb = ((Btn_EnableReverb.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(AudioEndpointUID, "HeadTracking_Reverb_Enable", reverb, out var _);
	}

	private void Btn_EnableHeadTracking_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_EnableHeadTracking.IsChecked == true) ? 1 : 0);
		bHeadTrackingOrientationEnable = Btn_EnableHeadTracking.IsChecked == true;
		XearSDK.SetProperty(AudioEndpointUID, "HeadTracking_Orientation_Enable", enable, out var _);
		if (guideWindow != null)
		{
			guideWindow.isEnableHeadTrackingPressed = Btn_EnableHeadTracking.IsChecked.Value;
			guideWindow.CheckStepState();
		}
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		DriverInstance.getInstance().HeadTrackingReset();
	}

	private void Btn_Guide_Click(object sender, RoutedEventArgs e)
	{
		if (guideWindow != null)
		{
			return;
		}
		guideWindow = new GuideWindow(htGuide, AudioEndpointUID);
		foreach (Window window in Application.Current.Windows)
		{
			if (window.GetType() == typeof(SubWindow))
			{
				guideWindow.Left = window.Left + window.Width + 10.0;
				guideWindow.Top = window.Top + 30.0;
				guideWindow.Owner = window;
				break;
			}
		}
		guideWindow.Closed += (object s, EventArgs ev) =>
		{
			guideWindow = null;
		};
		guideWindow.Show();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
