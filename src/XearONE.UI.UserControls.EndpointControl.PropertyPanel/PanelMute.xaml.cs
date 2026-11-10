using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelMute : UserControl, IComponentConnector
{
	private Guid _endpointUID;

	public PanelMute(XearSDK.CmDataFlow dataFlow, Guid endpointUID)
	{
		InitializeComponent();
		_endpointUID = endpointUID;
		InitMuteButtonIcon(dataFlow);
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	private void InitMuteButtonIcon(XearSDK.CmDataFlow dataFlow)
	{
		if (dataFlow == XearSDK.CmDataFlow.CDF_Capture)
		{
			Button_Mute.Style = (Style)FindResource("CheckBox_MuteCapture_Style");
		}
		else
		{
			Button_Mute.Style = (Style)FindResource("CheckBox_MutePlayback_Style");
		}
	}

	public void UpdateUI()
	{
		if (XearSDK.GetProperty(_endpointUID, "MutingState", out int outDat, out XearSDK.CmErr _))
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				Button_Mute.IsChecked = outDat == 1;
			}));
		}
		else
		{
			Console.WriteLine("Get MutingState Failed.");
		}
	}

	private void Button_Mute_Click(object sender, RoutedEventArgs e)
	{
		XearSDK.SetProperty(_endpointUID, "MutingState", (Button_Mute.IsChecked == true) ? 1 : 0, out var _);
	}
}
