using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Cmedia;
using Microsoft.Win32;

namespace XearONE.UI;

public partial class FirmwareUpdateWindowCM2026 : Window, IComponentConnector
{
	private struct FirmwareInfo
	{
		public int FirmwareType { get; set; }

		public Guid ProjectID { get; set; }

		public string ModelName { get; set; }

		public string FirmwareVersion { get; set; }
	}

	private IntPtr hWnd = IntPtr.Zero;

	private string fwFullpath = string.Empty;

	private const int deviceVid = 14391;

	private const int devicePid = 24581;

	private const string modelName = "MCHOSE G9 V2";

	private Guid guidOtaDongle = new Guid("454DB434-ACCF-4965-B912-052C434BE817");

	private Guid guidOtaHeadset = new Guid("377110D3-2DB4-4A7C-BDEA-44439568B2AC");

	private Guid deviceGuid = Guid.Empty;

	private readonly string notAvailable = "N/A";

	private bool rxConnected;

	private int batteryThreshold = 30;

	private FirmwareInfo binFile;

	private List<FirmwareInfo> devInfo = new List<FirmwareInfo>();

	public static readonly string DemoAppPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;

	public static readonly string DemoAppDir = Path.GetDirectoryName(DemoAppPath);

	public static readonly string FirmwareUpdateSDKPath = Path.Combine(DemoAppDir, "CmFirmwareUpdateSDK.dll");

	public static readonly Version FirmwareUpdateSDKVersion = new Version(FileVersionInfo.GetVersionInfo(FirmwareUpdateSDKPath).FileVersion);

	private FirmwareUpdateSDK.CmFuncPtrCallback EventCallbackFuncPtr;

	public FirmwareUpdateWindowCM2026(Window owner)
	{
		InitializeComponent();
		Owner = owner;
		Window window = Window.GetWindow(this);
		hWnd = new WindowInteropHelper(window).EnsureHandle();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		Hide();
		bool isInitOK = false;
		if (!FirmwareUpdateSDK.Initialize(out var _))
		{
			string caption = "SDK Error";
			MessageBoxButton button = MessageBoxButton.OK;
			MessageBoxImage icon = MessageBoxImage.Hand;
			MessageBox.Show("Update SDK Failed.", caption, button, icon, MessageBoxResult.OK);
		}
		else
		{
			EventCallbackFuncPtr = EventCallbackDispatcher;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = Environment.CurrentDirectory;
			openFileDialog.Filter = "fot files |*.fot";
			bool? result = openFileDialog.ShowDialog();
			Activate();
			Focus();
			if (result == true)
			{
				fwFullpath = openFileDialog.FileName;
				if (GetBinFileFirmwareInformation() && GetDeviceFirmwareInformation())
				{
					Button_Update.IsEnabled = true;
					isInitOK = true;
				}
			}
		}
		if (!isInitOK)
		{
			CloseWindow();
		}
		else
		{
			Show();
		}
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (!FirmwareUpdateSDK.Deinitialize(out var _))
		{
			e.Cancel = true;
		}
	}

	private void Button_SelectFile_Click(object sender, RoutedEventArgs e)
	{
	}

