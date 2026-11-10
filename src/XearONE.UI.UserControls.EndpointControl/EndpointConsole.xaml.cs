using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.ControlWindow.Content;
using XearONE.UI.Driver;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.UserControls.EndpointControl.PropertyPanel;

namespace XearONE.UI.UserControls.EndpointControl;

public partial class EndpointConsole : UserControl, IComponentConnector
{
	private JackDeviceInfo _devInfo;

	private PanelBattery _panelBattery;

	private PanelJackInformation _panelJackInformation;

	private PanelSession _panelSession;

	private PanelVolume _panelVolume;

	private PanelPeakMeter _panelPeakMeter;

	private PanelEffect _panelEffect;

	private PanelBalance _panelBalance;

	private PanelMute _panelMute;

	private bool isMouseDown;

	private double startPosition;

	private int leftBorder;

	private MainWindow MW
	{
		[DebuggerStepThrough]
		get
		{
			return (MainWindow)Window.GetWindow(this);
		}
	}

	public JackDeviceInfo devInfo => _devInfo;

	public int orderInCanvas { get; set; }

	public bool isShow { get; set; }

	public EndpointConsole(JackDeviceInfo devInfo)
	{
		_devInfo = new JackDeviceInfo();
		_devInfo = devInfo;
		InitializeComponent();
		InitHIDEffect();
		if (devInfo.string50 == string.Empty && devInfo.licenseCategoryList.Count <= 1)
		{
			Border_IsCMDevice.Visibility = Visibility.Hidden;
		}
	}

	private void EndpointConsole_Loaded(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("EndpointConsole_Loaded:{0} {1}", _devInfo.controllerDisplayName, _devInfo.jackType.ToString());
		_devInfo.licenseCategoryList.Clear();
		XearSDK.GetProperty(_devInfo.audEndptUID, "SupportedLicenseCategory", out XearSDK.CmLicenseCategory[] outDats, out XearSDK.CmErr _);
		XearSDK.CmLicenseCategory[] array = outDats;
		foreach (XearSDK.CmLicenseCategory j in array)
		{
			_devInfo.licenseCategoryList.Add(j);
		}
		InitAllPanel();
	}

	private void EndpointConsole_Unloaded(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("EndpointConsole_Unloaded:{0} {1}", _devInfo.controllerDisplayName, _devInfo.jackType.ToString());
		UninitAllPanel();
	}

	private void Button_DeviceRemove_Click(object sender, RoutedEventArgs e)
	{
		MW.ShowDevice(isShow: false, devInfo.audEndptUID);
	}

	private void Button_LEDControl_Click(object sender, RoutedEventArgs e)
	{
		LEDControl ledControl = new LEDControl(_devInfo);
		MW.OpenSubWindow(ledControl);
	}

	private void Button_JackSetting_Click(object sender, RoutedEventArgs e)
	{
		JackSetting jackSetting = new JackSetting(_devInfo);
		MW.OpenSubWindow(jackSetting);
	}

