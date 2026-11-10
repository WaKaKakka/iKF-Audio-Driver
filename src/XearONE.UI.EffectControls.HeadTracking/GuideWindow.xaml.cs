using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Cmedia;
using XearONE.UI.Driver;

namespace XearONE.UI.EffectControls.HeadTracking;

public partial class GuideWindow : Window, IComponentConnector
{
	private Guid AudioEndpointUID = Guid.Empty;

	private int stepPhase;

	private const int stepPhaseMin = 1;

	private const int stepPhaseMax = 6;

	private HeadTrackingGuideControl htGuide;

	private MediaPlayer mediaPlayer;

	public bool isEnableHeadTrackingPressed { get; set; }

	public GuideWindow(HeadTrackingGuideControl guide, Guid audioEndpointUID)
	{
		InitializeComponent();
		htGuide = guide;
		stepPhase = 1;
		AudioEndpointUID = audioEndpointUID;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		OnPhaseChange(stepPhase);
		htGuide.Visibility = Visibility.Visible;
		int enable = 1;
		XearSDK.GetProperty(AudioEndpointUID, "HeadTracking_Orientation_Enable", out enable, out XearSDK.CmErr _);
		isEnableHeadTrackingPressed = enable == 1;
		CheckStepState();
		InitTestSound();
	}

	private void Window_Unloaded(object sender, RoutedEventArgs e)
	{
		mediaPlayer.Stop();
		mediaPlayer.Close();
	}

	private void Button_Close_Click(object sender, RoutedEventArgs e)
	{
		htGuide.Visibility = Visibility.Hidden;
		Close();
	}

	private void Button_StepNext_Click(object sender, RoutedEventArgs e)
	{
		if (stepPhase <= 6)
		{
			stepPhase++;
		}
		if (stepPhase > 1)
		{
			Button_Prev.IsEnabled = true;
		}
		if (stepPhase == 6)
		{
			Button_Next.Style = (Style)FindResource("Button_StepFinish_Style");
		}
		if (stepPhase == 7)
		{
			Button_Close_Click(null, null);
		}
		OnPhaseChange(stepPhase);
	}

	private void Button_StepPrev_Click(object sender, RoutedEventArgs e)
	{
		if (stepPhase > 1)
		{
			stepPhase--;
		}
		if (stepPhase == 5)
		{
			Button_Next.Style = (Style)FindResource("Button_StepNext_Style");
		}
		if (stepPhase == 1)
		{
			Button_Prev.IsEnabled = false;
		}
		OnPhaseChange(stepPhase);
	}

	private void OnPhaseChange(int step)
	{
		label_Step.Content = step.ToString();
		bool isSwitchVisible = false;
		bool isFrameVisible = false;
		bool isReCenterVisible = false;
		bool isWave = false;
		switch (step)
		{
		case 1:
			Img_Guide.Source = (ImageSource)FindResource("Img_Step01");
			isSwitchVisible = true;
			isFrameVisible = false;
			isReCenterVisible = false;
			isWave = false;
			Button_Next.IsEnabled = false;
			break;
		case 2:
			Img_Guide.Source = (ImageSource)FindResource("Img_Step02");
			isSwitchVisible = false;
			isFrameVisible = false;
			isReCenterVisible = true;
			isWave = false;
			DriverInstance.getInstance().HeadTrackingReset();
			break;
		case 3:
			Img_Guide.Source = (ImageSource)FindResource("Img_Step03");
			isSwitchVisible = false;
			isFrameVisible = true;
			isReCenterVisible = false;
			isWave = false;
			break;
		case 4:
			Img_Guide.Source = (ImageSource)FindResource("Img_Step04");
			Label_Playing.Visibility = Visibility.Hidden;
			isSwitchVisible = false;
			isFrameVisible = true;
			isReCenterVisible = false;
			isWave = false;
			mediaPlayer.Stop();
			break;
		case 5:
			Img_Guide.Source = (ImageSource)FindResource("Img_Step04");
			Label_Playing.Visibility = Visibility.Visible;
			isSwitchVisible = false;
			isFrameVisible = true;
			isReCenterVisible = false;
			isWave = true;
			mediaPlayer.Play();
			break;
		case 6:
			Img_Guide.Source = (ImageSource)FindResource("Img_Step05");
			Label_Playing.Visibility = Visibility.Hidden;
			isSwitchVisible = false;
			isFrameVisible = false;
			isReCenterVisible = false;
			isWave = false;
			mediaPlayer.Stop();
			break;
		}
		CheckStepState();
		SetParameterToGuide(isSwitchVisible, isFrameVisible, isReCenterVisible, isWave);
		if (1 <= step && step <= 6)
		{
			TextBlock_Content.Text = FindResource($"Text_Step_{step.ToString()}").ToString();
		}
	}

	private void SetParameterToGuide(bool isSwitch, bool isFrame, bool isReCenter, bool isWave)
	{
		htGuide.Rect_Switch.Visibility = ((!isSwitch) ? Visibility.Hidden : Visibility.Visible);
		htGuide.Rect_Frame.Visibility = ((!isFrame) ? Visibility.Hidden : Visibility.Visible);
		htGuide.Rect_ReCenter.Visibility = ((!isReCenter) ? Visibility.Hidden : Visibility.Visible);
		htGuide.Img_Wave.Visibility = ((!isWave) ? Visibility.Hidden : Visibility.Visible);
	}

	public void CheckStepState()
	{
		if (stepPhase == 1 && isEnableHeadTrackingPressed)
		{
			Button_Next.IsEnabled = true;
		}
	}

	private void InitTestSound()
	{
		string fullPathName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\Sound\\HeadTracking\\", DriverInstance.getInstance().LoadTestSoundFileName());
		if (File.Exists(fullPathName))
		{
			mediaPlayer = new MediaPlayer();
			mediaPlayer.Open(new Uri(fullPathName, UriKind.RelativeOrAbsolute));
			mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
		}
	}

	private void MediaPlayer_MediaEnded(object sender, EventArgs e)
	{
		mediaPlayer.Position = TimeSpan.Zero;
		mediaPlayer.Play();
	}
}
