using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearSmartVolumeControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_SmartVolume_Enable = "Enable_ADAPTIVEVOLUME_GFX";

	private const string propertyName_SmartVolume_Level = "ADAPTIVEVOLUME_LEVEL";

	private const string propertyName_SmartVolume_Mode = "ADAPTIVEVOLUME_MODE";

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearSmartVolumeControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		_audEndptUID = audioEndpointUID;
		InitPropertyNames();
	}

	private void XearSmartVolumeControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_ADAPTIVEVOLUME_GFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "ADAPTIVEVOLUME_LEVEL", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
		int mode = 0;
		if (XearSDK.GetProperty(_audEndptUID, "ADAPTIVEVOLUME_MODE", out mode, out cmErr))
		{
			switch (mode)
			{
			case 0:
				RadioButton_Music_Mode.IsChecked = true;
				break;
			case 1:
				RadioButton_Speech_Mode.IsChecked = true;
				break;
			}
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_ADAPTIVEVOLUME_GFX");
		propertyNames.Add("ADAPTIVEVOLUME_LEVEL");
		propertyNames.Add("ADAPTIVEVOLUME_MODE");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_ADAPTIVEVOLUME_GFX", enable, out var _);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "ADAPTIVEVOLUME_LEVEL", level, out var _);
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 70.0;
		}));
	}

	private void RadioButton_Mode_Checked(object sender, RoutedEventArgs e)
	{
		int mode = Convert.ToInt32((sender as RadioButton).Tag);
		XearSDK.SetProperty(_audEndptUID, "ADAPTIVEVOLUME_MODE", mode, out var _);
	}
}
