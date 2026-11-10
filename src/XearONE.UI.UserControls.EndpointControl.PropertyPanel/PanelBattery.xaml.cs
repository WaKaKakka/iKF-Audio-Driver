using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using XearONE.UI.Driver;
using XearONE.UI.Helpers;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelBattery : UserControl, IComponentConnector
{
	private IntPtr _hidObj;

	private int _percentage;

	private int _chargingState;

	private Timer batteryQueryTimer;

	public PanelBattery(IntPtr hidObj)
	{
		InitializeComponent();
		_hidObj = hidObj;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		bool isShow = false;
		if (_hidObj == IntPtr.Zero)
		{
			isShow = false;
		}
		else if (DriverInstance.getInstance().RequestBattery(_hidObj))
		{
			isShow = true;
		}
		else
		{
			isShow = false;
		}
		Dispatcher.BeginInvoke((Action)(() =>
		{
			if (isShow)
			{
				Grid_Content.Visibility = Visibility.Visible;
			}
			else
			{
				Grid_Content.Visibility = Visibility.Collapsed;
			}
		}));
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	private void resetTimerMonitorBattery(bool monitor, int updateInterval = 10000)
	{
		batteryQueryTimer?.Change(-1, -1);
		batteryQueryTimer = null;
		if (monitor)
		{
			batteryQueryTimer = new Timer((object obj) =>
			{
				OnUpdateUI();
			}, this, 0, updateInterval);
		}
	}

	public void OnUpdateUI()
	{
		Console.WriteLine("OnUpdateUI()");
		if (!DriverInstance.getInstance().RequestBattery(_hidObj))
		{
			BatteryInfoLost();
		}
	}

	public void UpdateUI(int percentage, int chargingState)
	{
		LogHelper.Debug($"{_hidObj}: percentage: {percentage}, chargingState: 0x{chargingState:X2}", isPrintCaller: false, "UpdateUI", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\EndpointControl\\PropertyPanel\\PanelBattery.xaml.cs");
		_percentage = percentage;
		_chargingState = chargingState;
		BitmapImage bImage = null;
		if (_percentage <= 100 && _percentage >= 75)
		{
			bImage = (BitmapImage)FindResource("Img_BatteryLevel_100");
		}
		else if (_percentage < 75 && _percentage >= 50)
		{
			bImage = (BitmapImage)FindResource("Img_BatteryLevel_75");
		}
		else if (_percentage < 50 && _percentage >= 25)
		{
			bImage = (BitmapImage)FindResource("Img_BatteryLevel_50");
		}
		else if (_percentage < 25 && _percentage >= 5)
		{
			bImage = (BitmapImage)FindResource("Img_BatteryLevel_25");
		}
		else if (_percentage < 5 && _percentage >= 0)
		{
			bImage = (BitmapImage)FindResource("Img_BatteryLevel_00");
		}
		Dispatcher.BeginInvoke((Action)(() =>
		{
			if (Grid_Content.Visibility == Visibility.Collapsed)
			{
				Grid_Content.Visibility = Visibility.Visible;
			}
			Img_Battery.Source = bImage;
			Img_Battery.Visibility = Visibility.Visible;
			Label_Battery.Content = $"{_percentage}%";
			Label_Battery.Visibility = Visibility.Visible;
			Img_Battery_Disconnect.Visibility = Visibility.Hidden;
			if (_chargingState == 2 || _chargingState == 4)
			{
				Img_Battery_State.Visibility = Visibility.Hidden;
			}
			else if (_chargingState == 3)
			{
				Img_Battery_State.Visibility = Visibility.Visible;
				Img_Battery_State.Source = (BitmapImage)FindResource("Img_BatteryState_Charging");
			}
			else if (_chargingState == 26)
			{
				BatteryInfoLost();
			}
		}));
	}

	private void BatteryInfoLost()
	{
		Console.WriteLine("BatteryInfoLost()");
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Img_Battery.Visibility = Visibility.Hidden;
			Img_Battery_State.Visibility = Visibility.Hidden;
			Label_Battery.Visibility = Visibility.Hidden;
			Img_Battery_Disconnect.Visibility = Visibility.Visible;
		}));
	}
}
