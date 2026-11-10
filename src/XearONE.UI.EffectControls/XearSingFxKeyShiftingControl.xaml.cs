using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearSingFxKeyShiftingControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_KS_Enable = "Enable_KEYSHIFT_GFX";

	private const string propertyName_KS_Level = "KEYSHIFT_LEVEL";

	private const int defaultKSLevel = 0;

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearSingFxKeyShiftingControl(Guid audEndptUID)
	{
		InitializeComponent();
		InitPropertyNames();
		_audEndptUID = audEndptUID;
	}

	private void XearSingFxKeyShiftingControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_KEYSHIFT_GFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "KEYSHIFT_LEVEL", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_KEYSHIFT_GFX");
		propertyNames.Add("KEYSHIFT_LEVEL");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_KEYSHIFT_GFX", enable, out var _);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "KEYSHIFT_LEVEL", level, out var _);
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 0.0;
		}));
	}
}
