using System;

namespace XYZ.model.geom;

public class RHVector3
{
	public double x;

	public double y;

	public double z;

	public double Length => Math.Sqrt(x * x + y * y + z * z);

	public double this[int dimension]
	{
		get
		{
			return dimension switch
			{
				0 => x, 
				1 => y, 
				_ => z, 
			};
		}
		set
		{
			switch (dimension)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			default:
				z = value;
				break;
			}
		}
	}

	public RHVector3(double _x, double _y, double _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public RHVector3(float _x, float _y, float _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public RHVector3(RHVector3 orig)
	{
		if (orig == null)
		{
			x = 0.0;
			y = 0.0;
			z = 0.0;
		}
		else
		{
			x = orig.x;
			y = orig.y;
			z = orig.z;
		}
	}

	public void NormalizeSafe()
	{
		double l = Length;
		if (l == 0.0)
		{
			x = (y = 0.0);
			z = 0.0;
		}
		else
		{
			x /= l;
			y /= l;
			z /= l;
		}
	}

	public void StoreMinimum(RHVector3 vec)
	{
		x = Math.Min(x, vec.x);
		y = Math.Min(y, vec.y);
		z = Math.Min(z, vec.z);
	}

	public void StoreMaximum(RHVector3 vec)
	{
		x = Math.Max(x, vec.x);
		y = Math.Max(y, vec.y);
		z = Math.Max(z, vec.z);
	}

	public double Distance(RHVector3 point)
	{
		double num = point.x - x;
		double dy = point.y - y;
		double dz = point.z - z;
		return Math.Sqrt(num * num + dy * dy + dz * dz);
	}

	public void Scale(double factor)
	{
		x *= (float)factor;
		y *= (float)factor;
		z *= (float)factor;
	}

	public void Scale(float factor)
	{
		x *= factor;
		y *= factor;
		z *= factor;
	}

	public double ScalarProduct(RHVector3 vector)
	{
		return x * vector.x + y * vector.y + z * vector.z;
	}

	public double AngleForNormalizedVectors(RHVector3 direction)
	{
		return Math.Acos(ScalarProduct(direction));
	}

	public double Angle(RHVector3 direction)
	{
		return Math.Acos(ScalarProduct(direction) / (Length * direction.Length));
	}

	public RHVector3 Subtract(RHVector3 vector)
	{
		return new RHVector3(x - vector.x, y - vector.y, z - vector.z);
	}

	public RHVector3 Add(RHVector3 vector)
	{
		return new RHVector3(x + vector.x, y + vector.y, z + vector.z);
	}

	public void SubtractInternal(RHVector3 vector)
	{
		x -= vector.x;
		y -= vector.y;
		z -= vector.z;
	}

	public void AddInternal(RHVector3 vector)
	{
		x += vector.x;
		y += vector.y;
		z += vector.z;
	}

	public RHVector3 CrossProduct(RHVector3 vector)
	{
		return new RHVector3(y * vector.z - z * vector.y, z * vector.x - x * vector.z, x * vector.y - y * vector.x);
	}

	public override bool Equals(object obj)
	{
		RHVector3 compare = obj as RHVector3;
		if (x == compare.x && y == compare.y && z == compare.z)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (x * 5915587277.0 + y * 1500450271.0 + z * 3267000013.0).GetHashCode();
	}

	public override string ToString()
	{
		return "(" + x + ";" + y + ";" + z + ")";
	}
}
