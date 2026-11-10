using System;
using System.Collections.Generic;

namespace XYZ.model.geom;

public class TopoTriangleDistance : IComparer<TopoTriangleDistance>, IComparable<TopoTriangleDistance>
{
	public double distance;

	public TopoTriangle triangle;

	public TopoTriangleDistance(double dist, TopoTriangle tri)
	{
		triangle = tri;
		distance = dist;
	}

	public int Compare(TopoTriangleDistance td1, TopoTriangleDistance td2)
	{
		return -td1.distance.CompareTo(td2.distance);
	}

	public int CompareTo(TopoTriangleDistance td)
	{
		return -distance.CompareTo(td.distance);
	}
}
