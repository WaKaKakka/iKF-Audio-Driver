using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.Driver;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.EffectControls;
using XearONE.UI.EffectControls.HeadTracking;
using XearONE.UI.EffectControls.MagicVoice2;
using XearONE.UI.EffectControls.RecordingEffect;

namespace XearONE.UI.ControlWindow.Content;

public partial class EffectControl : UserControl, IComponentConnector
{
	private JackDeviceInfo _devInfo;

	public EffectControl(JackDeviceInfo devInfo)
	{
		InitializeComponent();
		Name = "EffectControl";
		_devInfo = devInfo;
	}

	private void EffectControl_Loaded(object sender, RoutedEventArgs e)
	{
		RebuildEffectPanel();
		if (_devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_HeadTracking))
		{
			SubWindow parentWindow = Window.GetWindow(this) as SubWindow;
			SubWindowTitleCover subWindowTitleCover = new SubWindowTitleCover(parentWindow)
			{
				Top = parentWindow.Top + 2.0
			};
			subWindowTitleCover.Height -= 2.0;
			subWindowTitleCover.Left = parentWindow.Left + 2.0;
			subWindowTitleCover.Width = parentWindow.Width - 4.0;
			subWindowTitleCover.Show();
		}
	}

	public void UpdateUI(string prop)
	{
		RefreshEffectPanel(prop);
	}

	public void RebuildEffectPanel()
	{
		EffectStackPanel.Children.Clear();
		List<XearSDK.CmLicenseCategory> orderedSupportedEffectList = new List<XearSDK.CmLicenseCategory>();
		DriverInstance.getInstance().GetOrderedSupportedEffectList(_devInfo, ref orderedSupportedEffectList);
		if (_devInfo.dataFlow == XearSDK.CmDataFlow.CDF_Render)
		{
			for (int index = 0; index < orderedSupportedEffectList.Count; index++)
			{
				if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_Equalizer10Bands)
				{
					EQControl effect = new EQControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_SpeakerShifter)
				{
					VirtualSpeakerShifterControl effect2 = new VirtualSpeakerShifterControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect2);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_XearSurroundHeadphone)
				{
					XearSurroundHeadphoneControl effect3 = new XearSurroundHeadphoneControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect3);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_EnvironmentEffects)
				{
					EnvironmentEffectsControl effect4 = new EnvironmentEffectsControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect4);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_XearSingFX_KeyShifting)
				{
					XearSingFxKeyShiftingControl effect5 = new XearSingFxKeyShiftingControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect5);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_XearSingFX_VocalFading)
				{
					XearSingFxVocalFadingControl effect6 = new XearSingFxVocalFadingControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect6);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_XearAudioBrilliant)
				{
					XearAudioBrilliantControl effect7 = new XearAudioBrilliantControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect7);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_XearDynamicBass)
				{
					XearDynamicBassControl effect8 = new XearDynamicBassControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect8);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_XearSmartVolume)
				{
					XearSmartVolumeControl effect9 = new XearSmartVolumeControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect9);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_XearVoiceClarity)
				{
					XearVoiceClarityControl effect10 = new XearVoiceClarityControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect10);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_SurroundMax)
				{
					XearSurroundMaxControl effect11 = new XearSurroundMaxControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect11);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_AINoiseReduction)
				{
					AINRPlaybackControl effect12 = new AINRPlaybackControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect12);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_HeadTracking)
				{
					HeadTrackingControl effect13 = new HeadTrackingControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect13);
				}
				else if (orderedSupportedEffectList[index] == XearSDK.CmLicenseCategory.CLC_MultiChannelEQ)
				{
					VigilanceModeControl effect14 = new VigilanceModeControl(_devInfo.audEndptUID);
					AddEffectToPanel(effect14);
				}
			}
			return;
		}
		if (_devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_NoiseGate) && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_Compressor) && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_LDR) && _devInfo.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_AINoiseReduction))
		{
			RecordingEffectControl effect15 = new RecordingEffectControl(_devInfo.audEndptUID);
			AddEffectToPanel(effect15);
		}
		if (orderedSupportedEffectList.Contains(XearSDK.CmLicenseCategory.CLC_AIMagicVoice))
		{
			MagicVoice2Control effect16 = new MagicVoice2Control(_devInfo.audEndptUID);
			AddEffectToPanel(effect16);
		}
		for (int i = 0; i < orderedSupportedEffectList.Count; i++)
		{
			if (orderedSupportedEffectList[i] == XearSDK.CmLicenseCategory.CLC_Equalizer10Bands)
			{
				MicEQControl effect17 = new MicEQControl(_devInfo.audEndptUID);
				AddEffectToPanel(effect17);
			}
			else if (orderedSupportedEffectList[i] == XearSDK.CmLicenseCategory.CLC_XearSingFX_Echo)
			{
				XearSingFxEchoControl effect18 = new XearSingFxEchoControl(_devInfo.audEndptUID);
				AddEffectToPanel(effect18);
			}
			else if (orderedSupportedEffectList[i] == XearSDK.CmLicenseCategory.CLC_NoiseReduction)
			{
				NoiseReductionControl effect19 = new NoiseReductionControl(_devInfo.audEndptUID);
				AddEffectToPanel(effect19);
			}
			else if (orderedSupportedEffectList[i] == XearSDK.CmLicenseCategory.CLC_XearSingFX_MagicVoice)
			{
				XearMagicVoiceControl effect20 = new XearMagicVoiceControl(_devInfo.audEndptUID);
				AddEffectToPanel(effect20);
			}
			else if (orderedSupportedEffectList[i] == XearSDK.CmLicenseCategory.CLC_AINoiseReduction)
			{
				AINRCaptureControl effect21 = new AINRCaptureControl(_devInfo.audEndptUID);
				AddEffectToPanel(effect21);
			}
		}
	}

	private void AddEffectToPanel(FrameworkElement effect)
	{
		effect.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
		effect.HorizontalAlignment = HorizontalAlignment.Center;
		effect.VerticalAlignment = VerticalAlignment.Top;
		EffectStackPanel.Children.Add(effect);
	}

	public void RefreshEffectPanel(string propName = null)
	{
		Console.WriteLine("RefreshEffectPanel()");
		Dispatcher.BeginInvoke((Action)(() =>
		{
			if (propName == null)
			{
				Console.WriteLine("RefreshEffectPanel()");
				{
					foreach (object child in EffectStackPanel.Children)
					{
						if (child is IUsrCtrlProperty usrCtrlProperty)
						{
							usrCtrlProperty.RefreshAllControls();
						}
					}
					return;
				}
			}
			Console.WriteLine("RefreshEffectPanel(): {0}", propName);
			foreach (object child2 in EffectStackPanel.Children)
			{
				if (child2 is IUsrCtrlProperty usrCtrlProperty2 && usrCtrlProperty2.propertyNames.Contains(propName))
				{
					Console.WriteLine("Found UserControl, Refresh it.");
					usrCtrlProperty2.RefreshAllControls();
					break;
				}
			}
		}));
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
