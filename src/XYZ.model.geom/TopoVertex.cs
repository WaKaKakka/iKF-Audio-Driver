namespace XYZ.model.geom;

public class TopoVertex
{
	public RHVector3 pos;

	public int id;

	public TopoVertex(int _id, RHVector3 _pos)
	{
		id = _id;
		pos = _pos;
	}

	public double distance(TopoVertex vertex)
	{
		return pos.Distance(vertex.pos);
	}

	public double distance(RHVector3 vertex)
	{
		return pos.Distance(vertex);
	}
}
