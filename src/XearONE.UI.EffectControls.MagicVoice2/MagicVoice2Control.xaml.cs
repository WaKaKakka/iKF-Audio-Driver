#define TRACE
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.Driver;
using XearONE.UI.Helpers;

namespace XearONE.UI.EffectControls.MagicVoice2;

public partial class MagicVoice2Control : UserControl, IComponentConnector
{
	private Guid AudioEndpointUID;

	private MagicVoice2Handler magicVoice2Handler;

	private IniFileHelper iniMagicVoice2;

	private string currentProfileName = "";

	private System.Timers.Timer timerUpdateMicMeter;

	private void outputErrorString()
	{
		tbMagicVoice2Enable.IsEnabled = false;
		grdAiMagicVoice.Visibility = Visibility.Hidden;
		grdPeakMeter.Visibility = Visibility.Hidden;
		grdProfile.Visibility = Visibility.Hidden;
		rbManAi.IsEnabled = false;
		rbWomanAi.IsEnabled = false;
		rbBoyAi.IsEnabled = false;
		rbGirlAi.IsEnabled = false;
		rbCartoon.IsEnabled = false;
		rbMonster.IsEnabled = false;
		rbMan.IsEnabled = false;
		rbWoman.IsEnabled = false;
		Trace.WriteLine("[CM][MagicVoice2] " + magicVoice2Handler.GetLastErrorString());
		ucWarningDialog.SetErrorDescText(magicVoice2Handler.GetLastErrorString());
		ucWarningDialog.Visibility = Visibility.Visible;
	}

	public MagicVoice2Control(Guid audioEndpointUID)
	{
		InitializeComponent();
		AudioEndpointUID = audioEndpointUID;
		iniMagicVoice2 = DriverInstance.getInstance().iniMagicVoice2;
		magicVoice2Handler = new MagicVoice2Handler(AudioEndpointUID);
		if (magicVoice2Handler.GetLastErrorCode() != 0)
		{
			outputErrorString();
			return;
		}
		timerUpdateMicMeter = new System.Timers.Timer(100.0);
		timerUpdateMicMeter.Elapsed += OnTimedUpdateMicMeter;
		timerUpdateMicMeter.AutoReset = true;
		timerUpdateMicMeter.Enabled = false;
		updateAllControls();
	}

	private void CheckHardwarePerformance()
	{
		bool flag = magicVoice2Handler.IsSysCheckResultPass();
		new PowerSettingsWindow();
		if (!flag && iniMagicVoice2.ReadString("SystemWarningWindow", "", "DoNotShowAgain") != "true")
		{
			SystemWarningWindow systemWarningWindow = new SystemWarningWindow();
			systemWarningWindow.ShowDialog();
			string value = ((systemWarningWindow.chkDoNotShowAgain.IsChecked == true) ? "true" : "false");
			iniMagicVoice2.Write("SystemWarningWindow", value, "DoNotShowAgain");
		}
	}

