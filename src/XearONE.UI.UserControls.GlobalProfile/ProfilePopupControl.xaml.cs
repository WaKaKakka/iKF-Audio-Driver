using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Driver;
using XearONE.UI.Driver.Profiles;
using XearONE.UI.Helpers;

namespace XearONE.UI.UserControls.GlobalProfile;

public partial class ProfilePopupControl : UserControl, IComponentConnector
{
	public ProfilePopupControl()
	{
		InitializeComponent();
	}

	private void ProfilePopupControl_Loaded(object sender, RoutedEventArgs e)
	{
	}

	public void Refresh()
	{
		LogHelper.Trace("", "Refresh", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\GlobalProfile\\ProfilePopupControl.xaml.cs");
		Panel_Profiles.Children.Clear();
		foreach (ProfileConfig item in DriverInstance.getInstance().ProfileCollection)
		{
			ProfileItemControl childProfile = new ProfileItemControl(item.profileName);
			childProfile.RefreshProfileList += RefreshProfileList;
			Panel_Profiles.Children.Add(childProfile);
		}
		ProfileAddControl profileAdd = new ProfileAddControl();
		profileAdd.RefreshProfileList += RefreshProfileList;
		Panel_Profiles.Children.Add(profileAdd);
	}

	private void RefreshProfileList(object sender, EventArgs e)
	{
		Refresh();
	}
}
