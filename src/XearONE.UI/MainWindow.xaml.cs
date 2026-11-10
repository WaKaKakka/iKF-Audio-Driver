using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.ControlWindow;
using XearONE.UI.ControlWindow.Content;
using XearONE.UI.ControlWindow.Content.AutoSwitch;
using XearONE.UI.Driver;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.Driver.Profiles;
using XearONE.UI.Helpers;
using XearONE.UI.Keyboard;
using XearONE.UI.UserControls;
using XearONE.UI.UserControls.EndpointControl;
using XearONE.UI.UserControls.GlobalProfile;

namespace XearONE.UI;

public partial class MainWindow : Window, IComponentConnector
{
	public delegate void FuncPtrDeviceState(Guid audEndptUID, XearSDK.CmDeviceState state);

	public delegate void FuncPtrDefaultDevice(Guid audEndptUID);

	public delegate void FuncPtrDefaultFormat(Guid audEndptUID);

	public delegate void FuncPtrDeviceVolume(Guid audEndptUID);

	public delegate void FuncPtrEffectValue(Guid audEndptUID, string prop);

	public delegate void FuncPtrAudioSession(Guid audEndptUID);

	public delegate void FuncPtrForegroundProcessInfo(XearSDK.ProcessInfo info);

	private enum ButtonState
	{
		None,
		Pressed,
		Released
	}

	public delegate void FuncPtrRefresh();

	public delegate void FuncPtrRefreshEffect(string prop);

	private IntPtr hWnd = IntPtr.Zero;

	private SystemTray _systemTray;

	private bool isSilent;

	private RegisterHotKeyHelper registerHotKeyHelper;

	private const int FullSize = 1366;

	private const int TripleSize = 1042;

	private const int DualSize = 730;

	private FuncPtrDeviceState FnPtrDevState;

	private FuncPtrDefaultDevice FnPtrDefaultDev;

	private FuncPtrDefaultFormat FnPtrDefaultFmt;

	private FuncPtrDeviceVolume FnPtrDevVol;

	private FuncPtrEffectValue FnPtrEffectVal;

	private FuncPtrAudioSession FnPtrAudSes;

	private FuncPtrForegroundProcessInfo FnPtrForegndInfo;

	private readonly XearSDK.CmFuncPtrCallback EventCallbackFuncPtr;

	private readonly HIDSDK.fp_NotifyEventHIDData HIDEventCallbackFuncPtr;

	private List<string> IgnorePropertyList = new List<string>
	{
		"HeadTracking_OrientationYaw", "HeadTracking_OrientationPitch", "HeadTracking_OrientationRoll", "SS_LF_ANGLE", "SS_RF_ANGLE", "SS_LS_ANGLE", "SS_RS_ANGLE", "SS_LB_ANGLE", "SS_RB_ANGLE", "SS_C_ANGLE",
		"SS_LFE_ANGLE", "SS_LF_DIST", "SS_RF_DIST", "SS_LS_DIST", "SS_RS_DIST", "SS_LB_DIST", "SS_RB_DIST", "SS_C_DIST", "SS_LFE_DIST"
	};

	private Timer deviceChangedDebouncer;

	private Timer defaultDeviceDebouncer;

	private List<(Guid, string)> propertyChangedList = new List<(Guid, string)>();

	private Timer profileChangedDebouncer;

	private const byte reportID_XearHID = 237;

	private const byte reportID_XSH = 2;

	private const byte htReportID = 85;

	private const byte htCommandID = 111;

	private const byte htBatteryID = 101;

	private const byte htCoordinateQ4 = 135;

	private const byte htCoordinateRAW = 136;

	private ButtonState _lastState;

	private SubWindow subWindow;

	private FuncPtrRefresh FnPtrRefresh;

	private FuncPtrRefreshEffect FnPtrRefreshEffect;

	private List<EndpointConsole> epConsoleList = new List<EndpointConsole>();

	private readonly object consoleListLock = new object();

	public readonly int ControlGap = 320;

	private List<EndpointConsole> tempConsoleList = new List<EndpointConsole>();

