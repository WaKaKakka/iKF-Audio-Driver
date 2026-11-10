using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.UserControls.EndpointControl;

namespace XearONE.UI.UserControls;

public partial class EndpointListButtonControl : UserControl, IComponentConnector
{
	private JackDeviceInfo _devInfo;

	public string displayName { get; set; }

	public string controllerDisplayName { get; set; }

	public bool IsAdded { get; set; }

	private MainWindow MW
	{
		[DebuggerStepThrough]
		get
		{
			return (MainWindow)Application.Current.MainWindow;
		}
	}

	public EndpointListButtonControl(JackDeviceInfo devInfo)
	{
		InitializeComponent();
		DataContext = this;
		Margin = new Thickness(5.0, 5.0, 5.0, 5.0);
		_devInfo = devInfo;
		displayName = devInfo.displayName;
		controllerDisplayName = devInfo.controllerDisplayName;
		EndpointConsole epConsole = MW.GetEndpointConsole(_devInfo.audEndptUID);
		if (epConsole != null)
		{
			IsAdded = epConsole.isShow;
		}
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void CheckBox_IsAdded_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine($"EndpointListButtonControl: {displayName} Clicked, {CheckBox_IsAdded.IsChecked.ToString()}");
		if (CheckBox_IsAdded.IsChecked.Value)
		{
			MW.ShowDevice(isShow: true, _devInfo.audEndptUID);
		}
		else
		{
			MW.ShowDevice(isShow: false, _devInfo.audEndptUID);
		}
	}
}
