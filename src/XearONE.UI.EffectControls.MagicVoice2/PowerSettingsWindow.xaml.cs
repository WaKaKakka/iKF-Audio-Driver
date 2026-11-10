#define TRACE
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Helpers;

namespace XearONE.UI.EffectControls.MagicVoice2;

public partial class PowerSettingsWindow : UserControl, IComponentConnector
{
	public IniFileHelper iniFile { get; set; }

	public PowerSettingsWindow()
	{
		InitializeComponent();
		new PowerManagerHelper();
	}

	public bool IsPowerSettingCorrect()
	{
		PowerManagerHelper pm = new PowerManagerHelper();
		byte aCLineStaus = 0;
		byte batterLifePercent = 0;
		pm.GetSystemPowerStatus(ref aCLineStaus, ref batterLifePercent);
		bool flag = aCLineStaus != 0;
		bool isCurrentPlanMaximumPerformance = pm.IsCurrentPlanMaximumPerformance();
		if (!flag)
		{
			Trace.WriteLine("[CM] IsPowerSettingCorrect() ==> AC is Not Plugged in.");
		}
		if (!isCurrentPlanMaximumPerformance)
		{
			Trace.WriteLine("[CM] IsPowerSettingCorrect() ==> Power Plan is Not best performance.");
		}
		return flag & isCurrentPlanMaximumPerformance;
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		Visibility = Visibility.Hidden;
	}

	private void DoNotShow_Click(object sender, RoutedEventArgs e)
	{
		string value = ((chkDoNotShowAgain.IsChecked == true) ? "true" : "false");
		if (iniFile != null)
		{
			iniFile.Write("PowerSettingsWindow", value, "DoNotShowAgain");
		}
	}
}
