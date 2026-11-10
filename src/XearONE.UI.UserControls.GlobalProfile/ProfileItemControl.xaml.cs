using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using XearONE.UI.Driver;
using XearONE.UI.Helpers;

namespace XearONE.UI.UserControls.GlobalProfile;

public partial class ProfileItemControl : UserControl, IComponentConnector
{
	public string profileName { get; set; }

	public bool isDefaultProfile { get; set; }

	public bool isCurrentProfile { get; set; }

	public event EventHandler RefreshProfileList;

	public ProfileItemControl(string _profileName)
	{
		InitializeComponent();
		DataContext = this;
		profileName = _profileName;
		if (profileName.Equals("Default"))
		{
			isDefaultProfile = true;
		}
		if (profileName.Equals(DriverInstance.getInstance().CurrentProfileName))
		{
			isCurrentProfile = true;
		}
	}

	private void ProfileItemControl_Loaded(object sender, RoutedEventArgs e)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			if (isDefaultProfile)
			{
				Button_Delete.Visibility = Visibility.Collapsed;
			}
			else
			{
				Button_Delete.Visibility = Visibility.Visible;
			}
			if (isCurrentProfile)
			{
				RadioButton_Profile.IsChecked = true;
			}
		}));
	}

	private void RadioButton_Profile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!profileName.Equals(DriverInstance.getInstance().CurrentProfileName))
		{
			DriverInstance.getInstance().ProfileChanged(profileName);
			RefreshProfileButton();
		}
	}

	private void Button_Delete_Click(object sender, RoutedEventArgs e)
	{
		DeletePopup.IsOpen = true;
	}

	private void DeletePopup_Opened(object sender, EventArgs e)
	{
		LogHelper.Trace("", "DeletePopup_Opened", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\GlobalProfile\\ProfileItemControl.xaml.cs");
		if (DeletePopup.Child is DeletePopupControl control)
		{
			control.Confirmed -= DeletePopupControl_Confirmed;
			control.Confirmed += DeletePopupControl_Confirmed;
		}
	}

	private void DeletePopup_Closed(object sender, EventArgs e)
	{
		LogHelper.Trace("", "DeletePopup_Closed", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\GlobalProfile\\ProfileItemControl.xaml.cs");
	}

	private void DeletePopupControl_Confirmed(object sender, bool isOk)
	{
		if (isOk)
		{
			if (!DriverInstance.getInstance().DeleteProfile(profileName))
			{
				return;
			}
			DriverInstance.getInstance().BindProfileToDefault(profileName);
			RefreshProfileList?.Invoke(this, EventArgs.Empty);
		}
		RefreshProfileButton();
		DeletePopup.IsOpen = false;
	}

	private void RefreshProfileButton()
	{
		((MainWindow)Application.Current.MainWindow).RefreshProfileButton();
	}
}
