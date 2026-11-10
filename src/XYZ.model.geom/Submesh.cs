using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace XYZ.model.geom;

public class Submesh
{
	public enum MeshColor
	{
		FrontBack = -1,
		ErrorFace = -2,
		ErrorEdge = -3,
		OutSide = -4,
		EdgeLoop = -5,
		CutEdge = -6,
		Normal = -7,
		Edge = -8,
		Back = -9,
		TransBlue = -10,
		OverhangLv1 = -11,
		OverhangLv2 = -12,
		OverhangLv3 = -13,
		ConeSupport = -14,
		TreeSymbol = -15,
		TreeMesh = -16,
		TreeError = -17,
		TreeSlect = -18,
		TreeTest = -19
	}

	public enum ClipType
	{
		None,
		Top,
		Bottom,
		Both
	}

	public const int MESHCOLOR_FRONTBACK = -1;

	public const int MESHCOLOR_ERRORFACE = -2;

	public const int MESHCOLOR_ERROREDGE = -3;

	public const int MESHCOLOR_OUTSIDE = -4;

	public const int MESHCOLOR_EDGE_LOOP = -5;

	public const int MESHCOLOR_CUT_EDGE = -6;

	public const int MESHCOLOR_NORMAL = -7;

	public const int MESHCOLOR_EDGE = -8;

	public const int MESHCOLOR_BACK = -9;

	public const int MESHCOLOR_TRANS_BLUE = -10;

	public const int MESHCOLOR_PINK = -11;

	public const int MESHCOLOR_LIGHTPINK = -12;

	public const int MESHCOLOR_LIGHTPINK_WHITE = -13;

	public const int MESHCOLOR_CONESUP = -14;

	public const int MESHCOLOR_TREESYM = -15;

	public const int MESHCOLOR_TREEMSH = -16;

	public const int MESHCOLOR_BRNHERR = -17;

	public const int MESHCOLOR_TREESEL = -18;

	public const int MESHCOLOR_TREETES = -19;

	public List<Vector3> vertices = new List<Vector3>();

	public List<SubmeshEdge> edges = new List<SubmeshEdge>();

	public List<SubmeshTriangle> triangles = new List<SubmeshTriangle>();

	public List<SubmeshTriangle> trianglesError = new List<SubmeshTriangle>();

	public bool selected;

	public int extruder;

	public bool clipEnable;

	public ClipType clipMode = ClipType.Top;

	public double[] clipPlaneEq = new double[4] { 0.0, 0.0, -1.0, 0.0 };

	public double[] clipPlaneEqbtm = new double[4] { 0.0, 0.0, 1.0, 0.0 };

	public float zLayerHeight;

	public float[] glVertices;

	public int[] glColors;

	public int[] glEdges;

	public int[] glTriangles;

	public int[] glTrianglesError;

	public int[] glBuffer;

	public float[] glNormals;

	private int ConvertColorIndex(int idx, Color frontBackColor)
	{
		return ColorToRgba32(Color.White);
	}

	private int ColorToRgba32(Color c)
	{
		return (c.A << 24) | (c.B << 16) | (c.G << 8) | c.R;
	}

	public void Compress()
	{
		Compress(override_color: false, 0);
	}

	public void Compress(bool override_color, int color)
	{
		glVertices = new float[3 * vertices.Count];
		glNormals = new float[3 * vertices.Count];
		glColors = new int[vertices.Count];
		glEdges = new int[edges.Count * 2];
		glTriangles = new int[triangles.Count * 3];
		glTrianglesError = new int[trianglesError.Count * 3];
		UpdateDrawLists();
		vertices.Clear();
	}

	public int VertexId(RHVector3 v)
	{
		int count = vertices.Count;
		vertices.Add(new Vector3((float)v.x, (float)v.y, (float)v.z));
		return count;
	}

	public void AddEdge(RHVector3 v1, RHVector3 v2, int color)
	{
		edges.Add(new SubmeshEdge(VertexId(v1), VertexId(v2), color));
	}

