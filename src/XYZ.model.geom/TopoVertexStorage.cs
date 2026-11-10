using System;
using System.Collections;
using System.Collections.Generic;

namespace XYZ.model.geom;

public class TopoVertexStorage
{
	public const int maxVerticesPerNode = 50;

	private TopoVertexStorage left;

	private TopoVertexStorage right;

	private TopoVertexStorageLeaf leaf;

	public List<TopoVertex> v = new List<TopoVertex>();

	private Hashtable hash = new Hashtable();

	private int splitDimension = -1;

	private int count;

	private double splitPosition;

	public bool IsLeaf => leaf != null;

	public int Count => count;

	public void Clear()
	{
		left = (right = null);
		leaf = null;
		count = 0;
	}

	public void ChangeCoordinates(TopoVertex vertex, RHVector3 newPos)
	{
		Remove(vertex);
		vertex.pos = new RHVector3(newPos);
		Add(vertex);
	}

	public void Add(TopoVertex vertex)
	{
		Add(vertex, 0);
	}

	public void Add(TopoVertex vertex, int level)
	{
		long temp = Convert.ToInt64(Math.Floor(vertex.pos.x * 100000.0)) * 5915587277L + Convert.ToInt64(Math.Floor(vertex.pos.y * 100000.0)) * 1500450271 + Convert.ToInt64(Math.Floor(vertex.pos.z * 100000.0)) * 3267000013u;
		if (hash[temp] == null)
		{
			hash.Add(temp, count);
			v.Add(vertex);
			count++;
		}
	}

	public TopoVertex SearchPoint(RHVector3 vertex)
	{
		long temp = Convert.ToInt64(Math.Floor(vertex.x * 100000.0)) * 5915587277L + Convert.ToInt64(Math.Floor(vertex.y * 100000.0)) * 1500450271 + Convert.ToInt64(Math.Floor(vertex.z * 100000.0)) * 3267000013u;
		if (hash[temp] != null)
		{
			return v[Convert.ToInt32(hash[temp])];
		}
		return null;
	}

	public void Remove(TopoVertex vertex)
	{
		if ((leaf != null || left != null) && RemoveTraverse(vertex))
		{
			count--;
		}
	}

	private bool RemoveTraverse(TopoVertex vertex)
	{
		if (IsLeaf)
		{
			if (leaf.vertices.Remove(vertex))
			{
				return true;
			}
			return false;
		}
		if (vertex.pos[splitDimension] < splitPosition)
		{
			return left.RemoveTraverse(vertex);
		}
		return right.RemoveTraverse(vertex);
	}

	public IEnumerator GetEnumerator()
	{
		if (left != null)
		{
			foreach (TopoVertex item in left)
			{
				yield return item;
			}
			foreach (TopoVertex item2 in right)
			{
				yield return item2;
			}
		}
		if (leaf == null)
		{
			yield break;
		}
		foreach (TopoVertex vertex in leaf.vertices)
		{
			yield return vertex;
		}
	}

	public HashSet<TopoVertex> SearchBox(RHBoundingBox box)
	{
		HashSet<TopoVertex> set = new HashSet<TopoVertex>();
		if (leaf != null || left != null)
		{
			SearchBoxTraverse(box, set);
		}
		return set;
	}

	private void SearchBoxTraverse(RHBoundingBox box, HashSet<TopoVertex> set)
	{
		if (!IsLeaf)
		{
			return;
		}
		foreach (TopoVertex v in leaf.vertices)
		{
			if (box.ContainsPoint(v.pos))
			{
				set.Add(v);
			}
		}
	}
}
