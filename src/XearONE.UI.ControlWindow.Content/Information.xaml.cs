using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Cmedia;
using XearONE.UI.Driver;
using XearONE.UI.Driver.DeviceInfo;

namespace XearONE.UI.ControlWindow.Content;

public partial class Information : UserControl, IComponentConnector
{
	private FirmwareUpdateSDK.CmFirmwareIC cmFirmwareIC;

	private MainWindow MW
	{
		[DebuggerStepThrough]
		get
		{
			return (MainWindow)Application.Current.MainWindow;
		}
	}

	public Information()
	{
		InitializeComponent();
		Name = "Information";
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
		Version XearSDKVersion = new Version(FileVersionInfo.GetVersionInfo(Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath.ToString())), "CmXearSDK.dll")).FileVersion);
		Label_APPVersionNumber.Content = appVersion.ToString();
		Label_SDKVersionNumber.Content = XearSDKVersion.ToString();
		ComboBox_Lang.ItemsSource = DriverInstance.getInstance().existLanguages;
		ComboBox_Lang.DisplayMemberPath = "Value";
		ComboBox_Lang.SelectedValuePath = "Key";
		ComboBox_Lang.SelectedValue = DriverInstance.getInstance().GetCurrentLang();
		bool isShowFWUpdateButton = false;
		List<JackDeviceInfo> deviceList = DriverInstance.getInstance().GetDeviceList();
		int deviceVid = DriverInstance.getInstance().LoadFWUpdateVid();
		int devicePid = DriverInstance.getInstance().LoadFWUpdatePid();
		foreach (JackDeviceInfo device in deviceList)
		{
			if (device.usbVendorID == deviceVid && device.usbProductID == devicePid)
			{
				cmFirmwareIC = FirmwareUpdateSDK.CmFirmwareIC.CFI_CM2025;
				isShowFWUpdateButton = true;
				break;
			}
			if (device.usbVendorID == 14391 && device.usbProductID == 24581)
			{
				cmFirmwareIC = FirmwareUpdateSDK.CmFirmwareIC.CFI_CM2206;
				isShowFWUpdateButton = true;
				break;
			}
		}
		Button_FWUpdate.Visibility = ((!isShowFWUpdateButton) ? Visibility.Hidden : Visibility.Visible);
	}

	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
		e.Handled = true;
	}

	private void ComboBox_Lang_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (ComboBox_Lang.SelectedValue.Equals(DriverInstance.getInstance().GetCurrentLang()))
		{
			Button_LangApply.IsEnabled = false;
		}
		else
		{
			Button_LangApply.IsEnabled = true;
		}
	}

	private void Button_LangApply_Click(object sender, RoutedEventArgs e)
	{
		DriverInstance.getInstance().ChangeLanguage(ComboBox_Lang.SelectedValue.ToString());
		Button_LangApply.IsEnabled = false;
		MW.RefreshLang();
	}

	private void Button_Website_Click(object sender, RoutedEventArgs e)
	{
		Process.Start($"http://{Button_Website.Content}");
	}

	private void Button_FWUpdate_Click(object sender, RoutedEventArgs e)
	{
		Window parentWindow = Window.GetWindow(this);
		Window firmwareUpdateWindow = null;
		if (cmFirmwareIC == FirmwareUpdateSDK.CmFirmwareIC.CFI_CM2025)
		{
			firmwareUpdateWindow = new FirmwareUpdateWindow(parentWindow);
		}
		else
		{
			if (cmFirmwareIC != FirmwareUpdateSDK.CmFirmwareIC.CFI_CM2206)
			{
				return;
			}
			firmwareUpdateWindow = new FirmwareUpdateWindowCM2026(parentWindow);
		}
		double left = (Application.Current.MainWindow.Width - firmwareUpdateWindow.Width) / 2.0;
		double top = (Application.Current.MainWindow.Height - firmwareUpdateWindow.Height) / 2.0;
		firmwareUpdateWindow.Left = Application.Current.MainWindow.Left + left;
		firmwareUpdateWindow.Top = Application.Current.MainWindow.Top + top;
		firmwareUpdateWindow.ShowDialog();
		parentWindow.Activate();
		parentWindow.Focus();
	}
}
