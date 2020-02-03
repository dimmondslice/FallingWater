﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugEnemyComp : MonoBehaviour
{
  public static int s_totalEnemiesAlive = 0;

  public float m_maxVelocity;
  public float m_moveForce;
  public int m_health;
  public int m_nextDir = 1;
  public float m_legStrideLength;
  public int m_legspeed;
  public int m_numFlashFrames;

  public Transform m_meshTrans;

  public Transform[] m_legs;
  private Quaternion[] m_initalLocalLegRot;
  private Vector3[] m_initalLocalLegPos;

  private Rigidbody m_rigid;
  private float m_initalXPos;
  private int m_maxHealth;

  private bool m_bDamageFlashCorRunning = false;

  //---------------------------------------------------------------------------------------------------------------------
  public void Kill()
  {
    s_totalEnemiesAlive--;
    Destroy(gameObject);
  }

  //---------------------------------------------------------------------------------------------------------------------
  void Start()
  {
    m_rigid = GetComponentInChildren<Rigidbody>();

    m_initalXPos = transform.position.x;

    m_maxHealth = m_health;

    m_initalLocalLegRot = new Quaternion[m_legs.Length];
    m_initalLocalLegPos = new Vector3[m_legs.Length];
    for(int i = 0; i < m_legs.Length; ++i)
    {
      m_initalLocalLegRot[i] = m_legs[i].localRotation;
      m_initalLocalLegPos[i] = m_legs[i].localPosition;
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
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

    //leg visual update
    for (int i = 0; i < m_legs.Length; ++i)
    {
      int upOrDown = (int)Mathf.Sign((i % 3) - 2 + (i % 3)); 
      m_legs[i].localPosition = m_initalLocalLegPos[i] + Vector3.forward * (Mathf.Sin(Time.fixedTime * m_legspeed) * upOrDown * m_legStrideLength);
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  private void OnParticleCollision(GameObject other)
  {
    if(other.layer == 4)//water layer
    {
      m_health--;

      if(m_health <= 0)
      {
        Kill();
        return;
      }

      //knock off his brokded legs
      int index = Mathf.CeilToInt(((float)m_health / m_maxHealth) * m_legs.Length);
      if( index < m_legs.Length && (index % 2 == 0) && m_legs[index].gameObject.activeInHierarchy)
      {
        m_legs[index].gameObject.SetActive(false);
        m_legs[index + 1].gameObject.SetActive(false);
      }

      if(!m_bDamageFlashCorRunning)
      {
       StartCoroutine(DamagedFlash_Cor());
      }
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.layer == 11) //nub trigger
    {
      m_meshTrans.localPosition = new Vector3(0.0f, 0.0f, -.25f);
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
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

  //---------------------------------------------------------------------------------------------------------------------
  private void OnTriggerStay(Collider other)
  {
    if(other.gameObject.layer == 8) //turnSignal layer
    {
      Vector3 force = (transform.right * (4f * m_nextDir)) + (transform.up * (-.0f));
      m_rigid.AddForce(force);
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  private IEnumerator DamagedFlash_Cor()
  {
    m_bDamageFlashCorRunning = true;

    MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
    //if (TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
    {
      for (int i = 0; i < m_numFlashFrames; ++i)
      {
        foreach(MeshRenderer mesh in renderers)
        {
          mesh.material.color = Color.grey;
        }
        yield return new WaitForSeconds(.1f);
        foreach (MeshRenderer mesh in renderers)
        {
          mesh.material.color = Color.white;
        }
        yield return new WaitForSeconds(.1f); ;
      }
    }

    yield return new WaitForSeconds(.2f);

    m_bDamageFlashCorRunning = false;
  }

}
