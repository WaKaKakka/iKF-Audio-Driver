using System;
using System.Collections.Generic;
using System.IO;
using XYZ.model.geom;

namespace XYZ.MeshInOut;

internal class MtlLoader
{
	public Dictionary<string, Material> Load(string mtlPath)
	{
		Dictionary<string, Material> materials = new Dictionary<string, Material>();
		using StreamReader reader = new StreamReader(mtlPath);
		Material currentMaterial = null;
		char[] split = new char[2] { ' ', '\t' };
		string line;
		while ((line = reader.ReadLine()) != null)
		{
			string[] tokens = line.Split(split, StringSplitOptions.RemoveEmptyEntries);
			if (tokens.Length == 0)
			{
				continue;
			}
			if (tokens[0] == "newmtl")
			{
				currentMaterial = new Material();
				string keyName = "";
				for (int i = 1; i < tokens.Length; i++)
				{
					keyName += tokens[i];
				}
				materials[keyName] = currentMaterial;
			}
			else if (tokens.Length > 1)
			{
				switch (tokens[0])
				{
				case "Ka":
					currentMaterial.AmbientColor = new float[3]
					{
						float.Parse(tokens[1]),
						float.Parse(tokens[2]),
						float.Parse(tokens[3])
					};
					break;
				case "Kd":
					currentMaterial.DiffuseColor = new float[3]
					{
						float.Parse(tokens[1]),
						float.Parse(tokens[2]),
						float.Parse(tokens[3])
					};
					break;
				case "Ks":
					currentMaterial.SpecularColor = new float[3]
					{
						float.Parse(tokens[1]),
						float.Parse(tokens[2]),
						float.Parse(tokens[3])
					};
					break;
				case "Ns":
					currentMaterial.Shininess = float.Parse(tokens[1]);
					break;
				case "map_Kd":
					currentMaterial.DiffuseTextureMap = tokens[1];
					break;
				}
			}
		}
		return materials;
	}
}
