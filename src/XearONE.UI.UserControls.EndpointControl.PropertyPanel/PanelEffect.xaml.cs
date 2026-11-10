using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using Newtonsoft.Json.Linq;
using XearONE.UI.ControlWindow.Content;
using XearONE.UI.Driver;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.Driver.Settings;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelEffect : UserControl, IComponentConnector
{
	private JackDeviceInfo _devInfo;

	private bool _isEffectPanelEnabled;

	private JsonEffectStatus effectStatus;

	private MainWindow MW
	{
		[DebuggerStepThrough]
		get
		{
			return (MainWindow)Application.Current.MainWindow;
		}
	}

	public void ShowGamingBorder(bool show)
	{
		Border_IsCMDevice.Visibility = ((!show) ? Visibility.Hidden : Visibility.Visible);
	}

	public PanelEffect(JackDeviceInfo devInfo)
	{
		InitializeComponent();
		_devInfo = devInfo;
		effectStatus = DriverInstance.getInstance().configManager.LoadEffectStatus(_devInfo);
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		ArrangeEffectButton();
		RefreshSystemEffectsControl();
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	public void UpdateUI(string propName)
	{
		if (StackPanel_EffectButtonList.Children.Count <= 0)
		{
			return;
		}
		foreach (object i in StackPanel_EffectButtonList.Children)
		{
			if (i is EffectSwitchControl effectSwitchControl)
			{
				if ((i as EffectSwitchControl).PropertyName().CompareTo(propName) == 0)
				{
					effectSwitchControl.RefreshEffect();
				}
			}
			else if (i is RecEffectSwitchControl recEffectSwitchControl)
			{
				recEffectSwitchControl.RefreshEffect();
			}
			else if (i is MV2SwitchControl mv2Control)
			{
				mv2Control.RefreshEffect();
			}
		}
	}

	public void RefreshLang()
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			if (StackPanel_EffectButtonList.Children.Count > 0)
			{
				foreach (object current in StackPanel_EffectButtonList.Children)
				{
					if (current is EffectSwitchControl effectSwitchControl)
					{
						effectSwitchControl.RefeshLang();
					}
					else if (current is RecEffectSwitchControl recEffectSwitchControl)
					{
						recEffectSwitchControl.RefeshLang();
					}
					else if (current is MV2SwitchControl mV2SwitchControl)
					{
						mV2SwitchControl.RefeshLang();
					}
				}
			}
		}));
	}

	private void CheckBox_EffectSwitch_Click(object sender, RoutedEventArgs e)
	{
		effectStatus.enable = CheckBox_EffectSwitch.IsChecked.Value;
		XearSDK.CmErr cmErr;
		if (CheckBox_EffectSwitch.IsChecked.Value)
		{
			foreach (KeyValuePair<string, JToken> effect in effectStatus.properties)
			{
				string propertyName = "";
				if (Enum.TryParse<XearSDK.CmLicenseCategory>(effect.Key, out var result))
				{
					DriverInstance.getInstance().GetPropertyKeyName(_devInfo.dataFlow, result, ref propertyName);
					XearSDK.SetProperty(_devInfo.audEndptUID, propertyName, Convert.ToInt16(effect.Value), out cmErr);
				}
			}
		}
		else
		{
			foreach (XearSDK.CmLicenseCategory effect2 in _devInfo.licenseCategoryList)
			{
				if (effect2 != XearSDK.CmLicenseCategory.CLC_Public)
				{
					string propertyName2 = "";
					DriverInstance.getInstance().GetPropertyKeyName(_devInfo.dataFlow, effect2, ref propertyName2);
					XearSDK.GetProperty(_devInfo.audEndptUID, propertyName2, out int outDat, out cmErr);
					effectStatus.properties[effect2.ToString()] = outDat;
					XearSDK.SetProperty(_devInfo.audEndptUID, propertyName2, 0, out cmErr);
				}
			}
		}
		DriverInstance.getInstance().configManager.SaveEffectStatus(_devInfo, effectStatus);
	}

	private void Button_EffectSetting_Click(object sender, RoutedEventArgs e)
	{
		EffectControl effectControl = new EffectControl(_devInfo);
		MW.OpenSubWindow(effectControl);
	}

	private void ArrangeEffectButton()
	{
		StackPanel_EffectButtonList.Children.Clear();
		if (_devInfo.licenseCategoryList.Count > 1)
		{
			Rect_EffectPanel_Disbale.Visibility = Visibility.Hidden;
			_isEffectPanelEnabled = true;
			List<XearSDK.CmLicenseCategory> orderedSupportedEffectList = new List<XearSDK.CmLicenseCategory>();
			DriverInstance.getInstance().GetOrderedSupportedEffectList(_devInfo, ref orderedSupportedEffectList);
			if (_devInfo.dataFlow == XearSDK.CmDataFlow.CDF_Capture)
			{
				if (_devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_NoiseGate) && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_Compressor) && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_LDR) && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_AINoiseReduction))
				{
					RecEffectSwitchControl recEffectSwitchControl = new RecEffectSwitchControl();
					recEffectSwitchControl.AudioEndpointUID = _devInfo.audEndptUID;
					recEffectSwitchControl.Margin = new Thickness(0.0, 0.0, 0.0, 5.0);
					StackPanel_EffectButtonList.Children.Add(recEffectSwitchControl);
				}
				if (orderedSupportedEffectList.Contains(XearSDK.CmLicenseCategory.CLC_AIMagicVoice))
				{
					MV2SwitchControl mv2SwitchControl = new MV2SwitchControl();
					mv2SwitchControl.AudioEndpointUID = _devInfo.audEndptUID;
					mv2SwitchControl.Margin = new Thickness(0.0, 0.0, 0.0, 5.0);
					StackPanel_EffectButtonList.Children.Add(mv2SwitchControl);
				}
			}
			{
				foreach (XearSDK.CmLicenseCategory license in orderedSupportedEffectList)
				{
					if (license != XearSDK.CmLicenseCategory.CLC_AIMagicVoice)
					{
						EffectSwitchControl effectSwitch = new EffectSwitchControl();
						effectSwitch.AudioEndpointUID = _devInfo.audEndptUID;
						effectSwitch.licenseCategoryObj = license;
						effectSwitch.dataFlow = _devInfo.dataFlow;
						effectSwitch.Margin = new Thickness(0.0, 0.0, 0.0, 5.0);
						StackPanel_EffectButtonList.Children.Add(effectSwitch);
					}
				}
				return;
			}
		}
		Rect_EffectPanel_Disbale.Visibility = Visibility.Visible;
		_isEffectPanelEnabled = false;
	}

	private void RefreshSystemEffectsControl()
	{
		if (_isEffectPanelEnabled)
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				CheckBox_EffectSwitch.IsChecked = effectStatus.enable;
			}));
			return;
		}
		Dispatcher.BeginInvoke((Action)(() =>
		{
			CheckBox_EffectSwitch.IsChecked = _isEffectPanelEnabled;
			CheckBox_EffectSwitch.IsEnabled = _isEffectPanelEnabled;
		}));
	}
}
