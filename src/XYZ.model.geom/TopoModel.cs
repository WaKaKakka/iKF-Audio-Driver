using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace XYZ.model.geom;

public class TopoModel
{
	public enum FileFormat
	{
		Unknown,
		Binary,
		UTF8
	}

	public const bool debugRepair = false;

	public const float epsilon = 0.001f;

	public TopoVertexStorage vertices = new TopoVertexStorage();

	public TopoTriangleStorage triangles = new TopoTriangleStorage();

	public RHBoundingBox boundingBox = new RHBoundingBox();

	public int badEdges;

	public int updatedNormals;

	public int loopEdges;

	public int manyShardEdges;

	public bool normalsOriented;

	public string usemtl;

	private double scaler = 1.0;

	private Submesh submesh;

	public Material material { get; set; }

	public void SetColor(Color modelColor)
	{
		if (material == null)
		{
			material = new Material();
		}
		material.AmbientColor = new float[3];
		material.AmbientColor[0] = (float)(int)modelColor.R / 255f;
		material.AmbientColor[1] = (float)(int)modelColor.G / 255f;
		material.AmbientColor[2] = (float)(int)modelColor.B / 255f;
	}

	public void Clear()
	{
		vertices.Clear();
		triangles.Clear();
		boundingBox.Clear();
		GC.Collect();
	}

	public void addVertex(TopoVertex v)
	{
		vertices.Add(v);
		boundingBox.Add(v.pos);
	}

	public TopoVertex findVertexOrNull(RHVector3 pos)
	{
		return vertices.SearchPoint(pos);
	}

	public TopoVertex addVertex(RHVector3 pos)
	{
		TopoVertex newVertex = findVertexOrNull(pos);
		if (newVertex == null)
		{
			newVertex = new TopoVertex(vertices.Count, pos);
			addVertex(newVertex);
		}
		return newVertex;
	}

	public void UpdateVertexNumbers()
	{
		int i = 1;
		foreach (TopoVertex item in vertices.v)
		{
			item.id = i++;
		}
	}

	public TopoTriangle addTriangle(double p1x, double p1y, double p1z, double p2x, double p2y, double p2z, double p3x, double p3y, double p3z, double nx, double ny, double nz)
	{
		TopoVertex v1 = addVertex(new RHVector3(p1x, p1y, p1z));
		TopoVertex v2 = addVertex(new RHVector3(p2x, p2y, p2z));
		TopoVertex v3 = addVertex(new RHVector3(p3x, p3y, p3z));
		TopoTriangle triangle = new TopoTriangle(this, v1, v2, v3, nx, ny, nz);
		return AddTriangle(triangle);
	}

	public TopoTriangle addTriangle(RHVector3 p1, RHVector3 p2, RHVector3 p3, RHVector3 normal)
	{
		TopoVertex v1 = addVertex(p1);
		TopoVertex v2 = addVertex(p2);
		TopoVertex v3 = addVertex(p3);
		TopoTriangle triangle = new TopoTriangle(this, v1, v2, v3, normal.x, normal.y, normal.z);
		return AddTriangle(triangle);
	}

	public TopoTriangle AddTriangle(TopoTriangle triangle)
	{
		if (triangle.IsDegenerated())
		{
			triangle.Unlink(this);
		}
		else
		{
			triangles.Add(triangle);
		}
		return triangle;
	}

	public void removeTriangle(TopoTriangle triangle)
	{
		triangle.Unlink(this);
		triangles.Remove(triangle);
	}

	private void FillMesh(int defaultColor)
	{
		if (submesh != null)
		{
			return;
		}
		submesh = new Submesh();
		foreach (TopoTriangle t in triangles)
		{
			submesh.AddTriangle(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos, defaultColor);
		}
		submesh.Compress();
	}

