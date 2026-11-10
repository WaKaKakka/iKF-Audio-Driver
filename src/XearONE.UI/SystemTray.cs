using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using XearONE.UI.Properties;

namespace XearONE.UI;

public class SystemTray : IDisposable
{
	private IContainer _components;

	private NotifyIcon _notifyIcon;

	private ToolStripMenuItem _OpenMenuItem;

	private ToolStripMenuItem _exitMenuItem;

	public event Action ResumeWindowRequested;

	public event Action CloseWindowRequested;

	public SystemTray()
	{
		_components = new Container();
		_notifyIcon = new NotifyIcon(_components)
		{
			ContextMenuStrip = new ContextMenuStrip(),
			Text = GetAppTitle(),
			Visible = true
		};
		string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Resources.IconPath);
		if (File.Exists(iconPath))
		{
			_notifyIcon.Icon = new Icon(iconPath);
		}
		else
		{
			_notifyIcon.Icon = Resources.Icon;
		}
		_notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
		_notifyIcon.DoubleClick += notifyIcon_DoubleClick;
		_notifyIcon.MouseUp += notifyIcon_MouseUp;
	}

	~SystemTray()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (_notifyIcon != null)
		{
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();
			_notifyIcon = null;
		}
	}

	private string GetAppTitle()
	{
		return (System.Windows.Application.Current.TryFindResource("Label_TrayName") as string) ?? "XearONE";
	}

	private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
	{
		e.Cancel = false;
		if (_notifyIcon.ContextMenuStrip.Items.Count == 0)
		{
			string trayOpen = (System.Windows.Application.Current.TryFindResource("Label_TrayOpen") as string) ?? "Open";
			string trayClose = (System.Windows.Application.Current.TryFindResource("Label_TrayClose") as string) ?? "Close";
			_OpenMenuItem = ToolStripMenuItemWithHandler(trayOpen, trayOpen + " " + GetAppTitle(), openItem_Click);
			_notifyIcon.ContextMenuStrip.Items.Add(_OpenMenuItem);
			_notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			_exitMenuItem = ToolStripMenuItemWithHandler(trayClose, trayClose + " " + GetAppTitle(), exitItem_Click);
			_notifyIcon.ContextMenuStrip.Items.Add(_exitMenuItem);
		}
		else
		{
			string trayOpen2 = (System.Windows.Application.Current.TryFindResource("Label_TrayOpen") as string) ?? "Open";
			string trayClose2 = (System.Windows.Application.Current.TryFindResource("Label_TrayClose") as string) ?? "Close";
			_OpenMenuItem.Text = trayOpen2;
			_OpenMenuItem.ToolTipText = trayOpen2 + " " + GetAppTitle();
			_exitMenuItem.Text = trayClose2;
			_exitMenuItem.ToolTipText = trayClose2 + " " + GetAppTitle();
		}
		_OpenMenuItem.Enabled = true;
		_exitMenuItem.Enabled = true;
	}

	private void notifyIcon_DoubleClick(object sender, EventArgs e)
	{
		ResumeWindow();
	}

	private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_notifyIcon, null);
		}
	}

	private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, string tooltipText, EventHandler eventHandler)
	{
		ToolStripMenuItem item = new ToolStripMenuItem(displayText);
		if (eventHandler != null)
		{
			item.Click += eventHandler;
		}
		item.ToolTipText = tooltipText;
		return item;
	}

	private void openItem_Click(object sender, EventArgs e)
	{
		ResumeWindow();
	}

	private void exitItem_Click(object sender, EventArgs e)
	{
		CloseWindowRequested?.Invoke();
	}

	private void ResumeWindow()
	{
		ResumeWindowRequested?.Invoke();
	}
}
