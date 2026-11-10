using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace XearONE.UI.EffectControls.MagicVoice2;

public partial class SystemWarningWindow : Window, IComponentConnector
{
	public SystemWarningWindow()
	{
		InitializeComponent();
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void Grid_Title_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}
}
