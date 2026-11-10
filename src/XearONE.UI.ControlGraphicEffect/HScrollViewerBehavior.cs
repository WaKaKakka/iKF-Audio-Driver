using System.Windows;
using System.Windows.Controls;

namespace XearONE.UI.ControlGraphicEffect;

public static class HScrollViewerBehavior
{
	public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(HScrollViewerBehavior), new UIPropertyMetadata(0.0, OnHorizontalOffsetChanged));

	public static void SetHorizontalOffset(FrameworkElement target, double value)
	{
		target.SetValue(HorizontalOffsetProperty, value);
	}

	public static double GetHorizontalOffset(FrameworkElement target)
	{
		return (double)target.GetValue(HorizontalOffsetProperty);
	}

	private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
	{
		(target as ScrollViewer)?.ScrollToHorizontalOffset((double)e.NewValue);
	}
}
