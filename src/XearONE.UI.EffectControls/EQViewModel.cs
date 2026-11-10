using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Cmedia;
using XearONE.UI.Driver;
using XearONE.UI.Driver.Settings;
using XearONE.UI.Helpers;

namespace XearONE.UI.EffectControls;

public class EQViewModel : INotifyPropertyChanged, IUsrCtrlProperty
{
	private const string propertyName_EQ_Enable = "Enable_EQ_GFX";

	private readonly string[] propertyName_EQSlider = new string[10] { "EQ_Slider1", "EQ_Slider2", "EQ_Slider3", "EQ_Slider4", "EQ_Slider5", "EQ_Slider6", "EQ_Slider7", "EQ_Slider8", "EQ_Slider9", "EQ_Slider10" };

	private readonly Guid _audEndptUID;

	private List<JsonEQPreset> EQPresetList = new List<JsonEQPreset>();

	private List<JsonEQCustom> EQCustomList = new List<JsonEQCustom>();

	private bool _enable;

	private readonly int[] _eqValues = new int[10];

	private int _selectedPreset;

	private ObservableCollection<JsonEQCustom> _profileNames = new ObservableCollection<JsonEQCustom>();

	private JsonEQCustom _selectedProfile;

	private string _profileName;

	private bool _enableSave;

	private readonly int[,] cmediaEQPresetValue = new int[12, 10]
	{
		{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 11, 7, 3, 0, 0, -6, -10, -10, -2, -2 },
		{ -8, 6, 0, 8, -8, 10, -2, 13, 8, 1 },
		{ 5, 2, 0, -10, -5, 0, 6, 12, 11, 10 },
		{ -6, 0, 4, 8, 9, 9, 5, 3, 2, 1 },
		{ -13, -9, -4, 2, 8, 12, 2, -11, -2, 8 },
		{ 0, 7, -1, -7, -12, -8, 0, 10, 4, -5 },
		{ 5, 5, 2, -2, -8, -12, -4, 0, 7, 9 },
		{ 12, 7, -3, 4, 13, 8, 3, -3, 8, 12 },
		{ -10, -11, -12, -6, 2, 8, 13, 13, 11, 14 },
		{ 6, 3, -8, -12, -4, 3, 8, 10, 10, 7 },
		{ -13, -8, 0, 6, 14, 4, -4, -7, -8, -10 }
	};

	private bool _isPreset00Visible = true;

	private bool _isPreset01Visible = true;

	private bool _isPreset02Visible = true;

	private bool _isPreset03Visible = true;

	private bool _isPreset04Visible = true;

	private bool _isPreset05Visible = true;

	private bool _isPreset06Visible = true;

	private bool _isPreset07Visible = true;

	private bool _isPreset08Visible = true;

	private bool _isPreset09Visible = true;

	private bool _isPreset10Visible = true;

	private bool _isPreset11Visible = true;

	public ICommand SaveCommand { get; }

	public ICommand DeleteProfileCommand { get; }

	public bool Enable
	{
		get
		{
			return _enable;
		}
		set
		{
			if (_enable != value)
			{
				_enable = value;
				OnPropertyChanged("Enable");
				XearSDK.SetProperty(_audEndptUID, "Enable_EQ_GFX", _enable ? 1 : 0, out var _);
			}
		}
	}

	public int Band1
	{
		get
		{
			return _eqValues[0];
		}
		set
		{
			SetBand(0, value);
		}
	}

	public int Band2
	{
		get
		{
			return _eqValues[1];
		}
		set
		{
			SetBand(1, value);
		}
	}

	public int Band3
	{
		get
		{
			return _eqValues[2];
		}
		set
		{
			SetBand(2, value);
		}
	}

	public int Band4
	{
		get
		{
			return _eqValues[3];
		}
		set
		{
			SetBand(3, value);
		}
	}

	public int Band5
	{
		get
		{
			return _eqValues[4];
		}
		set
		{
			SetBand(4, value);
		}
	}

	public int Band6
	{
		get
		{
			return _eqValues[5];
		}
		set
		{
			SetBand(5, value);
		}
	}

