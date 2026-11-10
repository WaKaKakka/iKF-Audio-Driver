#define TRACE
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace XearONE.UI.EffectControls.RecordingEffect;

public partial class RecordingEffectControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid AudioEndpointUID;

	private RecordingEffectHandler recordingEffectHandler;

	private readonly string[] CompressorSliders = new string[6] { "slCompressorThreshold", "slCompressorRatio", "slCompressorWidth", "slCompressorAttackTime", "slCompressorReleaseTime", "slCompressorGain" };

	private readonly string[] CompressorPresetNames = new string[3] { "RadioButtonCompressorMax", "RadioButtonCompressorDefault", "RadioButtonCompressorLow" };

	private readonly int[,] CompressorPresetValues = new int[3, 6]
	{
		{ 40, 6, 20, 2, 100, 26 },
		{ 31, 4, 20, 2, 100, 18 },
		{ 17, 4, 20, 2, 100, 10 }
	};

	public List<string> propertyNames { get; set; } = new List<string>();

	private void outputErrorString()
	{
		tbNoiseReductionEnable.IsEnabled = false;
		tbNoiseReductionEnable.IsChecked = false;
		Trace.WriteLine("[CM][RecordingEffect] " + recordingEffectHandler.GetLastErrorString());
	}

	public RecordingEffectControl(Guid audioEndpointUID)
	{
		InitializeComponent();
		AudioEndpointUID = audioEndpointUID;
		InitPropertyNames();
		recordingEffectHandler = new RecordingEffectHandler(AudioEndpointUID);
		if (recordingEffectHandler.GetLastErrorCode() != 0)
		{
			outputErrorString();
		}
	}

	private void RecordingEffectControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
		Bg.Height = ActualHeight;
	}

	public void RefreshAllControls()
	{
		if (recordingEffectHandler.IsEnabled())
		{
			tbNoiseReductionEnable.IsChecked = true;
			switch (recordingEffectHandler.GetLastMode())
			{
			case RecordingEffectMode.Compressor:
				rbCompressor.IsChecked = true;
				grdNoiseGateDetails.IsEnabled = true;
				updateCompressorSliders();
				break;
			case RecordingEffectMode.LDR:
				rbLDR.IsChecked = true;
				grdNoiseGateDetails.IsEnabled = false;
				break;
			case RecordingEffectMode.AINR:
				rbAINR.IsChecked = true;
				grdNoiseGateDetails.IsEnabled = false;
				break;
			}
		}
		else
		{
			rbCompressor.IsEnabled = false;
			rbLDR.IsEnabled = false;
			rbAINR.IsEnabled = false;
			grdNoiseGateDetails.IsEnabled = false;
		}
	}

	public void InitPropertyNames()
	{
		string[] compressorSliders = CompressorSliders;
		foreach (string name in compressorSliders)
		{
			propertyNames.Add(name);
		}
		compressorSliders = CompressorPresetNames;
		foreach (string name2 in compressorSliders)
		{
			propertyNames.Add(name2);
		}
	}

	private void NoiseReductionEnable_Click(object sender, RoutedEventArgs e)
	{
		if (tbNoiseReductionEnable.IsChecked == false)
		{
			if (recordingEffectHandler.GetLastMode() == RecordingEffectMode.Compressor)
			{
				recordingEffectHandler.SaveCompressorSettings();
			}
			recordingEffectHandler.SetMode(RecordingEffectMode.None);
			rbCompressor.IsEnabled = false;
			rbLDR.IsEnabled = false;
			rbAINR.IsEnabled = false;
			rbCompressor.IsChecked = false;
			rbLDR.IsChecked = false;
			rbAINR.IsChecked = false;
			grdNoiseGateDetails.IsEnabled = false;
			return;
		}
		rbCompressor.IsEnabled = true;
		rbLDR.IsEnabled = true;
		rbAINR.IsEnabled = true;
		switch (recordingEffectHandler.GetLastMode())
		{
		case RecordingEffectMode.Compressor:
			rbCompressor.IsChecked = true;
			Compressor_Click(null, null);
			break;
		case RecordingEffectMode.LDR:
			rbLDR.IsChecked = true;
			grdNoiseGateDetails.IsEnabled = false;
			recordingEffectHandler.SetMode(RecordingEffectMode.LDR);
			break;
		case RecordingEffectMode.AINR:
			rbAINR.IsChecked = true;
			grdNoiseGateDetails.IsEnabled = false;
			recordingEffectHandler.SetMode(RecordingEffectMode.AINR);
			break;
		default:
			CompressorPreset_Checked(RadioButtonCompressorDefault, null);
			rbCompressor.IsChecked = true;
			Compressor_Click(null, null);
			break;
		}
	}

	private void Compressor_Click(object sender, RoutedEventArgs e)
	{
		grdNoiseGateDetails.IsEnabled = true;
		recordingEffectHandler.RestoreCompressorSettings();
		updateCompressorSliders();
		recordingEffectHandler.SetMode(RecordingEffectMode.Compressor);
	}

	private void LDR_Click(object sender, RoutedEventArgs e)
	{
		if (recordingEffectHandler.GetLastMode() == RecordingEffectMode.Compressor)
		{
			recordingEffectHandler.SaveCompressorSettings();
		}
		grdNoiseGateDetails.IsEnabled = false;
		recordingEffectHandler.SetMode(RecordingEffectMode.LDR);
	}

	private void AINR_Click(object sender, RoutedEventArgs e)
	{
		if (recordingEffectHandler.GetLastMode() == RecordingEffectMode.Compressor)
		{
			recordingEffectHandler.SaveCompressorSettings();
		}
		grdNoiseGateDetails.IsEnabled = false;
		recordingEffectHandler.SetMode(RecordingEffectMode.AINR);
	}

	private void updateCompressorSliders()
	{
		int outValue = 0;
		outValue = 0;
		if (recordingEffectHandler.GetProperty("Compressor_Threshold", ref outValue) == 0)
		{
			slCompressorThreshold.Value = outValue;
		}
		outValue = 0;
		if (recordingEffectHandler.GetProperty("Compressor_Ratio", ref outValue) == 0)
		{
			slCompressorRatio.Value = outValue;
		}
		outValue = 0;
		if (recordingEffectHandler.GetProperty("Compressor_KneeWidth", ref outValue) == 0)
		{
			slCompressorWidth.Value = outValue;
		}
		outValue = 0;
		if (recordingEffectHandler.GetProperty("Compressor_AttackTime", ref outValue) == 0)
		{
			slCompressorAttackTime.Value = outValue;
		}
		outValue = 0;
		if (recordingEffectHandler.GetProperty("Compressor_ReleaseTime", ref outValue) == 0)
		{
			slCompressorReleaseTime.Value = outValue;
		}
		outValue = 0;
		if (recordingEffectHandler.GetProperty("Compressor_MakeUpGain", ref outValue) == 0)
		{
			slCompressorGain.Value = outValue;
		}
	}

	private void Compressor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (recordingEffectHandler != null)
		{
			int inValue = 0;
			Slider slider = sender as Slider;
			if (slider.Name == "slCompressorThreshold")
			{
				inValue = (int)slider.Value;
				recordingEffectHandler.SetProperty("Compressor_Threshold", inValue);
			}
			else if (slider.Name == "slCompressorRatio")
			{
				inValue = (int)slider.Value;
				recordingEffectHandler.SetProperty("Compressor_Ratio", inValue);
			}
			else if (slider.Name == "slCompressorWidth")
			{
				inValue = (int)slider.Value;
				recordingEffectHandler.SetProperty("Compressor_KneeWidth", inValue);
			}
			else if (slider.Name == "slCompressorAttackTime")
			{
				inValue = (int)slider.Value;
				recordingEffectHandler.SetProperty("Compressor_AttackTime", inValue);
			}
			else if (slider.Name == "slCompressorReleaseTime")
			{
				inValue = (int)slider.Value;
				recordingEffectHandler.SetProperty("Compressor_ReleaseTime", inValue);
			}
			else if (slider.Name == "slCompressorGain")
			{
				inValue = (int)slider.Value;
				recordingEffectHandler.SetProperty("Compressor_MakeUpGain", inValue);
			}
			updateCompressorPresetButtonStatus();
		}
	}

	private void updateCompressorPresetButtonStatus()
	{
		bool isIdentical = false;
		int foundPresetMode = -1;
		int[] currentCompressorValues = new int[CompressorSliders.Length];
		for (int i = 0; i < CompressorSliders.Length; i++)
		{
			if (FindName(CompressorSliders[i]) is Slider foundSlider)
			{
				currentCompressorValues[i] = Convert.ToInt32(foundSlider.Value);
			}
		}
		for (int presetMode = 0; presetMode < CompressorPresetNames.Length; presetMode++)
		{
			isIdentical = true;
			for (int sliderIndex = 0; sliderIndex < CompressorSliders.Length; sliderIndex++)
			{
				if (CompressorPresetValues[presetMode, sliderIndex] != currentCompressorValues[sliderIndex])
				{
					isIdentical = false;
					break;
				}
			}
			if (isIdentical)
			{
				foundPresetMode = presetMode;
				break;
			}
		}
		string[] compressorPresetNames;
		if ((foundPresetMode >= 0) & (foundPresetMode < CompressorPresetNames.Length))
		{
			compressorPresetNames = CompressorPresetNames;
			foreach (string radioButtonName in compressorPresetNames)
			{
				if (FindName(radioButtonName) is RadioButton foundRadioButton)
				{
					if (radioButtonName == CompressorPresetNames[foundPresetMode])
					{
						foundRadioButton.Checked -= CompressorPreset_Checked;
						foundRadioButton.IsChecked = true;
						foundRadioButton.Checked += CompressorPreset_Checked;
					}
					else
					{
						foundRadioButton.IsChecked = false;
					}
				}
			}
			return;
		}
		compressorPresetNames = CompressorPresetNames;
		foreach (string radioButtonName2 in compressorPresetNames)
		{
			if (FindName(radioButtonName2) is RadioButton foundRadioButton2)
			{
				foundRadioButton2.IsChecked = false;
			}
		}
	}

	private void CompressorPreset_Checked(object sender, RoutedEventArgs e)
	{
		RadioButton radioButton = sender as RadioButton;
		int presetNameIndex = 0;
		for (presetNameIndex = 0; presetNameIndex < CompressorPresetNames.Length && !(CompressorPresetNames[presetNameIndex] == radioButton.Name); presetNameIndex++)
		{
		}
		for (int iter = 0; iter < CompressorSliders.Length; iter++)
		{
			if (FindName(CompressorSliders[iter]) is Slider foundSlider)
			{
				foundSlider.Value = CompressorPresetValues[presetNameIndex, iter];
			}
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
