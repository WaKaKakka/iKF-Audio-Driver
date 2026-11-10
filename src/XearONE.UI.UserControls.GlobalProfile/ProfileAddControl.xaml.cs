using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Driver;
using XearONE.UI.Driver.Profiles;
using XearONE.UI.Helpers;

namespace XearONE.UI.UserControls.GlobalProfile;

public partial class ProfileAddControl : UserControl, IComponentConnector
{
	public event EventHandler RefreshProfileList;

	public ProfileAddControl()
	{
		InitializeComponent();
	}

	private void ProfileAddControl_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void Button_AddProfile_Click(object sender, RoutedEventArgs e)
	{
		string profileName = TextBox_ProfileName.Text;
		if (!string.IsNullOrEmpty(profileName))
		{
			if (DriverInstance.getInstance().ProfileCollection.Find((ProfileConfig x) => x.profileName == profileName) == null)
			{
				LogHelper.Info("Save Profile: " + profileName, isPrintCaller: false, "Button_AddProfile_Click", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\GlobalProfile\\ProfileAddControl.xaml.cs");
				DriverInstance.getInstance().AddNewProfile(profileName);
				RefreshProfileList?.Invoke(this, EventArgs.Empty);
				RefreshProfileButton();
			}
			else
			{
				LogHelper.Warning("Profile already exist: " + profileName, isPrintCaller: false, "Button_AddProfile_Click", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\GlobalProfile\\ProfileAddControl.xaml.cs");
				ShowErrorText("Name already exist !");
			}
		}
		else
		{
			LogHelper.Warning("Profile name is empty", isPrintCaller: false, "Button_AddProfile_Click", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\UserControls\\GlobalProfile\\ProfileAddControl.xaml.cs");
			ShowErrorText("Name can NOT empty !");
		}
	}

	private void ShowErrorText(string errorMsg)
	{
		Dispatcher.BeginInvoke((Action)(() =>
		{
			Label_Error.Content = errorMsg;
		}));
	}

	private void RefreshProfileButton()
	{
		((MainWindow)Application.Current.MainWindow).RefreshProfileButton();
	}
}
