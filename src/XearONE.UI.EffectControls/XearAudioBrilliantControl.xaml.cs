using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearAudioBrilliantControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_AudioBrilliant_Enable = "Enable_AUDIOBRILLIANT_LFX";

	private const string propertyName_AudioBrilliant_Level = "AUDIOBRILLIANT_LEVEL";

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearAudioBrilliantControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		_audEndptUID = audioEndpointUID;
		InitPropertyNames();
	}

	private void XearAudioBrilliantControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_AUDIOBRILLIANT_LFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "AUDIOBRILLIANT_LEVEL", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_AUDIOBRILLIANT_LFX");
		propertyNames.Add("AUDIOBRILLIANT_LEVEL");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_AUDIOBRILLIANT_LFX", enable, out var _);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "AUDIOBRILLIANT_LEVEL", level, out var _);
	}

	private void Btn_Reset_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 3.0;
		}));
	}
}
