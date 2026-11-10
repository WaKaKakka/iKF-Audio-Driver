using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Cmedia;

namespace XearONE.UI.EffectControls.MagicVoice2;

public partial class PitchEstimation : Window, IComponentConnector
{
	private Guid AudioEndpointUID;

	private Stopwatch swRecordingTime = new Stopwatch();

	private Timer timerPitchProgress;

	private Timer timerPitchComputing;

	private const int AudioHistoryDataCount = 20;

	private double[] audioData = new double[20];

	private bool isPitchEstSuccess;

	private MagicVoice2Handler magicVoice2Handler;

	public bool IsPitchEstSuccess()
	{
		return isPitchEstSuccess;
	}

	public PitchEstimation(MagicVoice2Handler handler, Guid audioEndpointUID)
	{
		PitchEstimation pitchEstimation = this;
		InitializeComponent();
		magicVoice2Handler = handler;
		timerPitchProgress = new Timer(100.0);
		timerPitchProgress.Elapsed += OnTimedEventPitchProgress;
		timerPitchProgress.AutoReset = true;
		timerPitchProgress.Enabled = false;
		timerPitchComputing = new Timer(1000.0);
		timerPitchComputing.Elapsed += OnTimedEventPitchComputing;
		timerPitchComputing.AutoReset = true;
		timerPitchComputing.Enabled = false;
		AudioEndpointUID = audioEndpointUID;
		if (XearSDK.GetProperty(AudioEndpointUID, "VolumeLevelScalar", out float[] outDatFloat, out XearSDK.CmErr _, -1))
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				pitchEstimation.slMasterVolume.Value = Math.Round(outDatFloat[0] * 100f);
			}));
		}
	}

	private void DrawWaveform()
	{
		if (audioData != null && audioData.Length != 0)
		{
			double actualWidth = waveformCanvas.ActualWidth;
			double canvasHeight = waveformCanvas.ActualHeight;
			double cellWidth = actualWidth / (double)audioData.Length;
			Polyline waveformLine = new Polyline();
			waveformLine.Stroke = Brushes.Blue;
			waveformLine.StrokeThickness = 1.0;
			for (int i = 0; i < audioData.Length; i++)
			{
				double x = (double)i * cellWidth + cellWidth / 2.0;
				double y = canvasHeight / 2.0 + audioData[i] * (canvasHeight / 2.0);
				waveformLine.Points.Add(new Point(x, y));
			}
			waveformCanvas.Children.Clear();
			waveformCanvas.Children.Add(waveformLine);
		}
	}

	private void statusUpdate(string updateMsg)
	{
		if (updateMsg.Contains("[NoLine]"))
		{
			return;
		}
		if (updateMsg.Contains("[AudioData]"))
		{
			string[] words = updateMsg.Split(' ');
			if (words.Count() >= 3)
			{
				double hiVal = Convert.ToDouble(words[1]);
				double loVal = Convert.ToDouble(words[2]);
				for (int i = 0; i < audioData.Length - 2; i++)
				{
					audioData[i] = audioData[i + 2];
				}
				audioData[audioData.Length - 2] = hiVal;
				audioData[audioData.Length - 1] = loVal;
				DrawWaveform();
			}
		}
		else
		{
			if (!updateMsg.Contains("[PitchEstimationResult]"))
			{
				return;
			}
			string[] words2 = updateMsg.Split(' ');
			if (words2.Count() < 3)
			{
				return;
			}
			switch (Convert.ToInt32(words2[1]))
			{
			case 0:
			{
				isPitchEstSuccess = true;
				double pitch = 0.0;
				try
				{
					pitch = Convert.ToDouble(words2[2]);
				}
				catch
				{
				}
				if (pitch > 16.0)
				{
					pitch = 16.0;
				}
				if (pitch < -16.0)
				{
					pitch = -16.0;
				}
				txtPitchResult.Text = pitch.ToString("0.#");
				txtPitchResult.Foreground = new SolidColorBrush(Colors.Green);
				break;
			}
			case 1:
				isPitchEstSuccess = false;
				txtPitchResult.Text = "NG";
				txtPitchResult.Foreground = new SolidColorBrush(Colors.Blue);
				break;
			default:
				isPitchEstSuccess = false;
				txtPitchResult.Text = "Fail";
				txtPitchResult.Foreground = new SolidColorBrush(Colors.Red);
				break;
			}
		}
	}

	private void OnTimedEventPitchProgress(object source, ElapsedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(() =>
		{
			if (swRecordingTime.ElapsedMilliseconds >= 10000)
			{
				progressRecordingTime.Value = 100.0;
				txtProgress.Text = "100%";
				timerPitchProgress.Enabled = false;
				Stop_Click(null, null);
			}
			else
			{
				double value = (double)swRecordingTime.ElapsedMilliseconds / 10000.0 * 100.0;
				progressRecordingTime.Value = value;
				txtProgress.Text = value.ToString("##") + "%";
			}
		});
	}

	private void OnTimedEventPitchComputing(object source, ElapsedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(() =>
		{
			txtPitchResult.Text += ".";
		});
	}

	private async void Next_Click(object sender, RoutedEventArgs e)
	{
		if (magicVoice2Handler.IsPitchEstimationDone())
		{
			Close();
			return;
		}
		btnNext.IsEnabled = false;
		Btn_Close.IsEnabled = false;
		timerPitchProgress.Enabled = true;
		swRecordingTime.Restart();
		await magicVoice2Handler.StartPitchEstimation(new Progress<string>(statusUpdate));
		if (magicVoice2Handler.IsPitchEstimationDone())
		{
			btnNext.Content = FindResource("MagicVoiceII_PersonalizedVoiceSetup_Exit");
		}
		btnNext.IsEnabled = true;
		Btn_Close.IsEnabled = true;
	}

	private async void Stop_Click(object sender, RoutedEventArgs e)
	{
		timerPitchProgress.Enabled = false;
		if (!btnNext.IsEnabled)
		{
			txtPitchResult.Text = ".";
			timerPitchComputing.Enabled = true;
			await Task.Run(() =>
			{
				magicVoice2Handler.StopPitchEstimation();
			});
			timerPitchComputing.Enabled = false;
		}
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		if (!timerPitchComputing.Enabled)
		{
			Close();
		}
	}

	private void Grid_Title_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		float VolumeScalar = (float)e.NewValue;
		XearSDK.SetProperty(AudioEndpointUID, "VolumeLevelScalar", new XearSDK.ChannelVolume
		{
			Channel = -1,
			Volume = VolumeScalar / 100f
		}, out var _);
	}
}