	private void Button_Update_Click(object sender, RoutedEventArgs e)
	{
		string caption = "Update Confirm";
		MessageBoxButton button = MessageBoxButton.YesNo;
		MessageBoxImage icon = MessageBoxImage.Question;
		MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to update the firmware?", caption, button, icon, MessageBoxResult.No);
		Activate();
		Focus();
		if (messageBoxResult != MessageBoxResult.Yes)
		{
			return;
		}
		int updateType = 0;
		if (binFile.FirmwareType == 2)
		{
			updateType = 0;
		}
		else if (binFile.FirmwareType == 3)
		{
			updateType = 1;
		}
		if (binFile.ProjectID != devInfo[updateType].ProjectID)
		{
			caption = "Update Confirm";
			button = MessageBoxButton.YesNo;
			icon = MessageBoxImage.Question;
			MessageBoxResult messageBoxResult2 = MessageBox.Show("This firmware may not be compatible with this device. Are you sure you want to update?", caption, button, icon, MessageBoxResult.No);
			Activate();
			Focus();
			if (messageBoxResult2 != MessageBoxResult.Yes)
			{
				return;
			}
		}
		PgBar.Value = 0.0;
		Label_Percentage.Content = "0%";
		if (binFile.FirmwareType == 3)
		{
			switch (CheckBatteryStatus())
			{
			case 0:
				caption = "Rx Disconnected Warning";
				button = MessageBoxButton.OK;
				icon = MessageBoxImage.Exclamation;
				MessageBox.Show("Please check if the RX is connected.", caption, button, icon, MessageBoxResult.OK);
				Activate();
				Focus();
				return;
			case 1:
				if (!CheckBatteryLevel())
				{
					string messageBoxText = string.Format("Current battery level is below {0}. Please charge the device above {0} before updating.", batteryThreshold);
					caption = "Battery Warning";
					button = MessageBoxButton.OK;
					icon = MessageBoxImage.Exclamation;
					MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
					Activate();
					Focus();
					return;
				}
				break;
			case 2:
			case 3:
				rxConnected = true;
				break;
			}
		}
		Console.WriteLine("Updating Check PASS");
		FirmwareUpdate();
	}

	private async void FirmwareUpdate()
	{
		await Task.Run(() =>
		{
			Dispatcher.BeginInvoke((Action)(() =>
			{
				Button_SelectFile.IsEnabled = false;
				Button_Update.IsEnabled = false;
				Btn_Close.IsEnabled = false;
				Grid_UpdateWarning.Visibility = Visibility.Visible;
			}));
			FirmwareUpdateSDK.CmErr updateCmErr = FirmwareUpdateSDK.CmErr.CE_UpdateError;
			FirmwareUpdateSDK.SetEventSettings(CreateEventSettings(doReg: true), out var cmErr);
			FirmwareUpdateSDK.StartUpdating(deviceGuid, fwFullpath, 120000, out updateCmErr);
			FirmwareUpdateSDK.SetEventSettings(CreateEventSettings(doReg: false), out cmErr);
			Dispatcher.BeginInvoke((Action)(() =>
			{
				Grid_UpdateWarning.Visibility = Visibility.Hidden;
				if (updateCmErr == FirmwareUpdateSDK.CmErr.CE_UpdateError || updateCmErr == FirmwareUpdateSDK.CmErr.CE_DeviceError)
				{
					string caption = "Update Error";
					MessageBoxButton button = MessageBoxButton.OK;
					MessageBoxImage icon = MessageBoxImage.Exclamation;
					MessageBox.Show("Update Failed! Please check device status and bin file.", caption, button, icon, MessageBoxResult.OK);
					Activate();
					Focus();
				}
			}));
		});
	}

