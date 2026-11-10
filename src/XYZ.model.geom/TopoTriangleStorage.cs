using System.Collections;
using System.Collections.Generic;

namespace XYZ.model.geom;

public class TopoTriangleStorage
{
	private Dictionary<TopoTriangle, int> _idDict = new Dictionary<TopoTriangle, int>();

	private List<TopoTriangle> _triangles = new List<TopoTriangle>();

	public List<TopoTriangle> triangles => _triangles;

	public int Count => _triangles.Count;

	public void Add(TopoTriangle triangle)
	{
		if (!_idDict.TryGetValue(triangle, out var id))
		{
			id = _triangles.Count;
			_idDict.Add(triangle, id);
			_triangles.Add(triangle);
		}
	}

	public bool Remove(TopoTriangle triangle)
	{
		if (_idDict.TryGetValue(triangle, out var id))
		{
			_idDict.Remove(triangle);
			_triangles.RemoveAt(id);
			return true;
		}
		return false;
	}

	public IEnumerator GetEnumerator()
	{
		foreach (TopoTriangle triangle in triangles)
		{
			yield return triangle;
		}
	}

	public void Clear()
	{
		_triangles.Clear();
		_idDict.Clear();
	}

	public bool Contains(TopoTriangle test)
	{
		int id;
		return _idDict.TryGetValue(test, out id);
	}
}
