using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
  Renderer rend = null;
  Collider TriggerCollider = null;
  Rigidbody rbody;
  public Vector3 Velocity { get { return rbody.velocity; } }

  void Awake()
  {
    rbody = GetComponent<Rigidbody>();

    if (rbody == null)
      Debug.Log("no rigidbody");

    if (rend == null)
      Debug.LogError("no renderer");

    // if (TeamAMaterial == null)
    //     Debug.LogError("No TeamAMaterial");
    // if (TeamBMaterial == null)
    //     Debug.LogError("No TeamBMaterial");
    // if (NeutralMaterial == null)
    //     Debug.LogError("No Neutral Material");

    if (TriggerCollider == null)
      Debug.LogError("No Trigger Collider");
  }
  // Start is called before the first frame update
  void Start()
  {
    IsHeld = false;
  }

  // Update is called once per frame
  void Update()
  {

  }

  private bool isHeld = false;

  public bool IsHeld
  {
    get { return isHeld; }
    set
    {
      isHeld = value;

      if (isHeld)
      {
        //get rid of dumb warning. will switch back to continuous at throw time
        rbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rbody.isKinematic = true;
      }
      else
      {
        transform.parent = null;
        rbody.isKinematic = false;
        rbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
      }
    }
  }

  public void INTERNAL_Throw(Vector3 vel)
  {
    if (IsHeld)
    {
      IsHeld = false;
      transform.parent = null; // TODO set to group, notify manager?
      rbody.isKinematic = false;
      rbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
      rbody.velocity = Vector3.zero;
      rbody.angularVelocity = Vector3.zero;

      rbody.AddForce(vel, ForceMode.VelocityChange);
    }
  }

}
