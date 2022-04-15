using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : IEqualityComparer<Edge>
{
  public int i0;
  public int i1;

  public Vector3 p0;
  public Vector3 p1;

  public Edge(int Index0, int Index1)
  {
    i0 = Index0;
    i1 = Index1;
  }

  public Edge(Vector3 v0, Vector3 v1, int index0, int index1)
  {
    p0 = v0;
    p1 = v1;
    i0 = index0;
    i1 = index1;
  }

  float epsilon = 0.0001f;
  bool Approx(Vector3 v1, Vector3 v2)
  {
    return (Mathf.Abs(v1.x - v2.x) < epsilon) && (Mathf.Abs(v1.y - v2.y) < epsilon) && (Mathf.Abs(v1.z - v2.z) < epsilon);
  }

  public bool IsConnected(Edge other)
  {
    // if (other.i0 == i0 || other.i1 == i0 || other.i0 == i1 || other.i1 == i1)
    // {
    //   return true;
    // }

    if (Approx(other.p0, p0) || Approx(other.p0, p1) || Approx(other.p1, p0) || Approx(other.p1, p1))
    {
      return true;
    }
    return false;
  }

  // because of floating point errors.
  // and indicies not actually being shared in the nav mesh.
  public override int GetHashCode()
  {
    // return 0;
    Vector3Int p0i = new Vector3Int(Mathf.RoundToInt(p0.x), Mathf.RoundToInt(p0.y), Mathf.RoundToInt(p0.z));
    Vector3Int p1i = new Vector3Int(Mathf.RoundToInt(p1.x), Mathf.RoundToInt(p1.y), Mathf.RoundToInt(p1.z));
    return (p0i + p1i).GetHashCode();
  }
  public override bool Equals(object obj)
  {
    Edge e = (Edge)obj;
    if ((e.i0 == i0 && e.i1 == i1) || (e.i1 == i0 && e.i0 == i1))
    {
      return true;
    }
    return (Approx(e.p0, p0) && Approx(e.p1, p1)) || (Approx(e.p1, p0) && Approx(e.p0, p1));
    // return (e.p0 == p0 && e.p1 == p1) || (e.p0 == p1 && e.p1 == p0);
  }

  public bool Equals(Edge x, Edge y)
  {
    return x.Equals(y);
  }

  public int GetHashCode(Edge obj)
  {
    return obj.GetHashCode();
  }

  public override string ToString()
  {
    return $"{p0},{p1}";
    // return base.ToString();
  }
}