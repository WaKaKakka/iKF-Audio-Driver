using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using XearONE.UI.Helpers;

namespace XearONE.UI.EffectControls.MagicVoice2;

public partial class WarningDialog : UserControl, IComponentConnector
{
	public IniFileHelper iniFile { get; set; }

	public WarningDialog()
	{
		InitializeComponent();
	}

	public void SetErrorDescText(string desc)
	{
		lblTitle.Content = "Error";
		lblDesc.Content = desc;
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
			iniFile.Write("WarningDialog", value, "DoNotShowAgain");
		}
	}
}
