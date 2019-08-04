using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileComp : MonoBehaviour
{
  public Transform m_flipPartner;
  public GameObject m_ControlNub;
  public Transform m_turnSignal;

  public AudioClip[] m_rotateSounds;
  public AudioClip[] m_flipSounds;

  private AudioSource audioSource;

  public void Start()
  {
    audioSource = GetComponent<AudioSource>();

    //reparent turn signal first so it doesn't move when tile is rotated ever
    if (m_turnSignal)
    {
      Quaternion temp = transform.rotation;
      transform.localRotation = Quaternion.identity;
      m_turnSignal.parent = transform.parent;

      transform.rotation = temp;
    }

    int onOff = Random.Range(0, 7);
    //gameObject.SetActive(onOff != 0);
    int rotations = Random.Range(0, 6);
    //transform.Rotate(Vector3.forward, 60.0f * rotations, Space.Self);

    //randomly enable nub
    onOff = Random.Range(0, 4);
    if (m_ControlNub && !m_ControlNub.activeSelf)
    {
      //m_nub.SetActive(onOff == 0);
    }
  }

  public enum ERotateDir
  {
    eRight,
    eLeft,
  }
  public void RotateTile(ERotateDir dir)
  {
    if (m_ControlNub.activeSelf)//only hexes with the control nub can be rotated
    {
      int sign = dir == ERotateDir.eRight ? 1 : -1;
      transform.Rotate(-Vector3.forward, 60.0f * sign, Space.Self);

      int index = Random.Range(0, m_rotateSounds.Length);
      audioSource.pitch = .9f + (index % 3 * .1f);
      audioSource.PlayOneShot(m_rotateSounds[index], .5f);
    }
  }
  public void FlipTile()
  {
    if (m_flipPartner)
    {
      //make sure to reposition turn signal
      Vector3 hexSpaceRelativePos = m_turnSignal.position - transform.position;

      Vector3 flipDir = transform.position - m_flipPartner.position;
      flipDir = Vector3.Cross(flipDir, -transform.forward);
      transform.RotateAround(transform.position, flipDir, 180.0f);
      m_flipPartner.RotateAround(m_flipPartner.position, flipDir, 180.0f);

      Vector3 tempPos = transform.position;
      transform.position = m_flipPartner.position;
      m_flipPartner.position = tempPos;

      //correct turn signal position
      m_turnSignal.position = transform.position + hexSpaceRelativePos;

      //sound queue
      int index = Random.Range(0, m_flipSounds.Length);
      audioSource.pitch = .9f + (index % 3 * .1f);
      audioSource.PlayOneShot(m_flipSounds[index], .5f);
    }
  }
}
