using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugEnemyComp : MonoBehaviour
{
  public float m_maxUpSpeed;
  public float m_moveForce;

  private Rigidbody m_rigid;
  private float m_initalXPos;
  private int m_nextDir;

  void Start()
  {
    m_rigid = GetComponentInChildren<Rigidbody>();

    m_nextDir = 1;
    m_initalXPos = transform.position.x;
  }

  private void FixedUpdate()
  {
    float xDelta = m_initalXPos - transform.position.x;


    Vector3 force = (m_moveForce * transform.up) + new Vector3(xDelta, 0.0f, 0.0f);
    m_rigid.AddForce(force);

    /*m_rigid.velocity = new Vector3(Mathf.Clamp(m_rigid.velocity.x, 0.0f, m_maxUpSpeed)
                                  , Mathf.Clamp(m_rigid.velocity.y, 0.0f, m_maxUpSpeed)
                                  , Mathf.Clamp(m_rigid.velocity.z, 0.0f, m_maxUpSpeed));
  */
  }

  private void OnTriggerExit(Collider other)
  {
    if (other.gameObject.layer == 8)
    {
      m_nextDir *= -1;
    }
  }
  private void OnTriggerStay(Collider other)
  {
    if(other.gameObject.layer == 8)
    {
      m_rigid.AddForce(3f * m_nextDir , 0.0f, 0.0f);
    }
  }
}
