using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugEnemyComp : MonoBehaviour
{
  public float m_speed;

  private Rigidbody m_rigid;

  void Start()
  {
    m_rigid = GetComponentInChildren<Rigidbody>();
    m_rigid.velocity = transform.up * m_speed;
  }
}