	public MainWindow()
	{
		InitializeComponent();
		Title = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false).OfType<AssemblyTitleAttribute>()
			.FirstOrDefault()?.Title;
		DriverInstance.getInstance();
		if (!XearSDK.Initialize(out var _))
		{
			string caption = "Error";
			MessageBoxButton button = MessageBoxButton.OK;
			MessageBoxImage icon = MessageBoxImage.Hand;
			MessageBox.Show("SDK Initial fail!", caption, button, icon, MessageBoxResult.OK);
			Close();
		}
		if (!DriverInstance.getInstance().HIDSDKInit())
		{
			string caption2 = "Error";
			MessageBoxButton button2 = MessageBoxButton.OK;
			MessageBoxImage icon2 = MessageBoxImage.Hand;
			MessageBox.Show("HIDSDK Initial fail!", caption2, button2, icon2, MessageBoxResult.OK);
			Close();
		}
		Window window = Window.GetWindow(this);
		hWnd = new WindowInteropHelper(window).EnsureHandle();
		EventCallbackFuncPtr = EventCallbackDispatcher;
		HIDEventCallbackFuncPtr = HIDeventCallback;
		LaunchArguments();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		LogHelper.Trace("Begin", "Window_Loaded", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.xaml.cs");
		DriverInstance.getInstance().ActivateGUIDLicense();
		XearSDK.SetEventSettings(CreateEventSettings(doReg: true), out var _);
		RegisterCallbackFunction();
		_systemTray = new SystemTray();
		_systemTray.ResumeWindowRequested += () =>
		{
			WindowState = WindowState.Normal;
			Show();
			Activate();
		};
		_systemTray.CloseWindowRequested += () =>
		{
			Close();
		};
		if (DriverInstance.getInstance().LoadHotKey())
		{
			registerHotKeyHelper = new RegisterHotKeyHelper();
			registerHotKeyHelper.RegisterHotKey(hWnd);
		}
		StartDeviceDiscovery();
		InitGuideControl();
		RefreshProfileButton();
		if (isSilent)
		{
			Hide();
		}
		LogHelper.Trace("End", "Window_Loaded", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.xaml.cs");
	}

	private void StartDeviceDiscovery()
	{
		Thread thread = new Thread(DeviceDiscoveryWorker)
		{
			IsBackground = true
		};
		thread.Start();
	}

	private void DeviceDiscoveryWorker()
	{
		try
		{
			DriverInstance.getInstance().RebuildAllDeviceList();
		}
		catch (Exception ex)
		{
			LogHelper.Debug("Background device discovery failed: " + ex.Message, isPrintCaller: false, "DeviceDiscoveryWorker", "MainWindow.xaml.cs");
		}

		if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
		{
			Dispatcher.Invoke(RefreshDeviceUi);
		}
	}

	private void RefreshDeviceUi()
	{
		InitHIDDevice();
		RebuildEndpointConsoleList();
		RefreshCanvas();
		RefreshSubWindow();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		Console.WriteLine("MainWindow: Window_Closing()");
		if (registerHotKeyHelper != null)
		{
			registerHotKeyHelper.UnregisterHotKey(hWnd);
		}
		XearSDK.SetEventSettings(CreateEventSettings(doReg: false), out var cmErr);
		DriverInstance.getInstance().SetHIDEventCallbackValue(new HIDSDK.CmValueEventCallbackHIDRemovalArrival
		{
			callback = null,
			userData = IntPtr.Zero
		});
		XearSDK.Deinitialize(out cmErr);
		DriverInstance.getInstance().HIDSDKUninit();
		_systemTray?.Dispose();
		Application.Current.Shutdown();
	}

	private void LaunchArguments()
	{
		string[] args = Environment.GetCommandLineArgs();
		foreach (string arg in args)
		{
			if (arg.Equals("-s", StringComparison.OrdinalIgnoreCase))
			{
				isSilent = true;
			}
			else if (arg.StartsWith("-log:", StringComparison.OrdinalIgnoreCase))
			{
				string levelStr = arg.Substring("-log:".Length);
				if (Enum.TryParse<LogHelper.LogLevel>(levelStr, ignoreCase: true, out var level))
				{
					LogHelper.CurrentLevel = level;
					LogHelper.Info("Log level set to " + level, isPrintCaller: false, "LaunchArguments", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.xaml.cs");
				}
				else
				{
					LogHelper.Warning("Invalid log level specified: " + levelStr, isPrintCaller: false, "LaunchArguments", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.xaml.cs");
				}
			}
		}
	}

	private void InitHIDDevice()
	{
		DriverInstance.getInstance().RegisterHIDCallBack(new HIDSDK.CmValueEventCallbackHIDData
		{
			callback = HIDEventCallbackFuncPtr,
			userData = hWnd
		});
	}

	private void InitGuideControl()
	{
		if (DriverInstance.getInstance().configManager.jsonAppConfig.DisableMainGuide != 1 && !DriverInstance.getInstance().LoadIsDisableMainGuide())
		{
			MainGuideControl mainGuideControl = new MainGuideControl();
			mainGuideControl.RequestRemoval += (object s, EventArgs e) =>
			{
				Panel_GuideControl.Children.Remove((MainGuideControl)s);
				AutoSize();
			};
			Panel_GuideControl.Children.Add(mainGuideControl);
		}
	}

	public void AutoSize(bool enable = true)
	{
		if (!enable)
		{
			Dispatcher.Invoke(() =>
			{
				Width = 1366.0;
				Rect_Background.Width = 1366.0;
			});
			return;
		}
		foreach (object child in Panel_GuideControl.Children)
		{
			if (child is MainGuideControl)
			{
				Dispatcher.Invoke(() =>
				{
					Width = 1366.0;
					Rect_Background.Width = 1366.0;
				});
				return;
			}
		}
		int countShown = 0;
		foreach (EndpointConsole epConsole in epConsoleList)
		{
			if (epConsole.isShow)
			{
				countShown++;
			}
		}
		Dispatcher.Invoke(() =>
		{
			if (countShown <= 2)
			{
				Width = 730.0;
				Rect_Background.Width = 730.0;
			}
			else if (countShown == 3)
			{
				Width = 1042.0;
				Rect_Background.Width = 1042.0;
			}
			else
			{
				Width = 1366.0;
				Rect_Background.Width = 1366.0;
			}
		});
	}

	public void UnregisterEventDeviceState()
	{
		FnPtrDevState = null;
	}

	public void RegisterEventDeviceState(FuncPtrDeviceState fnPtr)
	{
		FnPtrDevState = fnPtr.Invoke;
	}

	public void UnregisterEventDefaultDevice()
	{
		FnPtrDefaultDev = null;
	}

	public void RegisterEventDefaultDevice(FuncPtrDefaultDevice fnPtr)
	{
		FnPtrDefaultDev = fnPtr.Invoke;
	}

	public void UnregisterEventDefaultFormat()
	{
		FnPtrDefaultFmt = null;
	}

	public void RegisterEventDefaultFormat(FuncPtrDefaultFormat fnPtr)
	{
		FnPtrDefaultFmt = fnPtr.Invoke;
	}

	public void UnregisterEventDeviceVolume()
	{
		FnPtrDevVol = null;
	}

	public void RegisterEventDeviceVolume(FuncPtrDeviceVolume fnPtr)
	{
		FnPtrDevVol = fnPtr.Invoke;
	}

	public void UnregisterEventEffectValue()
	{
		FnPtrEffectVal = null;
	}

	public void RegisterEventEffectValue(FuncPtrEffectValue fnPtr)
	{
		FnPtrEffectVal = fnPtr.Invoke;
	}

	public void UnregisterEventAudioSession()
	{
		FnPtrAudSes = null;
	}

	public void RegisterEventAudioSession(FuncPtrAudioSession fnPtr)
	{
		FnPtrAudSes = fnPtr.Invoke;
	}

	public void DeregisterEventForegroundProcessInfo()
	{
		FnPtrForegndInfo = null;
	}

	public void RegisterEventForegroundProcessInfo(FuncPtrForegroundProcessInfo fnPtr)
	{
		FnPtrForegndInfo = fnPtr.Invoke;
	}

	private XearSDK.EventSettings CreateEventSettings(bool doReg)
	{
		XearSDK.EventSettings result = new XearSDK.EventSettings
		{
			Callback = (doReg ? EventCallbackFuncPtr : null)
		};
		IntPtr userData;
		if (doReg)
		{
			_ = hWnd;
			userData = new WindowInteropHelper(Window.GetWindow(this)).EnsureHandle();
		}
		else
		{
			userData = IntPtr.Zero;
		}
		result.UserData = userData;
		return result;
	}

	private void RegisterCallbackFunction()
	{
		RegisterEventDeviceState(OnDeviceStateChanged);
		RegisterEventDefaultDevice(OnDefaultDeviceChanged);
		RegisterEventDefaultFormat(OnDefaultFormatChanged);
		RegisterEventDeviceVolume(OnDeviceVolumeChanged);
		RegisterEventEffectValue(OnEffectValueChanged);
		RegisterEventAudioSession(OnAudioSessionChanged);
		RegisterEventForegroundProcessInfo(OnForegroundChanged);
	}

	private void EventCallbackDispatcher(XearSDK.CmCallbackType cbType, Guid audEndptUID, XearSDK.CmDataType datType, int arrLen, IntPtr arrDat, IntPtr usrDat)
	{
		if (!(usrDat != IntPtr.Zero) || !(HwndSource.FromHwnd(usrDat)?.RootVisual is MainWindow mw))
		{
			return;
		}
		if (!XearSDK.ToCSWrappedArray(datType, arrLen, arrDat, out var csArrDat))
		{
			return;
		}
		Console.WriteLine("EventCallbackDispatcher(): {0}", cbType.ToString());
		switch (cbType)
		{
		case XearSDK.CmCallbackType.CCT_DeviceState:
			mw.Dispatcher.InvokeAsync(() =>
			{
				FnPtrDevState?.Invoke(audEndptUID, (XearSDK.CmDeviceState)((int[])csArrDat)[0]);
			});
			break;
		case XearSDK.CmCallbackType.CCT_DefaultDevice:
			mw.Dispatcher.InvokeAsync(() =>
			{
				FnPtrDefaultDev?.Invoke(audEndptUID);
			});
			break;
		case XearSDK.CmCallbackType.CCT_DefaultFormat:
			mw.Dispatcher.InvokeAsync(() =>
			{
				FnPtrDefaultFmt?.Invoke(audEndptUID);
			});
			break;
		case XearSDK.CmCallbackType.CCT_DeviceVolume:
			mw.Dispatcher.InvokeAsync(() =>
			{
				FnPtrDevVol?.Invoke(audEndptUID);
			});
			break;
		case XearSDK.CmCallbackType.CCT_EffectValue:
			mw.Dispatcher.InvokeAsync(() =>
			{
				FnPtrEffectVal?.Invoke(audEndptUID, ((string[])csArrDat)[0]);
			});
			break;
		case XearSDK.CmCallbackType.CCT_AudioSession:
			mw.Dispatcher.InvokeAsync(() =>
			{
				FnPtrAudSes?.Invoke(audEndptUID);
			});
			break;
		case XearSDK.CmCallbackType.CCT_Foreground:
			mw.Dispatcher.InvokeAsync(() =>
			{
				FnPtrForegndInfo?.Invoke(((XearSDK.ProcessInfo[])csArrDat)[0]);
			});
			break;
		case XearSDK.CmCallbackType.CCT_SystemDynamicLighting:
			break;
		}
	}

	private async void OnDeviceStateChanged(Guid audEndptUID, XearSDK.CmDeviceState state)
	{
		Console.WriteLine("[OnDeviceStateChanged] audEndptUID: {0}, state: {1}", audEndptUID.ToString(), state.ToString());
		CloseSubWindowOnDeviceChange();
		await Task.Run(() =>
		{
			DeviceChangedDebouncer();
		});
	}

	private async void OnDefaultDeviceChanged(Guid audEndptUID)
	{
		Console.WriteLine("[OnDefaultDeviceChanged] audEndptUID: {0}", audEndptUID.ToString());
		await Task.Run(() =>
		{
			DefaultDeviceDebouncer();
		});
	}

	private async void OnDefaultFormatChanged(Guid audEndptUID)
	{
		Console.WriteLine("[OnDefaultFormatChanged] audEndptUID: {0}", audEndptUID.ToString());
		await Task.Run(() =>
		{
			RefreshSubWindow();
		});
	}

	private async void OnDeviceVolumeChanged(Guid audEndptUID)
	{
		Console.WriteLine("[OnDeviceVolumeChanged] audEndptUID: {0}", audEndptUID.ToString());
		await Task.Run(() =>
		{
			RefreshEndpointControl(audEndptUID, XearSDK.CmCallbackType.CCT_DeviceVolume);
		});
	}

	private async void OnEffectValueChanged(Guid audEndptUID, string prop)
	{
		foreach (string propertyName in IgnorePropertyList)
		{
			if (prop.CompareTo(propertyName) == 0)
			{
				return;
			}
		}
		Console.WriteLine("[OnEffectValueChanged] audEndptUID: {0}, prop: {1}", audEndptUID.ToString(), prop);
		ProfileChangedDebouncer(audEndptUID, prop);
		await Task.Run(() =>
		{
			RefreshEndpointControl(audEndptUID, prop);
			RefreshSubWindow(prop);
		});
	}

	private async void OnAudioSessionChanged(Guid audEndptUID)
	{
		Console.WriteLine("[OnAudioSessionChanged] audEndptUID: {0}", audEndptUID.ToString());
		await Task.Run(() =>
		{
			RefreshEndpointControl(audEndptUID, XearSDK.CmCallbackType.CCT_AudioSession);
		});
	}

	private async void OnForegroundChanged(XearSDK.ProcessInfo info)
	{
		string fileName = Path.GetFileName(info.FilePath);
		LogHelper.Debug("Process Info: " + fileName, isPrintCaller: false, "OnForegroundChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
		LogHelper.Debug("Process Path: " + info.FilePath, isPrintCaller: false, "OnForegroundChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
		await Task.Run(() =>
		{
			CheckProfileAutoSwitch(info);
		});
	}

	private void RefreshDefaultDeviceIcon()
	{
		Console.WriteLine("Refresh Default Device Icon");
		RefreshSubWindow();
		Dispatcher.Invoke(() =>
		{
			foreach (EndpointConsole item in EndpointControlCanvas.Children.Cast<EndpointConsole>().ToList())
			{
				item.RefreshPanelJackInformation();
			}
		});
	}

	private void RefreshEndpointControl(Guid audEndptUID, XearSDK.CmCallbackType eventID = XearSDK.CmCallbackType.CCT_Unknown)
	{
		Dispatcher.Invoke(() =>
		{
			if (EndpointControlCanvas.Children.Cast<EndpointConsole>().ToList().Count != 0)
			{
				if (eventID == XearSDK.CmCallbackType.CCT_DeviceVolume)
				{
					EndpointConsole endpointConsole = GetEndpointConsole(audEndptUID);
					if (endpointConsole != null)
					{
						endpointConsole.RefreshPanelMute();
						endpointConsole.RefreshPanelVolume();
					}
				}
				else if (eventID == XearSDK.CmCallbackType.CCT_AudioSession)
				{
					GetEndpointConsole(audEndptUID)?.RefreshPanelSession();
				}
			}
		});
	}

	private void RefreshEndpointControl(Guid audEndptUID, string propName)
	{
		GetEndpointConsole(audEndptUID)?.RefreshPanelEffect(propName);
	}

	private void AudioEndpointRemovalArrivalCallback()
	{
		StartDeviceDiscovery();
	}

	private void CheckProfileAutoSwitch(XearSDK.ProcessInfo info)
	{
		LogHelper.Trace("", "CheckProfileAutoSwitch", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
		AutoSwitchTemplate autoSwitch = DriverInstance.getInstance().AutoSwitchCollection.Find((AutoSwitchTemplate x) => x.appPath == info.FilePath);
		if (autoSwitch != null)
		{
			LogHelper.Debug("FilePath Matched, Auto Switch.", isPrintCaller: false, "CheckProfileAutoSwitch", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
			DriverInstance.getInstance().ProfileChanged(autoSwitch.bindingProfileName);
			RefreshProfileButton();
		}
	}

	private void DeviceChangedDebouncer()
	{
		if (deviceChangedDebouncer == null)
		{
			deviceChangedDebouncer = new Timer(500.0);
			deviceChangedDebouncer.Elapsed += (object s, ElapsedEventArgs e) =>
			{
				RefreshDevice();
			};
			deviceChangedDebouncer.AutoReset = false;
		}
		deviceChangedDebouncer.Stop();
		deviceChangedDebouncer.Start();
	}

	private void RefreshDevice()
	{
		deviceChangedDebouncer?.Stop();
		deviceChangedDebouncer = null;
		AudioEndpointRemovalArrivalCallback();
	}

	private void DefaultDeviceDebouncer()
	{
		if (defaultDeviceDebouncer == null)
		{
			defaultDeviceDebouncer = new Timer(500.0);
			defaultDeviceDebouncer.Elapsed += (object s, ElapsedEventArgs e) =>
			{
				RefreshDefaultDevice();
			};
			defaultDeviceDebouncer.AutoReset = false;
		}
		defaultDeviceDebouncer.Stop();
		defaultDeviceDebouncer.Start();
	}

	private void RefreshDefaultDevice()
	{
		defaultDeviceDebouncer?.Stop();
		defaultDeviceDebouncer = null;
		RefreshDefaultDeviceIcon();
	}

	private void ProfileChangedDebouncer(Guid audEndptUID, string propertyName)
	{
		if (profileChangedDebouncer == null)
		{
			profileChangedDebouncer = new Timer(500.0);
			profileChangedDebouncer.Elapsed += (object s, ElapsedEventArgs e) =>
			{
				CheckProfileProperty();
			};
			profileChangedDebouncer.AutoReset = false;
		}
		propertyChangedList.Add((audEndptUID, propertyName));
		profileChangedDebouncer.Stop();
		profileChangedDebouncer.Start();
	}

	private void CheckProfileProperty()
	{
		LogHelper.Trace("", "CheckProfileProperty", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
		profileChangedDebouncer?.Stop();
		profileChangedDebouncer = null;
		LogHelper.Debug($"propertyChangedList Count: {propertyChangedList.Count}", isPrintCaller: false, "CheckProfileProperty", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
		foreach (var property in propertyChangedList)
		{
			LogHelper.Debug("Check Property Name: " + property.Item2, isPrintCaller: false, "CheckProfileProperty", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
			if (DriverInstance.getInstance().CheckProfileChanged(property.Item1, property.Item2))
			{
				RefreshProfileButton();
				propertyChangedList.Clear();
				break;
			}
		}
		propertyChangedList.Clear();
	}

	private void HIDeventCallback(IntPtr sdkObj, IntPtr hidEndpointObj, HIDSDK.CmHIDEventID eventID, IntPtr data, int dataSize, IntPtr userData)
	{
		if (!(userData != IntPtr.Zero))
		{
			return;
		}
		byte[] managedArray = new byte[dataSize];
		Marshal.Copy(data, managedArray, 0, dataSize);
		StringBuilder log = new StringBuilder();
		log.Append($"[DeviceToHostCallback] sdkObj: {sdkObj}, hidEndpointObj: {hidEndpointObj}, data: ");
		for (int i = 0; i < managedArray.Length; i++)
		{
			log.Append(managedArray[i].ToString("X2"));
			log.Append(" ");
		}
		Console.WriteLine(log.ToString());
		if (managedArray[0] == 85)
		{
			if (managedArray[1] == 111)
			{
				Console.WriteLine($"Received HID response: 0x{managedArray[2]:X2}");
				DriverInstance.getInstance().UpdateDeviceStatus(managedArray[2]);
				if (managedArray[2] == 201 && managedArray[3] == byte.MaxValue && managedArray[4] == byte.MaxValue)
				{
					Console.WriteLine("Received Recenter button command");
					DriverInstance.getInstance().HeadTrackingReset();
				}
			}
			else if (managedArray[1] == 135 || managedArray[1] == 136)
			{
				DriverInstance.getInstance().lastHIDTime = Environment.TickCount;
			}
			else if (managedArray[1] == 101)
			{
				LogHelper.Debug($"hidObj {hidEndpointObj} Received Data: {managedArray[0]:X2} {managedArray[1]:X2} {managedArray[2]:X2} {managedArray[3]:X2}", isPrintCaller: false, "HIDeventCallback", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
				DriverInstance.getInstance().UpdateDeviceStatus();
				UpdateBatteryInfo(hidEndpointObj, managedArray[2], managedArray[3]);
			}
		}
		else if (managedArray[0] == 237)
		{
			DriverInstance.getInstance().OnHID_PropertyChanged(hidEndpointObj, managedArray[1]);
		}
		if (dataSize == 5)
		{
			byte stateByte = managedArray[2];
			ButtonState currentState = (((stateByte & 1) == 1) ? ButtonState.Pressed : (((stateByte & 1) == 0) ? ButtonState.Released : ButtonState.None));
			if ((_lastState == ButtonState.Released || _lastState == ButtonState.None) && currentState == ButtonState.Pressed)
			{
				LogHelper.Debug("Button pressed, Triggered 7.1 VSS", isPrintCaller: false, "HIDeventCallback", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.EventCallback.cs");
				DriverInstance.getInstance().OnHID_PropertyChanged(hidEndpointObj);
			}
			_lastState = currentState;
		}
	}

	public void RegisterEventRefresh(FuncPtrRefresh fnPtr)
	{
		FnPtrRefresh = fnPtr.Invoke;
	}

	public void UnregisterEventRefresh()
	{
		FnPtrRefresh = null;
	}

	public void RegisterEventRefreshEffect(FuncPtrRefreshEffect fnPtr)
	{
		FnPtrRefreshEffect = fnPtr.Invoke;
	}

	public void UnregisterEventRefreshEffect()
	{
		FnPtrRefreshEffect = null;
	}

	public void OpenSubWindow(UserControl userControl)
	{
		subWindow = new SubWindow(userControl);
		double left = (Application.Current.MainWindow.Width - subWindow.Width) / 2.0;
		double top = (Application.Current.MainWindow.Height - subWindow.Height) / 2.0;
		subWindow.Left = Application.Current.MainWindow.Left + left;
		subWindow.Top = Application.Current.MainWindow.Top + top;
		subWindow.Owner = this;
		subWindow.ShowDialog();
		subWindow = null;
		Activate();
		Focus();
	}

	private void RefreshSubWindow()
	{
		Dispatcher.InvokeAsync(() =>
		{
			FnPtrRefresh?.Invoke();
		});
	}

	private void RefreshSubWindow(string prop)
	{
		Dispatcher.InvokeAsync(() =>
		{
			FnPtrRefreshEffect?.Invoke(prop);
		});
	}

	private void CloseSubWindowOnDeviceChange()
	{
		Dispatcher.Invoke(() =>
		{
			if (subWindow != null && subWindow.IsNeedToClosed())
			{
				subWindow.Close();
			}
		});
	}

	public void RefreshProfileButton()
	{
		LogHelper.Trace("", "RefreshProfileButton", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.MainControl.cs");
		Button_Profile.Refresh();
	}

	public void RefreshLang()
	{
		Dispatcher.Invoke(() =>
		{
			foreach (EndpointConsole item in EndpointControlCanvas.Children.Cast<EndpointConsole>().ToList())
			{
				item.RefreshLang();
			}
			if (subWindow != null)
			{
				subWindow.RefreshLang();
			}
		});
	}

	private async void UpdateBatteryInfo(IntPtr hidObj, int percentage, int charging)
	{
		await Task.Run(() =>
		{
			Dispatcher.Invoke(() =>
			{
				foreach (EndpointConsole current in EndpointControlCanvas.Children.Cast<EndpointConsole>().ToList())
				{
					if (current.devInfo.HeadTrackingHIDObj == hidObj && current.devInfo.HeadTrackingHIDObj != IntPtr.Zero)
					{
						current.RefreshPanelBattery(percentage, charging);
					}
				}
			});
		});
	}

	private void RebuildEndpointConsoleList()
	{
		epConsoleList.Clear();
		lock (consoleListLock)
		{
			List<JackDeviceInfo> deviceList = DriverInstance.getInstance().GetDeviceList();
			List<string> deviceOrder = DriverInstance.getInstance().LoadAllOrder();
			if (deviceOrder.Count == 0)
			{
				Console.WriteLine("There have no order list. Build new one.");
				for (int index = 0; index < Enumerable.Count(deviceList); index++)
				{
					EndpointConsole epControl = new EndpointConsole(deviceList[index]);
					epControl.orderInCanvas = index;
					if (deviceList[index].licenseCategoryList.Count > 1)
					{
						epControl.isShow = true;
					}
					else
					{
						string usbInstance = deviceList[index].usbDeviceInstanceID;
						for (int j = 0; j < Enumerable.Count(deviceList); j++)
						{
							if (j != index && deviceList[j].usbDeviceInstanceID.Equals(usbInstance) && deviceList[j].licenseCategoryList.Count > 1)
							{
								epControl.isShow = true;
								break;
							}
						}
					}
					DriverInstance.getInstance().SaveEndpointShow(deviceList[index].instanceID, epControl.isShow);
					epConsoleList.Add(epControl);
					deviceOrder.Add(deviceList[index].instanceID);
				}
				DriverInstance.getInstance().SaveAllOrder(deviceOrder);
				return;
			}
			Console.WriteLine("Order list exist.");
			int i;
			for (i = 0; i < deviceList.Count; i++)
			{
				EndpointConsole epControl2 = new EndpointConsole(deviceList[i]);
				if (!deviceOrder.Contains(deviceList[i].instanceID))
				{
					epControl2.orderInCanvas = deviceOrder.Count;
					deviceOrder.Add(deviceList[i].instanceID);
				}
				else
				{
					epControl2.orderInCanvas = deviceOrder.FindIndex((string x) => x.Equals(deviceList[i].instanceID));
				}
				switch (DriverInstance.getInstance().LoadEndpointShow(deviceList[i].instanceID))
				{
				case -1:
					if (deviceList[i].licenseCategoryList.Count > 1)
					{
						epControl2.isShow = true;
					}
					else
					{
						epControl2.isShow = false;
					}
					DriverInstance.getInstance().SaveEndpointShow(epControl2.devInfo.instanceID, epControl2.isShow);
					break;
				case 0:
					epControl2.isShow = false;
					break;
				default:
					epControl2.isShow = true;
					break;
				}
				epConsoleList.Add(epControl2);
			}
			DriverInstance.getInstance().SaveAllOrder(deviceOrder);
			epConsoleList = epConsoleList.OrderBy((EndpointConsole child) => child.orderInCanvas).ToList();
		}
	}

	public void RefreshCanvas()
	{
		Console.WriteLine("RebuildCanvasFromConsoleList()");
		Dispatcher.Invoke(() =>
		{
			EndpointControlCanvas.Children.Clear();
			EndpointControlCanvas.Width = 0.0;
			lock (consoleListLock)
			{
				int num = 0;
				epConsoleList = epConsoleList.OrderBy((EndpointConsole child) => child.orderInCanvas).ToList();
				for (int num2 = 0; num2 < epConsoleList.Count; num2++)
				{
					if (epConsoleList[num2].isShow)
					{
						Canvas.SetLeft(epConsoleList[num2], num);
						Canvas.SetTop(epConsoleList[num2], 0.0);
						EndpointControlCanvas.Children.Add(epConsoleList[num2]);
						num = num + (int)epConsoleList[num2].Width + 10;
						EndpointControlCanvas.Width = num;
						Console.WriteLine($"EndpointControlCanvas.Width: {EndpointControlCanvas.Width}");
					}
				}
			}
			AutoSize();
		});
	}

	public void ShowDevice(bool isShow, Guid audEndptUID)
	{
		EndpointConsole epConsole = epConsoleList.Find((EndpointConsole x) => x.devInfo.audEndptUID == audEndptUID);
		epConsole.isShow = isShow;
		DriverInstance.getInstance().SaveEndpointShow(epConsole.devInfo.instanceID, epConsole.isShow);
		RefreshCanvas();
	}

	public void AlignConsole()
	{
		Console.WriteLine("AlignConsole()");
		List<EndpointConsole> CanvasList = EndpointControlCanvas.Children.Cast<EndpointConsole>().ToList();
		CanvasList = CanvasList.OrderBy((EndpointConsole child) => child.orderInCanvas).ToList();
		for (int iter = 0; iter < CanvasList.Count; iter++)
		{
			Canvas.SetLeft(CanvasList[iter], iter * ControlGap);
		}
	}

	public void OnEndpointConsoleMoved()
	{
		tempConsoleList.Clear();
		tempConsoleList = EndpointControlCanvas.Children.Cast<EndpointConsole>().ToList();
		tempConsoleList = tempConsoleList.OrderBy((EndpointConsole child) => Canvas.GetLeft(child)).ToList();
	}

	public void OnEndpointConsoleOrderdChanged(EndpointConsole epConsole, double movedPixel)
	{
		if (!tempConsoleList.Contains(epConsole))
		{
			return;
		}
		EndpointConsole movedEpConsole = tempConsoleList.Find((EndpointConsole x) => x == epConsole);
		int epConsoleIndex = tempConsoleList.FindIndex((EndpointConsole x) => x == epConsole);
		bool isChanged = false;
		if (epConsoleIndex == 0)
		{
			Console.WriteLine("The Leftest Console.");
			if (movedPixel > 0.0 && Canvas.GetLeft(movedEpConsole) > Canvas.GetLeft(tempConsoleList[epConsoleIndex + 1]))
			{
				int tempIndex = movedEpConsole.orderInCanvas;
				movedEpConsole.orderInCanvas = tempConsoleList[epConsoleIndex + 1].orderInCanvas;
				tempConsoleList[epConsoleIndex + 1].orderInCanvas = tempIndex;
				isChanged = true;
			}
		}
		else if (epConsoleIndex == tempConsoleList.Count - 1)
		{
			Console.WriteLine("The Rightest Console.");
			if (movedPixel < 0.0 && Canvas.GetLeft(tempConsoleList[epConsoleIndex - 1]) > Canvas.GetLeft(movedEpConsole))
			{
				int tempIndex2 = movedEpConsole.orderInCanvas;
				movedEpConsole.orderInCanvas = tempConsoleList[epConsoleIndex - 1].orderInCanvas;
				tempConsoleList[epConsoleIndex - 1].orderInCanvas = tempIndex2;
				isChanged = true;
			}
		}
		else
		{
			Console.WriteLine("The middle Console.");
			if (Canvas.GetLeft(movedEpConsole) > Canvas.GetLeft(tempConsoleList[epConsoleIndex + 1]))
			{
				int tempIndex3 = movedEpConsole.orderInCanvas;
				movedEpConsole.orderInCanvas = tempConsoleList[epConsoleIndex + 1].orderInCanvas;
				tempConsoleList[epConsoleIndex + 1].orderInCanvas = tempIndex3;
				isChanged = true;
			}
			else if (Canvas.GetLeft(tempConsoleList[epConsoleIndex - 1]) > Canvas.GetLeft(movedEpConsole))
			{
				int tempIndex4 = movedEpConsole.orderInCanvas;
				movedEpConsole.orderInCanvas = tempConsoleList[epConsoleIndex - 1].orderInCanvas;
				tempConsoleList[epConsoleIndex - 1].orderInCanvas = tempIndex4;
				isChanged = true;
			}
		}
		if (!isChanged)
		{
			return;
		}
		tempConsoleList = tempConsoleList.OrderBy((EndpointConsole child) => child.orderInCanvas).ToList();
		for (int iter = 0; iter < tempConsoleList.Count; iter++)
		{
			if (tempConsoleList[iter] != movedEpConsole)
			{
				Canvas.SetLeft(tempConsoleList[iter], iter * ControlGap);
			}
		}
		List<string> deviceOrder = DriverInstance.getInstance().LoadAllOrder();
		foreach (EndpointConsole console in epConsoleList)
		{
			if (console.orderInCanvas < deviceOrder.Count)
			{
				deviceOrder[console.orderInCanvas] = console.devInfo.instanceID;
			}
		}
		DriverInstance.getInstance().SaveAllOrder(deviceOrder);
	}

	public EndpointConsole GetEndpointConsole(Guid audEndptUID)
	{
		return epConsoleList.Find((EndpointConsole x) => x.devInfo.audEndptUID == audEndptUID);
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		Hide();
	}

	private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void Grid_Main_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	private void Btn_AddDevice_Click(object sender, RoutedEventArgs e)
	{
		EndpointList endpointList = new EndpointList();
		OpenSubWindow(endpointList);
	}

	private void Btn_Info_Click(object sender, RoutedEventArgs e)
	{
		Information information = new Information();
		OpenSubWindow(information);
	}

	public void Button_GlobalProfile_Click(object sender, RoutedEventArgs e)
	{
		ProfilePopup.IsOpen = true;
	}

	private void Button_ProfileAutoSwitch_Click(object sender, RoutedEventArgs e)
	{
		ProfileAutoSwitch profileAutoSwitchControl = new ProfileAutoSwitch();
		OpenSubWindow(profileAutoSwitchControl);
	}

	private void ProfilePopup_Opened(object sender, EventArgs e)
	{
		LogHelper.Trace("", "ProfilePopup_Opened", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.UIEvent.cs");
		if (ProfilePopup.Child is ProfilePopupControl control)
		{
			control.Refresh();
		}
	}

	private void ProfilePopup_Closed(object sender, EventArgs e)
	{
		LogHelper.Trace("", "ProfilePopup_Closed", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\MainWindow.UIEvent.cs");
	}

	private void Btn_Test_Click(object sender, RoutedEventArgs e)
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
