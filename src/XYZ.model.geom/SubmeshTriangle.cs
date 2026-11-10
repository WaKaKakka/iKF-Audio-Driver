using System;
using OpenTK;

namespace XYZ.model.geom;

public class SubmeshTriangle
{
	public int vertex1;

	public int vertex2;

	public int vertex3;

	public int color;

	public SubmeshTriangle(int v1, int v2, int v3, int col)
	{
		vertex1 = v1;
		vertex2 = v2;
		vertex3 = v3;
		color = col;
	}

	public void Normal(Submesh mesh, out float nx, out float ny, out float nz)
	{
		Vector3 v0 = mesh.vertices[vertex1];
		Vector3 v1 = mesh.vertices[vertex2];
		Vector3 vector = mesh.vertices[vertex3];
		float a1 = v1.X - v0.X;
		float a2 = v1.Y - v0.Y;
		float a3 = v1.Z - v0.Z;
		float b1 = vector.X - v1.X;
		float b2 = vector.Y - v1.Y;
		float b3 = vector.Z - v1.Z;
		nx = a2 * b3 - a3 * b2;
		ny = a3 * b1 - a1 * b3;
		nz = a1 * b2 - a2 * b1;
		float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
		if (length == 0f)
		{
			nx = (ny = 0f);
			nz = 1f;
		}
		else
		{
			nx /= length;
			ny /= length;
			nz /= length;
		}
	}
}
