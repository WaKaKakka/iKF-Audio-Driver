using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cmedia;
using Newtonsoft.Json.Linq;
using XearONE.Contracts;
using XearONE.UI.Driver.DeviceInfo;
using XearONE.UI.Driver.Profiles;
using XearONE.UI.Driver.Settings;
using XearONE.UI.Helpers;

namespace XearONE.UI.Driver;

public class DriverInstance
{
	private const int ReportLength = 16;

	private const int EffectCmdLength = 2;

	private const byte HIDreportID = 237;

	private const byte reportID_XSH = 2;

	private const byte reportID_LED = 11;

	private const byte reportID_AINR = 14;

	private const byte reportID_HeadTracking = 21;

	private const int reportID = 85;

	private const int cmdID = 111;

	private const int batteryID = 101;

	private const byte cmdReadFactoryCalibration = 128;

	private const byte cmdTransferData = 64;

	private const byte cmdAccNewCalibrationData = 32;

	private const byte cmdGyroNewCalibrationData = 16;

	private const byte cmdFilter = 8;

	private const byte cmdCalibrationAcc = 4;

	private const byte cmdCalibrationGyro = 2;

	private const byte cmdDataTypeIsQuat = 1;

	private bool IsDataTransfer;

	private AutoResetEvent manualEvent = new AutoResetEvent(initialState: false);

	private Timer startTimer;

	private bool isQuaternion = true;

	private const int startCommandTimeOutInterval = 5000;

	private Guid[] audioEndpointList;

	private List<JackDeviceInfo> JackDeviceList = new List<JackDeviceInfo>();

	private List<HIDDeviceInfo> HIDDeviceList = new List<HIDDeviceInfo>();

	public (Guid endpointUID, IntPtr HIDObj, int OutputReportByteLength) headtrackingDevice = (endpointUID: Guid.NewGuid(), HIDObj: IntPtr.Zero, OutputReportByteLength: 0);

	private List<string> DeviceOrderList = new List<string>();

	private readonly object JackDeviceListLocker = new object();

	private readonly object HIDDeviceListLocker = new object();

	private const string inputCapCount = "InputValueCapsCount";

	private const string inputCapID = "InputValueCapReportID";

	private const string outputCapCount = "OutputValueCapsCount";

	private const string outputCapID = "OutputValueCapReportID";

	private const byte XearHIDReportID = 237;

	private const byte XearLEDReportID = byte.MaxValue;

	private const byte HeadTrackingHIDReportID = 85;

	private int CM108DHIDDeviceCount;

	private int XearHIDDeviceCount;

	private int XearLEDDeviceCount;

	private int HeadTrackingHIDDeviceCount;

	private int lastApoYaw = -999;

	private int lastApoPitch = -999;

	private int lastApoRoll = -999;

	private List<XearSDK.CmLicenseCategory> PlaybackEffectList = new List<XearSDK.CmLicenseCategory>
	{
		XearSDK.CmLicenseCategory.CLC_HeadTracking,
		XearSDK.CmLicenseCategory.CLC_MultiChannelEQ,
		XearSDK.CmLicenseCategory.CLC_Equalizer10Bands,
		XearSDK.CmLicenseCategory.CLC_EnvironmentEffects,
		XearSDK.CmLicenseCategory.CLC_SpeakerShifter,
		XearSDK.CmLicenseCategory.CLC_XearSingFX_KeyShifting,
		XearSDK.CmLicenseCategory.CLC_XearSingFX_VocalFading,
		XearSDK.CmLicenseCategory.CLC_XearSurroundHeadphone,
		XearSDK.CmLicenseCategory.CLC_XearAudioBrilliant,
		XearSDK.CmLicenseCategory.CLC_XearDynamicBass,
		XearSDK.CmLicenseCategory.CLC_XearSmartVolume,
		XearSDK.CmLicenseCategory.CLC_XearVoiceClarity,
		XearSDK.CmLicenseCategory.CLC_SurroundMax,
		XearSDK.CmLicenseCategory.CLC_AINoiseReduction
	};

	private List<XearSDK.CmLicenseCategory> CaptureEffectList = new List<XearSDK.CmLicenseCategory>
	{
		XearSDK.CmLicenseCategory.CLC_AIMagicVoice,
		XearSDK.CmLicenseCategory.CLC_Equalizer10Bands,
		XearSDK.CmLicenseCategory.CLC_XearSingFX_Echo,
		XearSDK.CmLicenseCategory.CLC_XearSingFX_MagicVoice,
		XearSDK.CmLicenseCategory.CLC_NoiseReduction,
		XearSDK.CmLicenseCategory.CLC_HDNoiseReduction,
		XearSDK.CmLicenseCategory.CLC_AINoiseReduction
	};

	private Dictionary<XearSDK.CmLicenseCategory, string> PlaybackPropertyMap = new Dictionary<XearSDK.CmLicenseCategory, string>
	{
		{
			XearSDK.CmLicenseCategory.CLC_AINoiseReduction,
			"AI_Playback_NR_Enable"
		},
		{
			XearSDK.CmLicenseCategory.CLC_EnvironmentEffects,
			"Enable_RFX_GFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_Equalizer10Bands,
			"Enable_EQ_GFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_SpeakerShifter,
			"Enable_SS_LFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_SurroundMax,
			"Enable_Channel_COPY_LFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearAudioBrilliant,
			"Enable_AUDIOBRILLIANT_LFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearDynamicBass,
			"Enable_VIRTUALBASS_LFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearSingFX_KeyShifting,
			"Enable_KEYSHIFT_GFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearSingFX_VocalFading,
			"Enable_VOCALCANCEL_GFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearSmartVolume,
			"Enable_ADAPTIVEVOLUME_GFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearSurroundHeadphone,
			"Enable_CONLAB_HPSPK_LFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearVoiceClarity,
			"Enable_VOICECLARITY_LFX"
		},
		{
			XearSDK.CmLicenseCategory.CLC_HeadTracking,
			"HeadTracking_Enable"
		}
	};

	private Dictionary<XearSDK.CmLicenseCategory, string> CapturePropertyMap = new Dictionary<XearSDK.CmLicenseCategory, string>
	{
		{
			XearSDK.CmLicenseCategory.CLC_NoiseReduction,
			"NR_Enable"
		},
		{
			XearSDK.CmLicenseCategory.CLC_HDNoiseReduction,
			"HD_NR_Enable"
		},
		{
			XearSDK.CmLicenseCategory.CLC_AINoiseReduction,
			"AI_NR_Enable"
		},
		{
			XearSDK.CmLicenseCategory.CLC_Equalizer10Bands,
			"MicEQ_Enable"
		},
		{
			XearSDK.CmLicenseCategory.CLC_Compressor,
			"Compressor_Enable"
		},
		{
			XearSDK.CmLicenseCategory.CLC_NoiseGate,
			"NoiseGate_Enable"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearSingFX_Echo,
			"Enable_MICECHO"
		},
		{
			XearSDK.CmLicenseCategory.CLC_XearSingFX_MagicVoice,
			"Enable_MAGICVOICE"
		},
		{
			XearSDK.CmLicenseCategory.CLC_AIMagicVoice,
			"AI_MagicVoice_Enable"
		}
	};

	private Dictionary<string, string> PlaybackProfilePropertyMap = new Dictionary<string, string>
	{
		{ "AINoiseReduction", "AI_Playback_NR_Enable" },
		{ "AINoiseReduction_Level", "AI_Playback_NR_Level" },
		{ "EnvironmentEffects", "Enable_RFX_GFX" },
		{ "EnvironmentEffects_Mode", "RFX_ENVIRONMENT" },
		{ "EnvironmentEffects_Room", "RFX_ROOMSIZE" },
		{ "Equalizer10Bands", "Enable_EQ_GFX" },
		{ "EqBnad1", "EQ_Slider1" },
		{ "EqBnad2", "EQ_Slider2" },
		{ "EqBnad3", "EQ_Slider3" },
		{ "EqBnad4", "EQ_Slider4" },
		{ "EqBnad5", "EQ_Slider5" },
		{ "EqBnad6", "EQ_Slider6" },
		{ "EqBnad7", "EQ_Slider7" },
		{ "EqBnad8", "EQ_Slider8" },
		{ "EqBnad9", "EQ_Slider9" },
		{ "EqBnad10", "EQ_Slider10" },
		{ "SpeakerShifter", "Enable_SS_LFX" },
		{ "LF_ANGLE", "SS_LF_ANGLE" },
		{ "RF_ANGLE", "SS_RF_ANGLE" },
		{ "LS_ANGLE", "SS_LS_ANGLE" },
		{ "RS_ANGLE", "SS_RS_ANGLE" },
		{ "LB_ANGLE", "SS_LB_ANGLE" },
		{ "RB_ANGLE", "SS_RB_ANGLE" },
		{ "C_ANGLE", "SS_C_ANGLE" },
		{ "LFE_ANGLE", "SS_LFE_ANGLE" },
		{ "LF_DIST", "SS_LF_DIST" },
		{ "RF_DIST", "SS_RF_DIST" },
		{ "LS_DIST", "SS_LS_DIST" },
		{ "RS_DIST", "SS_RS_DIST" },
		{ "LB_DIST", "SS_LB_DIST" },
		{ "RB_DIST", "SS_RB_DIST" },
		{ "C_DIST", "SS_C_DIST" },
		{ "LFE_DIST", "SS_LFE_DIST" },
		{ "SurroundMax", "Enable_Channel_COPY_LFX" },
		{ "XearAudioBrilliant", "Enable_AUDIOBRILLIANT_LFX" },
		{ "XearAudioBrilliant_Level", "AUDIOBRILLIANT_LEVEL" },
		{ "XearDynamicBass", "Enable_VIRTUALBASS_LFX" },
		{ "XearDynamicBass_Level", "VIRTUALBASS_Level" },
		{ "XearDynamicBass_Freq", "VIRTUALBASS_CutOffFrequency" },
		{ "XearSingFX_KeyShifting", "Enable_KEYSHIFT_GFX" },
		{ "XearSingFX_KeyShifting_Level", "KEYSHIFT_LEVEL" },
		{ "XearSingFX_VocalFading", "Enable_VOCALCANCEL_GFX" },
		{ "XearSingFX_VocalFading_Level", "VOCALCANCEL_LEVEL" },
		{ "XearSmartVolume", "Enable_ADAPTIVEVOLUME_GFX" },
		{ "XearSmartVolume_Level", "ADAPTIVEVOLUME_LEVEL" },
		{ "XearSmartVolume_Mode", "ADAPTIVEVOLUME_MODE" },
		{ "XearSurroundHeadphone", "Enable_CONLAB_HPSPK_LFX" },
		{ "XearSurroundHeadphone_Mode", "CONLAB_HPSPK_MODE" },
		{ "XearSurroundHeadphone_Room", "CONLAB_HPSPK_ROOMSIZE" },
		{ "XearVoiceClarity", "Enable_VOICECLARITY_LFX" },
		{ "XearVoiceClarity_Level", "VOICECLARITY_LEVEL" },
		{ "XearVoiceClarity_NSLevel", "VOICECLARITY_NOISESUPP_LEVEL" },
		{ "HeadTracking", "HeadTracking_Enable" }
	};

	private Dictionary<string, string> CaptureProfilePropertyMap = new Dictionary<string, string>
	{
		{ "NoiseReduction", "NR_Enable" },
		{ "NoiseReduction_Level", "NR_Level" },
		{ "HDNoiseReduction", "HD_NR_Enable" },
		{ "AINoiseReduction", "AI_NR_Enable" },
		{ "AINoiseReduction_Level", "AI_NR_Level" },
		{ "Equalizer10Bands", "MicEQ_Enable" },
		{ "EqBnad1", "MicEQ_Slider1" },
		{ "EqBnad2", "MicEQ_Slider2" },
		{ "EqBnad3", "MicEQ_Slider3" },
		{ "EqBnad4", "MicEQ_Slider4" },
		{ "EqBnad5", "MicEQ_Slider5" },
		{ "EqBnad6", "MicEQ_Slider6" },
		{ "EqBnad7", "MicEQ_Slider7" },
		{ "EqBnad8", "MicEQ_Slider8" },
		{ "EqBnad9", "MicEQ_Slider9" },
		{ "EqBnad10", "MicEQ_Slider10" },
		{ "Compressor", "Compressor_Enable" },
		{ "NoiseGate", "NoiseGate_Enable" },
		{ "LDR", "NR_Enable" },
		{ "XearSingFX_Echo", "Enable_MICECHO" },
		{ "XearSingFX_Echo_Level", "MICECHO_Level" },
		{ "XearSingFX_MagicVoice", "Enable_MAGICVOICE" },
		{ "XearSingFX_MagicVoice_Mode", "MagicVoice_Selection" },
		{ "AIMagicVoice", "AI_MagicVoice_Enable" }
	};

