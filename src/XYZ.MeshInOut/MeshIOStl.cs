using System;
using System.Globalization;
using System.IO;
using XYZ.model.geom;

namespace XYZ.MeshInOut;

public class MeshIOStl : MeshIOBase
{
	public enum FileType
	{
		Unknown,
		Binary,
		UTF8
	}

	public override int Load(string filename, TopoModel model, Action<int> updateRate)
	{
		importSTL(filename, model, updateRate);
		return 0;
	}

	public int Load(string filename, TopoModel model, RHBoundingBox boundingBox)
	{
		importSTL(filename, model, boundingBox);
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

	private void ReadArray(Stream stream, byte[] data)
	{
		int offset = 0;
		int remaining = data.Length;
		try
		{
			while (remaining > 0)
			{
				int read = stream.Read(data, offset, remaining);
				if (read <= 0)
				{
					throw new EndOfStreamException($"End of stream reached with {remaining} bytes left to read");
				}
				remaining -= read;
				offset += read;
			}
		}
		catch
		{
		}
	}

	protected void importSTLAscii(string filename, TopoModel model, Action<int> updateRate)
	{
		string text = File.ReadAllText(filename);
		int lastP = 0;
		int count = 0;
		int max = text.Length;
		int p;
		while ((p = text.IndexOf("facet", lastP)) > 0)
		{
			count++;
			if (count % 4000 == 0)
			{
				updateRate?.Invoke((int)((double)lastP / (double)max * 100.0));
				if (Command == COMMAND.Abort)
				{
					Command = COMMAND.None;
					Status = STATUS.UserAbort;
					break;
				}
			}
			int num = text.IndexOf("endfacet", p + 5);
			int normal = text.IndexOf("normal", p) + 6;
			int outer = text.IndexOf("outer loop", normal);
			RHVector3 normalVect = extractVector(text.Substring(normal, outer - normal));
			normalVect.NormalizeSafe();
			outer += 10;
			int vertex = text.IndexOf("vertex", outer) + 6;
			int vertex2 = text.IndexOf("vertex", vertex);
			RHVector3 p2 = extractVector(text.Substring(vertex, vertex2 - vertex));
			vertex2 += 7;
			vertex = text.IndexOf("vertex", vertex2);
			RHVector3 p3 = extractVector(text.Substring(vertex2, vertex - vertex2));
			vertex += 7;
			vertex2 = text.IndexOf("endloop", vertex);
			RHVector3 p4 = extractVector(text.Substring(vertex, vertex2 - vertex));
			lastP = num + 8;
			model.addTriangle(p2, p3, p4, normalVect);
		}
	}

	protected void importSTL(string filename, TopoModel model, Action<int> updateRate)
	{
		Status = STATUS.Busy;
		model.Clear();
		try
		{
			FileStream f = File.OpenRead(filename);
			byte[] header = new byte[80];
			ReadArray(f, header);
			BinaryReader r = new BinaryReader(f);
			int nTri = r.ReadInt32();
			if (f.Length != 84 + nTri * 50)
			{
				r.Close();
				f.Close();
				importSTLAscii(filename, model, updateRate);
			}
			else
			{
				for (int i = 0; i < nTri; i++)
				{
					if (i > 0 && i % 4000 == 0)
					{
						updateRate?.Invoke((int)((double)i / (double)nTri * 100.0));
						if (Command == COMMAND.Abort)
						{
							Command = COMMAND.None;
							Status = STATUS.UserAbort;
							return;
						}
					}
					RHVector3 normal = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					RHVector3 p1 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					RHVector3 p2 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					RHVector3 p3 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					normal.NormalizeSafe();
					model.addTriangle(p1, p2, p3, normal);
					r.ReadUInt16();
				}
				r.Close();
				f.Close();
			}
		}
		catch
		{
			throw;
		}
		if (Status == STATUS.Busy)
		{
			Status = STATUS.Done;
		}
	}

	protected void importSTL(string filename, TopoModel model, RHBoundingBox boundingBox)
	{
		Status = STATUS.Busy;
		model.Clear();
		try
		{
			FileStream f = File.OpenRead(filename);
			byte[] header = new byte[80];
			ReadArray(f, header);
			BinaryReader r = new BinaryReader(f);
			int nTri = r.ReadInt32();
			if (f.Length != 84 + nTri * 50)
			{
				r.Close();
				f.Close();
				importSTLAscii(filename, model, null);
			}
			else
			{
				for (int i = 0; i < nTri; i++)
				{
					if (i > 0 && i % 4000 == 0 && Command == COMMAND.Abort)
					{
						Command = COMMAND.None;
						Status = STATUS.UserAbort;
						return;
					}
					RHVector3 normal = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					RHVector3 p1 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					RHVector3 p2 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					RHVector3 p3 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
					normal.NormalizeSafe();
					model.addTriangle(p1, p2, p3, normal);
					r.ReadUInt16();
					boundingBox.Add(p1);
					boundingBox.Add(p2);
					boundingBox.Add(p3);
				}
				r.Close();
				f.Close();
			}
		}
		catch
		{
			throw;
		}
		if (Status == STATUS.Busy)
		{
			Status = STATUS.Done;
		}
	}

	public void importByteArray(ref byte[] stlArr, TopoModel model, Action<int> updateRate = null)
	{
		MemoryStream stream = new MemoryStream();
		stream.Write(stlArr, 0, stlArr.Length);
		stream.Position = 0L;
		byte[] header = new byte[80];
		ReadArray(stream, header);
		BinaryReader r = new BinaryReader(stream);
		int nTri = r.ReadInt32();
		try
		{
			for (int i = 0; i < nTri; i++)
			{
				if (Command == COMMAND.Abort)
				{
					Command = COMMAND.None;
					Status = STATUS.UserAbort;
					return;
				}
				updateRate?.Invoke((int)((double)i / (double)nTri * 100.0));
				RHVector3 normal = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				RHVector3 p1 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				RHVector3 p2 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				RHVector3 p3 = new RHVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				normal.NormalizeSafe();
				model.addTriangle(p1, p2, p3, normal);
				r.ReadUInt16();
			}
			r.Close();
			stream.Close();
		}
		catch
		{
			throw;
		}
	}
}
