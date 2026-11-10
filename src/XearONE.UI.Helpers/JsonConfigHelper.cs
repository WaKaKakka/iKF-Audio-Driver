using System;
using System.IO;
using Newtonsoft.Json;

namespace XearONE.UI.Helpers;

public class JsonConfigHelper
{
	public void Save<T>(string filePath, T data)
	{
		try
		{
			string folder = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			using (FileStream fs = File.Create(filePath))
			{
				fs.Close();
			}
			string json = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(filePath, json);
		}
		catch (UnauthorizedAccessException ex)
		{
			LogHelper.Warning("Access denied writing to: " + filePath + "\n" + ex.Message, isPrintCaller: true, "Save", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Helpers\\JsonConfigHelper.cs");
		}
		catch (Exception ex2)
		{
			LogHelper.Warning("Error writing to: " + filePath + "\n" + ex2.Message, isPrintCaller: false, "Save", "D:\\Project_New\\UI\\customization\\CM002X1_C-Media_Standard2\\XearONE.UI\\Helpers\\JsonConfigHelper.cs");
		}
	}

	public T Load<T>(string filePath) where T : new()
	{
		try
		{
			if (!File.Exists(filePath))
			{
				Save(filePath, new T());
			}
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
		}
		catch
		{
			return new T();
		}
	}
}
