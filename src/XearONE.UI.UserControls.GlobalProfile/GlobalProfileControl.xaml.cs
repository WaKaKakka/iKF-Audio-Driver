using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Driver;

namespace XearONE.UI.UserControls.GlobalProfile;

public partial class GlobalProfileControl : UserControl, IComponentConnector
{
	public GlobalProfileControl()
	{
		InitializeComponent();
	}

	public void Refresh()
	{
		RefreshCurrentProfile();
	}

	private void GlobalProfileControl_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void RefreshCurrentProfile()
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Label_ProfileName.Content = DriverInstance.getInstance().CurrentProfileName;
		}));
	}

	private void Button_Profile_Click(object sender, RoutedEventArgs e)
	{
		((MainWindow)Application.Current.MainWindow).Button_GlobalProfile_Click(this, e);
	}
}
