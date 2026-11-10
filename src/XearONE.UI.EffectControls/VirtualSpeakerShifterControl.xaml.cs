using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Cmedia;
using XearONE.UI.Driver;

namespace XearONE.UI.EffectControls;

public partial class VirtualSpeakerShifterControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private enum DragMode
	{
		Clockwise,
		CounterClockwise,
		ManualClockwise,
		Manual
	}

	private Guid _audEndptUID;

	private const string propertyName_VSS_Enable = "Enable_SS_LFX";

	private DragMode dragMode = DragMode.Manual;

	private const int maxGain = 4;

	private const int minGain = -13;

	private Timer spinTimer;

	private const double CircleCenterX = 140.0;

	private const double CircleCenterY = 140.0;

	private const double OuterCircleRadius = 140.0;

	private const double InnerCircleRadius = 80.0;

	private double _outerCircleRadius;

	private double _innerCircleRadius;

	private bool isMouseDown;

	private int[] speakerAngle = new int[8];

	private const int spinInterval = 200;

	private const int spinStepAngle = 2;

	public List<string> propertyNames { get; set; } = new List<string>();

	public VirtualSpeakerShifterControl(Guid audEndptUID)
	{
		InitializeComponent();
		InitPropertyNames();
		_audEndptUID = audEndptUID;
		Button_Speaker_L.audEndptUID = _audEndptUID;
		Button_Speaker_L.Slider_gain = Slider_Gain_L;
		Button_Speaker_L.Label_gain = Label_Gain_L;
		Button_Speaker_R.audEndptUID = _audEndptUID;
		Button_Speaker_R.Slider_gain = Slider_Gain_R;
		Button_Speaker_R.Label_gain = Label_Gain_R;
		Button_Speaker_LS.audEndptUID = _audEndptUID;
		Button_Speaker_LS.Slider_gain = Slider_Gain_LS;
		Button_Speaker_LS.Label_gain = Label_Gain_LS;
		Button_Speaker_RS.audEndptUID = _audEndptUID;
		Button_Speaker_RS.Slider_gain = Slider_Gain_RS;
		Button_Speaker_RS.Label_gain = Label_Gain_RS;
		Button_Speaker_LB.audEndptUID = _audEndptUID;
		Button_Speaker_LB.Slider_gain = Slider_Gain_LB;
		Button_Speaker_LB.Label_gain = Label_Gain_LB;
		Button_Speaker_RB.audEndptUID = _audEndptUID;
		Button_Speaker_RB.Slider_gain = Slider_Gain_RB;
		Button_Speaker_RB.Label_gain = Label_Gain_RB;
		Button_Speaker_C.audEndptUID = _audEndptUID;
		Button_Speaker_C.Slider_gain = Slider_Gain_C;
		Button_Speaker_C.Label_gain = Label_Gain_C;
		Button_Speaker_W.audEndptUID = _audEndptUID;
		Button_Speaker_W.Slider_gain = Slider_Gain_W;
		Button_Speaker_W.Label_gain = Label_Gain_W;
	}

	private void VirtualSpeakerShifterControl_Loaded(object sender, RoutedEventArgs e)
	{
		RefreshAllControls();
	}

	private void VirtualSpeakerShifterControl_Unloaded(object sender, RoutedEventArgs e)
	{
		SetSpinTimer(monitor: false);
	}

	public void RefreshAllControls()
	{
		int enable = 0;
		if (XearSDK.GetProperty(_audEndptUID, "Enable_SS_LFX", out enable, out XearSDK.CmErr _))
		{
			Btn_Enable.IsChecked = enable == 1;
		}
		switch (dragMode)
		{
		case DragMode.Clockwise:
			RadioButton_Clockwise.IsChecked = true;
			break;
		case DragMode.CounterClockwise:
			RadioButton_CounterClockwise.IsChecked = true;
			break;
		case DragMode.ManualClockwise:
			RadioButton_ManualClockwise.IsChecked = true;
			break;
		case DragMode.Manual:
			RadioButton_Manual.IsChecked = true;
			break;
		}
		Button_Speaker_L.OnLoad();
		Button_Speaker_R.OnLoad();
		Button_Speaker_LS.OnLoad();
		Button_Speaker_RS.OnLoad();
		Button_Speaker_LB.OnLoad();
		Button_Speaker_RB.OnLoad();
		Button_Speaker_C.OnLoad();
		Button_Speaker_W.OnLoad();
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_SS_LFX");
		propertyNames.Add("SS_LF_ANGLE");
		propertyNames.Add("SS_RF_ANGLE");
		propertyNames.Add("SS_LS_ANGLE");
		propertyNames.Add("SS_RS_ANGLE");
		propertyNames.Add("SS_LB_ANGLE");
		propertyNames.Add("SS_RB_ANGLE");
		propertyNames.Add("SS_C_ANGLE");
		propertyNames.Add("SS_LFE_ANGLE");
		propertyNames.Add("SS_LF_DIST");
		propertyNames.Add("SS_RF_DIST");
		propertyNames.Add("SS_LS_DIST");
		propertyNames.Add("SS_RS_DIST");
		propertyNames.Add("SS_LB_DIST");
		propertyNames.Add("SS_RB_DIST");
		propertyNames.Add("SS_C_DIST");
		propertyNames.Add("SS_LFE_DIST");
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		int enable = ((Btn_Enable.IsChecked == true) ? 1 : 0);
		XearSDK.SetProperty(_audEndptUID, "Enable_SS_LFX", enable, out var _);
		if (enable == 1)
		{
			if (dragMode == DragMode.Clockwise || dragMode == DragMode.CounterClockwise)
			{
				SetSpinTimer(monitor: true);
			}
		}
		else if (dragMode == DragMode.Clockwise || dragMode == DragMode.CounterClockwise)
		{
			SetSpinTimer(monitor: false);
		}
		DriverInstance.getInstance().SetHIDStatus(_audEndptUID, "Enable_SS_LFX", (byte)enable);
	}

	private void Button_Reset_Click(object sender, RoutedEventArgs e)
	{
		Button_Speaker_L.OnDefault();
		Button_Speaker_R.OnDefault();
		Button_Speaker_LS.OnDefault();
		Button_Speaker_RS.OnDefault();
		Button_Speaker_LB.OnDefault();
		Button_Speaker_RB.OnDefault();
		Button_Speaker_C.OnDefault();
		Button_Speaker_W.OnDefault();
		Button_DragMode_Click(RadioButton_Manual, null);
		RadioButton_Manual.IsChecked = true;
	}

	private void Button_DragMode_Click(object sender, RoutedEventArgs e)
	{
		int mode = Convert.ToInt32((sender as RadioButton).Tag.ToString());
		dragMode = (DragMode)mode;
		Console.WriteLine("dragMode: {0}", dragMode.ToString());
		if (dragMode == DragMode.Clockwise || dragMode == DragMode.CounterClockwise)
		{
			SetSpinTimer(monitor: true);
		}
		else
		{
			SetSpinTimer(monitor: false);
		}
	}

	private void Button_Gain_Click(object sender, RoutedEventArgs e)
	{
		if (Convert.ToInt32((sender as Button).Tag.ToString()) == 1)
		{
			int gain = Button_Speaker_L.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_L.Gain = gain + 1);
			}
			gain = Button_Speaker_R.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_R.Gain = gain + 1);
			}
			gain = Button_Speaker_LS.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_LS.Gain = gain + 1);
			}
			gain = Button_Speaker_RS.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_RS.Gain = gain + 1);
			}
			gain = Button_Speaker_LB.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_LB.Gain = gain + 1);
			}
			gain = Button_Speaker_RB.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_RB.Gain = gain + 1);
			}
			gain = Button_Speaker_C.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_C.Gain = gain + 1);
			}
			gain = Button_Speaker_W.Gain;
			if (gain < 4)
			{
				gain = (Button_Speaker_W.Gain = gain + 1);
			}
		}
		else
		{
			int gain = Button_Speaker_L.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_L.Gain = gain - 1);
			}
			gain = Button_Speaker_R.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_R.Gain = gain - 1);
			}
			gain = Button_Speaker_LS.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_LS.Gain = gain - 1);
			}
			gain = Button_Speaker_RS.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_RS.Gain = gain - 1);
			}
			gain = Button_Speaker_LB.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_LB.Gain = gain - 1);
			}
			gain = Button_Speaker_RB.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_RB.Gain = gain - 1);
			}
			gain = Button_Speaker_C.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_C.Gain = gain - 1);
			}
			gain = Button_Speaker_W.Gain;
			if (gain > -13)
			{
				gain = (Button_Speaker_W.Gain = gain - 1);
			}
		}
	}

	private void Speaker_Move(object sender, MouseEventArgs e)
	{
		if (!isMouseDown)
		{
			return;
		}
		SpeakerButton btn = sender as SpeakerButton;
		int spkNum = Convert.ToInt32(btn.Tag.ToString());
		double newX = Canvas.GetLeft(btn) + e.GetPosition(btn).X;
		double newY = Canvas.GetTop(btn) + e.GetPosition(btn).Y;
		double dx = newX - 140.0;
		double dy = newY - 140.0;
		double distance = Math.Sqrt(dx * dx + dy * dy);
		if (distance > _outerCircleRadius)
		{
			double scale = _outerCircleRadius / distance;
			newX = 140.0 + dx * scale;
			newY = 140.0 + dy * scale;
		}
		else if (distance < _innerCircleRadius)
		{
			if (distance == 0.0)
			{
				dx = 1.0;
				dy = 0.0;
				distance = 1.0;
			}
			double scale2 = _innerCircleRadius / distance;
			newX = 140.0 + dx * scale2;
			newY = 140.0 + dy * scale2;
		}
		btn.Position = new Point(newX, newY);
		if (dragMode == DragMode.ManualClockwise)
		{
			int deltaAngle = speakerAngle[spkNum] - btn.Angle;
			DeltaAllSpkAngle(deltaAngle);
		}
	}

	private void Speaker_ButtonDown(object sender, MouseButtonEventArgs e)
	{
		Button btn = sender as Button;
		btn.CaptureMouse();
		if (dragMode != DragMode.Clockwise && dragMode != DragMode.CounterClockwise)
		{
			if (dragMode == DragMode.ManualClockwise)
			{
				double Left = Canvas.GetLeft(btn) + btn.ActualWidth / 2.0;
				double num = Canvas.GetTop(btn) + btn.ActualHeight / 2.0;
				double x = Left - 140.0;
				double y = num - 140.0;
				double circleDist = Math.Sqrt(x * x + y * y);
				_outerCircleRadius = circleDist + 0.5;
				_innerCircleRadius = circleDist - 0.5;
				SaveAllSpkAngle();
			}
			else if (dragMode == DragMode.Manual)
			{
				_outerCircleRadius = 140.0;
				_innerCircleRadius = 80.0;
			}
			Panel.SetZIndex(btn, 1);
			isMouseDown = true;
		}
	}

	private void Speaker_ButtonUp(object sender, MouseButtonEventArgs e)
	{
		isMouseDown = false;
		Button obj = sender as Button;
		obj.ReleaseMouseCapture();
		Panel.SetZIndex(obj, 0);
	}

	private void SaveAllSpkAngle()
	{
		speakerAngle[0] = Button_Speaker_L.Angle;
		speakerAngle[1] = Button_Speaker_R.Angle;
		speakerAngle[2] = Button_Speaker_LS.Angle;
		speakerAngle[3] = Button_Speaker_RS.Angle;
		speakerAngle[4] = Button_Speaker_LB.Angle;
		speakerAngle[5] = Button_Speaker_RB.Angle;
		speakerAngle[6] = Button_Speaker_C.Angle;
		speakerAngle[7] = Button_Speaker_W.Angle;
	}

	private void DeltaAllSpkAngle(int deltaAngle)
	{
		Button_Speaker_L.Angle = speakerAngle[0] - deltaAngle;
		Button_Speaker_R.Angle = speakerAngle[1] - deltaAngle;
		Button_Speaker_LS.Angle = speakerAngle[2] - deltaAngle;
		Button_Speaker_RS.Angle = speakerAngle[3] - deltaAngle;
		Button_Speaker_LB.Angle = speakerAngle[4] - deltaAngle;
		Button_Speaker_RB.Angle = speakerAngle[5] - deltaAngle;
		Button_Speaker_C.Angle = speakerAngle[6] - deltaAngle;
		Button_Speaker_W.Angle = speakerAngle[7] - deltaAngle;
	}

	private void SetSpinTimer(bool monitor, int updateInterval = 200)
	{
		spinTimer?.Change(-1, -1);
		spinTimer = null;
		if (monitor)
		{
			spinTimer = new Timer((object obj) =>
			{
				OnSpin();
			}, this, 0, updateInterval);
		}
	}

	private void OnSpin()
	{
		if (dragMode == DragMode.Clockwise)
		{
			int angle = Button_Speaker_L.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_L.Angle = angle;
			angle = Button_Speaker_R.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_R.Angle = angle;
			angle = Button_Speaker_LS.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_LS.Angle = angle;
			angle = Button_Speaker_RS.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_RS.Angle = angle;
			angle = Button_Speaker_LB.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_LB.Angle = angle;
			angle = Button_Speaker_RB.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_RB.Angle = angle;
			angle = Button_Speaker_C.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_C.Angle = angle;
			angle = Button_Speaker_W.Angle;
			angle += 2;
			if (angle > 180)
			{
				angle -= 360;
			}
			Button_Speaker_W.Angle = angle;
		}
		else if (dragMode == DragMode.CounterClockwise)
		{
			int angle = Button_Speaker_L.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_L.Angle = angle;
			angle = Button_Speaker_R.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_R.Angle = angle;
			angle = Button_Speaker_LS.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_LS.Angle = angle;
			angle = Button_Speaker_RS.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_RS.Angle = angle;
			angle = Button_Speaker_LB.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_LB.Angle = angle;
			angle = Button_Speaker_RB.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_RB.Angle = angle;
			angle = Button_Speaker_C.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_C.Angle = angle;
			angle = Button_Speaker_W.Angle;
			angle -= 2;
			if (angle < -180)
			{
				angle += 360;
			}
			Button_Speaker_W.Angle = angle;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}
}
