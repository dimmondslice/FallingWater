using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileComp : MonoBehaviour
{
  public GameObject m_nub;
  public Transform m_flipPartner;

  public void Start()
  {

    int onOff = Random.Range(0, 7);
    gameObject.SetActive(onOff != 0);
    int rotations = Random.Range(0, 6);
    transform.Rotate(Vector3.forward, 60.0f * rotations, Space.Self);

    //randomly enable nub
    onOff = Random.Range(0, 4);
    if (m_nub && !m_nub.activeSelf)
    {
      m_nub.SetActive(onOff == 0);
    }
  }

  public enum ERotateDir
  {
    eRight,
    eLeft,
  }
  public void RotateTile(ERotateDir dir)
  {
    if (!m_flipPartner)
    {
      int sign = dir == ERotateDir.eRight ? 1 : -1;
      transform.Rotate(-Vector3.forward, 60.0f * sign, Space.Self);
    }
  }
  public void FlipTile()
  {
    if (m_flipPartner)
    {
      Vector3 flipDir = transform.position - m_flipPartner.position;
      flipDir = Vector3.Cross(flipDir, -transform.forward);
      transform.RotateAround(transform.position, flipDir, 180.0f);
      m_flipPartner.RotateAround(m_flipPartner.position, flipDir, 180.0f);

      Vector3 tempPos = transform.position;
      transform.position = m_flipPartner.position;
      m_flipPartner.position = tempPos;
    }
  }
}
