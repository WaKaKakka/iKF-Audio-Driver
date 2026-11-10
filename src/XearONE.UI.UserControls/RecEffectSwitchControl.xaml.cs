using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Driver;
using XearONE.UI.EffectControls.RecordingEffect;
using XearONE.UI.Helpers;

namespace XearONE.UI.UserControls;

public partial class RecEffectSwitchControl : UserControl, IComponentConnector
{
	private RecordingEffectHandler recordingEffectHandler;

	private IniFileHelper iniRecordingEffect;

	public Guid AudioEndpointUID { get; set; }

	public RecEffectSwitchControl()
	{
		InitializeComponent();
		iniRecordingEffect = DriverInstance.getInstance().iniRecordingEffect;
	}

	private void RecEffectSwitchControl_Loaded(object sender, RoutedEventArgs e)
	{
		recordingEffectHandler = new RecordingEffectHandler(AudioEndpointUID);
		RefreshEffect();
	}

	public void RefeshLang()
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Label_EffectName.Content = FindResource("Effect_ShortName_REC_Effect").ToString();
			CheckBox_EffectSwitch.ToolTip = FindResource("ToolTip_CheckBox_REC_Switch").ToString();
		}));
	}

	public void RefreshEffect()
	{
		if (recordingEffectHandler.IsEnabled())
		{
			CheckBox_EffectSwitch.IsChecked = true;
		}
		else
		{
			CheckBox_EffectSwitch.IsChecked = false;
		}
	}

	private void EffectSwitch_Click(object sender, RoutedEventArgs e)
	{
		if (CheckBox_EffectSwitch.IsChecked == true)
		{
			switch (iniRecordingEffect.ReadString("LastMode", "", "Settings"))
			{
			case "Compressor":
				recordingEffectHandler.SetMode(RecordingEffectMode.Compressor);
				break;
			case "LDR":
				recordingEffectHandler.SetMode(RecordingEffectMode.LDR);
				break;
			case "AINR":
				recordingEffectHandler.SetMode(RecordingEffectMode.AINR);
				break;
			default:
				recordingEffectHandler.SetMode(RecordingEffectMode.Compressor);
				break;
			}
		}
		else
		{
			recordingEffectHandler.SetMode(RecordingEffectMode.None);
		}
	}
}
