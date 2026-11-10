using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Cmedia;
using XearONE.UI.Helpers;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelBalance : UserControl, IComponentConnector
{
	private enum BalanceStatus
	{
		BS_Center,
		BS_Left,
		BS_Right
	}

	private int _channelCount;

	private Guid _endpointUID;

	private int _balanceValue;

	private BalanceStatus _balanceStatus;

	private int BalanceValue
	{
		get
		{
			return _balanceValue;
		}
		set
		{
			_balanceValue = value;
			OnBalanceChanged();
			if (_balanceValue == 10)
			{
				RefreshBalanceIcon(BalanceStatus.BS_Center);
			}
			else if (_balanceValue < 10)
			{
				RefreshBalanceIcon(BalanceStatus.BS_Left);
			}
			else if (_balanceValue > 10)
			{
				RefreshBalanceIcon(BalanceStatus.BS_Right);
			}
			int BalanceSliderValue = _balanceValue * 5;
			string Balance_RValue = $"{BalanceSliderValue}%";
			string Balance_LValue = $"{100 - BalanceSliderValue}%";
			Dispatcher.BeginInvoke((Action)(() =>
			{
				Label_BalanceValueL.Content = Balance_LValue;
				Label_BalanceValueR.Content = Balance_RValue;
			}));
		}
	}

	public void ShowGamingBorder(bool show)
	{
		Border_IsCMDevice.Visibility = ((!show) ? Visibility.Hidden : Visibility.Visible);
	}

	public PanelBalance(int channelCount, Guid endpointUID)
	{
		InitializeComponent();
		_channelCount = channelCount;
		_endpointUID = endpointUID;
		_balanceValue = 10;
		_balanceStatus = BalanceStatus.BS_Center;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		InitBalancePanel();
		UpdateUI();
		Slider_Balance.Value = _balanceValue;
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	public void UpdateUI()
	{
		if (_channelCount > 1)
		{
			float[] channelVolume = new float[2];
			for (int volChannel = 0; volChannel < 2; volChannel++)
			{
				XearSDK.GetProperty(_endpointUID, "VolumeLevelScalar", out float[] outDatFloat, out XearSDK.CmErr _, volChannel);
				channelVolume[volChannel] = outDatFloat[0];
			}
			double ratio = 0.0;
			if (channelVolume[0] < channelVolume[1])
			{
				ratio = channelVolume[0] / channelVolume[1];
			}
			else if (channelVolume[0] > channelVolume[1])
			{
				ratio = channelVolume[1] / channelVolume[0];
			}
			double truncatedRatio = Math.Truncate(ratio * 10.0) / 10.0;
			int balanceOffset = (int)((1.0 - truncatedRatio) * 10.0);
			LogHelper.Debug($"balanceOffset: {balanceOffset}", isPrintCaller: false, "UpdateUI", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\EndpointControl\\PropertyPanel\\PanelBalance.xaml.cs");
			if (channelVolume[0] < channelVolume[1])
			{
				BalanceValue = 10 + balanceOffset;
			}
			else if (channelVolume[0] > channelVolume[1])
			{
				BalanceValue = 10 - balanceOffset;
			}
		}
	}

	private void InitBalancePanel()
	{
		int BalanceValue = Convert.ToInt32(_balanceValue) * 5;
		string Balance_RValue = $"{BalanceValue}%";
		string Balance_LValue = $"{100 - BalanceValue}%";
		Label_BalanceValueL.Content = Balance_LValue;
		Label_BalanceValueR.Content = Balance_RValue;
		if (_channelCount <= 1)
		{
			Grid_Balance.IsEnabled = false;
			Img_Balance_Icon.Source = (ImageSource)FindResource("Img_Balance_Disable");
		}
	}

	private void Slider_Balance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		BalanceValue = Convert.ToInt32(e.NewValue);
		LogHelper.Debug($"Balance Value: {_balanceValue}", isPrintCaller: false, "Slider_Balance_ValueChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\EndpointControl\\PropertyPanel\\PanelBalance.xaml.cs");
	}

	private void RefreshBalanceIcon(BalanceStatus balanceStatus)
	{
		Dispatcher.Invoke(() =>
		{
			if (_balanceStatus != balanceStatus)
			{
				LogHelper.Debug("Balance status changed.", isPrintCaller: false, "RefreshBalanceIcon", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\EndpointControl\\PropertyPanel\\PanelBalance.xaml.cs");
				_balanceStatus = balanceStatus;
				switch (_balanceStatus)
				{
				case BalanceStatus.BS_Center:
					Img_Balance_Icon.Source = (ImageSource)FindResource("Img_Balance_Center");
					break;
				case BalanceStatus.BS_Right:
					Img_Balance_Icon.Source = (ImageSource)FindResource("Img_Balance_Right");
					break;
				case BalanceStatus.BS_Left:
					Img_Balance_Icon.Source = (ImageSource)FindResource("Img_Balance_Left");
					break;
				}
			}
		});
	}

	private void OnBalanceChanged()
	{
		LogHelper.Trace("", "OnBalanceChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\EndpointControl\\PropertyPanel\\PanelBalance.xaml.cs");
		int volChannel = -1;
		XearSDK.GetProperty(_endpointUID, "VolumeLevelScalar", out float[] outDatFloat, out XearSDK.CmErr cmErr, volChannel);
		for (volChannel = 0; volChannel <= _channelCount; volChannel++)
		{
			XearSDK.SetProperty(_endpointUID, "VolumeLevelScalar", new XearSDK.ChannelVolume
			{
				Channel = volChannel,
				Volume = outDatFloat[0]
			}, out cmErr);
		}
		if (_balanceValue < 10)
		{
			float BalanceR = 0f;
			if (_balanceValue == 0)
			{
				outDatFloat[0] = 0f;
			}
			else
			{
				BalanceR = (float)_balanceValue / 10f;
				outDatFloat[0] *= BalanceR;
			}
			for (volChannel = 1; volChannel <= _channelCount; volChannel += 2)
			{
				XearSDK.SetProperty(_endpointUID, "VolumeLevelScalar", new XearSDK.ChannelVolume
				{
					Channel = volChannel,
					Volume = outDatFloat[0]
				}, out cmErr);
			}
		}
		else if (_balanceValue > 10)
		{
			float BalanceL = 0f;
			if (_balanceValue == 20)
			{
				outDatFloat[0] = 0f;
			}
			else
			{
				BalanceL = (float)(20 - _balanceValue) / 10f;
				outDatFloat[0] *= BalanceL;
			}
			for (volChannel = 0; volChannel <= _channelCount - 1; volChannel += 2)
			{
				XearSDK.SetProperty(_endpointUID, "VolumeLevelScalar", new XearSDK.ChannelVolume
				{
					Channel = volChannel,
					Volume = outDatFloat[0]
				}, out cmErr);
			}
		}
	}

	public void VolMinimizeTriggered(object sender, EventArgs e)
	{
		LogHelper.Debug("", isPrintCaller: false, "VolMinimizeTriggered", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\EndpointControl\\PropertyPanel\\PanelBalance.xaml.cs");
		Slider_Balance.Value = 10.0;
	}
}