	private int updateAllControls()
	{
		currentProfileName = iniMagicVoice2.ReadString("LastProfileName", "", "Settings");
		if ("" != currentProfileName)
		{
			string[] words = currentProfileName.Split(' ');
			if (words.Length >= 2)
			{
				comboProfile.SelectedIndex = Convert.ToInt32(words[1]) - 1;
			}
			else
			{
				comboProfile.SelectedIndex = 0;
			}
		}
		else
		{
			comboProfile.SelectedIndex = 0;
			currentProfileName = "Profile 1";
		}
		if ("Checked" == iniMagicVoice2.ReadString("ListenToYourVoice", "", "Settings"))
		{
			chkListen.IsChecked = true;
		}
		else
		{
			chkListen.IsChecked = false;
		}
		if (XearSDK.GetProperty(AudioEndpointUID, "VolumeLevelScalar", out float[] outDatFloat, out XearSDK.CmErr _, -1))
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				slMasterVolume.Value = Math.Round(outDatFloat[0] * 100f);
			}));
		}
		string sTarget = iniMagicVoice2.ReadString("LastTarget", "", "Settings");
		MagicVoice2Target target = MagicVoice2Target.None;
		foreach (MagicVoice2Target t in Enum.GetValues(typeof(MagicVoice2Target)))
		{
			if (t.ToString() == sTarget)
			{
				target = t;
				break;
			}
		}
		if (target != MagicVoice2Target.None)
		{
			magicVoice2Handler.SetTarget(target);
		}
		else if (magicVoice2Handler.IsAIMagicVoiceAvailable())
		{
			magicVoice2Handler.SetTarget(MagicVoice2Target.ManAi);
		}
		else
		{
			magicVoice2Handler.SetTarget(MagicVoice2Target.Cartoon);
		}
		tbMagicVoice2Enable.IsChecked = magicVoice2Handler.IsEnabled();
		MagicVoice2Enable_Click(null, null);
		return 0;
	}

	private void Pitch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (magicVoice2Handler == null)
		{
			return;
		}
		magicVoice2Handler.SetPitchValue(slPitch.Value);
		MagicVoice2Target target = magicVoice2Handler.GetTarget();
		iniMagicVoice2.Write(target.ToString() + "PitchValueFloat", slPitch.Value.ToString("0.#"), "Settings");
		if (target.ToString() == iniMagicVoice2.ReadString("LastTarget", "", currentProfileName))
		{
			double savedPitchValue = -99.0;
			string sSavedPitchValue = iniMagicVoice2.ReadString(target.ToString() + "PitchValueFloat", "", currentProfileName);
			if (sSavedPitchValue != "")
			{
				savedPitchValue = Convert.ToDouble(sSavedPitchValue);
			}
			if (Math.Abs(savedPitchValue - Math.Round(slPitch.Value, 1)) < 0.05)
			{
				btnSaveProfile.IsEnabled = false;
			}
			else
			{
				btnSaveProfile.IsEnabled = true;
			}
		}
		else
		{
			btnSaveProfile.IsEnabled = true;
		}
	}

	private async void MagicVoice2Enable_Click(object sender, RoutedEventArgs e)
	{
		bool isAIMagicVoiceAvailable = iniMagicVoice2.ReadString("ForceEnableAIMagicVoice", "", "Settings") == "true" || magicVoice2Handler.IsAIMagicVoiceAvailable();
		if (tbMagicVoice2Enable.IsChecked == true)
		{
			if (sender != null)
			{
				CheckHardwarePerformance();
			}
			if (magicVoice2Handler.GetLastErrorCode() != 0)
			{
				outputErrorString();
			}
			if (!isAIMagicVoiceAvailable)
			{
				grdPeakMeter.Visibility = Visibility.Visible;
				grdAiMagicVoice.Visibility = Visibility.Hidden;
				grdProfile.Visibility = Visibility.Hidden;
				rbManAi.IsEnabled = false;
				rbWomanAi.IsEnabled = false;
				rbBoyAi.IsEnabled = false;
				rbGirlAi.IsEnabled = false;
				rbCartoon.IsEnabled = true;
				rbMonster.IsEnabled = true;
				rbMan.IsEnabled = true;
				rbWoman.IsEnabled = true;
				if (magicVoice2Handler.GetTarget() == MagicVoice2Target.ManAi || magicVoice2Handler.GetTarget() == MagicVoice2Target.WomanAi)
				{
					magicVoice2Handler.SetTarget(MagicVoice2Target.Cartoon);
				}
			}
			else
			{
				grdAiMagicVoice.Visibility = Visibility.Visible;
				grdPeakMeter.Visibility = Visibility.Visible;
				grdProfile.Visibility = Visibility.Visible;
				rbManAi.IsEnabled = true;
				rbWomanAi.IsEnabled = true;
				rbBoyAi.IsEnabled = true;
				rbGirlAi.IsEnabled = true;
				rbCartoon.IsEnabled = true;
				rbMonster.IsEnabled = true;
				rbMan.IsEnabled = true;
				rbWoman.IsEnabled = true;
			}
			switch (magicVoice2Handler.GetTarget())
			{
			case MagicVoice2Target.ManAi:
				ManAi_Click(null, null);
				break;
			case MagicVoice2Target.WomanAi:
				WomanAi_Click(null, null);
				break;
			case MagicVoice2Target.BoyAi:
				BoyAi_Click(null, null);
				break;
			case MagicVoice2Target.GirlAi:
				GirlAi_Click(null, null);
				break;
			case MagicVoice2Target.Cartoon:
				Cartoon_Click(null, null);
				break;
			case MagicVoice2Target.Monster:
				Monster_Click(null, null);
				break;
			case MagicVoice2Target.Man:
				Man_Click(null, null);
				break;
			case MagicVoice2Target.Woman:
				Woman_Click(null, null);
				break;
			}
			timerUpdateMicMeter.Enabled = true;
			SoundRecorder.IsListenToMic = chkListen.IsChecked == true;
			await magicVoice2Handler.StartRecording(new Progress<string>(statusUpdate));
		}
		else
		{
			if (ucWarningDialog.Visibility == Visibility.Visible)
			{
				ucWarningDialog.Visibility = Visibility.Hidden;
			}
			timerUpdateMicMeter.Enabled = false;
			magicVoice2Handler.StopRecording();
			grdAiMagicVoice.Visibility = Visibility.Hidden;
			grdPeakMeter.Visibility = Visibility.Hidden;
			grdProfile.Visibility = Visibility.Hidden;
			rbManAi.IsEnabled = false;
			rbWomanAi.IsEnabled = false;
			rbBoyAi.IsEnabled = false;
			rbGirlAi.IsEnabled = false;
			rbCartoon.IsEnabled = false;
			rbMonster.IsEnabled = false;
			rbMan.IsEnabled = false;
			rbWoman.IsEnabled = false;
			rbManAi.IsChecked = false;
			rbWomanAi.IsChecked = false;
			rbCartoon.IsChecked = false;
			rbMonster.IsChecked = false;
			rbMan.IsChecked = false;
			rbWoman.IsChecked = false;
			magicVoice2Handler.SetEnable(enable: false);
		}
	}

	private void ScorllLeft_Click(object sender, RoutedEventArgs e)
	{
	}

	private void ScorllRight_Click(object sender, RoutedEventArgs e)
	{
	}

	private void EnableAiMagicVoiceControls(bool enable)
	{
		grdAiMagicVoice.IsEnabled = enable;
		grdProfile.IsEnabled = enable;
		if (enable)
		{
			Pitch_ValueChanged(null, null);
		}
	}

	private void Monster_Click(object sender, RoutedEventArgs e)
	{
		rbMonster.IsChecked = true;
		magicVoice2Handler.SetTarget(MagicVoice2Target.Monster);
		magicVoice2Handler.SetEnable(enable: true);
		slPitch.Value = 0.0;
		EnableAiMagicVoiceControls(enable: false);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
	}

	private void Cartoon_Click(object sender, RoutedEventArgs e)
	{
		rbCartoon.IsChecked = true;
		magicVoice2Handler.SetTarget(MagicVoice2Target.Cartoon);
		magicVoice2Handler.SetEnable(enable: true);
		slPitch.Value = 0.0;
		EnableAiMagicVoiceControls(enable: false);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
	}

	private void Man_Click(object sender, RoutedEventArgs e)
	{
		rbMan.IsChecked = true;
		magicVoice2Handler.SetTarget(MagicVoice2Target.Man);
		magicVoice2Handler.SetEnable(enable: true);
		slPitch.Value = 0.0;
		EnableAiMagicVoiceControls(enable: false);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
	}

	private void Woman_Click(object sender, RoutedEventArgs e)
	{
		rbWoman.IsChecked = true;
		magicVoice2Handler.SetTarget(MagicVoice2Target.Woman);
		magicVoice2Handler.SetEnable(enable: true);
		slPitch.Value = 0.0;
		EnableAiMagicVoiceControls(enable: false);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
	}

	public async Task ShowProcessingOverlay(int setTargetResult)
	{
		LoadingOverlay.Visibility = Visibility.Visible;
		await Task.Run(() =>
		{
			if (setTargetResult != -1)
			{
				bool flag = false;
				int num = 5;
				int num2 = 0;
				do
				{
					Thread.Sleep(1000);
					flag = magicVoice2Handler.IsChangeTargetDone();
					num2++;
				}
				while (setTargetResult == 0 && !flag && num2 < num);
			}
		});
		LoadingOverlay.Visibility = Visibility.Collapsed;
	}

	private async void ManAi_Click(object sender, RoutedEventArgs e)
	{
		rbManAi.IsChecked = true;
		int setTargetResult = magicVoice2Handler.SetTarget(MagicVoice2Target.ManAi);
		string sPitch = iniMagicVoice2.ReadString("ManAiPitchValueFloat", "", "Settings");
		if (sPitch == "")
		{
			sPitch = iniMagicVoice2.ReadString("ManAiPitchValueFloat", "", currentProfileName);
		}
		if (sPitch != "")
		{
			slPitch.Value = Convert.ToDouble(sPitch);
		}
		else
		{
			slPitch.Value = magicVoice2Handler.GetDefaultAIMagicVoicePitchValue(MagicVoice2Target.ManAi);
		}
		magicVoice2Handler.SetEnable(enable: true);
		EnableAiMagicVoiceControls(enable: true);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
		await ShowProcessingOverlay(setTargetResult);
	}

	private async void WomanAi_Click(object sender, RoutedEventArgs e)
	{
		rbWomanAi.IsChecked = true;
		int setTargetResult = magicVoice2Handler.SetTarget(MagicVoice2Target.WomanAi);
		string sPitch = iniMagicVoice2.ReadString("WomanAiPitchValueFloat", "", "Settings");
		if (sPitch == "")
		{
			sPitch = iniMagicVoice2.ReadString("WomanAiPitchValueFloat", "", currentProfileName);
		}
		if (sPitch != "")
		{
			slPitch.Value = Convert.ToDouble(sPitch);
		}
		else
		{
			slPitch.Value = magicVoice2Handler.GetDefaultAIMagicVoicePitchValue(MagicVoice2Target.WomanAi);
		}
		magicVoice2Handler.SetEnable(enable: true);
		EnableAiMagicVoiceControls(enable: true);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
		await ShowProcessingOverlay(setTargetResult);
	}

	private async void BoyAi_Click(object sender, RoutedEventArgs e)
	{
		rbBoyAi.IsChecked = true;
		int setTargetResult = magicVoice2Handler.SetTarget(MagicVoice2Target.BoyAi);
		string sPitch = iniMagicVoice2.ReadString("BoyAiPitchValueFloat", "", "Settings");
		if (sPitch == "")
		{
			sPitch = iniMagicVoice2.ReadString("BoyAiPitchValueFloat", "", currentProfileName);
		}
		if (sPitch != "")
		{
			slPitch.Value = Convert.ToDouble(sPitch);
		}
		else
		{
			slPitch.Value = magicVoice2Handler.GetDefaultAIMagicVoicePitchValue(MagicVoice2Target.BoyAi);
		}
		magicVoice2Handler.SetEnable(enable: true);
		EnableAiMagicVoiceControls(enable: true);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
		await ShowProcessingOverlay(setTargetResult);
	}

	private async void GirlAi_Click(object sender, RoutedEventArgs e)
	{
		rbGirlAi.IsChecked = true;
		int setTargetResult = magicVoice2Handler.SetTarget(MagicVoice2Target.GirlAi);
		string sPitch = iniMagicVoice2.ReadString("GirlAiPitchValueFloat", "", "Settings");
		if (sPitch == "")
		{
			sPitch = iniMagicVoice2.ReadString("GirlAiPitchValueFloat", "", currentProfileName);
		}
		if (sPitch != "")
		{
			slPitch.Value = Convert.ToDouble(sPitch);
		}
		else
		{
			slPitch.Value = magicVoice2Handler.GetDefaultAIMagicVoicePitchValue(MagicVoice2Target.GirlAi);
		}
		magicVoice2Handler.SetEnable(enable: true);
		EnableAiMagicVoiceControls(enable: true);
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), "Settings");
		await ShowProcessingOverlay(setTargetResult);
	}

	private void Profile_DropDownClosed(object sender, EventArgs e)
	{
		string text = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
		currentProfileName = text;
		iniMagicVoice2.Write("LastProfileName", currentProfileName, "Settings");
		string target = iniMagicVoice2.ReadString("LastTarget", "", currentProfileName);
		if (target == "ManAi")
		{
			string sPitch = iniMagicVoice2.ReadString("ManAiPitchValueFloat", "", currentProfileName);
			if (sPitch != "")
			{
				iniMagicVoice2.Write("ManAiPitchValueFloat", sPitch, "Settings");
			}
			ManAi_Click(null, null);
		}
		else if (target == "WomanAi")
		{
			string sPitch2 = iniMagicVoice2.ReadString("WomanAiPitchValueFloat", "", currentProfileName);
			if (sPitch2 != "")
			{
				iniMagicVoice2.Write("WomanAiPitchValueFloat", sPitch2, "Settings");
			}
			WomanAi_Click(null, null);
		}
		switch (target)
		{
		case "BoyAi":
		{
			string sPitch4 = iniMagicVoice2.ReadString("BoyAiPitchValueFloat", "", currentProfileName);
			if (sPitch4 != "")
			{
				iniMagicVoice2.Write("BoyAiPitchValueFloat", sPitch4, "Settings");
			}
			BoyAi_Click(null, null);
			break;
		}
		case "GirlAi":
		{
			string sPitch3 = iniMagicVoice2.ReadString("GirlAiPitchValueFloat", "", currentProfileName);
			if (sPitch3 != "")
			{
				iniMagicVoice2.Write("GirlAiPitchValueFloat", sPitch3, "Settings");
			}
			GirlAi_Click(null, null);
			break;
		}
		case "Cartoon":
			magicVoice2Handler.SetTarget(MagicVoice2Target.Cartoon);
			Cartoon_Click(null, null);
			break;
		case "Monster":
			magicVoice2Handler.SetTarget(MagicVoice2Target.Monster);
			Monster_Click(null, null);
			break;
		case "Man":
			magicVoice2Handler.SetTarget(MagicVoice2Target.Man);
			Man_Click(null, null);
			break;
		case "Woman":
			magicVoice2Handler.SetTarget(MagicVoice2Target.Woman);
			Woman_Click(null, null);
			break;
		}
	}

	private void ResetProfile_Click(object sender, RoutedEventArgs e)
	{
		foreach (MagicVoice2Target target in Enum.GetValues(typeof(MagicVoice2Target)))
		{
			if (isAiTarget(target))
			{
				iniMagicVoice2.Write(target.ToString() + "PitchValueFloat", "", currentProfileName);
			}
		}
		magicVoice2Handler.ClearPitchEstimationResult();
		slPitch.Value = magicVoice2Handler.GetDefaultAIMagicVoicePitchValue(magicVoice2Handler.GetTarget());
	}

	private void SaveProfile_Click(object sender, RoutedEventArgs e)
	{
		MagicVoice2Target target = magicVoice2Handler.GetTarget();
		iniMagicVoice2.Write("LastTarget", magicVoice2Handler.GetTarget().ToString(), currentProfileName);
		if (isAiTarget(target))
		{
			iniMagicVoice2.Write(target.ToString() + "PitchValueFloat", slPitch.Value.ToString("0.#"), currentProfileName);
		}
		btnSaveProfile.IsEnabled = false;
	}

	private void statusUpdate(string updateMsg)
	{
		if (!updateMsg.Contains("[NoLine]") && !updateMsg.Contains("[AudioData]"))
		{
			Trace.WriteLine("[CM][MagicVoice2Control]" + updateMsg);
		}
	}

	private void Btn_Listen_Click(object sender, RoutedEventArgs e)
	{
		SoundRecorder.IsListenToMic = chkListen.IsChecked == true;
		if (iniMagicVoice2 != null)
		{
			string value = ((chkListen.IsChecked == true) ? "Checked" : "Unchecked");
			iniMagicVoice2.Write("ListenToYourVoice", value, "Settings");
		}
	}

	private bool isAiTarget(MagicVoice2Target target)
	{
		return (target & MagicVoice2Target.ManAi) != 0;
	}

	private async void PitchEstimation_Click(object sender, RoutedEventArgs e)
	{
		magicVoice2Handler.StopRecording();
		magicVoice2Handler.ClearPitchEstimationResult();
		new PitchEstimation(magicVoice2Handler, AudioEndpointUID).ShowDialog();
		if (magicVoice2Handler.IsPitchEstimationDone())
		{
			double pitch = 0.0;
			foreach (MagicVoice2Target target in Enum.GetValues(typeof(MagicVoice2Target)))
			{
				if (isAiTarget(target))
				{
					pitch = magicVoice2Handler.GetDefaultAIMagicVoicePitchValue(target);
					iniMagicVoice2.Write(target.ToString() + "PitchValueFloat", pitch.ToString("0.#"), currentProfileName);
				}
				if (target == magicVoice2Handler.GetTarget())
				{
					slPitch.Value = pitch;
				}
			}
		}
		SoundRecorder.IsListenToMic = chkListen.IsChecked == true;
		await magicVoice2Handler.StartRecording(new Progress<string>(statusUpdate));
	}

	private void OnTimedUpdateMicMeter(object source, ElapsedEventArgs e)
	{
		XearSDK.GetProperty(AudioEndpointUID, "PeakValue", out float[] outDatFloat, out XearSDK.CmErr _, -1);
		UpdatePeakMeterValue(outDatFloat[0]);
	}

	public void UpdatePeakMeterValue(float MasterValue)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_PeakMeter_L.Value = MasterValue * 100f;
			Slider_PeakMeter_R.Value = MasterValue * 100f;
		}));
	}

	private void MagicVoice2_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void MagicVoice2_Unloaded(object sender, RoutedEventArgs e)
	{
		if (magicVoice2Handler != null)
		{
			magicVoice2Handler.StopRecording();
		}
	}

	private void Slider_MasterVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		float VolumeScalar = (float)e.NewValue;
		XearSDK.SetProperty(AudioEndpointUID, "VolumeLevelScalar", new XearSDK.ChannelVolume
		{
			Channel = -1,
			Volume = VolumeScalar / 100f
		}, out var _);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
