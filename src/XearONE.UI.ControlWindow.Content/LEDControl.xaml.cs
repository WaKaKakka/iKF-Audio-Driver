using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XearONE.UI.Driver;
using XearONE.UI.Driver.DeviceInfo;

namespace XearONE.UI.ControlWindow.Content;

public partial class LEDControl : UserControl, WindowConstantDefinition.IUsrCtrlProperty, IComponentConnector
{
	private enum LEDMode
	{
		Static,
		Blink,
		Breathe,
		HeartBeat
	}

	private JackDeviceInfo _devInfo;

	private WriteableBitmap _colorBitmap;

	private WriteableBitmap _valueBitmap;

	private ColorHelper _colorHelper;

	private readonly List<Color> defColorList = new List<Color>
	{
		Color.FromRgb(254, 3, 3),
		Color.FromRgb(byte.MaxValue, 160, 3),
		Color.FromRgb(254, 246, 5),
		Color.FromRgb(3, byte.MaxValue, 4),
		Color.FromRgb(4, 193, byte.MaxValue),
		Color.FromRgb(4, 102, 217),
		Color.FromRgb(253, 59, 232)
	};

	private bool isEnable = true;

	private Color _color = Color.FromRgb(254, 3, 3);

	private LEDMode _mode;

	private int _speed = 50;

	private int arraySize = 128;

	private const int FullArraySize = 128;

	private const int SingleArraySize = 1;

	private double h;

	private double s;

	private double l = 0.5;

	private Timer LEDDebouncer;

	private byte ColorR
	{
		get
		{
			return _color.R;
		}
		set
		{
			_color.R = value;
			OnColorChanged();
			Slider_Color_R.Value = (int)ColorR;
		}
	}

	private byte ColorG
	{
		get
		{
			return _color.G;
		}
		set
		{
			_color.G = value;
			OnColorChanged();
			Slider_Color_G.Value = (int)ColorG;
		}
	}

	private byte ColorB
	{
		get
		{
			return _color.B;
		}
		set
		{
			_color.B = value;
			OnColorChanged();
			Slider_Color_B.Value = (int)ColorB;
		}
	}

