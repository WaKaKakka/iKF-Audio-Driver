using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearMagicVoiceControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_XearMagicVoice_Enable = "Enable_MAGICVOICE";

	private const string propertyName_XearMagicVoice_Mode = "MagicVoice_Selection";

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearMagicVoiceControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		_audEndptUID = audioEndpointUID;
		InitPropertyNames();
	}

	private void XearMagicVoiceControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_MAGICVOICE", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int mode = 0;
		if (XearSDK.GetProperty(_audEndptUID, "MagicVoice_Selection", out mode, out cmErr))
		{
			switch (mode)
			{
			case 0:
				RadioButton_ModeMonster.IsChecked = true;
				break;
			case 1:
				RadioButton_ModeCartoon.IsChecked = true;
				break;
			case 2:
				RadioButton_ModeMan.IsChecked = true;
				break;
			case 3:
				RadioButton_ModeWoman.IsChecked = true;
				break;
			}
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_MAGICVOICE");
		propertyNames.Add("MagicVoice_Selection");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_MAGICVOICE", enable, out var _);
	}

	private void Btn_Mode_Click(object sender, RoutedEventArgs e)
	{
		int mode = Convert.ToInt32((sender as RadioButton).Tag.ToString());
		XearSDK.SetProperty(_audEndptUID, "MagicVoice_Selection", mode, out var _);
	}
}
