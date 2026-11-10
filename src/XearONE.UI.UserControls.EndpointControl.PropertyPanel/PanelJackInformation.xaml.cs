using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Cmedia;
using XearONE.UI.Driver.DeviceInfo;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelJackInformation : UserControl, IComponentConnector
{
	private JackDeviceInfo _devInfo;

	public string displayName { get; set; }

	public string controllerDisplayName { get; set; }

	public PanelJackInformation(JackDeviceInfo devInfo)
	{
		InitializeComponent();
		DataContext = this;
		_devInfo = devInfo;
		displayName = _devInfo.displayName;
		controllerDisplayName = _devInfo.controllerDisplayName;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		InitJackIcon();
		UpdateUI();
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	public void UpdateUI()
	{
		RefreshDefaultDeviceIcon();
	}

	private void InitJackIcon()
	{
		if (_devInfo.jackType != XearSDK.CmJackType.CJT_Unknown)
		{
			switch (_devInfo.jackType)
			{
			case XearSDK.CmJackType.CJT_Speaker:
			case XearSDK.CmJackType.CJT_SpeakerQuarter:
			case XearSDK.CmJackType.CJT_DesktopSpeaker:
			case XearSDK.CmJackType.CJT_EchoSuppressingSpeakerphone:
			case XearSDK.CmJackType.CJT_SpeakerPhone:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackSpeaker");
				break;
			case XearSDK.CmJackType.CJT_Headphone:
			case XearSDK.CmJackType.CJT_FrontHeadphone:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackHeadphone");
				break;
			case XearSDK.CmJackType.CJT_Headset:
			case XearSDK.CmJackType.CJT_CommunicationSpeaker:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackHeadset");
				break;
			case XearSDK.CmJackType.CJT_Microphone:
			case XearSDK.CmJackType.CJT_DesktopMicrophone:
			case XearSDK.CmJackType.CJT_FrontMic:
			case XearSDK.CmJackType.CJT_MicrophoneArray:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackMicrophone");
				break;
			case XearSDK.CmJackType.CJT_SPDIF:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackDigital");
				break;
			case XearSDK.CmJackType.CJT_Line:
			case XearSDK.CmJackType.CJT_AUX:
			case XearSDK.CmJackType.CJT_SPDIFInRCA:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackLineIn");
				break;
			case XearSDK.CmJackType.CJT_HDMI:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackHDMI");
				break;
			case XearSDK.CmJackType.CJT_CDPlayer:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_CDPlayer");
				break;
			case XearSDK.CmJackType.CJT_StereoMix:
				Img_JackIcon.Source = (ImageSource)FindResource("Img_JackStereoMix");
				break;
			}
		}
		else if (_devInfo.dataFlow == XearSDK.CmDataFlow.CDF_Capture)
		{
			Img_JackIcon.Source = (ImageSource)FindResource("Img_JackMicrophone");
		}
		else
		{
			Img_JackIcon.Source = (ImageSource)FindResource("Img_JackSpeaker");
		}
		Img_JackIcon.ToolTip = FindResource($"ToolTip_Icon_{_devInfo.jackType.ToString()}");
	}

	public void RefreshDefaultDeviceIcon()
	{
		int TypeOfDefault = 0;
		if (XearSDK.GetProperty(_devInfo.audEndptUID, "DefaultDevice", out var outDats, out var _, XearSDK.CmDefaultRole.CDR_Multimedia, XearSDK.CmDefaultRole.CDR_Communication))
		{
			if (outDats[1] == 1)
			{
				TypeOfDefault = 2;
			}
			if (outDats[0] == 1)
			{
				TypeOfDefault = 1;
			}
		}
		Dispatcher.Invoke(() =>
		{
			if (TypeOfDefault == 0)
			{
				Img_DefaultDevice.Visibility = Visibility.Hidden;
			}
			else if (TypeOfDefault == 1)
			{
				Img_DefaultDevice.Visibility = Visibility.Visible;
				Img_DefaultDevice.Source = (ImageSource)FindResource("Img_DefaultDevice");
			}
			else if (TypeOfDefault == 2)
			{
				Img_DefaultDevice.Visibility = Visibility.Visible;
				Img_DefaultDevice.Source = (ImageSource)FindResource("Img_DefaultCommunicationDevice");
			}
		});
	}

	private void Label_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		Label label = sender as Label;
		if (label.Parent is Canvas canvas)
		{
			UIEffect.MarqueeAnimation(label.ActualWidth > canvas.Width, label);
		}
	}
}
