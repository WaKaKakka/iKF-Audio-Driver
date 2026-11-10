using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.Driver.DeviceInfo;

namespace XearONE.UI.ControlWindow.Content;

public partial class JackSetting : UserControl, WindowConstantDefinition.IUsrCtrlProperty, IComponentConnector
{
	private Guid AudioEndpointUID;

	private static readonly int[] deviceFormatChannels = new int[5] { 1, 2, 4, 6, 8 };

	private static readonly int[] deviceFormatBitDepths = new int[3] { 16, 24, 32 };

	private static readonly int[] deviceFormatSampleRates = new int[12]
	{
		8000, 11025, 16000, 22050, 44100, 48000, 88200, 96000, 176400, 192000,
		352800, 384000
	};

	private List<Tuple<string, int, int, int>> FormatTuples = new List<Tuple<string, int, int, int>>();

	private const string propertyName_DefaultDevice = "DefaultDevice";

	private const string propertyName_IsFormatSupported = "IsFormatSupported";

	private const string propertyName_DefaultFormat = "DefaultFormat";

	public JackSetting(JackDeviceInfo devInfo)
	{
		InitializeComponent();
		Name = "JackSetting";
		Label_DeviceName.Content = devInfo.controllerDisplayName;
		Label_FirmwareVer.Content = devInfo.usbRevisionNumber;
		Label_DriverVer.Content = devInfo.driverVersion;
		AudioEndpointUID = devInfo.audEndptUID;
	}

	void WindowConstantDefinition.IUsrCtrlProperty.UpdateUI()
	{
		Console.WriteLine("{0}: UpdateUI()", GetType().ToString());
		Dispatcher.Invoke(() =>
		{
			UpdateUI();
		});
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshSupportedDeviceFormat();
		UpdateUI();
	}

	private void UpdateUI()
	{
		RefreshDefaultDevice();
		if (Combo_Format.Items.Count != 0)
		{
			RefreshCurrentDeviceFormat();
		}
	}

	private void RefreshDefaultDevice()
	{
		XearSDK.CmDefaultRole[] inDefaults = new XearSDK.CmDefaultRole[2]
		{
			XearSDK.CmDefaultRole.CDR_Multimedia,
			XearSDK.CmDefaultRole.CDR_Communication
		};
		if (XearSDK.GetProperty(AudioEndpointUID, "DefaultDevice", out var outDefaults, out var _, inDefaults))
		{
			RadioButton_DefaultDevice.IsChecked = outDefaults[0] == 1;
			RadioButton_DefaultCommunicationDevice.IsChecked = outDefaults[1] == 1;
		}
	}

	private void RadioButton_DefaultDevice_Click(object sender, RoutedEventArgs e)
	{
		XearSDK.CmDefaultRole cmDefaultRole = (XearSDK.CmDefaultRole)Convert.ToInt32((sender as RadioButton).Tag);
		if (XearSDK.GetProperty(AudioEndpointUID, "DefaultDevice", out var outDefaults, out var cmErr, cmDefaultRole) && outDefaults[0] == 1)
		{
			Console.WriteLine("Default device already, Return.");
		}
		else
		{
			XearSDK.SetProperty(AudioEndpointUID, "DefaultDevice", cmDefaultRole, out cmErr);
		}
	}

	private void Combo_Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Console.WriteLine("Combo_Format_SelectionChanged");
		if (XearSDK.GetProperty(AudioEndpointUID, "DefaultFormat", out XearSDK.AudioFormat outDat, out XearSDK.CmErr _))
		{
			if (string.Compare(Combo_Format.SelectedItem.ToString(), $"{outDat.BitDepth} bit, {outDat.SampleRate} Hz") != 0)
			{
				SetFormat(Combo_Format.SelectedItem.ToString());
			}
			else
			{
				Console.WriteLine("Format same, Don't set it.");
			}
		}
	}

	private void RefreshSupportedDeviceFormat()
	{
		Combo_Format.Items.Clear();
		FormatTuples.Clear();
		int[] array = deviceFormatChannels;
		foreach (int channel in array)
		{
			int[] array2 = deviceFormatBitDepths;
			foreach (int bitDepth in array2)
			{
				int[] array3 = deviceFormatSampleRates;
				foreach (int sampleRate in array3)
				{
					XearSDK.AudioFormat audioFormat = new XearSDK.AudioFormat
					{
						Channels = channel,
						BitDepth = bitDepth,
						SampleRate = sampleRate
					};
					if (XearSDK.GetProperty(AudioEndpointUID, "IsFormatSupported", out var outDats, out var _, audioFormat) && outDats[0] != 0)
					{
						string DeviceFormat = $"{audioFormat.BitDepth} bit, {audioFormat.SampleRate} Hz";
						Combo_Format.Items.Add(DeviceFormat);
						DeviceFormat = $"{audioFormat.Channels} channel, {audioFormat.BitDepth} bit, {audioFormat.SampleRate} Hz";
						FormatTuples.Add(new Tuple<string, int, int, int>(DeviceFormat, audioFormat.Channels, audioFormat.BitDepth, audioFormat.SampleRate));
					}
				}
			}
		}
	}

	public void RefreshCurrentDeviceFormat()
	{
		if (!XearSDK.GetProperty(AudioEndpointUID, "DefaultFormat", out XearSDK.AudioFormat outDat, out XearSDK.CmErr _))
		{
			return;
		}
		foreach (object i in (IEnumerable)Combo_Format.Items)
		{
			if (string.Compare(i.ToString(), $"{outDat.BitDepth} bit, {outDat.SampleRate} Hz") == 0)
			{
				Dispatcher.BeginInvoke((Action)(() =>
				{
					Combo_Format.SelectedItem = i;
				}));
				break;
			}
		}
	}

	private void SetFormat(string SelDeviceFormat)
	{
		foreach (Tuple<string, int, int, int> TupleIndex in FormatTuples)
		{
			if (TupleIndex.Item1.Contains(SelDeviceFormat))
			{
				XearSDK.AudioFormat audioFormat = new XearSDK.AudioFormat
				{
					Channels = TupleIndex.Item2,
					BitDepth = TupleIndex.Item3,
					SampleRate = TupleIndex.Item4
				};
				XearSDK.SetProperty(AudioEndpointUID, "DefaultFormat", audioFormat, out var _);
				break;
			}
		}
	}
}
