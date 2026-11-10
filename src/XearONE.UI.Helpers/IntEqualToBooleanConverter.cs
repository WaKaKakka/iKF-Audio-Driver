using System;
using System.Globalization;
using System.Windows.Data;

namespace XearONE.UI.Helpers;

public class IntEqualToBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null || parameter == null)
		{
			return false;
		}
		int num = System.Convert.ToInt32(value);
		int targetValue = System.Convert.ToInt32(parameter);
		return num == targetValue;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (parameter == null)
		{
			return Binding.DoNothing;
		}
		if (!System.Convert.ToBoolean(value))
		{
			return Binding.DoNothing;
		}
		return System.Convert.ToInt32(parameter);
	}
}
