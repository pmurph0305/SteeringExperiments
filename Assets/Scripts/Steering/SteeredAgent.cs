using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeredAgent : MonoBehaviour
{
  public Transform goal;
  public float maxSpeed = 5f;
  public float maxForce = 3f;
  public Vector3 seek;
  public Vector3 desiredSeek;
  public Vector3 flee;
  public Vector3 desiredFlee;
  public Vector3 velocity;
  public Vector3 steering;

  public Vector3 arrival;
  public Vector3 desiredArrival;
  float mass = 1;
  float t;

  Vector3 accel;
  private void Update()
  {


    // desired steering delta
    // seek = Seek(velocity, goal.position, maxForce, maxSpeed);
    arrival = Arrival(velocity, transform.position, goal.position, 3f, maxSpeed);
    // flee = Flee(velocity, goal.position, maxForce, maxSpeed);
    //forces are accelerations * time.delta time to a velocity.
    // steering = (seek + arrival + flee) * Time.deltaTime;
    steering = arrival + seek + flee;
    accel = steering / mass;



    // actual velocity change
    velocity = Vector3.ClampMagnitude(velocity + accel * Time.deltaTime, maxSpeed);
    t = Time.deltaTime;
    transform.position += velocity * Time.deltaTime;
  }

  Vector3 Flee(Vector3 currentVelocity, Vector3 goal, float maxFleeForce, float maxVelocity)
  {
    desiredFlee = Vector3.Normalize(transform.position - goal) * maxVelocity;
    // desired steering delta
    Vector3 fleeSteering = (desiredFlee - currentVelocity);
    fleeSteering = Vector3.ClampMagnitude(fleeSteering, maxFleeForce);
    return fleeSteering;
  }

  Vector3 Arrival(Vector3 currentVelocity, Vector3 position, Vector3 goal, float radius, float maxSpeed)
  {
    Vector3 targetOffset = goal - position;
    float distance = targetOffset.magnitude;
    float rampdSpeed = maxSpeed * distance / radius;
    float clippedSpeed = Mathf.Min(rampdSpeed, maxSpeed);
    desiredArrival = (clippedSpeed / distance) * targetOffset;
    arrival = desiredArrival - currentVelocity;
    arrival = Vector3.ClampMagnitude(arrival, maxForce);
    return arrival;
  }

  Vector3 Seek(Vector3 currentVelocity, Vector3 goal, float maxSeekForce, float maxVelocity)
  {
    desiredSeek = Vector3.Normalize(goal - transform.position) * maxVelocity;
    // desired steering delta
    Vector3 seeksteering = (desiredSeek - currentVelocity);
    seeksteering = Vector3.ClampMagnitude(seeksteering, maxSeekForce);
    return seeksteering;
  }

  private void OnDrawGizmos()
  {

    Gizmos.color = Color.gray;
    Gizmos.DrawLine(transform.position, transform.position + desiredSeek);
    Gizmos.color = Color.blue;
    Gizmos.DrawLine(transform.position + velocity, transform.position + velocity + seek);


    Gizmos.color = Color.gray;
    Gizmos.DrawLine(transform.position, transform.position + desiredFlee);
    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position + velocity, transform.position + velocity + flee);


    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + velocity);

    Gizmos.color = Color.gray;
    Gizmos.DrawLine(transform.position, transform.position + desiredArrival);
    Gizmos.color = Color.black;
    Gizmos.DrawLine(transform.position, transform.position + arrival);


    Gizmos.color = Color.white;
    Gizmos.DrawSphere(goal.position, 0.1f);
  }
}
