using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.Driver;

namespace XearONE.UI.UserControls;

public partial class EffectSwitchControl : UserControl, IComponentConnector
{
	private string propertyKeyName;

	private const string EffectPrefix = "Effect_ShortName_";

	public Guid AudioEndpointUID { get; set; }

	public XearSDK.CmLicenseCategory licenseCategoryObj { get; set; }

	public XearSDK.CmDataFlow dataFlow { get; set; }

	public string PropertyName()
	{
		return propertyKeyName;
	}

	public EffectSwitchControl()
	{
		InitializeComponent();
	}

	private void EffectSwitchControl_Loaded(object sender, RoutedEventArgs e)
	{
		DriverInstance.getInstance().GetPropertyKeyName(dataFlow, licenseCategoryObj, ref propertyKeyName);
		RefeshLang();
		RefreshEffect();
	}

	public void RefeshLang()
	{
		string key = "Effect_ShortName_" + licenseCategoryObj.ToString().Substring(4);
		string effectName = TryFindResource(key)?.ToString() ?? "DefaultEffect";
		string effectToolTip;
		if (licenseCategoryObj == XearSDK.CmLicenseCategory.CLC_Equalizer10Bands && dataFlow == XearSDK.CmDataFlow.CDF_Capture)
		{
			effectToolTip = TryFindResource("ToolTip_CLC_Equalizer10Bands_Mic")?.ToString() ?? "Mic_EQ";
		}
		else
		{
			string key2 = $"ToolTip_{licenseCategoryObj.ToString()}";
			effectToolTip = TryFindResource(key2)?.ToString() ?? "DefaultEffect";
		}
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Label_EffectName.Content = effectName;
			CheckBox_EffectSwitch.ToolTip = effectToolTip;
		}));
	}

	public void RefreshEffect()
	{
		if (XearSDK.GetProperty(AudioEndpointUID, propertyKeyName, out int outDat, out XearSDK.CmErr _))
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				CheckBox_EffectSwitch.IsChecked = outDat == 1;
			}));
		}
	}

	private void EffectSwitch_Click(object sender, RoutedEventArgs e)
	{
		int inDat = (CheckBox_EffectSwitch.IsChecked.Value ? 1 : 0);
		if (XearSDK.SetProperty(AudioEndpointUID, propertyKeyName, inDat, out var _))
		{
			DriverInstance.getInstance().SetHIDStatus(AudioEndpointUID, propertyKeyName, (byte)inDat);
		}
	}
}
