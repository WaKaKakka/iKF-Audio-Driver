using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.Driver;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.UserControls;

namespace XearONE.UI.ControlWindow.Content;

public partial class EndpointList : UserControl, WindowConstantDefinition.IUsrCtrlProperty, IComponentConnector
{
	public EndpointList()
	{
		InitializeComponent();
		Name = "EndpointList";
	}

	void WindowConstantDefinition.IUsrCtrlProperty.UpdateUI()
	{
		Console.WriteLine("{0}: UpdateUI()", GetType().ToString());
		Dispatcher.Invoke(() =>
		{
			UpdateUI();
		});
	}

	private void UpdateUI()
	{
		RefreshAllControls();
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		Dispatcher.Invoke(() =>
		{
			StackPanel_PlaybackDevice.Children.Clear();
			StackPanel_RecordingDevice.Children.Clear();
		});
		List<JackDeviceInfo> _JackDeviceList = DriverInstance.getInstance().GetDeviceList();
		for (int index = 0; index < _JackDeviceList.Count; index++)
		{
			EndpointListButtonControl endpointListButtonControl = new EndpointListButtonControl(_JackDeviceList[index]);
			Dispatcher.Invoke(() =>
			{
				if (_JackDeviceList[index].dataFlow == XearSDK.CmDataFlow.CDF_Render)
				{
					StackPanel_PlaybackDevice.Children.Add(endpointListButtonControl);
				}
				else if (_JackDeviceList[index].dataFlow == XearSDK.CmDataFlow.CDF_Capture)
				{
					StackPanel_RecordingDevice.Children.Add(endpointListButtonControl);
				}
			});
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
