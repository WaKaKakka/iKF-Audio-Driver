using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace XearONE.UI.UserControls.EndpointControl;

public static class UIEffect
{
	private static readonly DependencyProperty MarqueeStoryboardProperty = DependencyProperty.RegisterAttached("MarqueeStoryboard", typeof(Storyboard), typeof(UIEffect), new PropertyMetadata(null));

	private static Storyboard GetMarqueeStoryboard(DependencyObject obj)
	{
		return (Storyboard)obj.GetValue(MarqueeStoryboardProperty);
	}

	private static void SetMarqueeStoryboard(DependencyObject obj, Storyboard value)
	{
		obj.SetValue(MarqueeStoryboardProperty, value);
	}

	public static void MarqueeAnimation(bool isEnable, Label label)
	{
		if (label == null)
		{
			return;
		}
		Storyboard old = GetMarqueeStoryboard(label);
		if (old != null)
		{
			try
			{
				old.Remove(label);
			}
			catch
			{
			}
			SetMarqueeStoryboard(label, null);
		}
		if (!isEnable)
		{
			label.BeginAnimation(Canvas.LeftProperty, null);
		}
		else if (label.Parent is Canvas canvas)
		{
			if (double.IsNaN(label.ActualWidth) || label.ActualWidth <= 0.0)
			{
				label.UpdateLayout();
			}
			double textWidth = Math.Max(1.0, label.ActualWidth);
			double canvasWidth = ((double.IsNaN(canvas.Width) || canvas.Width <= 0.0) ? canvas.ActualWidth : canvas.Width);
			if (canvasWidth <= 0.0)
			{
				canvas.UpdateLayout();
				canvasWidth = Math.Max(1.0, canvas.ActualWidth);
			}
			double end = canvasWidth - textWidth;
			DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames
			{
				RepeatBehavior = RepeatBehavior.Forever,
				FillBehavior = FillBehavior.Stop
			};
			animation.KeyFrames.Add(new DiscreteDoubleKeyFrame
			{
				Value = 0.0,
				KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.0))
			});
			double seconds = Math.Max(1.0, Math.Abs(end) / 100.0);
			animation.KeyFrames.Add(new LinearDoubleKeyFrame
			{
				Value = end,
				KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.0 + seconds))
			});
			animation.KeyFrames.Add(new DiscreteDoubleKeyFrame
			{
				Value = end,
				KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.0 + seconds))
			});
			Storyboard.SetTarget(animation, label);
			Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Left)"));
			Storyboard storyboard = new Storyboard
			{
				FillBehavior = FillBehavior.Stop
			};
			storyboard.Children.Add(animation);
			label.BeginAnimation(Canvas.LeftProperty, null);
			storyboard.Begin(label, isControllable: true);
			SetMarqueeStoryboard(label, storyboard);
		}
	}
}