	private void ConfigureBasicLighting(Color modelColor)
	{
		float[] light_1 = new float[4]
		{
			0.2f * (float)(int)modelColor.R / 255f,
			0.2f * (float)(int)modelColor.G / 255f,
			0.2f * (float)(int)modelColor.B / 255f,
			1f
		};
		float[] light_2 = new float[4]
		{
			10f * (float)(int)modelColor.R / 255f,
			10f * (float)(int)modelColor.G / 255f,
			10f * (float)(int)modelColor.B / 255f,
			1f
		};
		float[] specref = new float[4]
		{
			0.2f * (float)(int)modelColor.R / 255f,
			0.2f * (float)(int)modelColor.G / 255f,
			0.2f * (float)(int)modelColor.B / 255f,
			1f
		};
		float[] specular_0 = new float[4] { -1f, -1f, 1f, 1f };
		float[] specular_1 = new float[4] { 1f, -1f, 1f, 1f };
		float[] lightPos_0 = new float[4] { 1000f, 1000f, -200f, 0f };
		float[] lightPos_1 = new float[4] { -1000f, 1000f, -200f, 0f };
		GL.Enable(EnableCap.Lighting);
		GL.Light(LightName.Light0, LightParameter.Ambient, light_1);
		GL.Light(LightName.Light0, LightParameter.Diffuse, light_2);
		GL.Light(LightName.Light0, LightParameter.Specular, specular_0);
		GL.Light(LightName.Light0, LightParameter.Position, lightPos_0);
		GL.Enable(EnableCap.Light0);
		GL.Light(LightName.Light1, LightParameter.Ambient, light_1);
		GL.Light(LightName.Light1, LightParameter.Diffuse, light_2);
		GL.Light(LightName.Light1, LightParameter.Specular, specular_1);
		GL.Light(LightName.Light1, LightParameter.Position, lightPos_1);
		GL.Enable(EnableCap.Light1);
		GL.Enable(EnableCap.ColorMaterial);
		GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
		GL.Material(MaterialFace.Front, MaterialParameter.Specular, specref);
		GL.Material(MaterialFace.Front, MaterialParameter.Shininess, 16);
		GL.Enable(EnableCap.Normalize);
	}

	public void Draw()
	{
		if (submesh == null)
		{
			FillMesh(-10);
		}
		Color color = Color.Crimson;
		ConfigureBasicLighting(color);
		GL.PushMatrix();
		GL.Scale(scaler, scaler, scaler);
		GL.EnableClientState(ArrayCap.VertexArray);
		GL.EnableClientState(ArrayCap.NormalArray);
		GL.VertexPointer(3, VertexPointerType.Float, 0, submesh.glVertices);
		GL.Color3((double)(int)color.R / 255.0, (double)(int)color.G / 255.0, (double)(int)color.B / 255.0);
		GL.NormalPointer(NormalPointerType.Float, 0, submesh.glNormals);
		GL.DrawArrays(PrimitiveType.Triangles, 0, submesh.glVertices.Length / 3);
		GL.DisableClientState(ArrayCap.NormalArray);
		GL.DisableClientState(ArrayCap.VertexArray);
		GL.PopMatrix();
	}

	public void DrawWithMaterials()
	{
		DrawWithMaterials_Color3();
	}

