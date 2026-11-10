using System;

namespace XYZ.model.geom;

public class TopoTriangle
{
	public TopoVertex[] vertices = new TopoVertex[3];

	public int id;

	public RHVector3 normal;

	public RHVector3 Center
	{
		get
		{
			RHVector3 rHVector = vertices[0].pos.Add(vertices[1].pos).Add(vertices[2].pos);
			rHVector.Scale(1.0 / 3.0);
			return rHVector;
		}
	}

	public TopoTriangle(int _id, TopoVertex v1, TopoVertex v2, TopoVertex v3)
	{
		id = _id;
		vertices[0] = v1;
		vertices[1] = v2;
		vertices[2] = v3;
		RecomputeNormal();
	}

	public TopoTriangle(TopoVertex v1, TopoVertex v2, TopoVertex v3)
	{
		vertices[0] = v1;
		vertices[1] = v2;
		vertices[2] = v3;
		RecomputeNormal();
	}

	public TopoTriangle(TopoModel model, TopoVertex v1, TopoVertex v2, TopoVertex v3, double nx, double ny, double nz)
	{
		vertices[0] = v1;
		vertices[1] = v2;
		vertices[2] = v3;
		normal = new RHVector3(nx, ny, nz);
	}

	public TopoTriangle(TopoModel model, TopoVertex v1, TopoVertex v2, TopoVertex v3)
	{
		vertices[0] = v1;
		vertices[1] = v2;
		vertices[2] = v3;
		RecomputeNormal();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is TopoTriangle))
		{
			return false;
		}
		if (((TopoTriangle)obj).vertices[0] == vertices[0] && ((TopoTriangle)obj).vertices[1] == vertices[1])
		{
			return ((TopoTriangle)obj).vertices[2] == vertices[2];
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((vertices[0].pos.x + vertices[0].pos.y + vertices[0].pos.z) * 5915587277.0 + (vertices[1].pos.x + vertices[1].pos.y + vertices[1].pos.z) * 1500450271.0 + (vertices[2].pos.x + vertices[2].pos.y + vertices[2].pos.z) * 3267000013.0).GetHashCode();
	}

	public void Unlink(TopoModel model)
	{
	}

	public void FlipDirection()
	{
		normal.Scale(-1f);
		TopoVertex v = vertices[0];
		vertices[0] = vertices[1];
		vertices[1] = v;
	}

	public void RecomputeNormal()
	{
		RHVector3 d1 = vertices[1].pos.Subtract(vertices[0].pos);
		RHVector3 d2 = vertices[2].pos.Subtract(vertices[1].pos);
		normal = d1.CrossProduct(d2);
		normal.NormalizeSafe();
	}

	public int VertexIndexFor(TopoVertex test)
	{
		if (test == vertices[0])
		{
			return 0;
		}
		if (test == vertices[1])
		{
			return 1;
		}
		if (test == vertices[2])
		{
			return 2;
		}
		return -1;
	}

	public double SignedVolume()
	{
		return vertices[0].pos.ScalarProduct(vertices[1].pos.CrossProduct(vertices[2].pos)) / 6.0;
	}

	public double Area()
	{
		RHVector3 d1 = vertices[1].pos.Subtract(vertices[0].pos);
		RHVector3 d2 = vertices[2].pos.Subtract(vertices[1].pos);
		return 0.5 * d1.CrossProduct(d2).Length;
	}

	public bool IsDegenerated()
	{
		if (vertices[0] == vertices[1] || vertices[1] == vertices[2] || vertices[2] == vertices[0])
		{
			return true;
		}
		return false;
	}

	public bool CheckIfColinear()
	{
		RHVector3 zero = new RHVector3(0f, 0f, 0f);
		RHVector3 rHVector = vertices[1].pos.Subtract(vertices[0].pos);
		RHVector3 d2 = vertices[2].pos.Subtract(vertices[1].pos);
		if (!rHVector.CrossProduct(d2).Equals(zero))
		{
			return false;
		}
		return true;
	}

	public int NumberOfSharedVertices(TopoTriangle tri)
	{
		int sameVertices = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (vertices[i] == tri.vertices[j])
				{
					sameVertices++;
					break;
				}
			}
		}
		return sameVertices;
	}

	public bool SameNormalOrientation(TopoTriangle test)
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (vertices[i] == test.vertices[j] && vertices[(i + 1) % 3] == test.vertices[(j + 2) % 3])
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ToSphere(out RHVector3 center, out double radius)
	{
		center = new RHVector3(Center);
		radius = vertices[0].pos.Distance(center);
	}

	public double DistanceToPlane(RHVector3 pos)
	{
		double d = vertices[0].pos.ScalarProduct(normal);
		return pos.ScalarProduct(normal) - d;
	}

	private void DominantAxis(out int d1, out int d2)
	{
		double n1 = Math.Abs(normal.x);
		double n2 = Math.Abs(normal.y);
		double n3 = Math.Abs(normal.z);
		if (n1 > n2 && n1 > n3)
		{
			d1 = 1;
			d2 = 2;
		}
		else if (n2 > n3)
		{
			d1 = 0;
			d2 = 2;
		}
		else
		{
			d1 = 0;
			d2 = 1;
		}
	}
}
