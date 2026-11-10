using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace XearONE.UI.UserControls.GlobalProfile;

public partial class DeletePopupControl : UserControl, IComponentConnector
{
	public event EventHandler<bool> Confirmed;

	public DeletePopupControl()
	{
		InitializeComponent();
	}

	private void Button_OK_Click(object sender, RoutedEventArgs e)
	{
		Confirmed?.Invoke(this, e: true);
	}

	private void Button_Cancel_Click(object sender, RoutedEventArgs e)
	{
		Confirmed?.Invoke(this, e: false);
	}
}