	public int Band7
	{
		get
		{
			return _eqValues[6];
		}
		set
		{
			SetBand(6, value);
		}
	}

	public int Band8
	{
		get
		{
			return _eqValues[7];
		}
		set
		{
			SetBand(7, value);
		}
	}

	public int Band9
	{
		get
		{
			return _eqValues[8];
		}
		set
		{
			SetBand(8, value);
		}
	}

	public int Band10
	{
		get
		{
			return _eqValues[9];
		}
		set
		{
			SetBand(9, value);
		}
	}

	public ObservableCollection<JsonEQCustom> ProfileNames
	{
		get
		{
			return _profileNames;
		}
		private set
		{
			if (_profileNames != value)
			{
				_profileNames = value;
				OnPropertyChanged("ProfileNames");
			}
		}
	}

	public int SelectedPreset
	{
		get
		{
			return _selectedPreset;
		}
		set
		{
			if (_selectedPreset == value)
			{
				return;
			}
			_selectedPreset = value;
			OnPropertyChanged("SelectedPreset");
			if (_selectedPreset >= 0 && _selectedPreset < EQPresetList.Count)
			{
				for (int i = 0; i < 10; i++)
				{
					_eqValues[i] = EQPresetList[_selectedPreset].eqValues[i];
					OnPropertyChanged($"Band{i + 1}");
					XearSDK.SetProperty(_audEndptUID, propertyName_EQSlider[i], _eqValues[i], out var _);
				}
			}
		}
	}

	public JsonEQCustom SelectedProfile
	{
		get
		{
			return _selectedProfile;
		}
		set
		{
			if (_selectedProfile == value)
			{
				return;
			}
			_selectedProfile = value;
			OnPropertyChanged("SelectedProfile");
			if (_selectedProfile != null)
			{
				for (int i = 0; i < 10; i++)
				{
					_eqValues[i] = _selectedProfile.eqValues[i];
					OnPropertyChanged($"Band{i + 1}");
					XearSDK.SetProperty(_audEndptUID, propertyName_EQSlider[i], _eqValues[i], out var _);
				}
				CheckEQValueIsModed();
			}
		}
	}

	public string ProfileName
	{
		get
		{
			return _profileName;
		}
		set
		{
			if (_profileName != value)
			{
				_profileName = value;
				OnPropertyChanged("ProfileName");
			}
		}
	}

	public bool EnableSave
	{
		get
		{
			return _enableSave;
		}
		set
		{
			if (_enableSave != value)
			{
				_enableSave = value;
				OnPropertyChanged("EnableSave");
			}
		}
	}

	public bool IsPreset00Visible
	{
		get
		{
			return _isPreset00Visible;
		}
		set
		{
			if (_isPreset00Visible != value)
			{
				_isPreset00Visible = value;
				OnPropertyChanged("IsPreset00Visible");
			}
		}
	}

	public bool IsPreset01Visible
	{
		get
		{
			return _isPreset01Visible;
		}
		set
		{
			if (_isPreset01Visible != value)
			{
				_isPreset01Visible = value;
				OnPropertyChanged("IsPreset01Visible");
			}
		}
	}

	public bool IsPreset02Visible
	{
		get
		{
			return _isPreset02Visible;
		}
		set
		{
			if (_isPreset02Visible != value)
			{
				_isPreset02Visible = value;
				OnPropertyChanged("IsPreset02Visible");
			}
		}
	}

	public bool IsPreset03Visible
	{
		get
		{
			return _isPreset03Visible;
		}
		set
		{
			if (_isPreset03Visible != value)
			{
				_isPreset03Visible = value;
				OnPropertyChanged("IsPreset03Visible");
			}
		}
	}

	public bool IsPreset04Visible
	{
		get
		{
			return _isPreset04Visible;
		}
		set
		{
			if (_isPreset04Visible != value)
			{
				_isPreset04Visible = value;
				OnPropertyChanged("IsPreset04Visible");
			}
		}
	}

	public bool IsPreset05Visible
	{
		get
		{
			return _isPreset05Visible;
		}
		set
		{
			if (_isPreset05Visible != value)
			{
				_isPreset05Visible = value;
				OnPropertyChanged("IsPreset05Visible");
			}
		}
	}

