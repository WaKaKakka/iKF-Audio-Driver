namespace XYZ.model.geom;

public class Material
{
	public float[] AmbientColor { get; set; }

	public float[] DiffuseColor { get; set; }

	public float[] SpecularColor { get; set; }

	public float Shininess { get; set; }

	public string DiffuseTextureMap { get; set; }
}
