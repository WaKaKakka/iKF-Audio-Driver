using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace XearONE.UI.EffectControls;

public partial class EQControl : UserControl, IUsrCtrlProperty, IComponentConnector
{
	private readonly EQViewModel _viewModel;

	public List<string> propertyNames
	{
		get
		{
			return _viewModel.propertyNames;
		}
		set
		{
		}
	}

	public EQControl(Guid audEndptUID)
	{
		InitializeComponent();
		_viewModel = new EQViewModel(audEndptUID);
		DataContext = _viewModel;
	}

	private void EQControl_Loaded(object sender, RoutedEventArgs e)
	{
		_viewModel.RefreshAllControls();
	}

	public void RefreshAllControls()
	{
		_viewModel.RefreshAllControls();
	}

	public void InitPropertyNames()
	{
		_viewModel.InitPropertyNames();
	}
}
