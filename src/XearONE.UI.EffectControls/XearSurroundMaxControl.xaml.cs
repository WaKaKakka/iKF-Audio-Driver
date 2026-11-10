using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class XearSurroundMaxControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_XSM_Enable = "Enable_Channel_COPY_LFX";

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearSurroundMaxControl(Guid audEndptUID)
	{
		InitializeComponent();
		InitPropertyNames();
		_audEndptUID = audEndptUID;
	}

	private void XearSurroundMaxControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_Channel_COPY_LFX", out enable, out XearSDK.CmErr _))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_Channel_COPY_LFX");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_Channel_COPY_LFX", enable, out var _);
	}
}