	private void DrawWithMaterials_BatuGL()
	{
		float[] ambientColor = material.AmbientColor;
		float[] diffuseColor = material.DiffuseColor;
		if (ambientColor == null)
		{
			ambientColor = new float[3] { 0.8784f, 0.7765f, 0.3412f };
			material.AmbientColor = ambientColor;
		}
		if (diffuseColor == null)
		{
			diffuseColor = ambientColor;
			material.DiffuseColor = diffuseColor;
		}
		float[] light_1 = new float[4]
		{
			0.2f * ambientColor[0],
			0.2f * ambientColor[1],
			0.2f * ambientColor[2],
			1f
		};
		float[] light_2 = new float[4]
		{
			10f * diffuseColor[0],
			10f * diffuseColor[1],
			10f * diffuseColor[2],
			1f
		};
		float[] specref = new float[4]
		{
			0.2f * ambientColor[0],
			0.2f * ambientColor[1],
			0.2f * ambientColor[2],
			1f
		};
		float[] specular_0 = new float[4] { -1f, -1f, 1f, 1f };
		float[] specular_1 = new float[4] { 1f, -1f, 1f, 1f };
		float[] lightPos_0 = new float[4] { 1000f, 1000f, -200f, 0f };
		float[] lightPos_1 = new float[4] { -1000f, 1000f, -200f, 0f };
		GL.Enable(EnableCap.Lighting);
		GL.Light(LightName.Light0, LightParameter.Ambient, light_1);
		GL.Light(LightName.Light0, LightParameter.Diffuse, light_2);
		GL.Light(LightName.Light0, LightParameter.Specular, specular_0);
		GL.Light(LightName.Light0, LightParameter.Position, lightPos_0);
		GL.Enable(EnableCap.Light0);
		GL.Light(LightName.Light1, LightParameter.Ambient, light_1);
		GL.Light(LightName.Light1, LightParameter.Diffuse, light_2);
		GL.Light(LightName.Light1, LightParameter.Specular, specular_1);
		GL.Light(LightName.Light1, LightParameter.Position, lightPos_1);
		GL.Enable(EnableCap.Light1);
		GL.Enable(EnableCap.ColorMaterial);
		GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
		GL.Material(MaterialFace.Front, MaterialParameter.Specular, specref);
		GL.Material(MaterialFace.Front, MaterialParameter.Shininess, 35);
		GL.Enable(EnableCap.Normalize);
		if (submesh == null)
		{
			FillMesh(-10);
		}
		GL.PushMatrix();
		GL.Scale(scaler, scaler, scaler);
		GL.EnableClientState(ArrayCap.VertexArray);
		GL.EnableClientState(ArrayCap.NormalArray);
		GL.VertexPointer(3, VertexPointerType.Float, 0, submesh.glVertices);
		GL.NormalPointer(NormalPointerType.Float, 0, submesh.glNormals);
		GL.DrawArrays(PrimitiveType.Triangles, 0, submesh.glVertices.Length / 3);
		GL.DisableClientState(ArrayCap.NormalArray);
		GL.DisableClientState(ArrayCap.VertexArray);
		GL.PopMatrix();
		GL.Disable(EnableCap.ColorMaterial);
		GL.DisableClientState(ArrayCap.ColorArray);
		GL.DisableClientState(ArrayCap.VertexArray);
		GL.DisableClientState(ArrayCap.NormalArray);
	}

	private void DrawWithMaterials_Color3()
	{
		if (material.AmbientColor == null)
		{
			material.AmbientColor = new float[3] { 0.8784f, 0.7765f, 0.3412f };
		}
		if (material.DiffuseColor == null)
		{
			material.DiffuseColor = material.AmbientColor;
		}
		ConfigureBasicLighting(Color.LightGray);
		GL.Color3(material.DiffuseColor);
		GL.Enable(EnableCap.Normalize);
		if (submesh == null)
		{
			FillMesh(-10);
		}
		GL.PushMatrix();
		GL.Scale(scaler, scaler, scaler);
		GL.EnableClientState(ArrayCap.VertexArray);
		GL.EnableClientState(ArrayCap.NormalArray);
		GL.VertexPointer(3, VertexPointerType.Float, 0, submesh.glVertices);
		GL.NormalPointer(NormalPointerType.Float, 0, submesh.glNormals);
		GL.DrawArrays(PrimitiveType.Triangles, 0, submesh.glVertices.Length / 3);
		GL.DisableClientState(ArrayCap.NormalArray);
		GL.DisableClientState(ArrayCap.VertexArray);
		GL.PopMatrix();
		GL.Disable(EnableCap.ColorMaterial);
		GL.DisableClientState(ArrayCap.ColorArray);
		GL.DisableClientState(ArrayCap.VertexArray);
		GL.DisableClientState(ArrayCap.NormalArray);
	}
}