	private bool GetBinFileFirmwareInformation()
	{
		StringBuilder sb = new StringBuilder();
		bool isBinOK = false;
		FirmwareUpdateSDK.CmFirmwareIC fwIC = FirmwareUpdateSDK.CmFirmwareIC.CFI_CM2206;
		binFile = default;
		if (FirmwareUpdateSDK.GetProperty(fwFullpath, "ProjectID", out Guid[] projectID, out FirmwareUpdateSDK.CmErr cmErr, fwIC) && FirmwareUpdateSDK.GetProperty(fwFullpath, "FirmwareVersion", out string[] firmwareVersion, out cmErr, fwIC))
		{
			int[] firmwareType = new int[0];
			if (projectID.Length != 0)
			{
				if (projectID[0] == guidOtaDongle)
				{
					firmwareType = new int[1] { 2 };
				}
				else if (projectID[0] == guidOtaHeadset)
				{
					firmwareType = new int[1] { 3 };
				}
			}
			if (projectID.Length != 0)
			{
				binFile.ProjectID = projectID[0];
			}
			binFile.ModelName = "MCHOSE G9 V2";
			if (firmwareVersion.Length != 0)
			{
				binFile.FirmwareVersion = firmwareVersion[0];
			}
			if (firmwareType.Length != 0)
			{
				binFile.FirmwareType = firmwareType[0];
			}
			if (binFile.ProjectID == Guid.Empty || binFile.FirmwareVersion.Equals(""))
			{
				string caption = "Bin File Error";
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Hand;
				MessageBox.Show("This file CAN NOT be updated to current device, Please check the file.", caption, button, icon, MessageBoxResult.OK);
				Activate();
				Focus();
			}
			else
			{
				sb.AppendFormat("GetProperty: {0}", fwFullpath).AppendLine();
				sb.AppendFormat("    ProjectID      : {0}", projectID[0]).AppendLine();
				sb.AppendFormat("    FirmwareVersion: {0}", firmwareVersion[0]).AppendLine();
				Console.WriteLine(sb.ToString());
				Dispatcher.BeginInvoke((Action)(() =>
				{
					Label_BinName.Content = new TextBlock
					{
						Text = Path.GetFileName(fwFullpath)
					};
					Label_BinDeviceName.Content = new TextBlock
					{
						Text = binFile.ModelName
					};
					if (binFile.FirmwareType == 2)
					{
						Label_BinType.Content = "TX";
					}
					else if (binFile.FirmwareType == 3)
					{
						Label_BinType.Content = "RX";
					}
					Label_BinFwVer.Content = new TextBlock
					{
						Text = binFile.FirmwareVersion
					};
				}));
				isBinOK = true;
			}
		}
		return isBinOK;
	}