	private bool isEnable = true;

	private byte colorR;

	private byte colorG;

	private byte colorB;

	private int arraySize = 128;

	private byte[] tableR = new byte[128];

	private byte[] tableG = new byte[128];

	private byte[] tableB = new byte[128];

	private int mode;

	private int speed = 50;

	private const int ledReportLength = 16;

	private const int ledReportID = 255;

	private const int ledStartID = 1;

	private const int ledStopID = 2;

	private const int ledConfigID = 3;

	private const int ledTableID = 4;

	private const int ledSaveID = 5;

	public byte[] redColor = new byte[128]
	{
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 190, 180, 170, 160, 150, 140, 130, 120, 100,
		90, 80, 70, 60, 50, 40, 30, 20, 10, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
		110, 120, 130, 140, 150, 160, 170, 180, 190, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200
	};

	public byte[] greenColor = new byte[128]
	{
		10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
		110, 120, 130, 140, 150, 160, 170, 180, 190, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 190, 180, 170, 160, 150, 140, 130, 120, 100,
		90, 80, 70, 60, 50, 40, 30, 20, 10, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0
	};

	public byte[] blueColor = new byte[128]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
		110, 120, 130, 140, 150, 160, 170, 180, 190, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 190, 180, 170, 160, 150, 140, 130, 120, 100,
		90, 80, 70, 60, 50, 40, 30, 20, 10, 0,
		0, 0, 0, 0, 0, 0, 0, 0
	};

	private const string contDefault = "Default";

	private const string contCurrent = "Current";

	private readonly object HIDSDKObjLock = new object();

	private IntPtr HIDsdkObj = IntPtr.Zero;

	public Dictionary<string, string> existLanguages = new Dictionary<string, string>();

	public Dictionary<string, string> langDisplayName = new Dictionary<string, string>();

	private readonly string sectionEQProfile = "EQ_Profile";

	private readonly string profileNameKey = "Name";

	private readonly string bandKey = "Band";

	private readonly string sectionLED = "LED";

	private readonly string keyEnable = "Enable";

	private readonly string keyArraySize = "ArraySize";

	private readonly string keyColorR = "ColorR";

	private readonly string keyColorG = "ColorG";

	private readonly string keyColorB = "ColorB";

	private readonly string keyMode = "Mode";

	private readonly string keySpeed = "Speed";

	private static DriverInstance instance = new DriverInstance();

	private IGuidProvider provider;

	public List<AutoSwitchTemplate> AutoSwitchCollection { get; set; } = new List<AutoSwitchTemplate>();

	public int lastHIDTime { get; set; }

	public string CurrentProfileName { get; set; } = "Default";

	public List<ProfileConfig> ProfileCollection { get; set; } = new List<ProfileConfig>();

	public ConfigManager configManager { get; set; } = new ConfigManager(AppContext.BaseDirectory, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + GetAssemblyCompanyAndProduct());

	public IniFileHelper iniAppTempSetting { get; set; }

	public IniFileHelper iniMagicVoice2 { get; set; }

	public IniFileHelper iniRecordingEffect { get; set; }

	private string appLanguage { get; set; }

	public void InitAutoSwitch()
	{
		LoadAutoSwitchConfig();
	}

	private void LoadAutoSwitchConfig()
	{
		if (!File.Exists(configManager.GetAutoSwitchConfigPath()))
		{
			InitDefaultAutoSwitchConfig();
		}
		AutoSwitchCollection.Clear();
		AutoSwitchCollection = configManager.LoadAutoSwitch();
	}

	public void SaveAutoSwitchConfig()
	{
		configManager.SaveAutoSwitch(AutoSwitchCollection);
	}

	public void AddNewAutoSwitchConfig(AutoSwitchTemplate template)
	{
		AutoSwitchCollection.Add(template);
	}

	public void DeleteAutoSwitchConfig(AutoSwitchTemplate template)
	{
		string iconPath = Path.Combine(configManager.GetAutoSwitchIconPath(), template.icon);
		if (File.Exists(iconPath))
		{
			try
			{
				if (File.Exists(iconPath))
				{
					File.Delete(iconPath);
				}
			}
			catch (IOException)
			{
			}
		}
		AutoSwitchCollection.Remove(template);
		SaveAutoSwitchConfig();
	}

	public string LoadAutoSwitchIcon(string iconName)
	{
		return Path.Combine(configManager.GetAutoSwitchIconPath(), iconName);
	}

	public void SaveAutoSwitchIconAsPng(ImageSource imageSource, string iconName)
	{
		string path = Path.Combine(configManager.GetAutoSwitchIconPath(), iconName);
		if (imageSource == null)
		{
			throw new ArgumentNullException("imageSource");
		}
		if (!(imageSource is BitmapSource bitmapSource))
		{
			throw new ArgumentException("imageSource 必須是 BitmapSource 型別", "imageSource");
		}
		string dir = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}
		PngBitmapEncoder encoder = new PngBitmapEncoder
		{
			Frames = { BitmapFrame.Create(bitmapSource) }
		};
		using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
		encoder.Save(stream);
	}

	private void InitDefaultAutoSwitchConfig()
	{
		if (!Directory.Exists(configManager.GetAutoSwitchIconPath()))
		{
			Directory.CreateDirectory(configManager.GetAutoSwitchIconPath());
		}
	}

	public void BindProfileToDefault(string profileName)
	{
		foreach (AutoSwitchTemplate item in AutoSwitchCollection.Where((AutoSwitchTemplate x) => x.bindingProfileName == profileName))
		{
			item.bindingProfileName = "Default";
		}
	}

	private void InitCM108DHID()
	{
		LogHelper.Trace("InitCM108DHID", "InitCM108DHID", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.CMA181HIDCommand.cs");
		int devCount = HIDDeviceList.Count;
		for (int i = 0; i < devCount; i++)
		{
			LogHelper.Debug($"Init CM108D HID device index: {i}", isPrintCaller: false, "InitCM108DHID", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.CMA181HIDCommand.cs");
			if (!HIDDeviceList[i].isSupportCM108DHID)
			{
				continue;
			}
			LogHelper.Debug($"Device {i} isSupportCM108DHID", isPrintCaller: false, "InitCM108DHID", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.CMA181HIDCommand.cs");
			byte[] writeData = new byte[5];
			SetProperty(HIDDeviceList[i].hidEndpointObj, "SetOutputReport", writeData);
			foreach (JackDeviceInfo device in JackDeviceList)
			{
				if (!device.usbDeviceInstanceID.Equals(HIDDeviceList[i].usbDeviceInstanceID) || device.dataFlow != XearSDK.CmDataFlow.CDF_Render || !device.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_SpeakerShifter))
				{
					continue;
				}
				if (XearSDK.GetProperty(device.audEndptUID, "Enable_SS_LFX", out int outDat, out XearSDK.CmErr _))
				{
					writeData = new byte[5] { 0, 0, 2, 0, 0 };
					if (outDat == 1)
					{
						writeData[3] = 2;
						SetProperty(HIDDeviceList[i].hidEndpointObj, "SetOutputReport", writeData);
					}
					else
					{
						writeData[3] = 0;
						SetProperty(HIDDeviceList[i].hidEndpointObj, "SetOutputReport", writeData);
					}
				}
				break;
			}
		}
	}

	public void OnHID_PropertyChanged(IntPtr hidObj)
	{
		if (hidObj == IntPtr.Zero)
		{
			return;
		}
		int devCount = HIDDeviceList.Count;
		for (int i = 0; i < devCount; i++)
		{
			if (!(HIDDeviceList[i].hidEndpointObj == hidObj))
			{
				continue;
			}
			{
				foreach (JackDeviceInfo device in JackDeviceList)
				{
					if (device.usbDeviceInstanceID.Equals(HIDDeviceList[i].usbDeviceInstanceID) && device.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_SpeakerShifter))
					{
						OnHID_PropertyControl(device.audEndptUID, hidObj);
						break;
					}
				}
				break;
			}
		}
	}

	private void OnHID_PropertyControl(Guid audEndptUID, IntPtr hidObj)
	{
		int isEnable = 0;
		if (XearSDK.GetProperty(audEndptUID, "Enable_SS_LFX", out int outDat, out XearSDK.CmErr cmErr))
		{
			isEnable = ((outDat == 0) ? 1 : 0);
			XearSDK.SetProperty(audEndptUID, "Enable_SS_LFX", isEnable, out cmErr);
			byte[] writeData = new byte[5] { 0, 0, 2, 0, 0 };
			if (isEnable == 1)
			{
				writeData[3] = 2;
				SetProperty(hidObj, "SetOutputReport", writeData);
			}
			else
			{
				writeData[3] = 0;
				SetProperty(hidObj, "SetOutputReport", writeData);
			}
		}
	}

	private void InitHIDEffect()
	{
		LogHelper.Trace("InitHIDEffect", "InitHIDEffect", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.CMediaHIDCommand.cs");
		foreach (HIDDeviceInfo device in HIDDeviceList.Where((HIDDeviceInfo x) => x.isSupportXearHID).ToList())
		{
			LogHelper.Debug($"Device {device.hidEndpointObj} isSupportXearHID", isPrintCaller: false, "InitHIDEffect", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.CMediaHIDCommand.cs");
			string string50 = string.Empty;
			JackDeviceInfo jackDevice = JackDeviceList.Find((JackDeviceInfo x) => x.usbDeviceInstanceID == device.usbDeviceInstanceID);
			if (jackDevice != null)
			{
				string50 = jackDevice.string50;
			}
			if (string50.Equals(""))
			{
				LogHelper.Debug("Device Supported XearHID, but not CMedia's device", isPrintCaller: false, "InitHIDEffect", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.CMediaHIDCommand.cs");
			}
			else
			{
				RebuildSupportedList(device);
			}
		}
	}

	private void RebuildSupportedList(HIDDeviceInfo devInfo)
	{
		if (devInfo.supportedHIDEffect.Length != 0)
		{
			return;
		}
		SetProperty(value: new byte[16]
		{
			237, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		}, hidEndpointObj: devInfo.hidEndpointObj, property: "SetOutputReport");
		byte[] getFeature = new byte[16]
		{
			237, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		GetProperty(devInfo.hidEndpointObj, "GetFeature", ref getFeature);
		if (getFeature[0] == 237 && getFeature[1] != 0)
		{
			int dataLength = getFeature[1];
			devInfo.supportedHIDEffect = new byte[dataLength];
			for (int i = 0; i < dataLength; i++)
			{
				devInfo.supportedHIDEffect[i] = getFeature[2 + i];
			}
		}
		else
		{
			devInfo.supportedHIDEffect = new byte[0];
		}
	}

	public byte[] GetSupportedList(IntPtr hidObj)
	{
		foreach (HIDDeviceInfo device in HIDDeviceList)
		{
			if (device.hidEndpointObj == hidObj)
			{
				return (byte[])device.supportedHIDEffect.Clone();
			}
		}
		return new byte[0];
	}

	public void OnHID_PropertyChanged(IntPtr hidObj, byte effect)
	{
		if (hidObj == IntPtr.Zero)
		{
			return;
		}
		_ = string.Empty;
		HIDDeviceInfo hidDevice = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		JackDeviceInfo jackDevice = JackDeviceList.Find((JackDeviceInfo x) => x.usbDeviceInstanceID == hidDevice.usbDeviceInstanceID);
		if (hidDevice == null || jackDevice == null)
		{
			return;
		}
		_ = jackDevice.string50;
		GetHIDStatus(hidObj, effect, out var data);
		if (data.Length != 0)
		{
			PrintByte(data, "OnHID_PropertyChanged");
			switch (effect)
			{
			case 2:
				OnHID_PropertyControl(hidObj, XearSDK.CmLicenseCategory.CLC_XearSurroundHeadphone, "Enable_CONLAB_HPSPK_LFX", data[0]);
				break;
			case 14:
				OnHID_PropertyControl(hidObj, XearSDK.CmLicenseCategory.CLC_AINoiseReduction, "AI_NR_Enable", data[0]);
				break;
			case 21:
				OnHID_PropertyControl(hidObj, XearSDK.CmLicenseCategory.CLC_HeadTracking, "HeadTracking_Enable", data[0]);
				break;
			default:
				Console.WriteLine("No HID handle.");
				break;
			}
		}
	}

	private void OnHID_PropertyControl(IntPtr hidObj, XearSDK.CmLicenseCategory lic, string propertyKey, byte isEnable)
	{
		foreach (JackDeviceInfo device in JackDeviceList.Where((JackDeviceInfo x) => x.CMediaHIDObj == hidObj).ToList())
		{
			if (device.licenseCategoryList.Contains(lic))
			{
				XearSDK.CmErr cmErr;
				if (lic != XearSDK.CmLicenseCategory.CLC_AINoiseReduction)
				{
					XearSDK.SetProperty(device.audEndptUID, propertyKey, isEnable, out cmErr);
					break;
				}
				if (device.string50.Equals("CMA196") && device.dataFlow == XearSDK.CmDataFlow.CDF_Render)
				{
					XearSDK.SetProperty(device.audEndptUID, "AI_Playback_NR_Enable", isEnable, out cmErr);
					break;
				}
				if (device.dataFlow == XearSDK.CmDataFlow.CDF_Capture)
				{
					XearSDK.SetProperty(device.audEndptUID, "AI_NR_Enable", isEnable, out cmErr);
					break;
				}
			}
		}
	}

	public void GetHIDStatus(IntPtr hidObj, byte effect, out byte[] status)
	{
		SetProperty(hidObj, "SetOutputReport", new byte[16]
		{
			237, effect, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		});
		byte[] getFeature = new byte[16]
		{
			237, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		GetProperty(hidObj, "GetFeature", ref getFeature);
		if (getFeature[1] != 0 && getFeature[2] == effect)
		{
			int dataLength = getFeature[1] - 1;
			status = new byte[dataLength];
			for (int i = 0; i < dataLength; i++)
			{
				status[i] = getFeature[3 + i];
			}
		}
		else
		{
			status = new byte[0];
		}
	}

	public int GetHIDStatus(Guid audEndptUID, string propertyName)
	{
		int result = -1;
		byte effectCode = 0;
		if (propertyName.Equals("Enable_CONLAB_HPSPK_LFX"))
		{
			effectCode = 2;
		}
		else
		{
			if (!propertyName.Equals("AI_NR_Enable") && !propertyName.Equals("AI_Playback_NR_Enable"))
			{
				return result;
			}
			effectCode = 14;
		}
		IntPtr hidObj = IntPtr.Zero;
		foreach (JackDeviceInfo device in GetDeviceList())
		{
			if (device.audEndptUID == audEndptUID && device.CMediaHIDObj != IntPtr.Zero)
			{
				hidObj = device.CMediaHIDObj;
				break;
			}
		}
		if (hidObj == IntPtr.Zero)
		{
			return result;
		}
		SetProperty(hidObj, "SetOutputReport", new byte[16]
		{
			237, effectCode, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		});
		byte[] getFeature = new byte[16]
		{
			237, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		GetProperty(hidObj, "GetFeature", ref getFeature);
		if (getFeature[1] != 0 && getFeature[2] == effectCode)
		{
			_ = getFeature[1];
			if (getFeature[3] == 0 || getFeature[3] == 1)
			{
				result = getFeature[3];
			}
		}
		else
		{
			result = -1;
		}
		return result;
	}

	public void SetHIDStatus(IntPtr hidObj, string propertyName, byte isEnable)
	{
		byte effect = 0;
		if (propertyName.Equals("Enable_CONLAB_HPSPK_LFX"))
		{
			effect = 2;
		}
		else if (propertyName.Equals("AI_NR_Enable"))
		{
			effect = 14;
		}
		else if (propertyName.Equals("HeadTracking_Enable"))
		{
			effect = 21;
		}
		SetProperty(hidObj, "SetOutputReport", new byte[16]
		{
			237, effect, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		});
		SetProperty(hidObj, "SetFeature", new byte[16]
		{
			237, 2, effect, isEnable, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		});
	}

	public void SetHIDStatus(Guid audEndptUID, string propertyName, byte isEnable)
	{
		IntPtr hidObj = IntPtr.Zero;
		JackDeviceInfo device = GetDeviceList().Find((JackDeviceInfo x) => x.audEndptUID == audEndptUID);
		if (device == null)
		{
			return;
		}
		if (propertyName.Equals("Enable_SS_LFX"))
		{
			if (device.CMedia108DHIDObj != IntPtr.Zero)
			{
				hidObj = device.CMedia108DHIDObj;
				byte[] writeData = new byte[5];
				writeData = new byte[5] { 0, 0, 2, 0, 0 };
				if (isEnable == 1)
				{
					writeData[3] = 2;
					SetProperty(hidObj, "SetOutputReport", writeData);
				}
				else
				{
					writeData[3] = 0;
					SetProperty(hidObj, "SetOutputReport", writeData);
				}
			}
		}
		else if (propertyName.Equals("AI_NR_Enable") || propertyName.Equals("AI_Playback_NR_Enable"))
		{
			if (!(device.CMediaHIDObj != IntPtr.Zero))
			{
				return;
			}
			hidObj = device.CMediaHIDObj;
			byte effect = 0;
			if (device.string50 == "CMA196" && device.dataFlow == XearSDK.CmDataFlow.CDF_Render)
			{
				effect = 14;
			}
			else
			{
				if (device.dataFlow != XearSDK.CmDataFlow.CDF_Capture)
				{
					return;
				}
				effect = 14;
			}
			SetProperty(hidObj, "SetOutputReport", new byte[16]
			{
				237, effect, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0
			});
			SetProperty(hidObj, "SetFeature", new byte[16]
			{
				237, 2, effect, isEnable, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0
			});
		}
		else if (device.CMediaHIDObj != IntPtr.Zero)
		{
			hidObj = device.CMediaHIDObj;
			byte effect2 = 0;
			if (propertyName.Equals("Enable_CONLAB_HPSPK_LFX"))
			{
				effect2 = 2;
			}
			else if (propertyName.Equals("HeadTracking_Enable"))
			{
				effect2 = 21;
			}
			SetProperty(hidObj, "SetOutputReport", new byte[16]
			{
				237, effect2, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0
			});
			SetProperty(hidObj, "SetFeature", new byte[16]
			{
				237, 2, effect2, isEnable, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0
			});
		}
	}

	private void PrintByte(byte[] data, [CallerMemberName] string callerName = "")
	{
		StringBuilder log = new StringBuilder();
		log.Append($"[{callerName}] data: ");
		for (int i = 0; i < data.Length; i++)
		{
			log.Append(data[i].ToString("X2"));
			log.Append(" ");
		}
		Console.WriteLine(log.ToString());
	}

	public void CheckDeviceType()
	{
		Console.WriteLine("CheckDeviceType()");
	}

	public bool StartSendData()
	{
		Console.WriteLine("StartSendData()");
		if (headtrackingDevice.OutputReportByteLength == 17)
		{
			isQuaternion = false;
		}
		byte[] data = new byte[headtrackingDevice.OutputReportByteLength];
		data[0] = 85;
		data[1] = 111;
		byte cmd = 0;
		cmd = (byte)((!isQuaternion) ? 64 : 73);
		data[2] = cmd;
		return SendCommand(data, 200);
	}

	public bool StopSendData()
	{
		Console.WriteLine("StopSendData()");
		byte[] data = new byte[headtrackingDevice.OutputReportByteLength];
		data[0] = 85;
		data[1] = 111;
		byte cmd = 0;
		if (isQuaternion)
		{
			cmd = 9;
		}
		data[2] = cmd;
		SetProperty(headtrackingDevice.HIDObj, "Write", data);
		return false;
	}

	public bool DoCalibrate()
	{
		Console.WriteLine("DoCalibrate()");
		byte[] data = new byte[headtrackingDevice.OutputReportByteLength];
		data[0] = 85;
		data[1] = 111;
		byte cmd = 0;
		cmd = (byte)((!isQuaternion) ? 66 : 75);
		data[2] = cmd;
		return SendCommand(data, 2000);
	}

	public bool RemoveCalibrationData()
	{
		Console.WriteLine("RemoveCalibrationData()");
		byte[] data = new byte[headtrackingDevice.OutputReportByteLength];
		data[0] = 85;
		data[1] = 111;
		byte cmd = 0;
		cmd = 128;
		data[2] = cmd;
		return SendCommand(data, 3000);
	}

	public bool InitBatteryProtocol(IntPtr hidObj)
	{
		Console.WriteLine("InitBatteryProtocol()");
		byte[] data = new byte[64];
		data[0] = 85;
		data[1] = 101;
		byte cmd = 0;
		data[2] = cmd;
		return SendCommand(hidObj, data, 500);
	}

	public bool RequestBattery(IntPtr hidObj)
	{
		Console.WriteLine("RequestBattery()");
		byte[] data = new byte[64];
		data[0] = 85;
		data[1] = 101;
		byte cmd = 1;
		data[2] = cmd;
		return SendCommand(hidObj, data, 200);
	}

	public void UpdateDeviceStatus(byte responseID)
	{
		if ((responseID & 0x80) > 0)
		{
			Console.WriteLine("Command Return Success! manualEvent.Set()");
			manualEvent.Set();
			IsDataTransfer = (responseID & 0x40) > 0;
		}
		else
		{
			Console.WriteLine("Command Return Fail! manualEvent.Set()");
		}
	}

	public void UpdateDeviceStatus()
	{
		Console.WriteLine("Command Return Success! manualEvent.Set()");
		manualEvent.Set();
	}

	public bool SendCommand(byte[] data, int timeOutInterval)
	{
		SetProperty(headtrackingDevice.HIDObj, "Write", data);
		bool flag = manualEvent.WaitOne(timeOutInterval);
		if (!flag)
		{
			Console.WriteLine("Command Response TimeOut");
			return flag;
		}
		Console.WriteLine("Command Response Success");
		return flag;
	}

	public bool SendCommand(IntPtr hidObj, byte[] data, int timeOutInterval)
	{
		bool isSuccess = false;
		if (SetProperty(hidObj, "Write", data) == HIDSDK.CmErr.CE_OK && manualEvent.WaitOne(timeOutInterval))
		{
			isSuccess = true;
		}
		if (!isSuccess)
		{
			Console.WriteLine("Command Failed");
		}
		else
		{
			Console.WriteLine("Command Response Success");
		}
		return isSuccess;
	}

	private void resetTimerMonitorStartCommand(bool monitor, int updateInterval = 5000)
	{
		startTimer?.Change(-1, -1);
		startTimer = null;
		if (monitor)
		{
			startTimer = new Timer((object obj) =>
			{
				OnStartCommandTimeOut();
			}, this, 0, updateInterval);
		}
	}

	private void OnStartCommandTimeOut()
	{
		Console.WriteLine("OnStartCommandTimeOut()");
		int lastTime = lastHIDTime;
		if (Environment.TickCount - lastTime > 3000)
		{
			StartSendData();
		}
	}

	public List<JackDeviceInfo> GetDeviceList()
	{
		return JackDeviceList;
	}

	public void RebuildDevice()
	{
		Console.WriteLine("RebuildDevice() Begin");
		lock (JackDeviceListLocker)
		{
			JackDeviceList.Clear();
			if (XearSDK.GetAudioEndpoints(out audioEndpointList, out var _))
			{
				for (int i = 0; i < audioEndpointList.Length; i++)
				{
					Console.WriteLine($"audioEndpointList index: {i}");
					JackDeviceInfo devInfo = new JackDeviceInfo();
					devInfo.audEndptUID = audioEndpointList[i];
					XearSDK.GetProperty(audioEndpointList[i], "DisplayName", out string displayName, out XearSDK.CmErr cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.displayName = displayName;
						Console.WriteLine($"DisplayName: {displayName}");
					}
					else
					{
						Console.WriteLine($"Get DisplayName Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "ControllerDisplayName", out string controllerDisplayName, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.controllerDisplayName = controllerDisplayName;
						Console.WriteLine($"ControllerDisplayName: {controllerDisplayName}");
					}
					else
					{
						Console.WriteLine($"Get ControllerDisplayName Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "DataFlow", out int dataFlow, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.dataFlow = (XearSDK.CmDataFlow)dataFlow;
						Console.WriteLine($"DataFlow: {dataFlow}");
					}
					else
					{
						Console.WriteLine($"Get DataFlow Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "JackType", out int jackType, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.jackType = (XearSDK.CmJackType)jackType;
						Console.WriteLine($"JackType: {devInfo.jackType.ToString()}");
					}
					else
					{
						Console.WriteLine($"Get JackType Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "EndpointID", out string endpointID, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.endpointID = endpointID;
						Console.WriteLine($"EndpointID: {endpointID}");
					}
					else
					{
						Console.WriteLine($"Get EndpointID Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "InstanceID", out string instanceID, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.instanceID = instanceID;
						Console.WriteLine($"InstanceID: {instanceID}");
					}
					else
					{
						Console.WriteLine($"Get InstanceID Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "USBVendorID", out int usbVendorID, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.usbVendorID = usbVendorID;
						Console.WriteLine($"USBVendorID: {usbVendorID}");
					}
					else
					{
						Console.WriteLine($"Get USBVendorID Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "USBProductID", out int usbProductID, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.usbProductID = usbProductID;
						Console.WriteLine($"USBProductID: {usbProductID}");
					}
					else
					{
						Console.WriteLine($"Get USBProductID Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "USBDeviceInstanceID", out string usbDeviceInstanceID, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.usbDeviceInstanceID = usbDeviceInstanceID;
						Console.WriteLine($"USBDeviceInstanceID: {usbDeviceInstanceID}");
					}
					else
					{
						Console.WriteLine($"Get USBDeviceInstanceID Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "DriverVersion", out string driverVersion, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.driverVersion = driverVersion;
						Console.WriteLine($"DriverVersion: {driverVersion}");
					}
					else
					{
						Console.WriteLine($"Get DriverVersion Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "USBRevisionNumber", out int usbRevisionNumber, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.usbRevisionNumber = usbRevisionNumber.ToString("X4");
						Console.WriteLine(string.Format("USBRevisionNumber: {0}", usbRevisionNumber.ToString("X4")));
					}
					else
					{
						Console.WriteLine($"Get FirmwareVersion Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "ChannelCount", out int channelCount, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.channelCount = channelCount;
						Console.WriteLine($"ChannelCount: {channelCount}");
					}
					else
					{
						Console.WriteLine($"Get ChannelCount Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "CmString50", out string string50, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						devInfo.string50 = string50;
						Console.WriteLine($"CmString50: {string50}");
					}
					else
					{
						Console.WriteLine($"Get FirmwareVersion Failed. CmErr: {cmErr2.ToString()}");
					}
					XearSDK.GetProperty(audioEndpointList[i], "SupportedLicenseCategory", out XearSDK.CmLicenseCategory[] outDats, out cmErr2);
					if (cmErr2 == XearSDK.CmErr.CE_OK)
					{
						XearSDK.CmLicenseCategory[] array = outDats;
						foreach (XearSDK.CmLicenseCategory j2 in array)
						{
							devInfo.licenseCategoryList.Add(j2);
						}
						Console.WriteLine($"SupportedLicenseCategory: {devInfo.licenseCategoryList.Count}");
					}
					else
					{
						Console.WriteLine($"Get SupportedLicenseCategory Failed. CmErr: {cmErr2.ToString()}");
					}
					JackDeviceList.Add(devInfo);
				}
			}
			SortJackDeviceList();
		}
		Console.WriteLine("RebuildDevice() End");
	}

	private void SortJackDeviceList()
	{
		int n = JackDeviceList.Count;
		bool swapJackDevice = false;
		for (int i = 0; i < n - 1; i++)
		{
			for (int j = 0; j < n - i - 1; j++)
			{
				swapJackDevice = false;
				if (JackDeviceList[j].dataFlow > JackDeviceList[j + 1].dataFlow)
				{
					swapJackDevice = true;
				}
				else if (JackDeviceList[j].dataFlow == JackDeviceList[j + 1].dataFlow && JackDeviceList[j].licenseCategoryList.Count < JackDeviceList[j + 1].licenseCategoryList.Count)
				{
					swapJackDevice = true;
				}
				if (swapJackDevice)
				{
					JackDeviceInfo temp = JackDeviceList[j];
					JackDeviceList[j] = JackDeviceList[j + 1];
					JackDeviceList[j + 1] = temp;
				}
			}
		}
	}

	public void RebuildHIDDevice()
	{
		Console.WriteLine("RebuildHIDDevice() Begin");
		lock (HIDDeviceListLocker)
		{
			HIDDeviceList.Clear();
			HIDSDK.CmErr result = HIDSDK.CmRebuildDeviceList(HIDsdkObj);
			Console.WriteLine($"CmRebuildDeviceList, HIDsdkObj: {HIDsdkObj}");
			if (result != HIDSDK.CmErr.CE_OK)
			{
				Console.WriteLine($"CmRebuildDeviceList Failed, ErrorCode: {result.ToString()}");
			}
			else
			{
				int hidEndpointCount = 0;
				result = HIDSDK.CmGetHIDEndpointCount(HIDsdkObj, ref hidEndpointCount);
				Console.WriteLine($"CmGetHIDEndpointCount, HIDsdkObj: {HIDsdkObj}, count: {hidEndpointCount}");
				if (result != HIDSDK.CmErr.CE_OK)
				{
					Console.WriteLine($"CmGetHIDEndpointCount Failed, ErrorCode: {result.ToString()}");
				}
				else
				{
					for (int i = 0; i < hidEndpointCount; i++)
					{
						IntPtr hidEndpointObj = IntPtr.Zero;
						HIDDeviceInfo deviceInfo = new HIDDeviceInfo();
						result = HIDSDK.CmGetHIDEndpointByIndex(HIDsdkObj, i, ref hidEndpointObj);
						Console.WriteLine($"CmGetHIDEndpointByIndex, index: {i}, hidEndpointObj: {hidEndpointObj}");
						if (result != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"CmGetHIDEndpointByIndex Failed, index: {i}, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
							continue;
						}
						deviceInfo.hidEndpointObj = hidEndpointObj;
						HIDSDK.CmValueINT32 vendorID = default;
						if (GetProperty(hidEndpointObj, "VendorID", ref vendorID) != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"GetVendorID Failed, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
						}
						else
						{
							deviceInfo.vendorID = vendorID.value;
							Console.WriteLine($"GetVendorID, hidEndpointObj: {hidEndpointObj}, VendorID: 0x{vendorID.value:X4}");
						}
						HIDSDK.CmValueINT32 productID = default;
						if (GetProperty(hidEndpointObj, "ProductID", ref productID) != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"GetProductID Failed, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
						}
						else
						{
							deviceInfo.productID = productID.value;
							Console.WriteLine($"GetProductID, hidEndpointObj: {hidEndpointObj}, ProductID: 0x{productID.value:X4}");
						}
						HIDSDK.CmValueString usbDeviceInstanceID = default;
						if (GetProperty(hidEndpointObj, "USBDeviceInstanceID", ref usbDeviceInstanceID) != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"GetUSBDeviceInstanceID Failed, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
						}
						else
						{
							deviceInfo.usbDeviceInstanceID = usbDeviceInstanceID.value;
							Console.WriteLine($"GetUSBDeviceInstanceID, hidEndpointObj: {hidEndpointObj}, ProductID: 0x{productID.value:X4}");
						}
						HIDSDK.CmValueINT32 revisionNumber = default;
						if (GetProperty(hidEndpointObj, "RevisionNumber", ref revisionNumber) != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"GetRevisionNumber Failed, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
						}
						else
						{
							deviceInfo.revisionNumber = revisionNumber.value;
							Console.WriteLine($"GetRevisionNumber, hidEndpointObj: {hidEndpointObj}, RevisionNumber: 0x{revisionNumber.value:X4}");
						}
						HIDSDK.CmValueINT32 inputReportByteLength = default;
						if (GetProperty(hidEndpointObj, "InputReportByteLength", ref inputReportByteLength) != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"GetInputReportByteLength Failed, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
						}
						else
						{
							deviceInfo.inputReportByteLength = inputReportByteLength.value;
							Console.WriteLine($"GetInputReportByteLength, hidEndpointObj: {hidEndpointObj}, InputReportByteLength: 0x{inputReportByteLength.value:X4}");
						}
						HIDSDK.CmValueINT32 outputReportByteLength = default;
						if (GetProperty(hidEndpointObj, "OutputReportByteLength", ref outputReportByteLength) != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"GetOutputReportByteLength Failed, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
						}
						else
						{
							deviceInfo.outputReportByteLength = outputReportByteLength.value;
							Console.WriteLine($"GetOutputReportByteLength, hidEndpointObj: {hidEndpointObj}, OutputReportByteLength: 0x{outputReportByteLength.value:X4}");
						}
						HIDSDK.CmValueINT32 featureReportByteLength = default;
						if (GetProperty(hidEndpointObj, "FeatureReportByteLength", ref featureReportByteLength) != HIDSDK.CmErr.CE_OK)
						{
							Console.WriteLine($"GetFeatureReportByteLength Failed, hidEndpointObj: {hidEndpointObj}, ErrorCode: {result.ToString()}");
						}
						else
						{
							deviceInfo.featureReportByteLength = featureReportByteLength.value;
							Console.WriteLine($"GetFeatureReportByteLength, hidEndpointObj: {hidEndpointObj}, FeatureReportByteLength: 0x{featureReportByteLength.value:X4}");
						}
						HIDDeviceList.Add(deviceInfo);
					}
				}
			}
		}
		Console.WriteLine("RebuildHIDDevice() End");
	}

	public void InitHIDFunction()
	{
		RefreshCMediaHIDDeviceCount();
		PairingJackHID();
		if (CM108DHIDDeviceCount > 0)
		{
			InitCM108DHID();
		}
		if (XearHIDDeviceCount > 0)
		{
			InitHIDEffect();
		}
		if (XearLEDDeviceCount > 0)
		{
			InitLEDConfig();
		}
		PairingHeadTrackingEndpoint();
	}

	public void RefreshCMediaHIDDeviceCount()
	{
		CM108DHIDDeviceCount = 0;
		XearHIDDeviceCount = 0;
		XearLEDDeviceCount = 0;
		HeadTrackingHIDDeviceCount = 0;
		if (HIDDeviceList.Count != 0)
		{
			lock (HIDDeviceListLocker)
			{
				for (int iter = 0; iter < HIDDeviceList.Count; iter++)
				{
					HIDSDK.CmValueINT32 cmvCapsCount = default;
					HIDSDK.CmValueOperationINT32 cmvReportID = default;
					if (GetProperty(HIDDeviceList[iter].hidEndpointObj, "InputValueCapsCount", ref cmvCapsCount) != HIDSDK.CmErr.CE_OK)
					{
						continue;
					}
					for (int index = 0; index < cmvCapsCount.value; index++)
					{
						cmvReportID.operation = index;
						if (GetProperty(HIDDeviceList[iter].hidEndpointObj, "InputValueCapReportID", ref cmvReportID) == HIDSDK.CmErr.CE_OK && cmvReportID.value == 85)
						{
							HIDDeviceList[iter].isSupportHeadTrackingHID = true;
							HeadTrackingHIDDeviceCount++;
							Console.WriteLine($"Found HeadTracking HID device. Vid: 0x{HIDDeviceList[iter].vendorID:X4}, Pid: 0x{HIDDeviceList[iter].productID:X4}, hidEndpointObj: {HIDDeviceList[iter].hidEndpointObj:X}.");
						}
					}
					if (GetProperty(HIDDeviceList[iter].hidEndpointObj, "OutputValueCapsCount", ref cmvCapsCount) != HIDSDK.CmErr.CE_OK)
					{
						continue;
					}
					for (int i = 0; i < cmvCapsCount.value; i++)
					{
						cmvReportID.operation = i;
						if (GetProperty(HIDDeviceList[iter].hidEndpointObj, "OutputValueCapReportID", ref cmvReportID) == HIDSDK.CmErr.CE_OK)
						{
							if (cmvReportID.value == 237)
							{
								HIDDeviceList[iter].isSupportXearHID = true;
								XearHIDDeviceCount++;
								Console.WriteLine($"Found CMedia HID device. Vid: 0x{HIDDeviceList[iter].vendorID:X4}, Pid: 0x{HIDDeviceList[iter].productID:X4}, hidEndpointObj: {HIDDeviceList[iter].hidEndpointObj:X}.");
							}
							if (cmvReportID.value == 255)
							{
								HIDDeviceList[iter].isSupportXearLED = true;
								XearLEDDeviceCount++;
								Console.WriteLine($"Found CMedia LED device. Vid: 0x{HIDDeviceList[iter].vendorID:X4}, Pid: 0x{HIDDeviceList[iter].productID:X4}, hidEndpointObj: {HIDDeviceList[iter].hidEndpointObj:X}.");
							}
							if (cmvReportID.value == 85)
							{
								HIDDeviceList[iter].isSupportHeadTrackingHID = true;
								HeadTrackingHIDDeviceCount++;
								Console.WriteLine($"Found HeadTracking HID device. Vid: 0x{HIDDeviceList[iter].vendorID:X4}, Pid: 0x{HIDDeviceList[iter].productID:X4}, hidEndpointObj: {HIDDeviceList[iter].hidEndpointObj:X}.");
							}
						}
					}
				}
			}
			Console.WriteLine($"Found {XearHIDDeviceCount} Xear HID device.");
			Console.WriteLine($"Found {XearLEDDeviceCount} Xear LED device.");
			Console.WriteLine($"Found {HeadTrackingHIDDeviceCount} HeadTracking HID device.");
		}
		if (HIDDeviceList.Count == 0)
		{
			return;
		}
		lock (HIDDeviceListLocker)
		{
			int devCount = HIDDeviceList.Count;
			for (int j = 0; j < devCount; j++)
			{
				if (HIDDeviceList[j].inputReportByteLength != 5 || HIDDeviceList[j].outputReportByteLength != 5)
				{
					continue;
				}
				foreach (JackDeviceInfo device in JackDeviceList)
				{
					if (device.usbDeviceInstanceID.Equals(HIDDeviceList[j].usbDeviceInstanceID) && (device.string50.Contains("CMA181") || device.string50.Contains("CMA203")))
					{
						HIDDeviceList[j].isSupportCM108DHID = true;
						CM108DHIDDeviceCount++;
						Console.WriteLine($"Found CM108D device. Vid: 0x{HIDDeviceList[j].vendorID:X4}, Pid: 0x{HIDDeviceList[j].productID:X4}, hidEndpointObj: {HIDDeviceList[j].hidEndpointObj:X}.");
						break;
					}
				}
			}
		}
		Console.WriteLine($"Found {CM108DHIDDeviceCount} CM108D HID device.");
	}

	public void PairingJackHID()
	{
		foreach (JackDeviceInfo jackDev in JackDeviceList)
		{
			foreach (HIDDeviceInfo hidDevice in HIDDeviceList)
			{
				if (jackDev.usbDeviceInstanceID.Equals(hidDevice.usbDeviceInstanceID))
				{
					if (hidDevice.isSupportCM108DHID)
					{
						jackDev.CMedia108DHIDObj = hidDevice.hidEndpointObj;
					}
					if (hidDevice.isSupportXearHID)
					{
						jackDev.CMediaHIDObj = hidDevice.hidEndpointObj;
					}
					if (hidDevice.isSupportXearLED)
					{
						jackDev.CMediaLEDObj = hidDevice.hidEndpointObj;
					}
					if (hidDevice.isSupportHeadTrackingHID)
					{
						jackDev.HeadTrackingHIDObj = hidDevice.hidEndpointObj;
					}
				}
			}
		}
	}

	public bool PairingHeadTrackingEndpoint()
	{
		bool isPaired = false;
		foreach (HIDDeviceInfo iterHIDEndpoint in HIDDeviceList)
		{
			if (iterHIDEndpoint.isSupportHeadTrackingHID)
			{
				foreach (JackDeviceInfo iterJackEndpoint in JackDeviceList)
				{
					if (iterJackEndpoint.licenseCategoryList.Contains(XearSDK.CmLicenseCategory.CLC_HeadTracking) && iterJackEndpoint.dataFlow == XearSDK.CmDataFlow.CDF_Render)
					{
						headtrackingDevice.endpointUID = iterJackEndpoint.audEndptUID;
						headtrackingDevice.HIDObj = iterHIDEndpoint.hidEndpointObj;
						headtrackingDevice.OutputReportByteLength = iterHIDEndpoint.outputReportByteLength;
						isPaired = true;
						Console.WriteLine("[PairingHeadTrackingEndpoint] Found HeadTracking Endpoint");
						break;
					}
				}
			}
			if (isPaired)
			{
				break;
			}
		}
		if (!isPaired)
		{
			Console.WriteLine("[PairingHeadTrackingEndpoint] NOT Found HeadTracking Endpoint");
		}
		return isPaired;
	}

	public void SetHeadAngleToAPO(int Yaw, int Pitch, int Roll)
	{
		int apoYaw = Yaw + 180;
		XearSDK.CmErr cmErr;
		if (apoYaw != lastApoYaw)
		{
			XearSDK.SetProperty(headtrackingDevice.endpointUID, "HeadTracking_OrientationYaw", apoYaw, out cmErr);
			lastApoYaw = apoYaw;
		}
		int apoPitch = Pitch + 180;
		if (apoPitch != lastApoPitch)
		{
			XearSDK.SetProperty(headtrackingDevice.endpointUID, "HeadTracking_OrientationPitch", apoPitch, out cmErr);
			lastApoPitch = apoPitch;
		}
		int apoRoll = Roll + 180;
		if (apoRoll != lastApoRoll)
		{
			XearSDK.SetProperty(headtrackingDevice.endpointUID, "HeadTracking_OrientationRoll", apoRoll, out cmErr);
			lastApoRoll = apoRoll;
		}
	}

	public void HeadTrackingReset()
	{
		int inValue = 1;
		XearSDK.SetProperty(headtrackingDevice.endpointUID, "HeadTracking_Reset", inValue, out var _);
	}

	public static T GetProperty<T>(IntPtr sdkObj, IntPtr hidEndpointObj, string property, T cmValue, ref HIDSDK.CmErr result)
	{
		IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
		Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: true);
		result = HIDSDK.CmGetProperty(sdkObj, hidEndpointObj, property, cmvIntPtr);
		T result2 = Marshal.PtrToStructure<T>(cmvIntPtr);
		Marshal.FreeHGlobal(cmvIntPtr);
		return result2;
	}

	public static T SetProperty<T>(IntPtr sdkObj, IntPtr hidEndpointObj, string property, T cmValue, ref HIDSDK.CmErr result)
	{
		IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
		Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: true);
		result = HIDSDK.CmSetProperty(sdkObj, hidEndpointObj, property, cmvIntPtr);
		Marshal.FreeHGlobal(cmvIntPtr);
		return cmValue;
	}

	public HIDSDK.CmErr GetProperty(IntPtr hidEndpointObj, string property, ref HIDSDK.CmValueString cmValue)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(hidEndpointObj == IntPtr.Zero) && !(HIDsdkObj == IntPtr.Zero))
		{
			cmValue = GetProperty(HIDsdkObj, hidEndpointObj, property, cmValue, ref cmErr);
			Console.WriteLine($"CmGetProperty, HIDsdkObj: {HIDsdkObj}, hidEndpointObj: {hidEndpointObj}, Property: {property}, {ToString(cmValue)}");
		}
		return cmErr;
	}

	public HIDSDK.CmErr GetProperty(IntPtr hidEndpointObj, string property, ref HIDSDK.CmValueINT32 cmValue)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(hidEndpointObj == IntPtr.Zero) && !(HIDsdkObj == IntPtr.Zero))
		{
			cmValue = GetProperty<HIDSDK.CmValueINT32>(HIDsdkObj, hidEndpointObj, property, default, ref cmErr);
			Console.WriteLine($"CmGetProperty, HIDsdkObj: {HIDsdkObj}, hidEndpointObj: {hidEndpointObj}, Property: {property}, {ToString(cmValue)}");
		}
		return cmErr;
	}

	public HIDSDK.CmErr GetProperty(IntPtr hidEndpointObj, string property, ref HIDSDK.CmValueOperationINT32 cmValue)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(hidEndpointObj == IntPtr.Zero) && !(HIDsdkObj == IntPtr.Zero))
		{
			cmValue = GetProperty(HIDsdkObj, hidEndpointObj, property, cmValue, ref cmErr);
			Console.WriteLine($"CmGetProperty, HIDsdkObj: {HIDsdkObj}, hidEndpointObj: {hidEndpointObj}, Property: {property}, {ToString(cmValue)}");
		}
		return cmErr;
	}

	private HIDSDK.CmErr GetProperty(IntPtr hidEndpointObj, string property, ref byte[] value)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(hidEndpointObj == IntPtr.Zero) && !(HIDsdkObj == IntPtr.Zero))
		{
			HIDSDK.CmValueVariableSizedArrayBYTE cmValue = new HIDSDK.CmValueVariableSizedArrayBYTE
			{
				dataSize = value.Length,
				data = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * value.Length)
			};
			Marshal.Copy(value, 0, cmValue.data, value.Length);
			cmValue = GetProperty(HIDsdkObj, hidEndpointObj, property, cmValue, ref cmErr);
			Console.WriteLine($"CmGetProperty, HIDsdkObj: {HIDsdkObj}, hidEndpointObj: {hidEndpointObj}, Property: {property}, {ToString(cmValue)}");
			Marshal.Copy(cmValue.data, value, 0, cmValue.dataSize);
			Marshal.FreeHGlobal(cmValue.data);
		}
		return cmErr;
	}

	public HIDSDK.CmErr GetHIDEventCallbackValue(IntPtr hidEndpointObj, ref HIDSDK.CmValueEventCallbackHIDData cmValue)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(hidEndpointObj == IntPtr.Zero) && !(HIDsdkObj == IntPtr.Zero))
		{
			IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HIDSDK.CmValueEventCallbackHIDData)));
			Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: true);
			cmErr = HIDSDK.CmGetHIDEndpointEventCallback(HIDsdkObj, hidEndpointObj, cmvIntPtr);
			cmValue = Marshal.PtrToStructure<HIDSDK.CmValueEventCallbackHIDData>(cmvIntPtr);
			Marshal.FreeHGlobal(cmvIntPtr);
			Console.WriteLine($"CmGetHIDEndpointEventCallback, HIDsdkObj: {HIDsdkObj}, hidEndpointObj: {hidEndpointObj}, {ToString(cmValue)}");
		}
		return cmErr;
	}

	public HIDSDK.CmErr GetHIDEventCallbackValue(ref HIDSDK.CmValueEventCallbackHIDRemovalArrival cmValue)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(HIDsdkObj == IntPtr.Zero))
		{
			IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HIDSDK.CmValueEventCallbackHIDRemovalArrival)));
			Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: true);
			cmErr = HIDSDK.CmGetEventCallbackHIDRemovalArrival(HIDsdkObj, cmvIntPtr);
			cmValue = Marshal.PtrToStructure<HIDSDK.CmValueEventCallbackHIDRemovalArrival>(cmvIntPtr);
			Marshal.FreeHGlobal(cmvIntPtr);
			Console.WriteLine($"CmGetEventCallbackHIDRemovalArrival, HIDsdkObj: {HIDsdkObj}, {ToString(cmValue)}");
		}
		return cmErr;
	}

	private HIDSDK.CmErr SetProperty(IntPtr hidEndpointObj, string property, byte[] value)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(hidEndpointObj == IntPtr.Zero) && !(HIDsdkObj == IntPtr.Zero))
		{
			HIDSDK.CmValueVariableSizedArrayBYTE cmValue = new HIDSDK.CmValueVariableSizedArrayBYTE
			{
				dataSize = value.Length,
				data = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * value.Length)
			};
			Marshal.Copy(value, 0, cmValue.data, value.Length);
			SetProperty(HIDsdkObj, hidEndpointObj, property, cmValue, ref cmErr);
			Console.WriteLine($"CmSetProperty, HIDsdkObj: {HIDsdkObj}, hidEndpointObj: {hidEndpointObj}, Property: {property}, {ToString(cmValue)}");
			Marshal.FreeHGlobal(cmValue.data);
		}
		return cmErr;
	}

	public HIDSDK.CmErr SetHIDEventCallbackValue(IntPtr hidEndpointObj, HIDSDK.CmValueEventCallbackHIDData cmValue)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(hidEndpointObj == IntPtr.Zero) && !(HIDsdkObj == IntPtr.Zero))
		{
			IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HIDSDK.CmValueEventCallbackHIDData)));
			Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: false);
			cmErr = HIDSDK.CmSetHIDEndpointEventCallback(HIDsdkObj, hidEndpointObj, cmvIntPtr);
			Marshal.FreeHGlobal(cmvIntPtr);
			Console.WriteLine($"CmSetHIDEventCallback, HIDsdkObj: {HIDsdkObj}, hidEndpointObj: {hidEndpointObj}, {ToString(cmValue)}");
		}
		return cmErr;
	}

	public HIDSDK.CmErr SetHIDEventCallbackValue(HIDSDK.CmValueEventCallbackHIDRemovalArrival cmValue)
	{
		HIDSDK.CmErr cmErr = HIDSDK.CmErr.CE_UnknownError;
		if (!(HIDsdkObj == IntPtr.Zero))
		{
			IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HIDSDK.CmValueEventCallbackHIDRemovalArrival)));
			Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: false);
			cmErr = HIDSDK.CmSetEventCallbackHIDRemovalArrival(HIDsdkObj, cmvIntPtr);
			Marshal.FreeHGlobal(cmvIntPtr);
			Console.WriteLine($"CmSetHIDEventCallbackRemovalArrival, HIDsdkObj: {HIDsdkObj}, {ToString(cmValue)}");
		}
		return cmErr;
	}

	public HIDSDK.CmErr GetLogSettings(IntPtr hidEndpointObj, ref HIDSDK.CmValueLogSettings cmValue)
	{
		IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HIDSDK.CmValueLogSettings)));
		Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: true);
		HIDSDK.CmErr result = HIDSDK.CmGetLogSettings(cmvIntPtr);
		cmValue = Marshal.PtrToStructure<HIDSDK.CmValueLogSettings>(cmvIntPtr);
		Marshal.FreeHGlobal(cmvIntPtr);
		Console.WriteLine($"CmGetLogSettings, {ToString(cmValue)}");
		return result;
	}

	public HIDSDK.CmErr SetLogSettings(IntPtr hidEndpointObj, HIDSDK.CmValueLogSettings cmValue)
	{
		IntPtr cmvIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HIDSDK.CmValueLogSettings)));
		Marshal.StructureToPtr(cmValue, cmvIntPtr, fDeleteOld: false);
		HIDSDK.CmErr result = HIDSDK.CmSetLogSettings(cmvIntPtr);
		Marshal.FreeHGlobal(cmvIntPtr);
		Console.WriteLine($"CmSetLogSettings, {ToString(cmValue)}");
		return result;
	}

	private string ToString(HIDSDK.CmValueString value)
	{
		return $"value: {value.value}";
	}

	private string ToString(HIDSDK.CmValueINT32 value)
	{
		return $"value: {value.value}";
	}

	private string ToString(HIDSDK.CmValueOperationINT32 value)
	{
		return $"value: {value.value}, operation: {value.operation}";
	}

	private string ToString(HIDSDK.CmValueVariableSizedArrayBYTE value)
	{
		byte[] buffer = new byte[value.dataSize];
		Marshal.Copy(value.data, buffer, 0, value.dataSize);
		return string.Format("dataSize: {0}, data: {1}", value.dataSize, BitConverter.ToString(buffer).Replace("-", " "));
	}

	private string ToString(HIDSDK.CmValueEventCallbackHIDData value)
	{
		return $"callback: {value.callback}, userData: {value.userData}";
	}

	private string ToString(HIDSDK.CmValueEventCallbackHIDRemovalArrival value)
	{
		return $"callback: {value.callback}, userData: {value.userData}";
	}

	private string ToString(HIDSDK.CmValueLogSettings value)
	{
		return $"type: {value.logType}, filePath: {value.filePath}, callback: {value.callback}, userData: {value.userData}";
	}

	public void GetOrderedSupportedEffectList(JackDeviceInfo jackDeviceInfo, ref List<XearSDK.CmLicenseCategory> effectList)
	{
		if (jackDeviceInfo.dataFlow == XearSDK.CmDataFlow.CDF_Render)
		{
			foreach (XearSDK.CmLicenseCategory iter in PlaybackEffectList)
			{
				if (jackDeviceInfo.licenseCategoryList.Contains(iter))
				{
					effectList.Add(iter);
				}
			}
			return;
		}
		foreach (XearSDK.CmLicenseCategory iter2 in CaptureEffectList)
		{
			if (jackDeviceInfo.licenseCategoryList.Contains(iter2))
			{
				effectList.Add(iter2);
			}
		}
		if (jackDeviceInfo.dataFlow == XearSDK.CmDataFlow.CDF_Capture && jackDeviceInfo.jackType == XearSDK.CmJackType.CJT_Microphone && jackDeviceInfo.string50.Contains("CMA175"))
		{
			effectList.Remove(XearSDK.CmLicenseCategory.CLC_NoiseReduction);
			effectList.Remove(XearSDK.CmLicenseCategory.CLC_HDNoiseReduction);
			effectList.Remove(XearSDK.CmLicenseCategory.CLC_AINoiseReduction);
			effectList.Remove(XearSDK.CmLicenseCategory.CLC_XearSingFX_MagicVoice);
		}
	}

	public void GetPropertyKeyName(XearSDK.CmDataFlow dataFlow, XearSDK.CmLicenseCategory licenseCategoryObj, ref string propertyKeyName)
	{
		if (dataFlow == XearSDK.CmDataFlow.CDF_Render)
		{
			if (PlaybackPropertyMap.ContainsKey(licenseCategoryObj))
			{
				PlaybackPropertyMap.TryGetValue(licenseCategoryObj, out propertyKeyName);
			}
			else
			{
				propertyKeyName = "";
			}
		}
		else if (CapturePropertyMap.ContainsKey(licenseCategoryObj))
		{
			CapturePropertyMap.TryGetValue(licenseCategoryObj, out propertyKeyName);
		}
		else
		{
			propertyKeyName = "";
		}
	}

	private void InitLEDConfig()
	{
		Console.WriteLine("InitLEDConfig()");
	}

	public bool SetLEDHIDStatus(IntPtr hidObj, bool isEnable)
	{
		bool isAllCommandOK = false;
		byte[] data = new byte[16]
		{
			237, 11, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		if (SetProperty(hidObj, "SetOutputReport", data) == HIDSDK.CmErr.CE_OK)
		{
			data[0] = 237;
			data[1] = 2;
			data[2] = 11;
			data[3] = (isEnable ? ((byte)1) : ((byte)0));
			if (SetProperty(hidObj, "SetFeature", data) == HIDSDK.CmErr.CE_OK)
			{
				isAllCommandOK = true;
			}
		}
		return isAllCommandOK;
	}

	public bool SetLEDStatus(IntPtr hidObj, bool isEnable)
	{
		Console.WriteLine("SetLEDStatus: isEnable: {0}", isEnable.ToString());
		bool isAllCommandOK = false;
		byte[] data = new byte[16]
		{
			255, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		if (isEnable)
		{
			data[1] = 1;
		}
		else
		{
			data[1] = 2;
		}
		if (SendCommand(hidObj, data))
		{
			isAllCommandOK = true;
		}
		return isAllCommandOK;
	}

	public bool SetLEDMode(IntPtr hidObj, int arraySize, int mode, int speed)
	{
		Console.WriteLine("SetLEDMode: arraySize: {0}, mode: {1}, speed: {2}", arraySize.ToString(), mode, speed);
		SetLEDStatus(hidObj, isEnable: false);
		bool isAllCommandOK = false;
		byte[] data = new byte[16]
		{
			255,
			3,
			(byte)mode,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};
		ushort wSize = (ushort)arraySize;
		ushort wInterval = (ushort)speed;
		byte bLSB = 0;
		byte bMSB = 0;
		switch (mode)
		{
		case 0:
			data[3] = 1;
			data[4] = bMSB;
			data[5] = 0;
			data[6] = 0;
			break;
		case 1:
			bLSB = (byte)wSize;
			bMSB = (byte)(wSize >> 8);
			data[3] = bLSB;
			data[4] = bMSB;
			bLSB = (byte)wInterval;
			bMSB = (byte)(wInterval >> 8);
			data[5] = bLSB;
			data[6] = bMSB;
			break;
		case 2:
			bLSB = (byte)wSize;
			bMSB = (byte)(wSize >> 8);
			data[3] = bLSB;
			data[4] = bMSB;
			bLSB = (byte)wInterval;
			bMSB = (byte)(wInterval >> 8);
			data[5] = bLSB;
			data[6] = bMSB;
			break;
		case 3:
			bLSB = (byte)wSize;
			bMSB = (byte)(wSize >> 8);
			data[3] = bLSB;
			data[4] = bMSB;
			data[5] = 0;
			data[6] = 0;
			break;
		}
		if (SendCommand(hidObj, data))
		{
			SaveLED(hidObj, mode, speed);
			isAllCommandOK = true;
		}
		SetLEDStatus(hidObj, isEnable: true);
		return isAllCommandOK;
	}

	public bool SetLEDColorArray(IntPtr hidObj, byte red, byte green, byte blue)
	{
		SetLEDStatus(hidObj, isEnable: false);
		bool isAllCommandOK = false;
		byte[] data = new byte[16]
		{
			255, 4, 0, 0, red, green, blue, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		for (int i = 7; i < 16; i++)
		{
			data[i] = 0;
		}
		if (SendCommand(hidObj, data))
		{
			isAllCommandOK = true;
		}
		SetLEDStatus(hidObj, isEnable: true);
		return isAllCommandOK;
	}

	public bool SetLEDColorArray(IntPtr hidObj, int arraySize, byte[] red, byte[] green, byte[] blue)
	{
		Console.WriteLine("SetLEDColorArray: arraySize: {0}", arraySize.ToString());
		SetLEDStatus(hidObj, isEnable: false);
		bool isAllCommandOK = false;
		byte[] data = new byte[16]
		{
			255, 4, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		int nIndex = 0;
		int diffNum = 0;
		int nArray = 0;
		for (int i = 0; i < arraySize; i += 4)
		{
			diffNum = ((arraySize - i > 3) ? 4 : (arraySize - i));
			data[2] = (byte)i;
			for (int j = 0; j < diffNum; j++)
			{
				nIndex = 4 + 3 * j;
				data[nIndex] = red[nArray];
				data[nIndex + 1] = green[nArray];
				data[nIndex + 2] = blue[nArray];
				nArray++;
			}
			SendCommand(hidObj, data);
		}
		isAllCommandOK = true;
		SetLEDStatus(hidObj, isEnable: true);
		return isAllCommandOK;
	}

	public bool SendCommand(IntPtr hidObj, byte[] data)
	{
		return SetProperty(hidObj, "SetOutputReport", data) == HIDSDK.CmErr.CE_OK;
	}

	public void InitProfiles()
	{
		CurrentProfileName = configManager.jsonTempConfig.CurrentProfileName;
		ProfileCollection.Clear();
		InitProfileConfig();
	}

	public void InitProfileConfig()
	{
		string configPath = configManager.GetProfileConfigPath();
		if (!string.IsNullOrEmpty(configPath) && !File.Exists(configPath))
		{
			LogHelper.Info("Profile config not found, Created.", isPrintCaller: true, "InitProfileConfig", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
			List<ProfileConfig> configs = new List<ProfileConfig>();
			ProfileConfig defaultConfig = new ProfileConfig
			{
				profileName = "Default"
			};
			configs.Add(defaultConfig);
			configManager.SaveProfile(configs);
		}
		ProfileCollection = configManager.LoadProfile();
		InitDefaultProfile(configPath);
	}

	private void InitDefaultProfile(string profileDir)
	{
		List<JackDeviceInfo> activatedDevices = GetActivatedDevices();
		ProfileConfig defaultConfig = ProfileCollection.Find((ProfileConfig x) => x.profileName == "Default");
		foreach (JackDeviceInfo device in activatedDevices)
		{
			bool isExist = false;
			foreach (ProfileTemplate template in defaultConfig.templates)
			{
				if (template.instanceID == device.instanceID)
				{
					isExist = true;
					break;
				}
			}
			if (!isExist)
			{
				ProfileTemplate templateDefault = new ProfileTemplate
				{
					instanceID = device.instanceID,
					vendorID = device.usbVendorID.ToString("X4"),
					productID = device.usbProductID.ToString("X4")
				};
				defaultConfig.templates.Add(templateDefault);
			}
		}
		configManager.SaveProfile(ProfileCollection);
	}

	public void ProfileChanged(string profileName)
	{
		if (profileName.Equals("Current"))
		{
			configManager.jsonTempConfigAutoSave.Update((JsonTempConfig tempConfig) =>
			{
				tempConfig.CurrentProfileName = "Current";
			});
			CurrentProfileName = profileName;
			return;
		}
		if (CurrentProfileName == profileName)
		{
			LogHelper.Info("Profile same, Don't change", isPrintCaller: true, "ProfileChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
			return;
		}
		ProfileConfig profileConfig = ProfileCollection.Find((ProfileConfig x) => x.profileName == profileName);
		if (profileConfig == null)
		{
			LogHelper.Warning("Profile Not Found!", isPrintCaller: true, "ProfileChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
			return;
		}
		CurrentProfileName = profileName;
		configManager.jsonTempConfigAutoSave.Update((JsonTempConfig tempConfig) =>
		{
			tempConfig.CurrentProfileName = CurrentProfileName;
		});
		foreach (JackDeviceInfo device in GetActivatedDevices())
		{
			ProfileTemplate template = profileConfig.templates.Find((ProfileTemplate x) => x.instanceID == device.instanceID);
			XearSDK.CmErr cmErr;
			if (template != null)
			{
				if (device.dataFlow == XearSDK.CmDataFlow.CDF_Render)
				{
					foreach (KeyValuePair<string, JToken> property in template.playbackProperties)
					{
						if (PlaybackProfilePropertyMap.TryGetValue(property.Key, out var propertyName))
						{
							int value = property.Value.Value<int>();
							XearSDK.SetProperty(device.audEndptUID, propertyName, value, out cmErr);
						}
					}
				}
				else
				{
					if (device.dataFlow != XearSDK.CmDataFlow.CDF_Capture)
					{
						continue;
					}
					foreach (KeyValuePair<string, JToken> property2 in template.captureProperties)
					{
						if (CaptureProfilePropertyMap.TryGetValue(property2.Key, out var propertyName2))
						{
							int value2 = property2.Value.Value<int>();
							XearSDK.SetProperty(device.audEndptUID, propertyName2, value2, out cmErr);
						}
					}
				}
				continue;
			}
			ProfileTemplate templateDefault = ProfileCollection.Find((ProfileConfig x) => x.profileName == "Default").templates.Find((ProfileTemplate x) => x.instanceID == device.instanceID);
			if (templateDefault == null)
			{
				continue;
			}
			if (device.dataFlow == XearSDK.CmDataFlow.CDF_Render)
			{
				foreach (KeyValuePair<string, JToken> property3 in templateDefault.playbackProperties)
				{
					if (PlaybackProfilePropertyMap.TryGetValue(property3.Key, out var propertyName3))
					{
						int value3 = property3.Value.Value<int>();
						XearSDK.SetProperty(device.audEndptUID, propertyName3, value3, out cmErr);
					}
				}
			}
			else
			{
				if (device.dataFlow != XearSDK.CmDataFlow.CDF_Capture)
				{
					continue;
				}
				foreach (KeyValuePair<string, JToken> property4 in templateDefault.captureProperties)
				{
					if (CaptureProfilePropertyMap.TryGetValue(property4.Key, out var propertyName4))
					{
						int value4 = property4.Value.Value<int>();
						XearSDK.SetProperty(device.audEndptUID, propertyName4, value4, out cmErr);
					}
				}
			}
		}
	}

	public bool CheckProfileChanged(Guid audEndptUID, string propertyName)
	{
		if (CurrentProfileName == "Current")
		{
			return false;
		}
		ProfileConfig profileConfig = ProfileCollection.Find((ProfileConfig x) => x.profileName == CurrentProfileName);
		if (profileConfig == null)
		{
			LogHelper.Warning("Profile Not Found!", isPrintCaller: true, "CheckProfileChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
		}
		else
		{
			List<JackDeviceInfo> activatedDevice = GetActivatedDevices();
			JackDeviceInfo device = activatedDevice.Find((JackDeviceInfo x) => x.audEndptUID == audEndptUID);
			if (device != null)
			{
				ProfileTemplate template = profileConfig.templates.Find((ProfileTemplate x) => x.instanceID == device.instanceID);
				if (template != null)
				{
					XearSDK.CmErr cmErr;
					if (device.dataFlow == XearSDK.CmDataFlow.CDF_Render)
					{
						foreach (KeyValuePair<string, JToken> property in template.playbackProperties)
						{
							if (PlaybackProfilePropertyMap.TryGetValue(property.Key, out var outPropertyName) && outPropertyName.Equals(propertyName))
							{
								if (!XearSDK.GetProperty(audEndptUID, propertyName, out int propertyValue, out cmErr))
								{
									LogHelper.Warning("Property get Failed.", isPrintCaller: true, "CheckProfileChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
									return false;
								}
								int value = property.Value.Value<int>();
								if (propertyValue != value)
								{
									ProfileChanged("Current");
									return true;
								}
							}
						}
					}
					else if (device.dataFlow == XearSDK.CmDataFlow.CDF_Capture)
					{
						foreach (KeyValuePair<string, JToken> property2 in template.captureProperties)
						{
							if (CaptureProfilePropertyMap.TryGetValue(property2.Key, out var outPropertyName2) && outPropertyName2.Equals(propertyName))
							{
								if (!XearSDK.GetProperty(audEndptUID, propertyName, out int propertyValue2, out cmErr))
								{
									LogHelper.Warning("Property get Failed.", isPrintCaller: true, "CheckProfileChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
									return false;
								}
								int value2 = property2.Value.Value<int>();
								if (propertyValue2 != value2)
								{
									ProfileChanged("Current");
									return true;
								}
							}
						}
					}
				}
				else
				{
					LogHelper.Warning("There has no endpoint setting profile.", isPrintCaller: true, "CheckProfileChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
				}
			}
			else
			{
				LogHelper.Warning("Device Not Found!", isPrintCaller: true, "CheckProfileChanged", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
			}
		}
		return false;
	}

	public void AddNewProfile(string profileName)
	{
		List<JackDeviceInfo> activatedDevices = GetActivatedDevices();
		ProfileConfig profileConfig = new ProfileConfig
		{
			profileName = profileName
		};
		foreach (JackDeviceInfo device in activatedDevices)
		{
			ProfileTemplate profileTemplate = new ProfileTemplate();
			profileTemplate.instanceID = device.instanceID;
			profileTemplate.vendorID = device.usbVendorID.ToString("X4");
			profileTemplate.productID = device.usbProductID.ToString("X4");
			XearSDK.CmErr cmErr;
			if (device.dataFlow == XearSDK.CmDataFlow.CDF_Render)
			{
				foreach (string property in profileTemplate.playbackProperties.Keys.ToList())
				{
					PlaybackProfilePropertyMap.TryGetValue(property, out var propertyName);
					if (XearSDK.GetProperty(device.audEndptUID, propertyName, out int outDat, out cmErr))
					{
						JToken token = JToken.FromObject(outDat);
						profileTemplate.playbackProperties[property] = token;
					}
				}
			}
			else if (device.dataFlow == XearSDK.CmDataFlow.CDF_Capture)
			{
				foreach (string property2 in profileTemplate.captureProperties.Keys.ToList())
				{
					CaptureProfilePropertyMap.TryGetValue(property2, out var propertyName2);
					if (XearSDK.GetProperty(device.audEndptUID, propertyName2, out int outDat2, out cmErr))
					{
						JToken token2 = JToken.FromObject(outDat2);
						profileTemplate.captureProperties[property2] = token2;
					}
				}
			}
			profileConfig.templates.Add(profileTemplate);
		}
		ProfileCollection.Add(profileConfig);
		configManager.SaveProfile(ProfileCollection);
		ProfileChanged(profileName);
	}

	public bool DeleteProfile(string profileName)
	{
		bool isDeleted = false;
		int index = ProfileCollection.FindIndex((ProfileConfig x) => x.profileName == profileName);
		if (index == -1)
		{
			LogHelper.Warning("Profile Delete Failed!", isPrintCaller: true, "DeleteProfile", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Profile.cs");
		}
		else
		{
			ProfileCollection.RemoveAt(index);
			isDeleted = true;
		}
		configManager.SaveProfile(ProfileCollection);
		if (profileName.Equals(CurrentProfileName))
		{
			ProfileChanged("Default");
		}
		return isDeleted;
	}

	private List<JackDeviceInfo> GetActivatedDevices()
	{
		List<JackDeviceInfo> activatedDevice = new List<JackDeviceInfo>();
		lock (JackDeviceListLocker)
		{
			foreach (JackDeviceInfo device in JackDeviceList)
			{
				if (device.licenseCategoryList.Count > 1)
				{
					activatedDevice.Add(device);
				}
			}
			return activatedDevice;
		}
	}

	public bool HIDSDKInit()
	{
		lock (HIDSDKObjLock)
		{
			HIDsdkObj = IntPtr.Zero;
			if (HIDSDK.CmHIDSDKInit(ref HIDsdkObj) != HIDSDK.CmErr.CE_OK)
			{
				return false;
			}
			return true;
		}
	}

	public bool HIDSDKUninit()
	{
		lock (HIDSDKObjLock)
		{
			if (HIDsdkObj == IntPtr.Zero)
			{
				return false;
			}
			if (HIDSDK.CmHIDSDKUninit(HIDsdkObj) != HIDSDK.CmErr.CE_OK)
			{
				return false;
			}
			HIDsdkObj = IntPtr.Zero;
			return true;
		}
	}

	private void InitProgramDataPath()
	{
		string programDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), GetAssemblyCompanyAndProduct());
		iniAppTempSetting = new IniFileHelper(Path.Combine(programDataFolder, "Settings.ini"));
		iniMagicVoice2 = new IniFileHelper(Path.Combine(programDataFolder, "MagicVoice2Settings.ini"));
		iniRecordingEffect = new IniFileHelper(Path.Combine(programDataFolder, "RecordingEffectSettings.ini"));
	}

	private static string GetAssemblyCompanyAndProduct()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string company = executingAssembly.GetCustomAttributes<AssemblyCompanyAttribute>().FirstOrDefault()?.Company ?? "Cmedia";
		string product = executingAssembly.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? "Standard";
		return company + "\\" + product + "\\";
	}

	public void LoadRecenterConfig(ref double dDetectionTime, ref double dFadingTime, ref double dYawShiftThreshold, ref double dYawPathThreshold, ref bool bAutoReset)
	{
		dDetectionTime = configManager.jsonHeadTrackingConfig.DetectionTime;
		dFadingTime = configManager.jsonHeadTrackingConfig.FadingTime;
		dYawShiftThreshold = configManager.jsonHeadTrackingConfig.YawShiftThreshold;
		dYawPathThreshold = configManager.jsonHeadTrackingConfig.YawPathThreshold;
		bAutoReset = configManager.jsonHeadTrackingConfig.AutoReset == 1;
	}

	public string LoadTestSoundFileName()
	{
		return configManager.jsonHeadTrackingConfig.TestSound;
	}

	public bool LoadHotKey()
	{
		bool isEnable = configManager.jsonHeadTrackingConfig.HotKey == 1;
		LogHelper.Debug($"HotKey isEnable: {isEnable.ToString()}", isPrintCaller: false, "LoadHotKey", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Settings.cs");
		return isEnable;
	}

	public int LoadBatteryThreshold()
	{
		return configManager.jsonHeadTrackingConfig.BatteryThreshold;
	}

	public int LoadFWUpdateVid()
	{
		return Convert.ToInt32(configManager.jsonHeadTrackingConfig.FWVid, 16);
	}

	public int LoadFWUpdatePid()
	{
		return Convert.ToInt32(configManager.jsonHeadTrackingConfig.FWPid, 16);
	}

	public string GetCurrentLang()
	{
		return appLanguage;
	}

	private void InitLanguage()
	{
		langDisplayName = configManager.GetLangDisplay();
		string AppDir = Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath);
		foreach (KeyValuePair<string, List<string>> dic in ScanLanguageFolders(Path.Combine(AppDir, "Resources", "Language")))
		{
			if (langDisplayName.ContainsKey(dic.Key))
			{
				existLanguages.Add(dic.Key, langDisplayName[dic.Key]);
			}
			else
			{
				existLanguages.Add(dic.Key, dic.Key);
			}
		}
		string tempLang = configManager.jsonTempConfig.Language;
		if (tempLang.Equals("System"))
		{
			string displayLanguage = CultureInfo.CurrentUICulture.Name;
			if (existLanguages.ContainsKey(displayLanguage))
			{
				appLanguage = displayLanguage;
			}
			if (!appLanguage.Equals("en-US"))
			{
				ChangeLanguage(appLanguage);
			}
		}
		else
		{
			ChangeLanguage(tempLang);
		}
	}

	public void ChangeLanguage(string lang)
	{
		if (!existLanguages.ContainsKey(lang))
		{
			LogHelper.Error("Language doesn't exist.", isPrintCaller: false, "ChangeLanguage", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Settings.cs");
			return;
		}
		foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
		{
			if (dictionary.Source.ToString().Contains("Language"))
			{
				string AppDir = Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath);
				string langPath = Path.Combine(AppDir, "Resources", "Language", lang, $"{lang}.xaml");
				if (!File.Exists(langPath))
				{
					LogHelper.Error("Language xaml file doesn't exist.", isPrintCaller: false, "ChangeLanguage", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Settings.cs");
					return;
				}
				dictionary.Source = new Uri(langPath);
				break;
			}
		}
		appLanguage = lang;
		configManager.jsonTempConfigAutoSave.Update((JsonTempConfig tempConfig) =>
		{
			tempConfig.Language = appLanguage;
		});
	}

	public Dictionary<string, List<string>> ScanLanguageFolders(string languagePath)
	{
		Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
		if (!Directory.Exists(languagePath))
		{
			LogHelper.Error("Language folder not found: " + languagePath, isPrintCaller: true, "ScanLanguageFolders", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.Settings.cs");
			return result;
		}
		string[] directories = Directory.GetDirectories(languagePath);
		foreach (string path in directories)
		{
			string langCode = Path.GetFileName(path);
			string[] files = Directory.GetFiles(path, "*.xaml");
			List<string> relativeFiles = new List<string>();
			string[] array = files;
			foreach (string file in array)
			{
				relativeFiles.Add(Path.GetFileName(file));
			}
			result[langCode] = relativeFiles;
		}
		return result;
	}

	public bool LoadIsDisableMainGuide()
	{
		return configManager.jsonTempConfig.DisableMainGuide == 1;
	}

	public void SaveIsDisableMainGuide(bool isDisable)
	{
		int disable = (isDisable ? 1 : 0);
		configManager.jsonTempConfigAutoSave.Update((JsonTempConfig tempConfig) =>
		{
			tempConfig.DisableMainGuide = disable;
		});
	}

	public void SaveAllOrder(List<string> deviceList)
	{
		configManager.SaveEndpointListOrder(deviceList);
	}

	public List<string> LoadAllOrder()
	{
		return configManager.LoadEndpointListOrder();
	}

	public void SaveEndpointShow(string instanceID, bool isShow)
	{
		int nIsShow = (isShow ? 1 : 0);
		Dictionary<string, int> dict = configManager.LoadEndpointListShow();
		if (dict.ContainsKey(instanceID))
		{
			dict[instanceID] = nIsShow;
		}
		else
		{
			dict.Add(instanceID, nIsShow);
		}
		configManager.SaveEndpointListShow(dict);
	}

	public int LoadEndpointShow(string instanceID)
	{
		Dictionary<string, int> dict = configManager.LoadEndpointListShow();
		if (dict.ContainsKey(instanceID))
		{
			return dict[instanceID];
		}
		dict.Add(instanceID, -1);
		return -1;
	}

	public void LoadEQProfile(ref string[] profileName, ref int[,] profileValue)
	{
		for (int iterProfile = 0; iterProfile < 3; iterProfile++)
		{
			string sectionName = $"{sectionEQProfile}_{iterProfile + 1}";
			string profileNameValue = iniAppTempSetting.ReadString(profileNameKey, "", sectionName);
			if (profileNameValue == string.Empty)
			{
				profileName[iterProfile] = $"Profile {iterProfile + 1}";
			}
			else
			{
				profileName[iterProfile] = profileNameValue;
			}
			for (int iterBand = 0; iterBand < 10; iterBand++)
			{
				string keyName = $"{bandKey}_{iterBand + 1}";
				int value = iniAppTempSetting.ReadInt(keyName, 0, sectionName);
				profileValue[iterProfile, iterBand] = value;
			}
		}
	}

	public void SaveEQProfile(string[] profileName, int[,] profileValue)
	{
		for (int iterProfile = 0; iterProfile < 3; iterProfile++)
		{
			string sectionName = $"{sectionEQProfile}_{iterProfile + 1}";
			string profileNameValue = profileName[iterProfile];
			iniAppTempSetting.Write(profileNameKey, profileNameValue, sectionName);
			for (int iterBand = 0; iterBand < 10; iterBand++)
			{
				string keyName = $"{bandKey}_{iterBand + 1}";
				string value = $"{profileValue[iterProfile, iterBand]}";
				iniAppTempSetting.Write(keyName, value, sectionName);
			}
		}
	}

	public void LoadLED(IntPtr hidObj, ref bool isEnable, ref byte colorR, ref byte colorG, ref byte colorB, ref int mode, ref int speed)
	{
		HIDDeviceInfo hidInfo = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		if (hidInfo != null)
		{
			string vid = $"0x{hidInfo.vendorID:X4}";
			string pid = $"0x{hidInfo.productID:X4}";
			string section = sectionLED + "_" + vid + "_" + pid;
			isEnable = iniAppTempSetting.ReadInt(keyEnable, 1, section) == 1;
			string strColorR = iniAppTempSetting.ReadString(keyColorR, "FF", section);
			string strColorG = iniAppTempSetting.ReadString(keyColorG, "FF", section);
			string strColorB = iniAppTempSetting.ReadString(keyColorB, "FF", section);
			if (strColorR != string.Empty)
			{
				colorR = Convert.ToByte(strColorR, 16);
			}
			else
			{
				colorR = byte.MaxValue;
			}
			if (strColorG != string.Empty)
			{
				colorG = Convert.ToByte(strColorG, 16);
			}
			else
			{
				colorG = byte.MaxValue;
			}
			if (strColorB != string.Empty)
			{
				colorB = Convert.ToByte(strColorB, 16);
			}
			else
			{
				colorB = byte.MaxValue;
			}
			mode = iniAppTempSetting.ReadInt(keyMode, 0, section);
			speed = iniAppTempSetting.ReadInt(keySpeed, 50, section);
		}
	}

	public void LoadLED(IntPtr hidObj, ref bool isEnable)
	{
		HIDDeviceInfo hidInfo = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		if (hidInfo != null)
		{
			string vid = $"0x{hidInfo.vendorID:X4}";
			string pid = $"0x{hidInfo.productID:X4}";
			string section = sectionLED + "_" + vid + "_" + pid;
			isEnable = iniAppTempSetting.ReadInt(keyEnable, 1, section) == 1;
		}
	}

	public void LoadLED(IntPtr hidObj, ref byte colorR, ref byte colorG, ref byte colorB)
	{
		HIDDeviceInfo hidInfo = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		if (hidInfo != null)
		{
			string vid = $"0x{hidInfo.vendorID:X4}";
			string pid = $"0x{hidInfo.productID:X4}";
			string section = sectionLED + "_" + vid + "_" + pid;
			string strColorR = iniAppTempSetting.ReadString(keyColorR, "FF", section);
			string strColorG = iniAppTempSetting.ReadString(keyColorG, "FF", section);
			string strColorB = iniAppTempSetting.ReadString(keyColorB, "FF", section);
			if (strColorR != string.Empty)
			{
				colorR = Convert.ToByte(strColorR, 16);
			}
			else
			{
				colorR = byte.MaxValue;
			}
			if (strColorG != string.Empty)
			{
				colorG = Convert.ToByte(strColorG, 16);
			}
			else
			{
				colorG = byte.MaxValue;
			}
			if (strColorB != string.Empty)
			{
				colorB = Convert.ToByte(strColorB, 16);
			}
			else
			{
				colorB = byte.MaxValue;
			}
		}
	}

	public void LoadLED(IntPtr hidObj, ref int mode, ref int speed)
	{
		HIDDeviceInfo hidInfo = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		if (hidInfo != null)
		{
			string vid = $"0x{hidInfo.vendorID:X4}";
			string pid = $"0x{hidInfo.productID:X4}";
			string section = sectionLED + "_" + vid + "_" + pid;
			mode = iniAppTempSetting.ReadInt(keyMode, 0, section);
			speed = iniAppTempSetting.ReadInt(keySpeed, 50, section);
		}
	}

	public void SaveLED(IntPtr hidObj, bool isEnable)
	{
		HIDDeviceInfo hidInfo = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		if (hidInfo != null)
		{
			string vid = $"0x{hidInfo.vendorID:X4}";
			string pid = $"0x{hidInfo.productID:X4}";
			string section = sectionLED + "_" + vid + "_" + pid;
			iniAppTempSetting.Write(keyEnable, isEnable ? "1" : "0", section);
		}
	}

	public void SaveLED(IntPtr hidObj, byte colorR, byte colorG, byte colorB)
	{
		HIDDeviceInfo hidInfo = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		if (hidInfo != null)
		{
			string vid = $"0x{hidInfo.vendorID:X4}";
			string pid = $"0x{hidInfo.productID:X4}";
			string section = sectionLED + "_" + vid + "_" + pid;
			iniAppTempSetting.Write(keyColorR, $"{colorR:X2}", section);
			iniAppTempSetting.Write(keyColorG, $"{colorG:X2}", section);
			iniAppTempSetting.Write(keyColorB, $"{colorB:X2}", section);
		}
	}

	public void SaveLED(IntPtr hidObj, int mode, int speed)
	{
		HIDDeviceInfo hidInfo = HIDDeviceList.Find((HIDDeviceInfo x) => x.hidEndpointObj == hidObj);
		if (hidInfo != null)
		{
			string vid = $"0x{hidInfo.vendorID:X4}";
			string pid = $"0x{hidInfo.productID:X4}";
			string section = sectionLED + "_" + vid + "_" + pid;
			iniAppTempSetting.Write(keyMode, mode.ToString(), section);
			iniAppTempSetting.Write(keySpeed, speed.ToString(), section);
		}
	}

	private DriverInstance()
	{
		InitPlugins();
		InitProgramDataPath();
		InitLanguage();
	}

	public static DriverInstance getInstance()
	{
		return instance;
	}

	private void InitPlugins()
	{
		provider = PluginLoader.LoadGuidProvider();
	}

	public void ActivateGUIDLicense()
	{
		if (provider == null)
		{
			LogHelper.Critical("Guid provider load failed, there have no license can activate.", isPrintCaller: true, "ActivateGUIDLicense", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.cs");
			return;
		}
		string[] Guids = provider.GetGuid();
		for (int index = 0; index < Guids.Length; index++)
		{
			try
			{
				if (XearSDK.ActivateLicense(Guid.Parse(Guids[index]), out var _))
				{
					LogHelper.Debug($"Activated GUID Successed, Index: {index}, GUID: {Guids[index]}.", isPrintCaller: false, "ActivateGUIDLicense", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.cs");
				}
				else
				{
					LogHelper.Debug($"Activated GUID Failed, Index: {index}, GUID: {Guids[index]}.", isPrintCaller: false, "ActivateGUIDLicense", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.cs");
				}
			}
			catch
			{
				LogHelper.Debug($"Activated GUID Catch Exception, Index: {index}, GUID: {Guids[index]}.", isPrintCaller: false, "ActivateGUIDLicense", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Driver\\DriverInstance.cs");
			}
		}
	}

	public void RebuildAllDeviceList()
	{
		RebuildHIDDevice();
		RebuildDevice();
		InitHIDFunction();
		InitProfiles();
		InitAutoSwitch();
	}

	public void RegisterHIDCallBack(HIDSDK.CmValueEventCallbackHIDData cbPtr)
	{
		if (XearHIDDeviceCount <= 0 && HeadTrackingHIDDeviceCount <= 0 && CM108DHIDDeviceCount <= 0)
		{
			return;
		}
		foreach (HIDDeviceInfo iterHIDEndpoint in HIDDeviceList)
		{
			if (iterHIDEndpoint.isSupportXearHID || iterHIDEndpoint.isSupportHeadTrackingHID || iterHIDEndpoint.isSupportCM108DHID)
			{
				SetHIDEventCallbackValue(iterHIDEndpoint.hidEndpointObj, cbPtr);
			}
		}
	}
}
