using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using XearONE.UI.Driver;
using XearONE.UI.Driver.Profiles;
using XearONE.UI.Helpers;

namespace XearONE.UI.ControlWindow.Content.AutoSwitch;

public partial class AutoSwitchSettingControl : UserControl, IComponentConnector
{
	private AutoSwitchTemplate template;

	private bool isNewConfig;

	private string oldWidgetName = "";

	private string oldAppPath = "";

	public string widgetName { get; set; }

	public string appPath { get; set; }

	public AutoSwitchSettingControl(AutoSwitchTemplate template, bool isNewConfig = false)
	{
		InitializeComponent();
		DataContext = this;
		this.template = template;
		widgetName = template.widgetName;
		appPath = template.appPath;
		this.isNewConfig = isNewConfig;
		oldWidgetName = widgetName;
		oldAppPath = appPath;
	}

	private void AutoSwitchSettingControl_Loaded(object sender, RoutedEventArgs e)
	{
		InitBindingProfile();
		string iconPath = DriverInstance.getInstance().LoadAutoSwitchIcon(template.icon);
		if (File.Exists(iconPath))
		{
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
			bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
			bitmap.EndInit();
			bitmap.Freeze();
			Image_Icon.Source = bitmap;
		}
	}

	private void Button_Browser_Click(object sender, RoutedEventArgs e)
	{
		OpenFileDialog dlg = new OpenFileDialog
		{
			Title = "選擇執行檔",
			Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*",
			Multiselect = false,
			CheckFileExists = true,
			CheckPathExists = true,
			DereferenceLinks = true
		};
		if (dlg.ShowDialog() == true)
		{
			string fullPath = dlg.FileName;
			fullPath = Path.GetFullPath(fullPath);
			TextBox_WidgetName.Text = Path.GetFileNameWithoutExtension(fullPath);
			TextBox_AppPath.Text = fullPath;
			Image_Icon.Source = IconHelper.GetExeIcon(fullPath);
		}
	}

	private void Button_Save_Click(object sender, RoutedEventArgs e)
	{
		widgetName = TextBox_WidgetName.Text;
		appPath = TextBox_AppPath.Text;
		if (ValidateInputs())
		{
			template.widgetName = widgetName;
			template.appPath = appPath;
			if (ComboBox_Profile.SelectedItem != null)
			{
				ProfileConfig profileConfig = ComboBox_Profile.SelectedItem as ProfileConfig;
				template.bindingProfileName = profileConfig.profileName;
			}
			else
			{
				template.bindingProfileName = "Default";
			}
			template.icon = Path.GetFileName(Path.ChangeExtension(appPath, "png"));
			DriverInstance.getInstance().SaveAutoSwitchIconAsPng(Image_Icon.Source, template.icon);
			if (isNewConfig)
			{
				DriverInstance.getInstance().AddNewAutoSwitchConfig(template);
			}
			DriverInstance.getInstance().SaveAutoSwitchConfig();
			Window.GetWindow(this).Close();
		}
	}

	private void Button_Icon_Click(object sender, RoutedEventArgs e)
	{
		OpenFileDialog dlg = new OpenFileDialog
		{
			Title = "選擇檔案",
			Filter = "所有檔案 (*.*)|*.*",
			Multiselect = false,
			CheckFileExists = true,
			CheckPathExists = true,
			DereferenceLinks = true
		};
		if (dlg.ShowDialog() != true)
		{
			return;
		}
		string path = dlg.FileName;
		string ext = Path.GetExtension(path).ToLower();
		string[] array = new string[8] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff", ".ico" };
		ImageSource imgSource = null;
		if (Array.Exists(array, (string x) => x == ext))
		{
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
			bitmapImage.UriSource = new Uri(path, UriKind.Absolute);
			bitmapImage.EndInit();
			bitmapImage.Freeze();
			imgSource = bitmapImage;
		}
		else
		{
			using Icon sysicon = Icon.ExtractAssociatedIcon(path);
			imgSource = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}
		if (imgSource != null)
		{
			Image_Icon.Source = imgSource;
		}
	}

	private void InitBindingProfile()
	{
		ComboBox_Profile.ItemsSource = DriverInstance.getInstance().ProfileCollection;
		ComboBox_Profile.DisplayMemberPath = "profileName";
		ComboBox_Profile.SelectedValuePath = "profileName";
		if (DriverInstance.getInstance().ProfileCollection.Find((ProfileConfig x) => x.profileName == template.bindingProfileName) != null)
		{
			ComboBox_Profile.SelectedValue = template.bindingProfileName;
			return;
		}
		ComboBox_Profile.SelectedValue = "Default";
		template.bindingProfileName = "Default";
	}

	private bool ValidateInputs()
	{
		Border_ErrorWidgetName.Visibility = Visibility.Collapsed;
		Border_ErrorAppLocation.Visibility = Visibility.Collapsed;
		string name = (widgetName ?? string.Empty).Trim();
		string oldName = (oldWidgetName ?? string.Empty).Trim();
		string path = (appPath ?? string.Empty).Trim();
		string oldPath = (oldAppPath ?? string.Empty).Trim();
		if (string.IsNullOrWhiteSpace(name))
		{
			Border_ErrorWidgetName.Visibility = Visibility.Visible;
			return false;
		}
		if (string.IsNullOrWhiteSpace(path))
		{
			Border_ErrorAppLocation.Visibility = Visibility.Visible;
			return false;
		}
		if (!string.Equals(oldName, name, StringComparison.OrdinalIgnoreCase) && !IsWidgetNameUnique())
		{
			Border_ErrorWidgetName.Visibility = Visibility.Visible;
			return false;
		}
		if (!string.Equals(oldPath, path, StringComparison.OrdinalIgnoreCase) && !IsAppUnique())
		{
			Border_ErrorAppLocation.Visibility = Visibility.Visible;
			return false;
		}
		return true;
	}

	private bool IsWidgetNameUnique()
	{
		if (DriverInstance.getInstance().AutoSwitchCollection.Find((AutoSwitchTemplate x) => x.widgetName == widgetName) != null)
		{
			return false;
		}
		return true;
	}

	private bool IsAppUnique()
	{
		if (DriverInstance.getInstance().AutoSwitchCollection.Find((AutoSwitchTemplate x) => x.appPath == appPath) != null)
		{
			return false;
		}
		return true;
	}
}
