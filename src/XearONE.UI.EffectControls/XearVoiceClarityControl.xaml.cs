using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearVoiceClarityControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_VoiceClarity_Enable = "Enable_VOICECLARITY_LFX";

	private const string propertyName_VoiceClarity_Level = "VOICECLARITY_LEVEL";

	private const string propertyName_VoiceClarity_NoisesuppLevel = "VOICECLARITY_NOISESUPP_LEVEL";

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearVoiceClarityControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		_audEndptUID = audioEndpointUID;
		InitPropertyNames();
	}

	private void XearVoiceClarityControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_VOICECLARITY_LFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int level = 0;
		if (XearSDK.GetProperty(_audEndptUID, "VOICECLARITY_LEVEL", out level, out cmErr))
		{
			Slider_Level.Value = level;
		}
		int ns = 0;
		if (XearSDK.GetProperty(_audEndptUID, "VOICECLARITY_NOISESUPP_LEVEL", out ns, out cmErr))
		{
			Slider_NS.Value = ns;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_VOICECLARITY_LFX");
		propertyNames.Add("VOICECLARITY_LEVEL");
		propertyNames.Add("VOICECLARITY_NOISESUPP_LEVEL");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_VOICECLARITY_LFX", enable, out var _);
	}

	private void Slider_Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "VOICECLARITY_LEVEL", level, out var _);
	}

	private void Btn_Reset_Level_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_Level.Value = 3.0;
		}));
	}

	private void Slider_NS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		int level = (int)e.NewValue;
		XearSDK.SetProperty(_audEndptUID, "VOICECLARITY_NOISESUPP_LEVEL", level, out var _);
	}

	private void Btn_Reset_NS_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_NS.Value = 0.0;
		}));
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