	private bool GetDeviceFirmwareInformation()
	{
		bool isOK = false;
		StringBuilder sb = new StringBuilder();
		rxConnected = false;
		if (FirmwareUpdateSDK.GetDevices(14391, 24581, out var devUIDs, out var cmErr))
		{
			if (devUIDs.Length == 1)
			{
				deviceGuid = devUIDs[0];
				FirmwareUpdateSDK.CmFirmwareType[] fwTypes = new FirmwareUpdateSDK.CmFirmwareType[2]
				{
					FirmwareUpdateSDK.CmFirmwareType.CFT_TXDevice,
					FirmwareUpdateSDK.CmFirmwareType.CFT_RXDevice
				};
				int retryCount = 0;
				Guid[] projectIDs;
				string[] firmwareVersions;
				int[] firmwareTypes;
				do
				{
					FirmwareUpdateSDK.GetProperty(devUIDs[0], "ProjectID", out projectIDs, out cmErr, fwTypes);
					FirmwareUpdateSDK.GetProperty(devUIDs[0], "FirmwareVersion", out firmwareVersions, out cmErr, fwTypes);
					FirmwareUpdateSDK.GetProperty(devUIDs[0], "FirmwareType", out firmwareTypes, out cmErr, fwTypes);
					retryCount++;
				}
				while ((projectIDs.Length < 2 || firmwareVersions.Length < 2 || firmwareTypes.Length < 2) && retryCount < 3);
				if (projectIDs.Length == 2 && firmwareVersions.Length == 2 && firmwareTypes.Length == 2)
				{
					devInfo.Clear();
					for (int i = 0; i < 2; i++)
					{
						FirmwareInfo dev = new FirmwareInfo
						{
							FirmwareType = firmwareTypes[i],
							ProjectID = projectIDs[i],
							ModelName = "MCHOSE G9 V2",
							FirmwareVersion = firmwareVersions[i]
						};
						devInfo.Add(dev);
						sb.AppendFormat("GetProperty: {0}", fwTypes[i]).AppendLine();
						sb.AppendFormat("    ProjectID      : {0}", projectIDs[i]).AppendLine();
						sb.AppendFormat("    FirmwareVersion: {0}", firmwareVersions[i]).AppendLine();
						sb.AppendFormat("    FirmwareType   : {0}", (FirmwareUpdateSDK.CmFirmwareType)firmwareTypes[i]).AppendLine();
					}
					Console.WriteLine(sb.ToString());
					Dispatcher.BeginInvoke((Action)(() =>
					{
						Label_DevTxName.Content = new TextBlock
						{
							Text = devInfo[0].ModelName
						};
						Label_DevTxFwVer.Content = new TextBlock
						{
							Text = devInfo[0].FirmwareVersion
						};
						if (devInfo[1].ModelName.Equals(""))
						{
							Label_DevRxName.Content = new TextBlock
							{
								Text = notAvailable
							};
						}
						else
						{
							Label_DevRxName.Content = new TextBlock
							{
								Text = devInfo[1].ModelName
							};
						}
						if (devInfo[1].FirmwareVersion.Equals("0.0.0.0_0000000000") || devInfo[1].FirmwareVersion.Equals(""))
						{
							Label_DevRxFwVer.Content = new TextBlock
							{
								Text = notAvailable
							};
						}
						else
						{
							Label_DevRxFwVer.Content = new TextBlock
							{
								Text = devInfo[1].FirmwareVersion
							};
						}
					}));
					isOK = true;
				}
				else
				{
					string caption = "Device Error";
					MessageBoxButton button = MessageBoxButton.OK;
					MessageBoxImage icon = MessageBoxImage.Exclamation;
					MessageBox.Show("Get Device Infomation Failed, Please check device's status", caption, button, icon, MessageBoxResult.OK);
					Activate();
					Focus();
				}
			}
			else if (devUIDs.Length > 1)
			{
				string caption2 = "Device Error";
				MessageBoxButton button2 = MessageBoxButton.OK;
				MessageBoxImage icon2 = MessageBoxImage.Exclamation;
				MessageBox.Show("Multiple devices have been detected. Please remove devices you DO NOT wish to update.", caption2, button2, icon2, MessageBoxResult.OK);
				Activate();
				Focus();
			}
			else
			{
				string caption3 = "Device Error";
				MessageBoxButton button3 = MessageBoxButton.OK;
				MessageBoxImage icon3 = MessageBoxImage.Exclamation;
				MessageBox.Show("Get Device Infomation Failed, Please check device's status", caption3, button3, icon3, MessageBoxResult.OK);
				Activate();
				Focus();
			}
		}
		return isOK;
	}

	private void EventCallbackDispatcher(FirmwareUpdateSDK.CmCallbackType cbType, Guid reserved, FirmwareUpdateSDK.CmDataType datType, int arrLen, IntPtr arrDat, IntPtr usrDat)
	{
		if (!(usrDat != IntPtr.Zero) || !(usrDat == hWnd) || !FirmwareUpdateSDK.ToCSWrappedArray(datType, arrLen, arrDat, out var csArrDat))
		{
			return;
		}
		switch (cbType)
		{
		case FirmwareUpdateSDK.CmCallbackType.CCT_FirmwareUpdatingState:
			Dispatcher.BeginInvoke((Action)(() =>
			{
				Console.WriteLine("Update State: {0}", ((FirmwareUpdateSDK.CmFirmwareUpdatingState[])csArrDat)[0].ToString());
				string text = ((FirmwareUpdateSDK.CmFirmwareUpdatingState[])csArrDat)[0].ToString();
				int num = text.IndexOf('_');
				string text2 = text.Substring(num + 1);
				try
				{
					string content = (string)FindResource($"Label_UpdatingState_{text2}");
					Label_Status.Content = content;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Get updating state failed: {0}", ex.ToString());
					Label_Status.Content = text2;
				}
				if (((FirmwareUpdateSDK.CmFirmwareUpdatingState[])csArrDat)[0] == FirmwareUpdateSDK.CmFirmwareUpdatingState.CFUS_Finished)
				{
					Button_Update.Visibility = Visibility.Hidden;
					Button_Done.Visibility = Visibility.Visible;
					Btn_Close.IsEnabled = true;
					Grid_UpdateWarning.Visibility = Visibility.Hidden;
				}
			}));
			break;
		case FirmwareUpdateSDK.CmCallbackType.CCT_FirmwareDataTransmittedRatio:
			Dispatcher.BeginInvoke((Action)(() =>
			{
				PgBar.Value = ((float[])csArrDat)[0] * 100f;
				int num = (int)PgBar.Value;
				Label_Percentage.Content = $"{num.ToString()}%";
			}));
			break;
		}
	}

