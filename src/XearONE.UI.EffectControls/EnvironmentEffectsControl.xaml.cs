using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.EffectControls;

public partial class EnvironmentEffectsControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private enum Room_Size
	{
		Small,
		Medium,
		Large
	}

	private Guid _audEndptUID;

	private const string propertyName_EM_Enable = "Enable_RFX_GFX";

	private const string propertyName_EM_Mode = "RFX_ENVIRONMENT";

	private const string propertyName_EM_RoomSize = "RFX_ROOMSIZE";

	private int _currentEnvMode;

	public List<string> propertyNames { get; set; } = new List<string>();

	public int CurrentEnvMode
	{
		get
		{
			return _currentEnvMode;
		}
		set
		{
			ComboBox_EnvMode.SelectedIndex = value;
			if (_currentEnvMode != value)
			{
				_currentEnvMode = value;
				XearSDK.SetProperty(_audEndptUID, "RFX_ENVIRONMENT", _currentEnvMode, out var _);
				CheckModeButton();
			}
		}
	}

	public EnvironmentEffectsControl(Guid audEndptUID)
	{
		InitializeComponent();
		InitPropertyNames();
		_audEndptUID = audEndptUID;
	}

	private void EnvironmentEffectsControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_RFX_GFX", out enable, out XearSDK.CmErr cmErr))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		int mode = 0;
		if (XearSDK.GetProperty(_audEndptUID, "RFX_ENVIRONMENT", out mode, out cmErr))
		{
			Console.WriteLine("Get propertyName_EM_Mode: {0}", mode);
			CurrentEnvMode = mode;
		}
		int room = 0;
		if (XearSDK.GetProperty(_audEndptUID, "RFX_ROOMSIZE", out room, out cmErr))
		{
			switch (room)
			{
			case 0:
				RadioButton_RoomSizeSmall.IsChecked = true;
				break;
			case 1:
				RadioButton_RoomSizeMedium.IsChecked = true;
				break;
			case 2:
				RadioButton_RoomSizeLarge.IsChecked = true;
				break;
			}
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_RFX_GFX");
		propertyNames.Add("RFX_ENVIRONMENT");
		propertyNames.Add("RFX_ROOMSIZE");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_RFX_GFX", enable, out var _);
	}

	private void Btn_EMRoomSize_Click(object sender, RoutedEventArgs e)
	{
		if (sender is RadioButton radioButton)
		{
			int roomSize = Convert.ToInt32(radioButton.Tag.ToString());
			Console.WriteLine("Clicked RoomSize: {0}", roomSize);
			XearSDK.SetProperty(_audEndptUID, "RFX_ROOMSIZE", roomSize, out var _);
		}
	}

	private void Btn_EMMode_Click(object sender, RoutedEventArgs e)
	{
		if (sender is RadioButton radioButton)
		{
			int mode = Convert.ToInt32(radioButton.Tag.ToString());
			Console.WriteLine("Clicked ModeButton: {0}", mode);
			XearSDK.SetProperty(_audEndptUID, "RFX_ENVIRONMENT", mode, out var _);
		}
	}

	private void ComboBox_EnvMode_DropDownClosed(object sender, EventArgs e)
	{
		if (CurrentEnvMode != ComboBox_EnvMode.SelectedIndex)
		{
			CurrentEnvMode = ComboBox_EnvMode.SelectedIndex;
		}
	}

	private void CheckModeButton()
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			RadioButton_ModeConcertHall.IsChecked = false;
			RadioButton_ModeUnderwater.IsChecked = false;
			RadioButton_ModeBathroom.IsChecked = false;
			RadioButton_ModePsychotic.IsChecked = false;
			if (_currentEnvMode == 8)
			{
				RadioButton_ModeConcertHall.IsChecked = true;
			}
			else if (_currentEnvMode == 23)
			{
				RadioButton_ModeUnderwater.IsChecked = true;
			}
			else if (_currentEnvMode == 4)
			{
				RadioButton_ModeBathroom.IsChecked = true;
			}
			else if (_currentEnvMode == 26)
			{
				RadioButton_ModePsychotic.IsChecked = true;
			}
		}));
	}
}
