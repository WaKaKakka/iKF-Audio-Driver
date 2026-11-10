using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Driver;
using XearONE.UI.EffectControls.MagicVoice2;
using XearONE.UI.Helpers;

namespace XearONE.UI.UserControls;

public partial class MV2SwitchControl : UserControl, IComponentConnector
{
	private MagicVoice2Handler magicVoice2Handler;

	private IniFileHelper iniMagicVoice2;

	public Guid AudioEndpointUID { get; set; }

	public MV2SwitchControl()
	{
		InitializeComponent();
		iniMagicVoice2 = DriverInstance.getInstance().iniMagicVoice2;
	}

	private void MV2SwitchControl_Loaded(object sender, RoutedEventArgs e)
	{
		magicVoice2Handler = new MagicVoice2Handler(AudioEndpointUID);
		RefeshLang();
		RefreshEffect();
	}

	public void RefeshLang()
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Label_EffectName.Content = FindResource("Effect_ShortName_MagicVoiceII").ToString();
			CheckBox_EffectSwitch.ToolTip = FindResource("ToolTip_CheckBox_MV2_Switch").ToString();
		}));
	}

	public void RefreshEffect()
	{
		CheckBox_EffectSwitch.IsChecked = magicVoice2Handler.IsEnabled();
	}

	private void EffectSwitch_Click(object sender, RoutedEventArgs e)
	{
		if (CheckBox_EffectSwitch.IsChecked == true)
		{
			switch (iniMagicVoice2.ReadString("LastTarget", "", iniMagicVoice2.ReadString("ProfileName", "Settings")))
			{
			case "ManAi":
				magicVoice2Handler.SetTarget(MagicVoice2Target.ManAi);
				magicVoice2Handler.SetEnable(enable: true);
				break;
			case "WomanAi":
				magicVoice2Handler.SetTarget(MagicVoice2Target.WomanAi);
				magicVoice2Handler.SetEnable(enable: true);
				break;
			case "Cartoon":
				magicVoice2Handler.SetTarget(MagicVoice2Target.Cartoon);
				magicVoice2Handler.SetEnable(enable: true);
				break;
			case "Monster":
				magicVoice2Handler.SetTarget(MagicVoice2Target.Monster);
				magicVoice2Handler.SetEnable(enable: true);
				break;
			case "Man":
				magicVoice2Handler.SetTarget(MagicVoice2Target.Man);
				magicVoice2Handler.SetEnable(enable: true);
				break;
			case "Woman":
				magicVoice2Handler.SetTarget(MagicVoice2Target.Woman);
				magicVoice2Handler.SetEnable(enable: true);
				break;
			default:
				magicVoice2Handler.SetEnable(enable: true);
				break;
			}
		}
		else
		{
			magicVoice2Handler.SetEnable(enable: false);
		}
	}
}
