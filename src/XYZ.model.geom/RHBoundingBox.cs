using System;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace XYZ.model.geom;

public class RHBoundingBox
{
	private static double epsilon = 1E-07;

	public RHVector3 minPoint;

	public RHVector3 maxPoint;

	public double xMin => MinPoint.x;

	public double yMin => MinPoint.y;

	public double zMin => MinPoint.z;

	public double xMax => MaxPoint.x;

	public double yMax => MaxPoint.y;

	public double zMax => MaxPoint.z;

	public float xMinf => Convert.ToSingle(MinPoint.x);

	public float yMinf => Convert.ToSingle(MinPoint.y);

	public float zMinf => Convert.ToSingle(MinPoint.z);

	public float xMaxf => Convert.ToSingle(MaxPoint.x);

	public float yMaxf => Convert.ToSingle(MaxPoint.y);

	public float zMaxf => Convert.ToSingle(MaxPoint.z);

	public RHVector3 MaxPoint
	{
		get
		{
			if (maxPoint != null)
			{
				return maxPoint;
			}
			return new RHVector3(0f, 0f, 0f);
		}
	}

	public RHVector3 MinPoint
	{
		get
		{
			if (minPoint != null)
			{
				return minPoint;
			}
			return new RHVector3(0f, 0f, 0f);
		}
	}

	public RHVector3 Size => MaxPoint.Subtract(MinPoint);

	public RHVector3 Center
	{
		get
		{
			RHVector3 rHVector = MaxPoint.Add(MinPoint);
			rHVector.Scale(0.5);
			return rHVector;
		}
	}

	public TopoTriangle[] getBoundingTri()
	{
		TopoTriangle[] array = new TopoTriangle[12];
		TopoVertex[] vertices = getVertices();
		array[0] = new TopoTriangle(vertices[0], vertices[1], vertices[3]);
		array[1] = new TopoTriangle(vertices[0], vertices[3], vertices[2]);
		array[2] = new TopoTriangle(vertices[4], vertices[6], vertices[7]);
		array[3] = new TopoTriangle(vertices[4], vertices[7], vertices[5]);
		array[4] = new TopoTriangle(vertices[2], vertices[3], vertices[7]);
		array[5] = new TopoTriangle(vertices[2], vertices[7], vertices[6]);
		array[6] = new TopoTriangle(vertices[1], vertices[5], vertices[7]);
		array[7] = new TopoTriangle(vertices[1], vertices[7], vertices[3]);
		array[8] = new TopoTriangle(vertices[0], vertices[4], vertices[5]);
		array[9] = new TopoTriangle(vertices[0], vertices[5], vertices[1]);
		array[10] = new TopoTriangle(vertices[0], vertices[2], vertices[6]);
		array[11] = new TopoTriangle(vertices[0], vertices[6], vertices[4]);
		return array;
	}

	public TopoVertex[] getVertices()
	{
		TopoVertex[] array = new TopoVertex[8];
		RHVector3 MAX = maxPoint;
		RHVector3 min = minPoint;
		array[0] = new TopoVertex(0, new RHVector3(min.x, min.y, min.z));
		array[1] = new TopoVertex(1, new RHVector3(min.x, min.y, MAX.z));
		array[2] = new TopoVertex(2, new RHVector3(min.x, MAX.y, min.z));
		array[3] = new TopoVertex(3, new RHVector3(min.x, MAX.y, MAX.z));
		array[4] = new TopoVertex(4, new RHVector3(MAX.x, min.y, min.z));
		array[5] = new TopoVertex(5, new RHVector3(MAX.x, min.y, MAX.z));
		array[6] = new TopoVertex(6, new RHVector3(MAX.x, MAX.y, min.z));
		array[7] = new TopoVertex(7, new RHVector3(MAX.x, MAX.y, MAX.z));
		return array;
	}

	public bool containTri(TopoTriangle triangle)
	{
		TopoVertex[] vertices = triangle.vertices;
		foreach (TopoVertex ver in vertices)
		{
			if (ContainsPoint(ver.pos))
			{
				return true;
			}
		}
		return false;
	}

