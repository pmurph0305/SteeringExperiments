using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;
public class EdgeFinder : MonoBehaviour
{
  NavMeshHit hit;

  private void OnDrawGizmosSelected()
  {
    NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas);
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(hit.position, 1f);
    Gizmos.color = Color.red;
    Gizmos.DrawLine(hit.position, hit.position + hit.normal * hit.distance);
    DrawEdges();
  }

  public NavMeshEdges GetEdges()
  {
    NavMeshTriangulation nmt = NavMesh.CalculateTriangulation();
    HashSet<Edge> allEdges = new HashSet<Edge>();
    HashSet<Edge> duplicatedEdges = new HashSet<Edge>();
    for (int i = 0; i < nmt.indices.Length; i += 3)
    {
      int i0 = nmt.indices[i];
      int i1 = nmt.indices[i + 1];
      int i2 = nmt.indices[i + 2];
      Vector3 v0 = nmt.vertices[i0];
      Vector3 v1 = nmt.vertices[i1];
      Vector3 v2 = nmt.vertices[i2];
      Edge e0 = new Edge(v0, v1, i0, i1);
      Edge e1 = new Edge(v1, v2, i1, i2);
      Edge e2 = new Edge(v2, v0, i2, i0);

      if (!allEdges.Add(e0))
      {
        duplicatedEdges.Add(e0);
      }
      if (!allEdges.Add(e1))
      {
        duplicatedEdges.Add(e1);
      }
      if (!allEdges.Add(e2))
      {
        duplicatedEdges.Add(e2);
      }
    }
    // remove duplicated edges to make them the unique edges.
    allEdges.ExceptWith(duplicatedEdges);
    NavMeshEdges edges = new NavMeshEdges(allEdges, duplicatedEdges);
    return edges;
  }

  float epsilon = 0.0001f;
  bool Approx(Vector3 v1, Vector3 v2)
  {

    return (Mathf.Abs(v1.x - v2.x) < epsilon) && (Mathf.Abs(v1.y - v2.y) < epsilon) && (Mathf.Abs(v1.z - v2.z) < epsilon);
  }

  public List<List<RVO.Vector2>> ToRVOObstacles(List<List<Vector3>> v3Lists)
  {
    List<List<RVO.Vector2>> rvoLists = new List<List<RVO.Vector2>>();

    foreach (var list in v3Lists)
    {
      List<RVO.Vector2> rvoList = new List<RVO.Vector2>();
      foreach (Vector3 v in list)
      {
        rvoList.Add(new RVO.Vector2(v.x, v.z));
      }
      rvoLists.Add(rvoList);
    }
    return rvoLists;
  }
  public bool isLeft(Vector3 a, Vector3 b, Vector3 c)
  {
    return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) > 0;
  }

  public List<List<Vector3>> ToPolygons(NavMeshEdges navEdges)
  {
    List<List<Vector3>> VerticesLists = new List<List<Vector3>>();
    HashSet<Edge> uniqueEdges = new HashSet<Edge>(navEdges.UniqueEdges);
    HashSet<Edge> usedEdges = new HashSet<Edge>();
    foreach (var edge in uniqueEdges)
    {

      if (usedEdges.Contains(edge)) continue;
      usedEdges.Add(edge);
      List<Edge> connectedEdges = new List<Edge>();
      List<Vector3> VerticesList = new List<Vector3>();
      VerticesList.Add(edge.p0);
      VerticesList.Add(edge.p1);
      Vector3 vert = edge.p1;

      bool foundConnected = true;
      // essentially just while we continue to find pairing vertices, keep pairing them
      // by adding the vertex that isn't the same and use that as the comparison until no more are found.
      while (foundConnected)
      {
        foundConnected = false;
        foreach (var otherEdge in uniqueEdges)
        {
          if (usedEdges.Contains(otherEdge)) continue;
          if (Approx(otherEdge.p0, vert))
          {
            VerticesList.Add(otherEdge.p1);
            vert = otherEdge.p1;
            usedEdges.Add(otherEdge);
            foundConnected = true;
          }
          else if (Approx(otherEdge.p1, vert))
          {
            VerticesList.Add(otherEdge.p0);
            vert = otherEdge.p0;
            usedEdges.Add(otherEdge);
            foundConnected = true;
          }
        }
      }
      VerticesList.RemoveAt(VerticesList.Count - 1);
      VerticesLists.Add(VerticesList);
    }

    //CCW polygons.
    List<List<Vector3>> ccwVertices = new List<List<Vector3>>();
    foreach (var l in VerticesLists)
    {
      List<Vector3> ccwVerts = new List<Vector3>();
      int startIndex = 0;
      Vector3 c = l[0];
      for (int i = 1; i < l.Count; i++)
      {
        Vector3 n = l[i];
        if (n.z >= c.z)
        {
          if (n.x >= c.x)
          {
            c = n;
            startIndex = i;
          }
        }
      }
      int prev = startIndex - 1;
      int next = startIndex + 1;
      prev = prev < 0 ? l.Count - 1 : prev;
      next = next >= l.Count ? 0 : next;
      int start = next;
      if (isLeft(l[next], l[startIndex], l[prev]))
      {
        l.Reverse();
      }
      ccwVertices.Add(l);
    }

    int listToReverse = 0;
    // find bounding box set and reverse it so it's an inverted obstacle.
    foreach (var l in ccwVertices)
    {
      Bounds b = new Bounds(l[0], Vector3.zero);
      bool containsAll = true;
      foreach (var v in l)
      {
        b.Encapsulate(v);
      }

      foreach (var ol in ccwVertices)
      {

        foreach (var ov in ol)
        {
          if (!b.Contains(ov))
          {
            containsAll = false;
            break;
          }
        }
        if (!containsAll)
        {
          break;
        }
      }

      if (containsAll)
      {
        listToReverse = ccwVertices.IndexOf(l);
        break;
      }
    }
    // Debug.Log("List to reverse:" + listToReverse);
    if (listToReverse < ccwVertices.Count)
    {
      ccwVertices[listToReverse].Reverse();
    }
    return ccwVertices;
  }


  void DrawEdges()
  {
    NavMeshTriangulation nmt = NavMesh.CalculateTriangulation();
    // Dictionary<int, int> counts = new Dictionary<int, int>();
    // for (int i = 0; i < nmt.indices.Length; i++)
    // {
    //   if (counts.ContainsKey(nmt.indices[i]))
    //   {
    //     counts[nmt.indices[i]] += 1;
    //   }
    //   else
    //   {
    //     counts.Add(nmt.indices[i], 1);
    //   }
    // }
    // int sharedIndices = 0;
    // foreach (var kvp in counts)
    // {
    //   if (kvp.Value > 1)
    //   {
    //     sharedIndices++;
    //   }
    // }
    // Debug.Log("Shared indices:" + sharedIndices);

    NavMeshEdges edges = GetEdges();
    List<List<Vector3>> lists = ToPolygons(edges);
    // List<List<Edge>> lists = ToPolygons(edges);
    Debug.Log("List count:" + lists.Count);
    Gizmos.color = Color.yellow;
    Color colorStart = Color.green;
    Color colorEnd = Color.red;
    Vector3 upset = Vector3.up;
    foreach (var l in lists)
    {

      for (int i = 0; i < l.Count; i++)
      {
        Gizmos.color = Color.Lerp(colorStart, colorEnd, (float)i / (float)l.Count);
        Gizmos.DrawLine(l[i] + upset, l[i + 1 >= l.Count ? 0 : i + 1] + upset);
      }

    }
    Debug.Log("Unique:" + edges.UniqueEdges.Count + " Shared:" + edges.SharedEdges.Count);

    Gizmos.color = Color.magenta;
    foreach (var e in edges.UniqueEdges)
    {
      Gizmos.DrawLine(e.p0, e.p1);
    }
    Gizmos.color = Color.cyan;

    Vector3 offset = Vector3.up * 0.1f;
    foreach (var e in edges.SharedEdges)
    {
      Gizmos.DrawLine(e.p0 + offset, e.p1 + offset);
    }
  }
}
