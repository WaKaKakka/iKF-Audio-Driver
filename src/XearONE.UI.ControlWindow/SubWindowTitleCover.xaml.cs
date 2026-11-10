using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace XearONE.UI.ControlWindow;

public partial class SubWindowTitleCover : Window, IComponentConnector
{
	private Window parentWindow;

	public SubWindowTitleCover(Window parentWindow)
	{
		InitializeComponent();
		this.parentWindow = parentWindow;
		Owner = parentWindow;
		Label_Title.Content = FindResource("Label_WindowTitle_EffectControl");
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		LocationChanged += Window_LocationChanged;
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		parentWindow.Close();
		Application.Current.MainWindow.Focus();
		Close();
	}

	private void Window_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			Close();
			parentWindow.Close();
		}
	}

	private void Grid_Title_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	private void Window_LocationChanged(object sender, EventArgs e)
	{
		if (parentWindow != null)
		{
			parentWindow.Left = Left - 2.0;
			parentWindow.Top = Top - 2.0;
		}
	}
}
