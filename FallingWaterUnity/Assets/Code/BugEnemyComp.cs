using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugEnemyComp : MonoBehaviour
{
  public float m_maxVelocity;
  public float m_moveForce;
  public int m_health;
  public int m_nextDir = 1;

  public Transform m_meshTrans;

  private Rigidbody m_rigid;
  private float m_initalXPos;

  void Start()
  {
    m_rigid = GetComponentInChildren<Rigidbody>();

    m_initalXPos = transform.position.x;
  }

  private void FixedUpdate()
  {
    Vector3 force = (m_moveForce * transform.up) ;

    float xDelta = m_initalXPos - transform.position.x;
    if (Mathf.Abs(xDelta) > 0.1f)
    {
      force += transform.right * xDelta * 1.25f;
    }
    force += transform.forward * .5f; //magnetize bug to ground kinda

    m_rigid.AddForce(force);
    Debug.DrawRay(transform.position, force, Color.green);

    if(m_rigid.velocity.magnitude > m_maxVelocity)
    {
      m_rigid.velocity = m_rigid.velocity.normalized * m_maxVelocity;
    }
    /*m_rigid.velocity = new Vector3(Mathf.Clamp(m_rigid.velocity.x, 0.0f, m_maxUpSpeed)
                                  , Mathf.Clamp(m_rigid.velocity.y, 0.0f, m_maxUpSpeed)
                                  , Mathf.Clamp(m_rigid.velocity.z, 0.0f, m_maxUpSpeed));
  */
  }

  private void OnParticleCollision(GameObject other)
  {
    if(other.layer == 4)//water layer
    {
      m_health--;
      if(m_health <= 0)
      {
        Destroy(gameObject);
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.layer == 11) //nub trigger
    {
      m_meshTrans.localPosition = new Vector3(0.0f, 0.0f, -.25f);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if (other.gameObject.layer == 8)
    {
      m_nextDir *= -1;
    }
    else if (other.gameObject.layer == 11) //nub trigger
    {
      m_meshTrans.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
    }
  }
  private void OnTriggerStay(Collider other)
  {
    if(other.gameObject.layer == 8) //turnSignal layer
    {
      Vector3 force = (transform.right * (4f * m_nextDir)) + (transform.up * (-.0f));
      m_rigid.AddForce(force);
    }
  }
}
