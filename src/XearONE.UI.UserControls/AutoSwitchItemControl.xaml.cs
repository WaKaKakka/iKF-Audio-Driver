using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using XearONE.UI.ControlWindow;
using XearONE.UI.ControlWindow.Content.AutoSwitch;
using XearONE.UI.Driver;
using XearONE.UI.Driver.Profiles;

namespace XearONE.UI.UserControls;

public partial class AutoSwitchItemControl : UserControl, IComponentConnector
{
	private AutoSwitchTemplate template;

	public string widgetName { get; set; }

	public string bindingProfileName { get; set; }

	public event EventHandler RefreshPanel;

	public AutoSwitchItemControl(AutoSwitchTemplate template)
	{
		InitializeComponent();
		DataContext = this;
		this.template = template;
		widgetName = template.widgetName;
		bindingProfileName = template.bindingProfileName;
	}

	private void AutoSwitchItemControl_Loaded(object sender, RoutedEventArgs e)
	{
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
			Image_AppIcon.Source = bitmap;
		}
	}

	private void Button_AutoSwitchSetting_Click(object sender, RoutedEventArgs e)
	{
		Window parentWindow = Window.GetWindow(this);
		SubWindow subWindow = new SubWindow(new AutoSwitchSettingControl(template), isRegisterCallback: false);
		double left = (Application.Current.MainWindow.Width - subWindow.Width) / 2.0;
		double top = (Application.Current.MainWindow.Height - subWindow.Height) / 2.0;
		subWindow.Left = Application.Current.MainWindow.Left + left;
		subWindow.Top = Application.Current.MainWindow.Top + top;
		subWindow.Owner = parentWindow;
		subWindow.ShowDialog();
		subWindow = null;
		parentWindow.Activate();
		parentWindow.Focus();
		RefreshPanel?.Invoke(this, EventArgs.Empty);
	}

	private void Button_Delete_Click(object sender, RoutedEventArgs e)
	{
		DriverInstance.getInstance().DeleteAutoSwitchConfig(template);
		RefreshPanel?.Invoke(this, EventArgs.Empty);
	}
}
