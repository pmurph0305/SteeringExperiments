using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using Vector2 = UnityEngine.Vector2;
[System.Serializable]
public class RVO2AgentParams
{
  public bool UseCustomAgentParams;
  public float NeighbourDistance;
  public int MaxNeighbours;
  public float TimeHorizon;
  public float TimeHorizonObst;
  public float Radius;
  public float MaxSpeed;
  public int AgentId;
  public Vector2 Velocity;
  public RVO2AgentParams()
  {
    NeighbourDistance = 15f;
    MaxNeighbours = 10;
    TimeHorizon = 5f;
    TimeHorizonObst = 1.5f;
    Radius = 1f;
    MaxSpeed = 2f;
  }

  public int AddAgent(Vector2 position)
  {
    if (UseCustomAgentParams)
    {
      AgentId = Simulator.Instance.addAgent(position, NeighbourDistance, MaxNeighbours, TimeHorizon, TimeHorizonObst, Radius, MaxSpeed, Velocity);
    }
    else
    {
      AgentId = Simulator.Instance.addAgent(position);
    }

    return AgentId;
  }

  public void DebugAgent()
  {
    float maxSpeed = Simulator.Instance.getAgentMaxSpeed(AgentId);
    float neighbourDistance = Simulator.Instance.getAgentNeighborDist(AgentId);
    int maxNeighbors = Simulator.Instance.getAgentMaxNeighbors(AgentId);

    float radius = Simulator.Instance.getAgentRadius(AgentId);
    float timeHorizon = Simulator.Instance.getAgentTimeHorizon(AgentId);
    float timeHorizonObst = Simulator.Instance.getAgentTimeHorizonObst(AgentId);

    Vector2 velocity = Simulator.Instance.getAgentVelocity(AgentId);
    Vector2 prefVelocity = Simulator.Instance.getAgentPrefVelocity(AgentId);
    Vector2 position = Simulator.Instance.getAgentPosition(AgentId);


    Debug.Log($"AgentID:{AgentId} \nVelocity:{velocity} Pref Velocity:{prefVelocity} position:{position}");
  }
}
