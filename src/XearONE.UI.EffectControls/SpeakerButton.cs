using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cmedia;

namespace XearONE.UI.EffectControls;

public class SpeakerButton : Button
{
	private string[] propertyName_SpkAngle = new string[8] { "SS_LF_ANGLE", "SS_RF_ANGLE", "SS_LS_ANGLE", "SS_RS_ANGLE", "SS_LB_ANGLE", "SS_RB_ANGLE", "SS_C_ANGLE", "SS_LFE_ANGLE" };

	private string[] propertyName_SpkDist = new string[8] { "SS_LF_DIST", "SS_RF_DIST", "SS_LS_DIST", "SS_RS_DIST", "SS_LB_DIST", "SS_RB_DIST", "SS_C_DIST", "SS_LFE_DIST" };

	private int[] DefaultSpkAngle = new int[8] { -45, 45, -90, 90, -135, 135, 0, 15 };

	private double[] RotateOffset = new double[8] { 135.0, 45.0, 180.0, 0.0, -135.0, -45.0, 90.0, 75.0 };

	private Dictionary<int, int> dicGainDistance = new Dictionary<int, int>
	{
		{ -13, 50 },
		{ -12, 47 },
		{ -11, 44 },
		{ -10, 42 },
		{ -9, 40 },
		{ -8, 37 },
		{ -7, 35 },
		{ -6, 33 },
		{ -5, 31 },
		{ -4, 30 },
		{ -3, 28 },
		{ -2, 26 },
		{ -1, 25 },
		{ 0, 24 },
		{ 1, 22 },
		{ 2, 21 },
		{ 3, 20 },
		{ 4, 19 }
	};

	private Guid _audEndptUID;

	private Slider _slider_gain;

	private Label _label_gain;

	private Point _position;

	private int _gain = -4;

	private int _angle;

	private const double CircleCenterX = 140.0;

	private const double CircleCenterY = 140.0;

	private const double OuterCircleRadius = 140.0;

	private const double InnerCircleRadius = 80.0;

	private double _outerCircleRadius;

	private double _innerCircleRadius;

	private const int maxGain = 4;

	private const int minGain = -13;

	public Guid audEndptUID
	{
		get
		{
			return _audEndptUID;
		}
		set
		{
			_audEndptUID = value;
		}
	}

	public Slider Slider_gain
	{
		get
		{
			return _slider_gain;
		}
		set
		{
			_slider_gain = value;
		}
	}

	public Label Label_gain
	{
		get
		{
			return _label_gain;
		}
		set
		{
			_label_gain = value;
		}
	}