	public bool IsPreset06Visible
	{
		get
		{
			return _isPreset06Visible;
		}
		set
		{
			if (_isPreset06Visible != value)
			{
				_isPreset06Visible = value;
				OnPropertyChanged("IsPreset06Visible");
			}
		}
	}

	public bool IsPreset07Visible
	{
		get
		{
			return _isPreset07Visible;
		}
		set
		{
			if (_isPreset07Visible != value)
			{
				_isPreset07Visible = value;
				OnPropertyChanged("IsPreset07Visible");
			}
		}
	}

	public bool IsPreset08Visible
	{
		get
		{
			return _isPreset08Visible;
		}
		set
		{
			if (_isPreset08Visible != value)
			{
				_isPreset08Visible = value;
				OnPropertyChanged("IsPreset08Visible");
			}
		}
	}

	public bool IsPreset09Visible
	{
		get
		{
			return _isPreset09Visible;
		}
		set
		{
			if (_isPreset09Visible != value)
			{
				_isPreset09Visible = value;
				OnPropertyChanged("IsPreset09Visible");
			}
		}
	}

	public bool IsPreset10Visible
	{
		get
		{
			return _isPreset10Visible;
		}
		set
		{
			if (_isPreset10Visible != value)
			{
				_isPreset10Visible = value;
				OnPropertyChanged("IsPreset10Visible");
			}
		}
	}

	public bool IsPreset11Visible
	{
		get
		{
			return _isPreset11Visible;
		}
		set
		{
			if (_isPreset11Visible != value)
			{
				_isPreset11Visible = value;
				OnPropertyChanged("IsPreset11Visible");
			}
		}
	}

	public List<string> propertyNames { get; set; } = new List<string>();

	public event PropertyChangedEventHandler PropertyChanged;

	private void SetBand(int index, int value)
	{
		if (_eqValues[index] != value)
		{
			_eqValues[index] = value;
			OnPropertyChanged($"Band{index + 1}");
			XearSDK.SetProperty(_audEndptUID, propertyName_EQSlider[index], value, out var _);
			CheckEQValueIsModed();
			CheckEQValueIsCustom();
		}
	}

	public EQViewModel(Guid audEndptUID)
	{
		_audEndptUID = audEndptUID;
		InitPropertyNames();
		LoadEQPreset();
		LoadEQCustom();
		InitCustomCombobox();
		RefreshAllControls();
		SaveCommand = new RelayCommand((object _) =>
		{
			SaveProfile();
		});
		DeleteProfileCommand = new RelayCommand((object obj) =>
		{
			DeleteProfile(obj as JsonEQCustom);
		});
	}

	public void RefreshAllControls()
	{
		if (XearSDK.GetProperty(_audEndptUID, "Enable_EQ_GFX", out int enable, out XearSDK.CmErr cmErr))
		{
			_enable = enable == 1;
		}
		for (int i = 0; i < 10; i++)
		{
			if (XearSDK.GetProperty(_audEndptUID, propertyName_EQSlider[i], out int value, out cmErr))
			{
				_eqValues[i] = value;
			}
		}
		CheckEQValueIsModed();
		CheckEQValueIsCustom();
		OnPropertyChanged("Enable");
		for (int j = 0; j < 10; j++)
		{
			OnPropertyChanged($"Band{j + 1}");
		}
	}

	public void InitPropertyNames()
	{
		propertyNames.Add("Enable_EQ_GFX");
		string[] array = propertyName_EQSlider;
		foreach (string name in array)
		{
			propertyNames.Add(name);
		}
	}

	private void LoadEQPreset()
	{
		if (!File.Exists(DriverInstance.getInstance().configManager.GetEQPresetPath()))
		{
			EQPresetList.Clear();
			for (int i = 0; i <= 11; i++)
			{
				JsonEQPreset preset = new JsonEQPreset
				{
					index = i
				};
				for (int j = 0; j < 10; j++)
				{
					preset.eqValues[j] = cmediaEQPresetValue[i, j];
				}
				EQPresetList.Add(preset);
			}
		}
		else
		{
			EQPresetList = DriverInstance.getInstance().configManager.LoadEQPreset();
		}
		ApplyCountToPresetVisibility(EQPresetList.Count);
	}

