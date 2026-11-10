using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Cmedia;
using XearONE.UI.Driver;

namespace XearONE.UI.EffectControls;

public partial class XearSurroundHeadphoneControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private Guid _audEndptUID;

	private const string propertyName_SurroundHeadphone_Enable = "Enable_CONLAB_HPSPK_LFX";

	private const string propertyName_SurroundHeadphone_Mode = "CONLAB_HPSPK_MODE";

	private const string propertyName_SurroundHeadphone_Roomsize = "CONLAB_HPSPK_ROOMSIZE";

	private const string propertyName_SurroundHeadphone_UseHP = "CONLAB_HPSPK_USEHP";

	private MediaPlayer mediaPlayer;

	private List<string> testCheckBoxNameList = new List<string> { "CheckBox_L", "CheckBox_R", "CheckBox_C", "CheckBox_W", "CheckBox_LB", "CheckBox_RB", "CheckBox_LS", "CheckBox_RS" };

	public List<string> propertyNames { get; set; } = new List<string>();

	public XearSurroundHeadphoneControl(Guid audEndptUID)
	{
		InitializeComponent();
		_audEndptUID = audEndptUID;
		mediaPlayer = new MediaPlayer();
		InitPropertyNames();
	}

	private void XearSurroundHeadphoneControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_CONLAB_HPSPK_LFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int mode = 0;
		if (XearSDK.GetProperty(_audEndptUID, "CONLAB_HPSPK_MODE", out mode, out cmErr))
		{
			switch (mode)
			{
			case 0:
				RadioButton_MusicGamingMode.IsChecked = true;
				break;
			case 1:
				RadioButton_MovieMode.IsChecked = true;
				break;
			}
		}
		else
		{
			RadioButton_MusicGamingMode.IsChecked = true;
		}
		int room = 0;
		if (XearSDK.GetProperty(_audEndptUID, "CONLAB_HPSPK_ROOMSIZE", out room, out cmErr))
		{
			switch (room)
			{
			case 0:
				RadioButton_RoomSizeLarge.IsChecked = true;
				break;
			case 1:
				RadioButton_RoomSizeMedium.IsChecked = true;
				break;
			case 2:
				RadioButton_RoomSizeSmall.IsChecked = true;
				break;
			}
		}
		else
		{
			RadioButton_RoomSizeMedium.IsChecked = true;
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_CONLAB_HPSPK_LFX");
		propertyNames.Add("CONLAB_HPSPK_MODE");
		propertyNames.Add("CONLAB_HPSPK_ROOMSIZE");
		propertyNames.Add("CONLAB_HPSPK_USEHP");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_CONLAB_HPSPK_LFX", enable, out var cmErr);
		int useHP = 1;
		XearSDK.SetProperty(_audEndptUID, "CONLAB_HPSPK_USEHP", useHP, out cmErr);
		DriverInstance.getInstance().SetHIDStatus(_audEndptUID, "Enable_CONLAB_HPSPK_LFX", (byte)enable);
	}

	private void Btn_SurroundHeadphoneMode_Click(object sender, RoutedEventArgs e)
	{
		int mode = int.Parse((sender as RadioButton).Tag.ToString());
		XearSDK.SetProperty(_audEndptUID, "CONLAB_HPSPK_MODE", mode, out var _);
	}

	private void Btn_SurroundHeadphoneRoomSize_Click(object sender, RoutedEventArgs e)
	{
		int room = int.Parse((sender as RadioButton).Tag.ToString());
		XearSDK.SetProperty(_audEndptUID, "CONLAB_HPSPK_ROOMSIZE", room, out var _);
	}

	private void StopTestSound(object sender, EventArgs e)
	{
		mediaPlayer.Stop();
		EnableTestButton();
		UnCheckedTestButton();
	}

	private void CheckBox_Speaker_Click(object sender, RoutedEventArgs e)
	{
		CheckBox CheckBox_Speaker = sender as CheckBox;
		if (CheckBox_Speaker.IsChecked == true)
		{
			DisableTestButton(CheckBox_Speaker.Name);
			string TestSoundPath = string.Empty;
			switch (int.Parse(CheckBox_Speaker.Tag.ToString()))
			{
			case 0:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\0_L.wav";
				break;
			case 1:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\1_R.wav";
				break;
			case 2:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\2_C.wav";
				break;
			case 3:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\3_W.wav";
				break;
			case 4:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\4_LB.wav";
				break;
			case 5:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\5_RB.wav";
				break;
			case 6:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\6_LS.wav";
				break;
			case 7:
				TestSoundPath = AppContext.BaseDirectory + "Resources\\Sound\\SurroundHeadphone\\7_RS.wav";
				break;
			}
			if (TestSoundPath != string.Empty && File.Exists(TestSoundPath))
			{
				mediaPlayer.Open(new Uri(TestSoundPath));
				mediaPlayer.MediaEnded += StopTestSound;
				mediaPlayer.Play();
			}
			else
			{
				EnableTestButton();
				CheckBox_Speaker.IsChecked = false;
			}
		}
		else
		{
			StopTestSound(null, null);
		}
	}

	private void EnableTestButton()
	{
		for (int n = 0; n < testCheckBoxNameList.Count; n++)
		{
			(FindName(testCheckBoxNameList[n]) as CheckBox).IsEnabled = true;
		}
	}

	private void DisableTestButton(string controlName)
	{
		for (int n = 0; n < testCheckBoxNameList.Count; n++)
		{
			if (controlName != testCheckBoxNameList[n])
			{
				(FindName(testCheckBoxNameList[n]) as CheckBox).IsEnabled = false;
			}
		}
	}

	private void UnCheckedTestButton()
	{
		for (int n = 0; n < testCheckBoxNameList.Count; n++)
		{
			(FindName(testCheckBoxNameList[n]) as CheckBox).IsChecked = false;
		}
	}
}
