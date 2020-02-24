using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugEnemyComp : MonoBehaviour
{
  public static int s_totalEnemiesAlive = 0;

  public float m_maxVelocity;
  public float m_moveForce;
  public int m_health;
  public EMovementType m_movementType;
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
  private int m_nextDir = 1;

  private bool m_bDamageFlashCorRunning = false;

  private Vector3 m_goalMoveDir;

  public enum EMovementType
  {
    left = -1,
    center = 0,
    right = 1
  };

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

    m_goalMoveDir = transform.up;

    if(m_movementType != EMovementType.center)
      m_nextDir = (int)m_movementType;

    m_initalLocalLegRot = new Quaternion[m_legs.Length];
    m_initalLocalLegPos = new Vector3[m_legs.Length];
    for(int i = 0; i < m_legs.Length; ++i)
    {
      m_initalLocalLegRot[i] = m_legs[i].localRotation;
      m_initalLocalLegPos[i] = m_legs[i].localPosition;
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  public void SetNextDirection(int sign)
  {
    if (m_movementType == EMovementType.center)
    {
      m_nextDir = sign;
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  private void FixedUpdate()
  {
    Movement();

    //leg visual update
    for (int i = 0; i < m_legs.Length; ++i)
    {
      int upOrDown = (int)Mathf.Sign((i % 3) - 2 + (i % 3)); 
      m_legs[i].localPosition = m_initalLocalLegPos[i] + Vector3.forward * (Mathf.Sin(Time.fixedTime * m_legspeed) * upOrDown * m_legStrideLength);
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  private void Movement()
  {
    Vector3 force = m_moveForce * m_goalMoveDir;
    //switch(m_movementType)
    //{
    //  case EMovementType.center:
    //  {
    //    float xDelta = m_initalXPos - transform.position.x;
    //    if (Mathf.Abs(xDelta) > 0.1f)
    //    {
    //      force += transform.right * xDelta * 1.25f;
    //    }
    //    force += transform.forward * .5f; //magnetize bug to ground kinda
    //  } break;
    //  case EMovementType.left:
    //  case EMovementType.right:
    //    force += m_moveForce * transform.right * (int)m_movementType;
    //    break;     
    //}


    m_rigid.AddForce(force);
    //m_rigid.velocity = force;
    Debug.DrawRay(transform.position, force, Color.green);

    //constrain movement to max velocity
    if (m_rigid.velocity.magnitude > m_maxVelocity)
    {
      m_rigid.velocity = m_rigid.velocity.normalized * m_maxVelocity;
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
    else if (other.gameObject.layer == 12) //Top NavVert
    {
      m_goalMoveDir = transform.up;

    }
    else if (other.gameObject.layer == 13) //bottomNavVert
    {
      if (m_movementType == EMovementType.center)
      {
        m_nextDir *= -1;
      }

      //m_goalMoveDir = Mathf.Sin(30 * Mathf.Deg2Rad) * transform.up + (transform.right * Mathf.Cos(60 * Mathf.Deg2Rad) * m_nextDir);
      // m_goalMoveDir = transform.InverseTransformDirection(Mathf.Sin(60 * Mathf.Deg2Rad) * Vector3.up + m_nextDir * Mathf.Cos(60 * Mathf.Deg2Rad) * Vector3.right);
      //m_rigid.velocity *= new Vector3();

      m_goalMoveDir = transform.up + (transform.right * m_nextDir);
    }
  }

  //---------------------------------------------------------------------------------------------------------------------
  private void OnTriggerExit(Collider other)
  {
    if (other.gameObject.layer == 11) //nub trigger
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
      //m_rigid.AddForce(force);
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