	private void LoadEQCustom()
	{
		if (File.Exists(DriverInstance.getInstance().configManager.GetEQCustomPath()))
		{
			EQCustomList.Clear();
			EQCustomList = DriverInstance.getInstance().configManager.LoadEQCustom();
		}
	}

	private void CheckEQValueIsModed()
	{
		int matchedIndex = -1;
		int modeCount = EQPresetList.Count;
		for (int mode = 0; mode < modeCount; mode++)
		{
			bool matched = true;
			for (int b = 0; b < 10; b++)
			{
				if (EQPresetList[mode].eqValues[b] != _eqValues[b])
				{
					matched = false;
					break;
				}
			}
			if (matched)
			{
				matchedIndex = mode;
				break;
			}
		}
		_selectedPreset = matchedIndex;
		OnPropertyChanged("SelectedPreset");
	}

	private void CheckEQValueIsCustom()
	{
		new JsonEQCustom();
		bool matched = true;
		if (EQCustomList.Count == 0)
		{
			EnableSave = true;
		}
		foreach (JsonEQCustom custom in EQCustomList)
		{
			matched = true;
			for (int b = 0; b < 10; b++)
			{
				if (custom.eqValues[b] != _eqValues[b])
				{
					matched = false;
					break;
				}
			}
			if (matched)
			{
				SelectedProfile = custom;
				EnableSave = false;
				break;
			}
		}
		if (!matched)
		{
			SelectedProfile = null;
			EnableSave = true;
		}
		OnPropertyChanged("SelectedProfile");
	}

	private void InitCustomCombobox()
	{
		ProfileNames = new ObservableCollection<JsonEQCustom>(EQCustomList);
	}

	private void SaveProfile()
	{
		string profileName = ProfileName;
		if (string.IsNullOrEmpty(profileName))
		{
			int number = 1;
			string numbered;
			while (true)
			{
				numbered = $"Profile {number}";
				if (EQCustomList.All((JsonEQCustom x) => x.name != numbered))
				{
					break;
				}
				number++;
			}
			profileName = numbered;
		}
		JsonEQCustom jsonEQCustom = new JsonEQCustom
		{
			name = profileName,
			eqValues = new List<int>(_eqValues)
		};
		EQCustomList.Add(jsonEQCustom);
		DriverInstance.getInstance().configManager.SaveEQCustom(EQCustomList);
		InitCustomCombobox();
		SelectedProfile = jsonEQCustom;
		ProfileName = "";
	}

	private void DeleteProfile(JsonEQCustom profile)
	{
		if (profile != null && EQCustomList.Contains(profile))
		{
			EQCustomList.Remove(profile);
			DriverInstance.getInstance().configManager.SaveEQCustom(EQCustomList);
			InitCustomCombobox();
			CheckEQValueIsCustom();
		}
	}

	public void ApplyCountToPresetVisibility(int count)
	{
		if (count < 1)
		{
			count = 1;
		}
		if (count > 12)
		{
			count = 12;
		}
		IsPreset00Visible = 0 < count;
		IsPreset01Visible = 1 < count;
		IsPreset02Visible = 2 < count;
		IsPreset03Visible = 3 < count;
		IsPreset04Visible = 4 < count;
		IsPreset05Visible = 5 < count;
		IsPreset06Visible = 6 < count;
		IsPreset07Visible = 7 < count;
		IsPreset08Visible = 8 < count;
		IsPreset09Visible = 9 < count;
		IsPreset10Visible = 10 < count;
		IsPreset11Visible = 11 < count;
	}

	private void BuildPresetFileTEST()
	{
		EQPresetList.Clear();
		for (int i = 0; i <= 11; i++)
		{
			JsonEQPreset preset = new JsonEQPreset
			{
				index = i,
				eqValues = new List<int>(_eqValues)
			};
			for (int j = 0; j < 10; j++)
			{
				preset.eqValues[j] = cmediaEQPresetValue[i, j];
			}
			EQPresetList.Add(preset);
		}
		DriverInstance.getInstance().configManager.SaveEQPreset(EQPresetList);
	}

	private void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
