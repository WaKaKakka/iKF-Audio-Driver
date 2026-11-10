using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XearONE.UI.ControlWindow.Content;

public class ColorHelper
{
	public Color HslToColor(double h, double s, double l)
	{
		h = (h % 360.0 + 360.0) % 360.0;
		s = Math.Max(0.0, Math.Min(1.0, s));
		l = Math.Max(0.0, Math.Min(1.0, l));
		double c = (1.0 - Math.Abs(2.0 * l - 1.0)) * s;
		double x = c * (1.0 - Math.Abs(h / 60.0 % 2.0 - 1.0));
		double m = l - c / 2.0;
		double rPrime;
		double gPrime;
		double bPrime;
		if (h < 60.0)
		{
			rPrime = c;
			gPrime = x;
			bPrime = 0.0;
		}
		else if (h < 120.0)
		{
			rPrime = x;
			gPrime = c;
			bPrime = 0.0;
		}
		else if (h < 180.0)
		{
			rPrime = 0.0;
			gPrime = c;
			bPrime = x;
		}
		else if (h < 240.0)
		{
			rPrime = 0.0;
			gPrime = x;
			bPrime = c;
		}
		else if (h < 300.0)
		{
			rPrime = x;
			gPrime = 0.0;
			bPrime = c;
		}
		else
		{
			rPrime = c;
			gPrime = 0.0;
			bPrime = x;
		}
		byte r = (byte)((rPrime + m) * 255.0);
		byte G = (byte)((gPrime + m) * 255.0);
		byte B = (byte)((bPrime + m) * 255.0);
		return Color.FromRgb(r, G, B);
	}

	public void RGBtoHSL(byte R, byte G, byte B, out double H, out double S, out double L)
	{
		double r = (double)(int)R / 255.0;
		double g = (double)(int)G / 255.0;
		double b = (double)(int)B / 255.0;
		double max = Math.Max(r, Math.Max(g, b));
		double min = Math.Min(r, Math.Min(g, b));
		double delta = max - min;
		L = (max + min) / 2.0;
		if (delta == 0.0)
		{
			S = 0.0;
			H = 0.0;
			return;
		}
		if (L < 0.5)
		{
			S = delta / (2.0 * L);
		}
		else
		{
			S = delta / (2.0 - 2.0 * L);
		}
		double hue;
		if (max == r)
		{
			hue = (g - b) / delta % 6.0;
		}
		else
		{
			hue = ((max != g) ? ((r - g) / delta + 4.0) : ((b - r) / delta + 2.0));
		}
		hue *= 60.0;
		if (hue < 0.0)
		{
			hue += 360.0;
		}
		H = hue;
	}

	public double LToCoordinate(double l, double height)
	{
		l = Math.Max(0.0, Math.Min(1.0, l));
		return (1.0 - l) * height;
	}

	public Point RgbToCanvasCoordinates(byte R, byte G, byte B, double width, double height)
	{
		RGBtoHSL(R, G, B, out var h, out var s, out var _);
		double x = h / 360.0 * width;
		double y = (1.0 - s) * height;
		return new Point(x, y);
	}

	public unsafe int ReadPixelColor(WriteableBitmap colorBitmap, int x, int y)
	{
		colorBitmap.Lock();
		int* pBackBuffer = (int*)(void*)colorBitmap.BackBuffer;
		int result = pBackBuffer[x + y * colorBitmap.PixelWidth];
		colorBitmap.Unlock();
		return result;
	}

	public void CalculateRGBtoZero(int arraySize, byte colorR, byte colorG, byte colorB, out byte[] tableR, out byte[] tableG, out byte[] tableB)
	{
		tableR = new byte[arraySize];
		tableG = new byte[arraySize];
		tableB = new byte[arraySize];
		tableR[0] = colorR;
		tableG[0] = colorG;
		tableB[0] = colorB;
		double dR = (int)colorR;
		double dG = (int)colorG;
		double dB = (int)colorB;
		int nRGBValue = 0;
		double R_delt = (double)(int)colorR / (double)(arraySize - 1);
		double G_delt = (double)(int)colorG / (double)(arraySize - 1);
		double B_delt = (double)(int)colorB / (double)(arraySize - 1);
		for (int i = 1; i < arraySize; i++)
		{
			dR -= R_delt;
			nRGBValue = (int)dR;
			if (nRGBValue < 0)
			{
				nRGBValue = 0;
			}
			tableR[i] = (byte)nRGBValue;
			dG -= G_delt;
			nRGBValue = (int)dG;
			if (nRGBValue < 0)
			{
				nRGBValue = 0;
			}
			tableG[i] = (byte)nRGBValue;
			dB -= B_delt;
			nRGBValue = (int)dB;
			if (nRGBValue < 0)
			{
				nRGBValue = 0;
			}
			tableB[i] = (byte)nRGBValue;
		}
	}
}
