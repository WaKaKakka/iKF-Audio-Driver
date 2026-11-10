using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class MicEQControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_EQ_Enable = "MicEQ_Enable";

	private readonly string[] MicEQ_Slider = new string[10] { "MicEQ_Slider1", "MicEQ_Slider2", "MicEQ_Slider3", "MicEQ_Slider4", "MicEQ_Slider5", "MicEQ_Slider6", "MicEQ_Slider7", "MicEQ_Slider8", "MicEQ_Slider9", "MicEQ_Slider10" };

	public List<string> propertyNames { get; set; } = new List<string>();

	public MicEQControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		_audEndptUID = audioEndpointUID;
		InitPropertyNames();
	}

	private void MicEQControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "MicEQ_Enable", out enable, out XearSDK.CmErr _))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		string[] micEQ_Slider = MicEQ_Slider;
		foreach (string SliderControl in micEQ_Slider)
		{
			GetEQValue(SliderControl);
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("MicEQ_Enable");
		string[] micEQ_Slider = MicEQ_Slider;
		foreach (string name in micEQ_Slider)
		{
			propertyNames.Add(name);
		}
	}

	private void GetEQValue(string EQBand)
	{
		if (XearSDK.GetProperty(_audEndptUID, EQBand, out int value, out XearSDK.CmErr _) && FindName(EQBand) is Slider foundSlider)
		{
			foundSlider.Value = value;
		}
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "MicEQ_Enable", enable, out var _);
	}

	private void Slider_EQBand_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (sender is FrameworkElement { Name: var SliderName })
		{
			SetEQValue(SliderName, (int)e.NewValue);
		}
	}

	private void SetEQValue(string EQBand, int EQvalue)
	{
		XearSDK.SetProperty(_audEndptUID, EQBand, EQvalue, out var _);
	}

	private void Button_Reset_Click(object sender, RoutedEventArgs e)
	{
		string[] micEQ_Slider = MicEQ_Slider;
		foreach (string slider in micEQ_Slider)
		{
			SetEQValue(slider, 0);
			if (FindName(slider) is Slider foundSlider)
			{
				foundSlider.Value = 0.0;
			}
		}
	}
}