	private LEDMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			OnModeChanged();
		}
	}

	private int Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
			OnSpeedChanged();
		}
	}

	public LEDControl(JackDeviceInfo devInfo)
	{
		InitializeComponent();
		Name = "LEDControl";
		_devInfo = devInfo;
		if (_devInfo.string50.Contains("CMA201"))
		{
			ComboBox_LEDMode.Items.Remove(Mode_HeartBeat);
		}
		_colorHelper = new ColorHelper();
	}

	void WindowConstantDefinition.IUsrCtrlProperty.UpdateUI()
	{
		Console.WriteLine("{0}: UpdateUI()", GetType().ToString());
		Dispatcher.Invoke(() =>
		{
			UpdateUI();
		});
	}

	private void UpdateUI()
	{
		RefreshAllControls();
	}

	private void LEDControl_Loaded(object sender, RoutedEventArgs e)
	{
		InitColorCanvas();
		RefreshAllControls();
	}

	private void RefreshAllControls()
	{
		byte colorR = 0;
		byte colorG = 0;
		byte colorB = 0;
		int LEDMode = 0;
		int LEDSpeed = 50;
		DriverInstance.getInstance().LoadLED(_devInfo.CMediaLEDObj, ref isEnable, ref colorR, ref colorG, ref colorB, ref LEDMode, ref LEDSpeed);
		Btn_Enable.IsChecked = isEnable;
		if (isEnable)
		{
			ColorR = colorR;
			ColorG = colorG;
			ColorB = colorB;
		}
		else
		{
			ColorR = 0;
			ColorG = 0;
			ColorB = 0;
		}
		Mode = (LEDMode)LEDMode;
		Speed = LEDSpeed;
		UpdateControlStatus();
	}

	private void Btn_Enable_Click(object sender, RoutedEventArgs e)
	{
		isEnable = Btn_Enable.IsChecked.Value;
		if (isEnable)
		{
			byte colorR = 0;
			byte colorG = 0;
			byte colorB = 0;
			DriverInstance.getInstance().LoadLED(_devInfo.CMediaLEDObj, ref colorR, ref colorG, ref colorB);
			ColorR = colorR;
			ColorG = colorG;
			ColorB = colorB;
		}
		else
		{
			ColorR = 0;
			ColorG = 0;
			ColorB = 0;
		}
		DriverInstance.getInstance().SetLEDStatus(_devInfo.CMediaLEDObj, isEnable);
		DriverInstance.getInstance().SetLEDHIDStatus(_devInfo.CMediaLEDObj, isEnable);
		DriverInstance.getInstance().SaveLED(_devInfo.CMediaLEDObj, isEnable);
	}

	private void ColorCanvas_MouseAction(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed || e.RoutedEvent == Mouse.MouseDownEvent)
		{
			Point p = e.GetPosition(ColorCanvas);
			int x = (int)p.X;
			int y = (int)p.Y;
			if (x >= 0 && x < _colorBitmap.PixelWidth && y >= 0 && y < _colorBitmap.PixelHeight)
			{
				int colorData = _colorHelper.ReadPixelColor(_colorBitmap, x, y);
				l = 0.5;
				ColorR = (byte)((colorData >> 16) & 0xFF);
				ColorG = (byte)((colorData >> 8) & 0xFF);
				ColorB = (byte)(colorData & 0xFF);
			}
		}
	}

	private void ValueCanvas_MouseAction(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed || e.RoutedEvent == Mouse.MouseDownEvent)
		{
			Point p = e.GetPosition(ValueCanvas);
			int y = (int)p.Y;
			if (y >= 0 && y < _valueBitmap.PixelHeight)
			{
				double offset = 1.0 - p.Y / ValueCanvas.ActualHeight;
				l = Math.Max(0.0, Math.Min(1.0, offset));
				Color c = _colorHelper.HslToColor(h, s, l);
				ColorR = c.R;
				ColorG = c.G;
				ColorB = c.B;
			}
		}
	}

	private void Slider_Color_PreviewMouseAction(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && e.RoutedEvent == Mouse.PreviewMouseMoveEvent)
		{
			ColorR = (byte)Convert.ToInt16(Slider_Color_R.Value);
			ColorG = (byte)Convert.ToInt16(Slider_Color_G.Value);
			ColorB = (byte)Convert.ToInt16(Slider_Color_B.Value);
		}
	}

	private void Button_DefColor_Click(object sender, RoutedEventArgs e)
	{
		int defColor = Convert.ToInt32((sender as Button).Tag.ToString());
		ColorR = defColorList[defColor].R;
		ColorG = defColorList[defColor].G;
		ColorB = defColorList[defColor].B;
	}

	private void ComboBox_LEDMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		int selectedMode = ComboBox_LEDMode.SelectedIndex;
		Mode = (LEDMode)selectedMode;
		if (Mode == LEDMode.Static)
		{
			arraySize = 1;
			DriverInstance.getInstance().SetLEDMode(_devInfo.CMediaLEDObj, arraySize, (int)Mode, Speed);
			DriverInstance.getInstance().SetLEDColorArray(_devInfo.CMediaLEDObj, ColorR, ColorG, ColorB);
			if (isEnable)
			{
				DriverInstance.getInstance().SaveLED(_devInfo.CMediaLEDObj, ColorR, ColorG, ColorB);
			}
		}
		else if (Mode == LEDMode.Blink)
		{
			arraySize = 128;
			DriverInstance.getInstance().SetLEDMode(_devInfo.CMediaLEDObj, arraySize, (int)Mode, Speed);
			DriverInstance.getInstance().SetLEDColorArray(_devInfo.CMediaLEDObj, arraySize, DriverInstance.getInstance().redColor, DriverInstance.getInstance().greenColor, DriverInstance.getInstance().blueColor);
			if (isEnable)
			{
				DriverInstance.getInstance().SaveLED(_devInfo.CMediaLEDObj, ColorR, ColorG, ColorB);
			}
		}
		else if (Mode == LEDMode.Breathe)
		{
			arraySize = 128;
			DriverInstance.getInstance().SetLEDMode(_devInfo.CMediaLEDObj, arraySize, (int)Mode, Speed);
			_colorHelper.CalculateRGBtoZero(arraySize, ColorR, ColorG, ColorB, out var calTableR, out var calTableG, out var calTableB);
			DriverInstance.getInstance().SetLEDColorArray(_devInfo.CMediaLEDObj, arraySize, calTableR, calTableG, calTableB);
			if (isEnable)
			{
				DriverInstance.getInstance().SaveLED(_devInfo.CMediaLEDObj, calTableR[0], calTableG[0], calTableB[0]);
			}
		}
		else if (Mode == LEDMode.HeartBeat)
		{
			arraySize = 128;
			DriverInstance.getInstance().SetLEDMode(_devInfo.CMediaLEDObj, arraySize, (int)Mode, 0);
			_colorHelper.CalculateRGBtoZero(arraySize, ColorR, ColorG, ColorB, out var calTableR2, out var calTableG2, out var calTableB2);
			DriverInstance.getInstance().SetLEDColorArray(_devInfo.CMediaLEDObj, arraySize, calTableR2, calTableG2, calTableB2);
			if (isEnable)
			{
				DriverInstance.getInstance().SaveLED(_devInfo.CMediaLEDObj, calTableR2[0], calTableG2[0], calTableB2[0]);
			}
		}
		UpdateControlStatus();
	}

	private void Slider_Speed_PreviewMouseAction(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && e.RoutedEvent == Mouse.PreviewMouseMoveEvent && Speed != Convert.ToInt16(Slider_Speed.Value))
		{
			Speed = Convert.ToInt16(Slider_Speed.Value);
		}
	}

	private void UpdateControlStatus()
	{
		switch (Mode)
		{
		case LEDMode.Static:
			ColorCanvas.IsEnabled = true;
			ValueCanvas.IsEnabled = true;
			Grid_ColorSlider.IsEnabled = true;
			Grid_ColorButton.IsEnabled = true;
			Grid_Speed.IsEnabled = false;
			break;
		case LEDMode.Blink:
			ColorCanvas.IsEnabled = false;
			ValueCanvas.IsEnabled = false;
			Grid_ColorSlider.IsEnabled = false;
			Grid_ColorButton.IsEnabled = false;
			Grid_Speed.IsEnabled = true;
			break;
		case LEDMode.Breathe:
			ColorCanvas.IsEnabled = true;
			ValueCanvas.IsEnabled = true;
			Grid_ColorSlider.IsEnabled = true;
			Grid_ColorButton.IsEnabled = true;
			Grid_Speed.IsEnabled = true;
			break;
		case LEDMode.HeartBeat:
			ColorCanvas.IsEnabled = true;
			ValueCanvas.IsEnabled = true;
			Grid_ColorSlider.IsEnabled = true;
			Grid_ColorButton.IsEnabled = true;
			Grid_Speed.IsEnabled = false;
			break;
		}
	}

	private async void OnColorChanged()
	{
		_colorHelper.RGBtoHSL(ColorR, ColorG, ColorB, out h, out s, out l);
		RepaintValueCanvas();
		SelectedColorRect.Fill = new SolidColorBrush(Color.FromRgb(ColorR, ColorG, ColorB));
		Label_Color_R.Content = ColorR.ToString();
		Label_Color_G.Content = ColorG.ToString();
		Label_Color_B.Content = ColorB.ToString();
		Point crosshairPos = _colorHelper.RgbToCanvasCoordinates(ColorR, ColorG, ColorB, ColorCanvas.Width, ColorCanvas.Height);
		double offsetX = crosshairPos.X - Img_Crosshair.ActualWidth / 2.0;
		double offsetY = crosshairPos.Y - Img_Crosshair.ActualHeight / 2.0;
		Canvas.SetLeft(Img_Crosshair, offsetX);
		Canvas.SetTop(Img_Crosshair, offsetY);
		_colorHelper.RGBtoHSL(ColorR, ColorG, ColorB, out h, out s, out l);
		double offset = _colorHelper.LToCoordinate(l, ValueCanvas.Height) - Img_ValueCursor.ActualHeight / 2.0;
		Canvas.SetTop(Img_ValueCursor, offset);
		await Task.Run(() =>
		{
			LEDCommandDebouncer();
		});
	}

	private void OnModeChanged()
	{
		ComboBox_LEDMode.SelectedIndex = (int)Mode;
	}

	private void OnSpeedChanged()
	{
		Slider_Speed.Value = Speed;
		DriverInstance.getInstance().SetLEDMode(_devInfo.CMediaLEDObj, arraySize, (int)Mode, Speed);
	}

	private void OnSendLED()
	{
		if (Mode == LEDMode.Breathe || Mode == LEDMode.HeartBeat)
		{
			arraySize = 128;
			_colorHelper.CalculateRGBtoZero(arraySize, ColorR, ColorG, ColorB, out var calTableR, out var calTableG, out var calTableB);
			DriverInstance.getInstance().SetLEDColorArray(_devInfo.CMediaLEDObj, arraySize, calTableR, calTableG, calTableB);
			if (isEnable)
			{
				DriverInstance.getInstance().SaveLED(_devInfo.CMediaLEDObj, calTableR[0], calTableG[0], calTableB[0]);
			}
		}
		else
		{
			DriverInstance.getInstance().SetLEDColorArray(_devInfo.CMediaLEDObj, ColorR, ColorG, ColorB);
			if (isEnable)
			{
				DriverInstance.getInstance().SaveLED(_devInfo.CMediaLEDObj, ColorR, ColorG, ColorB);
			}
		}
	}

	private void LEDCommandDebouncer()
	{
		if (LEDDebouncer == null)
		{
			LEDDebouncer = new Timer(200.0);
			LEDDebouncer.Elapsed += (object s, ElapsedEventArgs e) =>
			{
				OnSendLED();
			};
			LEDDebouncer.AutoReset = false;
		}
		LEDDebouncer.Stop();
		LEDDebouncer.Start();
	}

	private unsafe void InitColorCanvas()
	{
		int width = (int)ColorCanvas.Width;
		int height = (int)ColorCanvas.Height;
		_colorBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32, null);
		_colorBitmap.Lock();
		int* pBackBuffer = (int*)(void*)_colorBitmap.BackBuffer;
		for (int y = 0; y < height; y++)
		{
			double s = 1.0 - (double)y / (double)height;
			for (int x = 0; x < width; x++)
			{
				double h = (double)x / (double)width * 360.0;
				Color c = _colorHelper.HslToColor(h, s, 0.5);
				pBackBuffer[x + y * width] = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
			}
		}
		_colorBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
		_colorBitmap.Unlock();
		Img_ColorBrush.Source = _colorBitmap;
	}

	private unsafe void RepaintValueCanvas()
	{
		int width = (int)ValueCanvas.Width;
		int height = (int)ValueCanvas.Height;
		_valueBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32, null);
		_valueBitmap.Lock();
		int* pBackBuffer = (int*)(void*)_valueBitmap.BackBuffer;
		for (int y = 0; y < height; y++)
		{
			double l = 1.0 - (double)y / (double)(height - 1);
			Color c = _colorHelper.HslToColor(h, s, l);
			for (int x = 0; x < width; x++)
			{
				pBackBuffer[x + y * width] = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
			}
		}
		_valueBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
		_valueBitmap.Unlock();
		Img_ValueBrush.Source = _valueBitmap;
	}
}
