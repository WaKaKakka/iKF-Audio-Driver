using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Cmedia;

namespace XearONE.UI.UserControls.EndpointControl.PropertyPanel;

public partial class PanelSession : UserControl, IComponentConnector
{
	private Guid _endpointUID;

	private object _sessionLocker = new object();

	private readonly ObservableCollection<string> _SessionInstanceIDs = new ObservableCollection<string>();

	private readonly ObservableCollection<string> _SessionDescs = new ObservableCollection<string>();

	public const string PropertyName_SessionInstanceID = "SessionInstanceID";

	public const string PropertyName_SessionProcessDescription = "SessionProcessDescription";

	public void ShowGamingBorder(bool show)
	{
		Border_IsCMDevice.Visibility = ((!show) ? Visibility.Hidden : Visibility.Visible);
	}

	public PanelSession(Guid endpointUID)
	{
		InitializeComponent();
		_endpointUID = endpointUID;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		UpdateUI();
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	public void UpdateUI()
	{
		lock (_sessionLocker)
		{
			Console.WriteLine("RefreshSessionControl()");
			bool getSuccess = false;
			_SessionInstanceIDs.Clear();
			if (XearSDK.GetProperty(_endpointUID, "SessionInstanceID", out string[] outInstanceIDs, out XearSDK.CmErr cmErr))
			{
				string[] array = outInstanceIDs;
				foreach (string i2 in array)
				{
					_SessionInstanceIDs.Add(i2);
				}
				_SessionDescs.Clear();
				string[] sessionUIDs = new string[_SessionInstanceIDs.Count];
				for (int j = 0; j < _SessionInstanceIDs.Count; j++)
				{
					sessionUIDs[j] = _SessionInstanceIDs[j];
				}
				if (XearSDK.GetProperty(_endpointUID, "SessionProcessDescription", out string[] outDescs, out cmErr, sessionUIDs))
				{
					array = outDescs;
					foreach (string i3 in array)
					{
						_SessionDescs.Add(i3);
					}
					getSuccess = true;
				}
			}
			StringBuilder SessionName = new StringBuilder();
			if (getSuccess)
			{
				for (int k = 0; k < _SessionDescs.Count; k++)
				{
					if (_SessionDescs[k].IndexOf("Idle") == -1 && !(_SessionDescs[k] == ""))
					{
						if (SessionName.Length != 0)
						{
							SessionName.Append($", {_SessionDescs[k]}");
						}
						else
						{
							SessionName.Append($"{_SessionDescs[k]}");
						}
					}
				}
				Dispatcher.BeginInvoke((Action)(() =>
				{
					if (SessionName.Length == 0)
					{
						SessionName.Append("Idle, ...");
					}
					Label_JackSession.Content = SessionName.ToString();
				}));
				return;
			}
			Dispatcher.BeginInvoke((Action)(() =>
			{
				if (SessionName.Length == 0)
				{
					SessionName.Append("Idle, ...");
				}
				Label_JackSession.Content = SessionName.ToString();
				UIEffect.MarqueeAnimation(isEnable: false, Label_JackSession);
			}));
		}
	}

	private void Label_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		Label label = sender as Label;
		if (label.Parent is Canvas canvas)
		{
			UIEffect.MarqueeAnimation(label.ActualWidth > canvas.Width, label);
		}
	}
}
