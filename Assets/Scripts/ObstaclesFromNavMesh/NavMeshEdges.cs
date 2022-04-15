using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshEdges
{
  public HashSet<Edge> UniqueEdges = new HashSet<Edge>();
  public HashSet<Edge> SharedEdges = new HashSet<Edge>();

  public NavMeshEdges(HashSet<Edge> unique, HashSet<Edge> common)
  {
    UniqueEdges = unique;
    SharedEdges = common;
  }
}