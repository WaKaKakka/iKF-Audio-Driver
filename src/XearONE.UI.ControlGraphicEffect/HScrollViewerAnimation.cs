using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace XearONE.UI.ControlGraphicEffect;

public class HScrollViewerAnimation : ScrollViewer
{
	private double _lastLocation;

	private DispatcherTimer _scrollTemplateTimer;

	public HScrollViewerAnimation()
	{
		HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
		VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
		_scrollTemplateTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(100.0)
		};
		_scrollTemplateTimer.Tick += OnScrollTemplateLoaded;
		_scrollTemplateTimer.Start();
	}

	private void OnScrollTemplateLoaded(object sender, EventArgs e)
	{
		ScrollBar horiScrollBar = (ScrollBar)Template.FindName("PART_HorizontalScrollBar", this);
		if (horiScrollBar != null)
		{
			horiScrollBar.MouseMove += horiScrollBarMouseMove;
			_scrollTemplateTimer.Stop();
			_scrollTemplateTimer = null;
		}
	}

	private void horiScrollBarMouseMove(object sender, MouseEventArgs e)
	{
		_lastLocation = HorizontalOffset;
	}

	public new void ScrollToHome()
	{
		BeginAnimation(HScrollViewerBehavior.HorizontalOffsetProperty, null);
		_lastLocation = 0.0;
		base.ScrollToHome();
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		double wheelChange = e.Delta;
		double newOffset = _lastLocation - wheelChange * 1.0;
		ScrollToHorizontalOffset(_lastLocation);
		if (newOffset < 0.0)
		{
			newOffset = 0.0;
		}
		if (newOffset > ScrollableWidth)
		{
			newOffset = ScrollableWidth;
		}
		AnimateScroll(newOffset);
		_lastLocation = newOffset;
		e.Handled = true;
	}

	private void AnimateScroll(double toValue)
	{
		BeginAnimation(HScrollViewerBehavior.HorizontalOffsetProperty, null);
		DoubleAnimation animation = new DoubleAnimation
		{
			EasingFunction = new CubicEase
			{
				EasingMode = EasingMode.EaseOut
			},
			From = HorizontalOffset,
			To = toValue,
			Duration = TimeSpan.FromMilliseconds(400.0)
		};
		BeginAnimation(HScrollViewerBehavior.HorizontalOffsetProperty, animation);
	}
}
