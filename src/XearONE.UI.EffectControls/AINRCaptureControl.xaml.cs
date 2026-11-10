using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.Driver;

namespace XearONE.UI.EffectControls;

public partial class AINRCaptureControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_AINRCapture_Enable = "AI_NR_Enable";

	private const string propertyName_AINRCapture_Level = "AI_NR_Level";

	public List<string> propertyNames { get; set; } = new List<string>();

	public AINRCaptureControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		_audEndptUID = audioEndpointUID;
		InitPropertyNames();
	}

	private void AINRCaptureControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "AI_NR_Enable", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "AI_NR_Level", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("AI_NR_Enable");
		propertyNames.Add("AI_NR_Level");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "AI_NR_Enable", enable, out var _);
		DriverInstance.getInstance().SetHIDStatus(_audEndptUID, "AI_NR_Enable", (byte)enable);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "AI_NR_Level", level, out var _);
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 2.0;
		}));
	}
}
