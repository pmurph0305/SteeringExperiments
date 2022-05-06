using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SteeredAgent : MonoBehaviour
{
  public SteerData data;

  [Header("General")]


  [SerializeField] float Mass = 1;
  [SerializeField] float MaxSpeed = 5f;
  [SerializeField] bool KeepWithinBounds = true;
  Vector3 seek;
  Vector3 desiredSeek;
  Vector3 flee;
  Vector3 desiredFlee;
  Vector3 velocity;
  Vector3 steering;
  Vector3 arrival;
  Vector3 desiredArrival;
  Vector3 avoidance;
  Vector3 desiredAvoidance;
  Vector3 accel;

  Bounds levelBounds;
  public void SetBounds(Bounds b)
  {
    levelBounds = b;
  }
  IGoalProvider goalProvider;
  public void SetGoal(IGoalProvider provider)
  {
    goalProvider = provider;
  }

  void Start()
  {
    steeringEnabled = true;
    goalProvider = GoalProvider.Instance;
  }



  bool steeringEnabled;
  public void EnableSteering(bool enabled)
  {
    steeringEnabled = enabled;
    if (!enabled)
    {
      velocity = Vector3.zero;
    }
  }

  public Vector3 GetVelocity()
  {
    return velocity;
  }
  public float GetPercentMaxVelocity()
  {
    return velocity.magnitude / MaxSpeed;
  }




  private void Update()
  {
    if (goalProvider == null) return;
    if (!steeringEnabled)
    {
      return;
    }
    float d = Vector3.Distance(transform.position, goalProvider.GetPosition());

    // each checks the steering flags to see if it should be used.
    // get the seek force towards the goal
    if (data.Should(SteeringOperations.Seek))
    {
      if (data.PursuitSeek)
      {
        seek = Pursuit(goalProvider.GetPosition(), goalProvider.GetVelocity(), data.MaxSeekForce);
      }
      else
      {
        seek = Seek(velocity, goalProvider.GetPosition(), data.MaxSeekForce);
      }
    }
    else
    {
      seek = Vector3.zero;
    }

    // get the arrival force (Which helps prevent over-shooting)
    arrival = data.Should(SteeringOperations.Arrival) ? Arrival(velocity, transform.position, goalProvider.GetPosition(), 3f, data.MaxArrivalForce) : Vector3.zero;

    avoidance = data.Should(SteeringOperations.Avoidance) ? AvoidBarriers(velocity, transform.position, data.MaxAvoidanceForce) : Vector3.zero;
    // works but not needed.
    flee = data.Should(SteeringOperations.Flee) ? Flee(velocity, goalProvider.GetPosition(), data.MaxFleeForce) : Vector3.zero;

    wander = data.Should(SteeringOperations.Wander) ? Wander(transform.position, velocity) : Vector3.zero;
    charge = data.Should(SteeringOperations.Charge) ? Charge(transform.position, goalProvider.GetPosition(), velocity, data.MaxChargeForce) : Vector3.zero;
    // add up all the steering forces to get the total force contribution.
    steering = arrival + seek + flee + avoidance + wander + charge;
    // calculate the accel for the forces.
    accel = steering / Mass;

    // actual velocity change using acceleration.
    velocity = Vector3.ClampMagnitude(velocity + accel * Time.deltaTime, MaxSpeed);

    if (KeepWithinBounds)
    {
      Vector3 expectedPosition = transform.position + velocity * Time.deltaTime;
      if (!levelBounds.Contains(expectedPosition))
      {
        expectedPosition = levelBounds.ClosestPoint(expectedPosition);
      }
      transform.position = expectedPosition;
    }
    else
    {
      transform.position += velocity * Time.deltaTime;
    }

  }


  Vector3 charge;
  Vector3 Charge(Vector3 position, Vector3 target, Vector3 velocity, float maxForce)
  {

    Vector3 dir = (target - position).normalized;
    float dotCharge = Vector3.Dot(dir, velocity.normalized);
    if (dotCharge > data.MinDotProduct)
    {
      return dir * dotCharge * maxForce;
    }
    return Vector3.zero;
  }

  Vector3 wander;
  float wanderAngle = 90f;
  float angleChange = 0.1f;
  Vector3 Wander(Vector3 position, Vector3 velocity)
  {
    Vector3 center = velocity.normalized;
    center *= data.WanderCircleDistance;
    wander = -Vector3.right * data.WanderCircleRadius;
    wander.x = Mathf.Cos(wanderAngle) * data.WanderCircleRadius;
    wander.z = Mathf.Sin(wanderAngle) * data.WanderCircleRadius;
    wanderAngle += Random.Range(0f, 1f) * angleChange - angleChange * .5f;
    return center + wander;
  }

  RaycastHit hit;
  Vector3 AvoidBarriers(Vector3 currentVelocity, Vector3 position, float maxForce)
  {
    if (Physics.CapsuleCast(transform.position - data.castOffset, transform.position + data.castOffset, data.sphereRadius, currentVelocity, out hit, data.avoidDistance, data.collisionMask))
    {
      float d = hit.distance;
      float percentMax = 1 - (d / data.avoidDistance);
      desiredAvoidance = hit.normal * percentMax * maxForce;
      avoidance = (desiredAvoidance - currentVelocity);
      return desiredAvoidance;
    }
    // if (Physics.SphereCast(position + sphereCastOffset, sphereRadius, currentVelocity, out hit, avoidDistance, collisionMask))
    // {
    //   float d = hit.distance;
    //   float percentMax = 1 - (d / avoidDistance);
    //   desiredAvoidance = hit.normal * percentMax * maxForce;
    //   avoidance = (desiredAvoidance - currentVelocity);
    //   return avoidance;
    // }
    desiredAvoidance = Vector3.zero;
    return Vector3.zero;
  }

  Vector3 Flee(Vector3 currentVelocity, Vector3 goal, float maxForce)
  {
    desiredFlee = (transform.position - goal).normalized * maxForce;
    // desired steering delta
    Vector3 fleeSteering = (desiredFlee - currentVelocity);
    fleeSteering = Vector3.ClampMagnitude(fleeSteering, maxForce);
    return fleeSteering;
  }

  Vector3 Arrival(Vector3 currentVelocity, Vector3 position, Vector3 goal, float radius, float maxForce)
  {
    Vector3 targetOffset = goal - position;
    float distance = targetOffset.magnitude;
    //kinematic eqns
    if (distance < radius)
    {
      float vel = currentVelocity.magnitude;
      arrival = currentVelocity.normalized * -vel / (2 * distance);
    }
    else
    {
      arrival = Vector3.zero;
    }
    // return arrival;
    // if (distance < radius)
    // {
    //   float rampdSpeed = maxSpeed * 2 * distance / radius;
    //   desiredArrival = (rampdSpeed / distance) * targetOffset;
    //   arrival = desiredArrival - currentVelocity;
    //   arrival = Vector3.ClampMagnitude(arrival, maxForce);
    // }
    // else
    // {
    //   arrival = Vector3.zero;
    // }
    return arrival;
  }

  Vector3 Seek(Vector3 currentVelocity, Vector3 goal, float maxForce)
  {
    // from gdc the next vector, fixing seek to be a force.
    //     desiredSeek = goal - transform.position;
    // desiredSeek *= MaxSpeed / desiredSeek.magnitude;

    // // desired steering delta
    // Vector3 seeksteering = (desiredSeek - currentVelocity);
    // seeksteering *= maxForce / MaxSpeed;
    // return seeksteering;

    // a velocity.
    desiredSeek = Vector3.Normalize(goal - transform.position) * MaxSpeed;
    // desired steering
    Vector3 seeksteering = (desiredSeek - currentVelocity); // a velocity.
    seeksteering *= (maxForce / MaxSpeed); // as a force.
    return seeksteering;
  }

  Vector3 estimatedPursuitPosition;
  Vector3 Pursuit(Vector3 targetPosition, Vector3 targetVelocity, float maxForce)
  {
    float d = Vector3.Distance(targetPosition, transform.position);
    float PursuitMultiplier = d / MaxSpeed;
    estimatedPursuitPosition = targetPosition + targetVelocity * PursuitMultiplier * data.VelOffsetMultiplier;
    return Seek(velocity, estimatedPursuitPosition, maxForce);
  }

  private void OnDrawGizmos()
  {
    if (goalProvider == null) return;

    if (seek != Vector3.zero)
    {
      Gizmos.color = Color.gray;
      Gizmos.DrawLine(transform.position, transform.position + desiredSeek);
      Gizmos.color = Color.blue;
      Gizmos.DrawLine(transform.position + velocity, transform.position + velocity + seek);
      if (data.PursuitSeek)
      {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(estimatedPursuitPosition, 0.25f);
      }
    }

    if (flee != Vector3.zero)
    {
      Gizmos.color = Color.gray;
      Gizmos.DrawLine(transform.position, transform.position + desiredFlee);
      Gizmos.color = Color.red;
      Gizmos.DrawLine(transform.position + velocity, transform.position + velocity + flee);
    }



    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + velocity);

    if (arrival != Vector3.zero)
    {
      Gizmos.color = Color.gray;
      Gizmos.DrawLine(transform.position, transform.position + desiredArrival);
      Gizmos.color = Color.black;
      Gizmos.DrawLine(transform.position, transform.position + arrival);
    }



    Gizmos.color = Color.white;
    Gizmos.DrawSphere(goalProvider.GetPosition(), 0.1f);

    if (avoidance != Vector3.zero)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawLine(transform.position, transform.position + avoidance);
      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(transform.position, transform.position + desiredAvoidance);
    }


    if (wander != Vector3.zero)
    {
      Gizmos.color = Color.white;
      Gizmos.DrawLine(transform.position, transform.position + wander);
    }

    if (charge != Vector3.zero)
    {
      Gizmos.color = Color.cyan;
      Gizmos.DrawLine(transform.position, transform.position + charge);
    }


  }
}
