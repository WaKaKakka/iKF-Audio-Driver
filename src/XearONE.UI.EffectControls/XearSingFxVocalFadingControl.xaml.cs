using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearSingFxVocalFadingControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_VF_Enable = "Enable_VOCALCANCEL_GFX";

	private const string propertyName_VF_Level = "VOCALCANCEL_LEVEL";

	private const int defaultVFLevel = 50;

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearSingFxVocalFadingControl(Guid audEndptUID)
	{
		InitializeComponent();
		InitPropertyNames();
		_audEndptUID = audEndptUID;
	}

	private void XearSingFxVocalFadingControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_VOCALCANCEL_GFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "VOCALCANCEL_LEVEL", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_VOCALCANCEL_GFX");
		propertyNames.Add("VOCALCANCEL_LEVEL");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_VOCALCANCEL_GFX", enable, out var _);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "VOCALCANCEL_LEVEL", level, out var _);
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 50.0;
		}));
	}
}