	private FirmwareUpdateSDK.EventSettings CreateEventSettings(bool doReg)
	{
		return Dispatcher.Invoke(() => new FirmwareUpdateSDK.EventSettings
		{
			Callback = (doReg ? EventCallbackFuncPtr : null),
			UserData = ((doReg && Window.GetWindow(this) != null) ? hWnd : IntPtr.Zero)
		});
	}

	private void LogCallbackDispatcher(FirmwareUpdateSDK.CmCallbackType cbType, Guid audEndptUID, FirmwareUpdateSDK.CmDataType datType, int arrLen, IntPtr arrDat, IntPtr usrDat)
	{
		if (cbType == FirmwareUpdateSDK.CmCallbackType.CCT_Log && usrDat != IntPtr.Zero && FirmwareUpdateSDK.ToCSWrappedArray(datType, arrLen, arrDat, out var csArrDat))
		{
			Console.WriteLine(((string[])csArrDat)[0]);
		}
	}

	private void CloseWindow()
	{
		Close();
		Owner.Focus();
	}

	private void Btn_Close_Click(object sender, RoutedEventArgs e)
	{
		CloseWindow();
	}

	private void Grid_Title_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	private async void Button_Test_Click(object sender, RoutedEventArgs e)
	{
		await Task.Run(() =>
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (FirmwareUpdateSDK.GetDevices(14391, 24581, out var devUIDs, out var cmErr) && devUIDs.Length == 1)
			{
				deviceGuid = devUIDs[0];
				FirmwareUpdateSDK.GetProperty(devUIDs[0], "BatteryLevel", out float outDat, out cmErr);
				stringBuilder.AppendFormat("GetProperty: {0}", devUIDs[0]).AppendLine();
				stringBuilder.AppendFormat("    BatteryLevel      : {0}", outDat).AppendLine();
				Console.WriteLine(stringBuilder.ToString());
			}
		});
	}

	private bool CheckBatteryLevel()
	{
		bool isOK = false;
		StringBuilder sb = new StringBuilder();
		if (FirmwareUpdateSDK.GetDevices(14391, 24581, out var devUIDs, out var cmErr) && devUIDs.Length == 1)
		{
			deviceGuid = devUIDs[0];
			if (FirmwareUpdateSDK.GetProperty(devUIDs[0], "BatteryLevel", out float Battery, out cmErr))
			{
				sb.AppendFormat("GetProperty: {0}", devUIDs[0]).AppendLine();
				sb.AppendFormat("    BatteryLevel      : {0}", Battery).AppendLine();
				Console.WriteLine(sb.ToString());
				isOK = (int)((double)Battery * 100.0) > batteryThreshold;
			}
		}
		return isOK;
	}

	private int CheckBatteryStatus()
	{
		int status = 0;
		StringBuilder sb = new StringBuilder();
		if (FirmwareUpdateSDK.GetDevices(14391, 24581, out var devUIDs, out var cmErr) && devUIDs.Length == 1)
		{
			deviceGuid = devUIDs[0];
			if (FirmwareUpdateSDK.GetProperty(devUIDs[0], "BatteryStatus", out status, out cmErr))
			{
				sb.AppendFormat("GetProperty: {0}", devUIDs[0]).AppendLine();
				sb.AppendFormat("    BatteryStatus      : {0}", status).AppendLine();
				Console.WriteLine(sb.ToString());
			}
		}
		return status;
	}
}