	public void Add(RHVector3 point)
	{
		if (minPoint == null)
		{
			minPoint = new RHVector3(point);
			maxPoint = new RHVector3(point);
		}
		else
		{
			minPoint.StoreMinimum(point);
			maxPoint.StoreMaximum(point);
		}
	}

	public void Add(double x, double y, double z)
	{
		Add(new RHVector3(x, y, z));
	}

	public void Add(RHBoundingBox box)
	{
		if (box != null && box.minPoint != null)
		{
			Add(box.minPoint);
			Add(box.maxPoint);
		}
	}

	public void Clear()
	{
		minPoint = (maxPoint = null);
	}

	public bool ContainsPoint(RHVector3 point)
	{
		if (minPoint == null)
		{
			return false;
		}
		if (point.x >= minPoint.x && point.x <= maxPoint.x && point.y >= minPoint.y && point.y <= maxPoint.y && point.z >= minPoint.z)
		{
			return point.z <= maxPoint.z;
		}
		return false;
	}

	public bool IntersectsBox(RHBoundingBox box)
	{
		if (minPoint == null || box.minPoint == null)
		{
			return false;
		}
		bool flag = Overlap(minPoint.x, maxPoint.x, box.minPoint.x, box.maxPoint.x);
		bool yOverlap = Overlap(minPoint.y, maxPoint.y, box.minPoint.y, box.maxPoint.y);
		bool zOverlap = Overlap(minPoint.z, maxPoint.z, box.minPoint.z, box.maxPoint.z);
		return flag & yOverlap & zOverlap;
	}

	private bool Overlap(double p1min, double p1max, double p2min, double p2max)
	{
		if (p2min > p1max + epsilon)
		{
			return false;
		}
		if (p2max + epsilon < p1min)
		{
			return false;
		}
		return true;
	}

	public void Draw()
	{
		Color col = Color.Blue;
		GL.Material(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse, new Color4(0, 0, 0, byte.MaxValue));
		GL.Material(MaterialFace.Front, MaterialParameter.Emission, new Color4(0, 0, 0, 0));
		GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[4] { 0f, 0f, 0f, 1f });
		GL.Material(MaterialFace.Front, MaterialParameter.Emission, new Color4(col.R, col.G, col.B, col.A));
		GL.PushMatrix();
		GL.Begin(PrimitiveType.Lines);
		GL.Vertex3(MinPoint.x, MinPoint.y, MinPoint.z);
		GL.Vertex3(MaxPoint.x, MinPoint.y, MinPoint.z);
		GL.Vertex3(MinPoint.x, MinPoint.y, MinPoint.z);
		GL.Vertex3(MinPoint.x, MaxPoint.y, MinPoint.z);
		GL.Vertex3(MinPoint.x, MinPoint.y, MinPoint.z);
		GL.Vertex3(MinPoint.x, MinPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MaxPoint.y, MaxPoint.z);
		GL.Vertex3(MinPoint.x, MaxPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MaxPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MinPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MaxPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MaxPoint.y, MinPoint.z);
		GL.Vertex3(MinPoint.x, MaxPoint.y, MaxPoint.z);
		GL.Vertex3(MinPoint.x, MaxPoint.y, MinPoint.z);
		GL.Vertex3(MinPoint.x, MaxPoint.y, MaxPoint.z);
		GL.Vertex3(MinPoint.x, MinPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MaxPoint.y, MinPoint.z);
		GL.Vertex3(MaxPoint.x, MinPoint.y, MinPoint.z);
		GL.Vertex3(MaxPoint.x, MaxPoint.y, MinPoint.z);
		GL.Vertex3(MinPoint.x, MaxPoint.y, MinPoint.z);
		GL.Vertex3(MaxPoint.x, MinPoint.y, MaxPoint.z);
		GL.Vertex3(MinPoint.x, MinPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MinPoint.y, MaxPoint.z);
		GL.Vertex3(MaxPoint.x, MinPoint.y, MinPoint.z);
		GL.End();
		GL.PopMatrix();
	}
}
