using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace XearONE.UI.ControlGraphicEffect;

public class ScrollViewerAnimation : ScrollViewer
{
	private double _lastLocation;

	private DispatcherTimer _scrollTemplateTimer;

	public ScrollViewerAnimation()
	{
		_scrollTemplateTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(100.0)
		};
		_scrollTemplateTimer.Tick += OnScrollTemplateLoaded;
		_scrollTemplateTimer.Start();
	}

	private void OnScrollTemplateLoaded(object sender, EventArgs e)
	{
		ScrollBar verScrollBar = (ScrollBar)Template.FindName("PART_VerticalScrollBar", this);
		if (verScrollBar != null)
		{
			verScrollBar.MouseMove += VerScrollBarMouseMove;
			_scrollTemplateTimer.Stop();
			_scrollTemplateTimer = null;
		}
	}

	private void VerScrollBarMouseMove(object sender, MouseEventArgs e)
	{
		_lastLocation = VerticalOffset;
	}

	public new void ScrollToHome()
	{
		BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
		_lastLocation = 0.0;
		base.ScrollToHome();
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		double wheelChange = e.Delta;
		double newOffset = _lastLocation - wheelChange * 1.0;
		ScrollToVerticalOffset(_lastLocation);
		if (newOffset < 0.0)
		{
			newOffset = 0.0;
		}
		if (newOffset > ScrollableHeight)
		{
			newOffset = ScrollableHeight;
		}
		AnimateScroll(newOffset);
		_lastLocation = newOffset;
		e.Handled = true;
	}

	private void AnimateScroll(double toValue)
	{
		BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
		DoubleAnimation animation = new DoubleAnimation
		{
			EasingFunction = new CubicEase
			{
				EasingMode = EasingMode.EaseOut
			},
			From = VerticalOffset,
			To = toValue,
			Duration = TimeSpan.FromMilliseconds(400.0)
		};
		BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
	}
}
