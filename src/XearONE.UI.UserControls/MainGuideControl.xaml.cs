using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using XearONE.UI.Driver;
using XearONE.UI.Properties;

namespace XearONE.UI.UserControls;

public partial class MainGuideControl : UserControl, IComponentConnector
{
	private int stepPhase;

	private const int stepPhaseMin = 1;

	private const int stepPhaseMax = 3;

	private MainWindow MW
	{
		[DebuggerStepThrough]
		get
		{
			return (MainWindow)Application.Current.MainWindow;
		}
	}

	public event EventHandler RequestRemoval;

	public MainGuideControl()
	{
		InitializeComponent();
		stepPhase = 1;
	}

	private void MainGuideControl_Loaded(object sender, RoutedEventArgs e)
	{
		MW.AutoSize(enable: false);
	}

	private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		stepPhase++;
		HideAllMsg();
		switch (stepPhase)
		{
		case 2:
			Img_Main_Guide_Bg.Source = new BitmapImage(new Uri(XearONE.UI.Properties.Resources.Img_Main_Guide02, UriKind.Absolute));
			Img_Main_Guide_Msg04.Visibility = Visibility.Visible;
			break;
		case 3:
			Img_Main_Guide_Bg.Source = new BitmapImage(new Uri(XearONE.UI.Properties.Resources.Img_Main_Guide03, UriKind.Absolute));
			Img_Main_Guide_Msg05.Visibility = Visibility.Visible;
			break;
		}
		if (stepPhase > 3)
		{
			RequestRemoval?.Invoke(this, EventArgs.Empty);
		}
	}

	private void HideAllMsg()
	{
		Img_Main_Guide_Msg01.Visibility = Visibility.Hidden;
		Img_Main_Guide_Msg02.Visibility = Visibility.Hidden;
		Img_Main_Guide_Msg03.Visibility = Visibility.Hidden;
		Img_Main_Guide_Msg04.Visibility = Visibility.Hidden;
		Img_Main_Guide_Msg05.Visibility = Visibility.Hidden;
	}

	private void CheckBox_DisableGuide_Click(object sender, RoutedEventArgs e)
	{
		bool isShow = CheckBox_DisableGuide.IsChecked.Value;
		DriverInstance.getInstance().SaveIsDisableMainGuide(isShow);
	}
}
