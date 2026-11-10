using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using XearONE.UI.ControlWindow.Content;

namespace XearONE.UI.ControlWindow;

public partial class SubWindow : Window, IComponentConnector
{
	private string _controlName = string.Empty;

	private bool isRegisterCallback;

	public SubWindow(UserControl userControl, bool isRegisterCallback = true)
	{
		InitializeComponent();
		Owner = Application.Current.MainWindow;
		DataContext = this;
		RefreshLang();
		_controlName = userControl.Name;
		Width = userControl.Width;
		Height = userControl.Height + 40.0;
		Grid_Content.Children.Add(userControl);
		this.isRegisterCallback = isRegisterCallback;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		if (isRegisterCallback)
		{
			MainWindow MW = (MainWindow)Owner;
			Dispatcher.InvokeAsync(() =>
			{
				MW.RegisterEventRefresh(UpdateUI);
			});
			Dispatcher.InvokeAsync(() =>
			{
				MW.RegisterEventRefreshEffect(UpdateUI);
			});
		}
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (isRegisterCallback)
		{
			MainWindow MW = (MainWindow)Owner;
			Dispatcher.InvokeAsync(() =>
			{
				MW.UnregisterEventRefresh();
			});
			Dispatcher.InvokeAsync(() =>
			{
				MW.UnregisterEventRefreshEffect();
			});
		}
	}

	public void UpdateUI()
	{
		Console.WriteLine("SubWindow UpdateUI()");
		Dispatcher.BeginInvoke((Action)(() =>
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Grid_Content); i++)
			{
				if (VisualTreeHelper.GetChild(Grid_Content, i) is WindowConstantDefinition.IUsrCtrlProperty)
				{
					((WindowConstantDefinition.IUsrCtrlProperty)VisualTreeHelper.GetChild(Grid_Content, i)).UpdateUI();
				}
			}
		}));
	}

	public void UpdateUI(string prop)
	{
		Console.WriteLine("SubWindow UpdateUI(string prop)");
		Dispatcher.BeginInvoke((Action)(() =>
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(Grid_Content);
			for (int i = 0; i < childrenCount; i++)
			{
				if (VisualTreeHelper.GetChild(Grid_Content, i) is EffectControl effectControl)
				{
					effectControl.UpdateUI(prop);
				}
			}
		}));
	}

	public void RefreshLang()
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			WindowTitle.Content = (TryFindResource("Label_WindowTitle_" + _controlName) as string) ?? _controlName;
		}));
	}

	public bool IsNeedToClosed()
	{
		foreach (object userControl in Grid_Content.Children)
		{
			if (userControl.GetType() == typeof(EffectControl) || userControl.GetType() == typeof(JackSetting) || userControl.GetType() == typeof(LEDControl))
			{
				return true;
			}
		}
		return false;
	}

	private void Grid_Title_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		CloseWindow();
	}

	private void Window_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			CloseWindow();
		}
	}

	private void CloseWindow()
	{
		Close();
		Owner.Focus();
	}
}
