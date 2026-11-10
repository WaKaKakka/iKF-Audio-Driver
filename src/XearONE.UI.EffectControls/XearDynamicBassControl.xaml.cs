using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearDynamicBassControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_DynamicBass_Enable = "Enable_VIRTUALBASS_LFX";

	private const string propertyName_DynamicBass_Level = "VIRTUALBASS_Level";

	private const string propertyName_DynamicBass_Frequency = "VIRTUALBASS_CutOffFrequency";

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearDynamicBassControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		_audEndptUID = audioEndpointUID;
		InitPropertyNames();
	}

	private void XearDynamicBassControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_VIRTUALBASS_LFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "VIRTUALBASS_Level", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
		int freq = 0;
		if (XearSDK.GetProperty(_audEndptUID, "VIRTUALBASS_CutOffFrequency", out freq, out cmErr))
		{
			Slider_Frequency.Value = freq;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_VIRTUALBASS_LFX");
		propertyNames.Add("VIRTUALBASS_Level");
		propertyNames.Add("VIRTUALBASS_CutOffFrequency");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_VIRTUALBASS_LFX", enable, out var _);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "VIRTUALBASS_Level", level, out var _);
	}

	private void Btn_Reset_Level_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 5.0;
		}));
	}

	private void Slider_Frequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int freq = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "VIRTUALBASS_CutOffFrequency", freq, out var _);
	}

	private void Btn_Reset_Freq_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Frequency.Value = 200.0;
		}));
	}
}
