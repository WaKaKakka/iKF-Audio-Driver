using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Driver;
using XearONE.UI.Driver.Profiles;
using XearONE.UI.Helpers;
using XearONE.UI.UserControls;

namespace XearONE.UI.ControlWindow.Content.AutoSwitch;

public partial class ProfileAutoSwitch : UserControl, IComponentConnector
{
	public ProfileAutoSwitch()
	{
		InitializeComponent();
		Name = "ProfileAutoSwitch";
	}

	private void ProfileAutoSwitchControl_Loaded(object sender, RoutedEventArgs e)
	{
		LogHelper.Trace("", "ProfileAutoSwitchControl_Loaded", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\ControlWindow\\Content\\AutoSwitch\\ProfileAutoSwitch.xaml.cs");
		RefreshPanel();
	}

	private void Button_AddAutoSwitch_Click(object sender, RoutedEventArgs e)
	{
		Window parentWindow = Window.GetWindow(this);
		SubWindow subWindow = new SubWindow(new AutoSwitchSettingControl(new AutoSwitchTemplate(), isNewConfig: true), isRegisterCallback: false);
		double left = (Application.Current.MainWindow.Width - subWindow.Width) / 2.0;
		double top = (Application.Current.MainWindow.Height - subWindow.Height) / 2.0;
		subWindow.Left = Application.Current.MainWindow.Left + left;
		subWindow.Top = Application.Current.MainWindow.Top + top;
		subWindow.Owner = parentWindow;
		subWindow.ShowDialog();
		subWindow = null;
		parentWindow.Activate();
		parentWindow.Focus();
		RefreshPanel();
	}

	private void RefreshPanel()
	{
		Panel_Apps.Children.Clear();
		foreach (AutoSwitchTemplate item in DriverInstance.getInstance().AutoSwitchCollection)
		{
			AutoSwitchItemControl childAutoSwitch = new AutoSwitchItemControl(item);
			childAutoSwitch.RefreshPanel += RefreshEvent;
			Panel_Apps.Children.Add(childAutoSwitch);
		}
	}

	private void RefreshEvent(object sender, EventArgs e)
	{
		RefreshPanel();
	}
}