	public Point Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
			OnPositionChanged();
		}
	}

	public int Gain
	{
		get
		{
			return _gain;
		}
		set
		{
			_gain = value;
			Dispatcher.BeginInvoke((Action)(() =>
			{
				OnGainChanged();
			}));
		}
	}

	public int Angle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = value;
			Dispatcher.BeginInvoke((Action)(() =>
			{
				OnAngleChanged();
			}));
		}
	}

	public void OnLoad()
	{
		int spkNum = Convert.ToInt32(Tag.ToString());
		if (!XearSDK.GetProperty(_audEndptUID, propertyName_SpkAngle[spkNum], out int apoAngle, out XearSDK.CmErr cmErr))
		{
			Angle = DefaultSpkAngle[spkNum] - 90;
		}
		else
		{
			Angle = apoAngle - 180 - 90;
		}
		if (!XearSDK.GetProperty(_audEndptUID, propertyName_SpkDist[spkNum], out int apoGain, out cmErr))
		{
			Gain = -4;
			return;
		}
		int foundGain = 0;
		foreach (KeyValuePair<int, int> pair in dicGainDistance)
		{
			if (pair.Value == apoGain)
			{
				foundGain = pair.Key;
				break;
			}
		}
		Gain = foundGain;
	}

	public void OnDefault()
	{
		int spkNum = Convert.ToInt32(Tag.ToString());
		Angle = DefaultSpkAngle[spkNum] - 90;
		Gain = -4;
	}

	private void SetSpkAngle(int angle)
	{
		int spkNum = Convert.ToInt32(Tag.ToString());
		int apoAngle = angle + 90 + 180;
		if (apoAngle > 360)
		{
			apoAngle -= 360;
		}
		else if (apoAngle < -360)
		{
			apoAngle += 360;
		}
		XearSDK.SetProperty(_audEndptUID, propertyName_SpkAngle[spkNum], apoAngle, out var _);
	}

	private void SetSpkGain(int gain)
	{
		int spkNum = Convert.ToInt32(Tag.ToString());
		if (!dicGainDistance.TryGetValue(gain, out var apoGain))
		{
			apoGain = -4;
		}
		XearSDK.SetProperty(_audEndptUID, propertyName_SpkDist[spkNum], apoGain, out var _);
	}

	public void OnPositionChanged()
	{
		int spkNum = Convert.ToInt32(Tag.ToString());
		double X = _position.X - ActualWidth / 2.0;
		double Y = _position.Y - ActualHeight / 2.0;
		Canvas.SetLeft(this, X);
		Canvas.SetTop(this, Y);
		double dx = X + ActualWidth / 2.0 - 140.0;
		double dy = Y + ActualHeight / 2.0 - 140.0;
		double angleInRadians = Math.Atan2(dy, dx);
		_angle = (int)Math.Round(angleInRadians * 180.0 / Math.PI);
		double rotateAngle = (double)_angle + RotateOffset[spkNum];
		if (Template.FindName("RotateAngle", this) is RotateTransform image)
		{
			image.Angle = rotateAngle;
		}
		double distance = Math.Sqrt(dx * dx + dy * dy);
		_gain = ConvertDistanceToGain(distance);
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_gain.Value = _gain;
			Label_gain.Content = _gain.ToString();
		}));
		Console.WriteLine("OnPositionChanged() Spk: {0}, Angle: {1}, Gain: {2}", Name, _angle, _gain);
		SetSpkAngle(_angle);
		SetSpkGain(_gain);
	}

	public void OnGainChanged()
	{
		Console.WriteLine("OnGainChanged() Spk: {0}, Gain: {1}", Name, _gain);
		Convert.ToInt32(Tag.ToString());
		double distance = ConvertGainToDistance(_gain);
		Point position = GetPosByDistanceDegree(distance, _angle);
		double X = position.X - ActualWidth / 2.0;
		double Y = position.Y - ActualHeight / 2.0;
		Canvas.SetLeft(this, X);
		Canvas.SetTop(this, Y);
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Slider_gain.Value = _gain;
			Label_gain.Content = _gain.ToString();
		}));
		SetSpkGain(_gain);
	}

	public void OnAngleChanged()
	{
		Console.WriteLine("OnAngleChanged() Spk: {0}, Angle: {1}", Name, _angle);
		int spkNum = Convert.ToInt32(Tag.ToString());
		double distance = ConvertGainToDistance(_gain);
		Point position = GetPosByDistanceDegree(distance, _angle);
		double X = position.X - ActualWidth / 2.0;
		double Y = position.Y - ActualHeight / 2.0;
		Canvas.SetLeft(this, X);
		Canvas.SetTop(this, Y);
		double rotateAngle = (double)_angle + RotateOffset[spkNum];
		if (Template.FindName("RotateAngle", this) is RotateTransform image)
		{
			image.Angle = rotateAngle;
		}
		SetSpkAngle(_angle);
	}

	private Point GetPosByDistanceDegree(double distance, double angle)
	{
		double radians = angle * Math.PI / 180.0;
		double x = distance * Math.Cos(radians);
		double y = distance * Math.Sin(radians);
		return new Point(x + 140.0, y + 140.0);
	}

	private int ConvertDistanceToGain(double distance)
	{
		if (distance < 80.0)
		{
			distance = 80.0;
		}
		if (distance > 140.0)
		{
			distance = 140.0;
		}
		double t = (distance - 140.0) / -60.0;
		return (int)Math.Round(-13.0 + t * 17.0);
	}

	private double ConvertGainToDistance(int gain)
	{
		if (gain < -13)
		{
			gain = -13;
		}
		if (gain > 4)
		{
			gain = 4;
		}
		double t = (double)(gain + 13) / 17.0;
		return Math.Round(140.0 + t * -60.0);
	}
}