	public void AddTriangle(RHVector3 v1, RHVector3 v2, RHVector3 v3, int color)
	{
		if (color == -2)
		{
			trianglesError.Add(new SubmeshTriangle(VertexId(v1), VertexId(v2), VertexId(v3), color));
		}
		else
		{
			triangles.Add(new SubmeshTriangle(VertexId(v1), VertexId(v2), VertexId(v3), color));
		}
	}

	public void UpdateDrawLists()
	{
		int idx = 0;
		foreach (SubmeshTriangle t in triangles)
		{
			int n1 = 3 * t.vertex1;
			int n2 = 3 * t.vertex2;
			int n3 = 3 * t.vertex3;
			Vector3 v1 = vertices[t.vertex1];
			Vector3 v2 = vertices[t.vertex2];
			Vector3 v3 = vertices[t.vertex3];
			t.Normal(this, out glNormals[n1], out glNormals[n1 + 1], out glNormals[n1 + 2]);
			glNormals[n2] = (glNormals[n3] = glNormals[n1]);
			glNormals[n2 + 1] = (glNormals[n3 + 1] = glNormals[n1 + 1]);
			glNormals[n2 + 2] = (glNormals[n3 + 2] = glNormals[n1 + 2]);
			glVertices[n1++] = v1.X;
			glVertices[n1++] = v1.Y;
			glVertices[n1] = v1.Z;
			glVertices[n2++] = v2.X;
			glVertices[n2++] = v2.Y;
			glVertices[n2] = v2.Z;
			glVertices[n3++] = v3.X;
			glVertices[n3++] = v3.Y;
			glVertices[n3] = v3.Z;
			glTriangles[idx++] = t.vertex1;
			glTriangles[idx++] = t.vertex2;
			glTriangles[idx++] = t.vertex3;
		}
		idx = 0;
		foreach (SubmeshTriangle t2 in trianglesError)
		{
			int n4 = 3 * t2.vertex1;
			int n5 = 3 * t2.vertex2;
			int n6 = 3 * t2.vertex3;
			Vector3 v4 = vertices[t2.vertex1];
			Vector3 v5 = vertices[t2.vertex2];
			Vector3 v6 = vertices[t2.vertex3];
			t2.Normal(this, out glNormals[n4], out glNormals[n4 + 1], out glNormals[n4 + 2]);
			glNormals[n5] = (glNormals[n6] = glNormals[n4]);
			glNormals[n5 + 1] = (glNormals[n6 + 1] = glNormals[n4 + 1]);
			glNormals[n5 + 2] = (glNormals[n6 + 2] = glNormals[n4 + 2]);
			glVertices[n4++] = v4.X;
			glVertices[n4++] = v4.Y;
			glVertices[n4] = v4.Z;
			glVertices[n5++] = v5.X;
			glVertices[n5++] = v5.Y;
			glVertices[n5] = v5.Z;
			glVertices[n6++] = v6.X;
			glVertices[n6++] = v6.Y;
			glVertices[n6] = v6.Z;
			glTrianglesError[idx++] = t2.vertex1;
			glTrianglesError[idx++] = t2.vertex2;
			glTrianglesError[idx++] = t2.vertex3;
		}
		idx = 0;
		foreach (SubmeshEdge e in edges)
		{
			int n7 = 3 * e.vertex1;
			int n8 = 3 * e.vertex2;
			Vector3 v7 = vertices[e.vertex1];
			Vector3 v8 = vertices[e.vertex2];
			glNormals[n7] = (glNormals[n8] = 0f);
			glNormals[n7 + 1] = (glNormals[n8 + 1] = 0f);
			glNormals[n7 + 2] = (glNormals[n8 + 2] = 1f);
			glVertices[n7++] = v7.X;
			glVertices[n7++] = v7.Y;
			glVertices[n7] = v7.Z;
			glVertices[n8++] = v8.X;
			glVertices[n8++] = v8.Y;
			glVertices[n8] = v8.Z;
			glEdges[idx++] = e.vertex1;
			glEdges[idx++] = e.vertex2;
		}
	}
}
