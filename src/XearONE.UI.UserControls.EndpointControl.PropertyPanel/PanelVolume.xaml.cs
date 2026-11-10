using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelVolume : UserControl, IComponentConnector
{
	private Guid _endpointUID;

	public event EventHandler VolMinimizeTriggered;

	public void ShowGamingBorder(bool show)
	{
		Border_IsCMDevice.Visibility = ((!show) ? Visibility.Hidden : Visibility.Visible);
	}

	public PanelVolume(Guid endpointUID)
	{
		InitializeComponent();
		_endpointUID = endpointUID;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		UpdateUI();
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	public void UpdateUI()
	{
		Console.WriteLine("RefreshMasterVolume(): {0}", _endpointUID);
		if (XearSDK.GetProperty(_endpointUID, "VolumeLevelScalar", out float[] outDatFloat, out XearSDK.CmErr _, -1))
		{
			if (CheckBox_UnitChange.IsChecked == true)
			{
				if (XearSDK.GetProperty(_endpointUID, "VolumeLevel", out float[] outDatFloat2, out XearSDK.CmErr _, -1))
				{
					Dispatcher.BeginInvoke((Action)(() =>
					{
						Slider_MasterVolume.Value = outDatFloat[0] * 100f;
						Label_VolumeLevel.Content = $"{Math.Round(outDatFloat2[0], 1):F1}";
					}));
				}
				else
				{
					Console.WriteLine("Get VolumeLevel Failed.");
				}
			}
			else
			{
				Dispatcher.BeginInvoke((Action)(() =>
				{
					Slider_MasterVolume.Value = outDatFloat[0] * 100f;
					Label_VolumeLevel.Content = Convert.ToInt32(Slider_MasterVolume.Value).ToString();
				}));
			}
		}
		else
		{
			Console.WriteLine("Get VolumeLevel Failed.");
		}
	}

	private void CheckBox_UnitChange_Click(object sender, RoutedEventArgs e)
	{
		if ((sender as CheckBox).IsChecked == true)
		{
			if (XearSDK.GetProperty(_endpointUID, "VolumeLevel", out float[] outDatFloat, out XearSDK.CmErr _, -1))
			{
				Dispatcher.BeginInvoke((Action)(() =>
				{
					Label_Percentage.Content = "dB";
					Label_VolumeLevel.Content = $"{Math.Round(outDatFloat[0], 1):F1}";
					Label_VolumeLevel.FontSize = 16.0;
				}));
			}
		}
		else
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				Label_Percentage.Content = "%";
				Label_VolumeLevel.Content = Convert.ToInt32(Slider_MasterVolume.Value).ToString();
				Label_VolumeLevel.FontSize = 22.0;
			}));
		}
	}

	private void Slider_MasterVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		SetVolume(-1, (float)e.NewValue);
		if ((double)(float)e.NewValue < 0.01)
		{
			VolMinimizeTriggered?.Invoke(this, EventArgs.Empty);
		}
	}

	private void SetVolume(int channel, float VolumeScalar)
	{
		XearSDK.SetProperty(_endpointUID, "VolumeLevelScalar", new XearSDK.ChannelVolume
		{
			Channel = -1,
			Volume = VolumeScalar / 100f
		}, out var cmErr);
		if (CheckBox_UnitChange.IsChecked == true)
		{
			if (XearSDK.GetProperty(_endpointUID, "VolumeLevel", out float[] outDatFloat, out cmErr, -1))
			{
				Console.WriteLine($"dB value: {outDatFloat[0]}");
				Dispatcher.BeginInvoke((Action)(() =>
				{
					Label_VolumeLevel.Content = $"{Math.Round(outDatFloat[0], 1):F1}";
				}));
			}
		}
		else
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				Label_VolumeLevel.Content = Convert.ToInt32(Slider_MasterVolume.Value).ToString();
			}));
		}
	}
}
