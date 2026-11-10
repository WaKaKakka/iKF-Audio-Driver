using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using XYZ.model.geom;

namespace XYZ.MeshInOut;

public class MeshIOObj : MeshIOBase
{
	public int Load(string filename, List<TopoModel> models, RHBoundingBox boundingBox)
	{
		Status = STATUS.Busy;
		char[] split = new char[1] { ' ' };
		models.Clear();
		bool error = false;
		Dictionary<string, Material> dicMaterials = null;
		TopoModel model = null;
		string previous_cmd = "";
		try
		{
			string[] array = File.ReadAllText(filename).Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
			int vPos = 0;
			int vnPos = 0;
			List<TopoVertex> vList = new List<TopoVertex>();
			List<RHVector3> vnList = new List<RHVector3>();
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string line = array2[i].Trim();
				if (line.Length == 0)
				{
					continue;
				}
				int p = line.IndexOf(" ");
				if (p < 0)
				{
					continue;
				}
				string cmd = line.Substring(0, p);
				switch (cmd)
				{
				case "v":
				{
					RHVector3 vert2 = extractVector(line.Substring(p + 1));
					TopoVertex vertex = new TopoVertex(vPos, vert2);
					boundingBox.Add(vert2);
					vList.Add(vertex);
					vPos++;
					break;
				}
				case "vn":
				{
					RHVector3 vert = extractVector(line.Substring(p + 1));
					vnList.Add(vert);
					vnPos++;
					break;
				}
				case "mtllib":
				{
					string mtlFilename = line.Substring(p + 1);
					mtlFilename = filename.Remove(filename.LastIndexOf("\\") + 1) + mtlFilename;
					dicMaterials = new MtlLoader().Load(mtlFilename);
					break;
				}
				case "usemtl":
				{
					if (previous_cmd == "f")
					{
						models.Add(model);
						previous_cmd = cmd;
						model = null;
					}
					model = new TopoModel();
					string[] parts = line.Split(split, StringSplitOptions.RemoveEmptyEntries);
					string mtlName = "";
					for (int j = 1; j < parts.Length; j++)
					{
						mtlName += parts[j];
					}
					if (parts.Length > 1)
					{
						model.usemtl = mtlName;
						model.material = dicMaterials[mtlName];
					}
					break;
				}
				case "f":
				{
					previous_cmd = cmd;
					line = line.Substring(p + 1).Replace("  ", " ");
					string[] parts2 = line.Split(split, StringSplitOptions.RemoveEmptyEntries);
					int[] vidx = new int[parts2.Length];
					int[] nidx = new int[parts2.Length];
					RHVector3 norm = null;
					p = 0;
					string[] array3 = parts2;
					for (int k = 0; k < array3.Length; k++)
					{
						string[] sp = array3[k].Split('/');
						if (sp.Length != 0)
						{
							int.TryParse(sp[0], out vidx[p]);
							if (vidx[p] < 0)
							{
								vidx[p] += vPos;
							}
							else
							{
								vidx[p]--;
							}
							if (vidx[p] < 0 || vidx[p] >= vPos)
							{
								error = true;
								break;
							}
						}
						if (sp.Length > 2 && norm == null)
						{
							int.TryParse(sp[2], out nidx[p]);
							if (nidx[p] < 0)
							{
								nidx[p] += vnPos;
							}
							else
							{
								nidx[p]--;
							}
							if (nidx[p] >= 0 && nidx[p] < vnPos)
							{
								norm = vnList[nidx[p]];
							}
						}
						p++;
					}
					TopoTriangle triangle = new TopoTriangle(model, vList[vidx[0]], vList[vidx[1]], vList[vidx[2]]);
					model.AddTriangle(triangle);
					if (parts2.Length >= 4)
					{
						TopoTriangle triangle2 = new TopoTriangle(model, vList[vidx[0]], vList[vidx[2]], vList[vidx[3]]);
						model.AddTriangle(triangle2);
					}
					if (parts2.Length >= 5)
					{
						TopoTriangle triangle3 = new TopoTriangle(model, vList[vidx[0]], vList[vidx[3]], vList[vidx[4]]);
						model.AddTriangle(triangle3);
					}
					break;
				}
				default:
					if (cmd != "f" && cmd != "s" && previous_cmd == "f")
					{
						models.Add(model);
						previous_cmd = cmd;
						model = null;
					}
					break;
				}
				if (error)
				{
					break;
				}
			}
			if (model != null)
			{
				models.Add(model);
			}
		}
		catch
		{
			error = true;
		}
		if (error)
		{
			model?.Clear();
		}
		if (Status == STATUS.Busy)
		{
			Status = STATUS.Done;
		}
		if (error)
		{
			return -1;
		}
		return 0;
	}

	private RHVector3 extractVector(string s)
	{
		RHVector3 v = new RHVector3(0f, 0f, 0f);
		s = s.Trim().Replace("\t", " ");
		int p = s.IndexOf(' ');
		if (p < 0)
		{
			throw new Exception("Format error");
		}
		double.TryParse(s.Substring(0, p), NumberStyles.Float, NumFormat, out v.x);
		s = s.Substring(p).Trim();
		p = s.IndexOf(' ');
		if (p < 0)
		{
			throw new Exception("Format error");
		}
		double.TryParse(s.Substring(0, p), NumberStyles.Float, NumFormat, out v.y);
		s = s.Substring(p).Trim();
		double.TryParse(s, NumberStyles.Float, NumFormat, out v.z);
		return v;
	}
}
