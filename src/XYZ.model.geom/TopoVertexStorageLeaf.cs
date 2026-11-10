using System.Collections.Generic;

namespace XYZ.model.geom;

public class TopoVertexStorageLeaf
{
	public RHBoundingBox box = new RHBoundingBox();

	public List<TopoVertex> vertices = new List<TopoVertex>();

	public void Add(TopoVertex vertex)
	{
		vertices.Add(vertex);
		box.Add(vertex.pos);
	}

	public int LargestDimension()
	{
		RHVector3 size = box.Size;
		if (size.x > size.y && size.x > size.z)
		{
			return 0;
		}
		if (size.y > size.z)
		{
			return 1;
		}
		return 2;
	}

	public TopoVertex SearchPoint(RHVector3 vertex)
	{
		foreach (TopoVertex v in vertices)
		{
			if (vertex.Distance(v.pos) < 0.0010000000474974513)
			{
				return v;
			}
		}
		return null;
	}
}
