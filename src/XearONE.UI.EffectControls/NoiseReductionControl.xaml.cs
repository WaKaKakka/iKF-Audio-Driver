using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class NoiseReductionControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_NR_Enable = "NR_Enable";

	private const string propertyName_NR_Level = "NR_Level";

	private const int defaultNRLevel = 15;

	public List<string> propertyNames { get; set; } = new List<string>();

	public NoiseReductionControl(Guid audEndptUID)
	{
		InitializeComponent();
		InitPropertyNames();
		_audEndptUID = audEndptUID;
	}

	private void NoiseReductionControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "NR_Enable", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "NR_Level", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("NR_Enable");
		propertyNames.Add("NR_Level");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "NR_Enable", enable, out var _);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "NR_Level", level, out var _);
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 15.0;
		}));
	}
}
