using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class VigilanceModeControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_VigilanceMode = "VigilanceMode";

	private XearSDK.VigilanceMode VM;

	public List<string> propertyNames { get; set; } = new List<string>();

	public VigilanceModeControl(Guid audEndptUID)
	{
		InitializeComponent();
		_audEndptUID = audEndptUID;
		InitPropertyNames();
	}

	private void VigilanceModeControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		if (XearSDK.GetProperty(_audEndptUID, "VigilanceMode", out XearSDK.VigilanceMode out_VM, out XearSDK.CmErr _))
		{
			VM = out_VM;
		}
		else
		{
			VM.Enable = false;
			VM.Mode = XearSDK.CmVigilanceMode.CVT_Unknown;
		}
		Btn_Enable.IsChecked = VM.Enable;
		switch (VM.Mode)
		{
		case XearSDK.CmVigilanceMode.CVT_Unknown:
			RadioButton_VigilanceMode_0.IsChecked = true;
			break;
		case XearSDK.CmVigilanceMode.CVT_Omni:
			RadioButton_VigilanceMode_1.IsChecked = true;
			break;
		case XearSDK.CmVigilanceMode.CVT_Rear:
			RadioButton_VigilanceMode_2.IsChecked = true;
			break;
		default:
			RadioButton_VigilanceMode_0.IsChecked = true;
			break;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("VigilanceMode");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		VM.Enable = Btn_Enable.IsChecked.Value;
		XearSDK.SetProperty(_audEndptUID, "VigilanceMode", VM, out var _);
	}

	private void RadioButton_VigilanceMode_Checked(object sender, RoutedEventArgs e)
	{
		XearSDK.CmVigilanceMode mode = (XearSDK.CmVigilanceMode)Convert.ToInt32((sender as RadioButton).Tag);
		VM.Mode = mode;
		XearSDK.SetProperty(_audEndptUID, "VigilanceMode", VM, out var _);
	}
}