	private void InitHIDEffect()
	{
		XearSDK.CmErr cmErr;
		if (devInfo.CMediaHIDObj != IntPtr.Zero)
		{
			byte[] EffectCode = DriverInstance.getInstance().GetSupportedList(devInfo.CMediaHIDObj);
			if (EffectCode.Length != 0)
			{
				byte[] array = EffectCode;
				foreach (byte effect in array)
				{
					if (effect == 0 || effect == 11)
					{
						continue;
					}
					int isEnable = 0;
					switch (effect)
					{
					case 2:
						if (_devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_XearSurroundHeadphone))
						{
							XearSDK.GetProperty(_devInfo.audEndptUID, "Enable_CONLAB_HPSPK_LFX", out isEnable, out cmErr);
							DriverInstance.getInstance().SetHIDStatus(devInfo.CMediaHIDObj, "Enable_CONLAB_HPSPK_LFX", (byte)isEnable);
						}
						break;
					case 14:
						if (_devInfo.string50.Equals("CMA196") && _devInfo.dataFlow == XearSDK.CmDataFlow.CDF_Render)
						{
							if (_devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_AINoiseReduction))
							{
								XearSDK.GetProperty(_devInfo.audEndptUID, "AI_Playback_NR_Enable", out isEnable, out cmErr);
								DriverInstance.getInstance().SetHIDStatus(devInfo.CMediaHIDObj, "AI_Playback_NR_Enable", (byte)isEnable);
							}
						}
						else if (_devInfo.dataFlow == XearSDK.CmDataFlow.CDF_Capture && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_AINoiseReduction))
						{
							XearSDK.GetProperty(_devInfo.audEndptUID, "AI_NR_Enable", out isEnable, out cmErr);
							DriverInstance.getInstance().SetHIDStatus(devInfo.CMediaHIDObj, "AI_NR_Enable", (byte)isEnable);
						}
						break;
					case 21:
						if (_devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_HeadTracking))
						{
							XearSDK.GetProperty(_devInfo.audEndptUID, "HeadTracking_Enable", out isEnable, out cmErr);
							DriverInstance.getInstance().SetHIDStatus(devInfo.CMediaHIDObj, "HeadTracking_Enable", (byte)isEnable);
						}
						break;
					}
				}
			}
		}
		if (devInfo.CMedia108DHIDObj != IntPtr.Zero && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_SpeakerShifter))
		{
			XearSDK.GetProperty(_devInfo.audEndptUID, "Enable_SS_LFX", out int isEnable2, out cmErr);
			DriverInstance.getInstance().SetHIDStatus(devInfo.CMediaHIDObj, "Enable_SS_LFX", (byte)isEnable2);
		}
	}

	private void InitAllPanel()
	{
		_panelBattery = new PanelBattery(_devInfo.HeadTrackingHIDObj);
		Grid_PanelBattery.Children.Add(_panelBattery);
		_panelJackInformation = new PanelJackInformation(_devInfo);
		Grid_PanelJackInformation.Children.Add(_panelJackInformation);
		_panelSession = new PanelSession(_devInfo.audEndptUID);
		Grid_PanelSession.Children.Add(_panelSession);
		_panelVolume = new PanelVolume(_devInfo.audEndptUID);
		Grid_PanelVolume.Children.Add(_panelVolume);
		_panelPeakMeter = new PanelPeakMeter(_devInfo.dataFlow, _devInfo.channelCount <= 1, _devInfo.audEndptUID);
		Grid_PanelPeakMeter.Children.Add(_panelPeakMeter);
		_panelEffect = new PanelEffect(_devInfo);
		_panelEffect.HorizontalAlignment = HorizontalAlignment.Right;
		Grid_PanelEffect.Children.Add(_panelEffect);
		_panelBalance = new PanelBalance(_devInfo.channelCount, _devInfo.audEndptUID);
		Grid_PanelBalance.Children.Add(_panelBalance);
		_panelVolume.VolMinimizeTriggered += _panelBalance.VolMinimizeTriggered;
		_panelMute = new PanelMute(_devInfo.dataFlow, _devInfo.audEndptUID);
		Grid_PanelMute.Children.Add(_panelMute);
		if (_devInfo.CMediaLEDObj == IntPtr.Zero)
		{
			Button_LEDControl.Visibility = Visibility.Hidden;
		}
		bool showGamingBorder = Border_IsCMDevice.Visibility == Visibility.Visible;
		_panelSession.ShowGamingBorder(showGamingBorder);
		_panelEffect.ShowGamingBorder(showGamingBorder);
		_panelBalance.ShowGamingBorder(showGamingBorder);
		_panelVolume.ShowGamingBorder(showGamingBorder);
	}

	private void UninitAllPanel()
	{
		Grid_PanelBattery.Children.Clear();
		Grid_PanelJackInformation.Children.Clear();
		Grid_PanelSession.Children.Clear();
		Grid_PanelVolume.Children.Clear();
		Grid_PanelPeakMeter.Children.Clear();
		Grid_PanelEffect.Children.Clear();
		_panelVolume.VolMinimizeTriggered -= _panelBalance.VolMinimizeTriggered;
		Grid_PanelBalance.Children.Clear();
		Grid_PanelMute.Children.Clear();
	}

	public void RefreshPanelBattery(int percentage, int chargingState)
	{
		if (_panelBattery != null)
		{
			Dispatcher.InvokeAsync(() =>
			{
				_panelBattery.UpdateUI(percentage, chargingState);
			});
		}
	}

	public void RefreshPanelJackInformation()
	{
		if (_panelJackInformation != null)
		{
			Dispatcher.InvokeAsync(() =>
			{
				_panelJackInformation.UpdateUI();
			});
		}
	}

	public void RefreshPanelSession()
	{
		if (_panelSession != null)
		{
			Dispatcher.InvokeAsync(() =>
			{
				_panelSession.UpdateUI();
			});
		}
	}

	public void RefreshPanelVolume()
	{
		if (_panelVolume != null)
		{
			Dispatcher.InvokeAsync(() =>
			{
				_panelVolume.UpdateUI();
			});
		}
	}

	public void RefreshPanelMute()
	{
		if (_panelMute != null)
		{
			Dispatcher.InvokeAsync(() =>
			{
				_panelMute.UpdateUI();
			});
		}
	}

	public void RefreshPanelEffect(string propName)
	{
		if (_panelEffect != null)
		{
			Dispatcher.InvokeAsync(() =>
			{
				_panelEffect.UpdateUI(propName);
			});
		}
	}

	public void RefreshLang()
	{
		if (_panelEffect != null)
		{
			Dispatcher.InvokeAsync(() =>
			{
				_panelEffect.RefreshLang();
			});
		}
	}

	private void Grid_DragArea_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (isMouseDown)
		{
			Canvas.SetLeft(this, Canvas.GetLeft(this) + e.GetPosition(this).X - (double)leftBorder);
			Canvas.GetLeft(this);
			_ = startPosition;
			MW.OnEndpointConsoleOrderdChanged(this, Canvas.GetLeft(this) - startPosition);
			Console.WriteLine($"MouseMove: {Canvas.GetLeft(this) - startPosition}");
		}
	}

	private void Grid_DragArea_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		(sender as Grid).CaptureMouse();
		startPosition = Canvas.GetLeft(this);
		Console.WriteLine($"startPosition: X:{startPosition}");
		Panel.SetZIndex(this, 1);
		leftBorder = Convert.ToInt32(e.GetPosition(this).X);
		MW.OnEndpointConsoleMoved();
		isMouseDown = true;
	}

	private void Grid_DragArea_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		isMouseDown = false;
		(sender as Grid).ReleaseMouseCapture();
		Panel.SetZIndex(this, 0);
		MW.AlignConsole();
	}
}
