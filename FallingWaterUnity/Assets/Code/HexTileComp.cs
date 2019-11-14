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

  private bool m_bSelected = false;
  private int m_targetSlice;

  //--------------------------------------
  public enum ERotateDir
  {
    eRight,
    eLeft,
  }

  //-----------------------------------------------------------------------------------------
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

    int rotations = Random.Range(0, 6);
    if (GameManagerComp.m_currentLevelIndex == 4)
    {
      gameObject.SetActive(onOff != 0);
      transform.Rotate(Vector3.forward, 60.0f * rotations, Space.Self);
    }
    //randomly enable nub
    onOff = Random.Range(0, 4);
    if (m_ControlNub && !m_ControlNub.activeInHierarchy && GameManagerComp.m_currentLevelIndex == 4)
    {
      //m_nub.SetActive(onOff == 0);
    }
  }

  //-----------------------------------------------------------------------------------------
  public void Update()
  {
    if (Camera.current)
    {
      Vector3 hexPos_ScreenSpace = Camera.current.WorldToScreenPoint(transform.position);
      hexPos_ScreenSpace.z = 0;
      Vector3 mousePos_HexSpace = Input.mousePosition - hexPos_ScreenSpace;

      float currentAngleCW = ((transform.rotation.eulerAngles.z + 360) % 360);
      currentAngleCW = (int)(-currentAngleCW + 90) % 360;

      float targetAngleCW = Vector3.Angle(mousePos_HexSpace, Vector3.up);
      if (Mathf.Sign(mousePos_HexSpace.x) == -1)
      {
        targetAngleCW = 360 - targetAngleCW;
      }

      if (m_bSelected)
        print(mousePos_HexSpace);
      if (m_bSelected && mousePos_HexSpace.magnitude/Screen.width > (15 / Screen.width)/*est hex screen space size sq*/)
      {
        m_targetSlice = (int)targetAngleCW / 60;
      }

      float targetZAngleCW = m_targetSlice * 60 + 30;
      float targetZAngleReal = -1 * (targetZAngleCW - 90);
      if(m_bSelected)
        print("currentAngleCW:" + currentAngleCW + "\ttargetHexSlice: " + m_targetSlice + "\ttargetAngleCW: " + targetAngleCW + "\ttargetZAngleReal: " + targetZAngleReal);

      Vector3 ZRotNormalVec = new Vector3(-Mathf.Sin((targetZAngleCW - 90) * Mathf.Deg2Rad), Mathf.Cos((targetZAngleCW - 90) * Mathf.Deg2Rad), 0.0f);
      int rotDir = (int)Mathf.Sign((int)Vector3.Dot(mousePos_HexSpace, ZRotNormalVec));
      float targetRotDelta = rotDir * Mathf.DeltaAngle(targetZAngleReal, transform.rotation.eulerAngles.z);
      

      if (targetRotDelta > 10)
      {
        transform.Rotate(-Vector3.forward, targetRotDelta * .2f, Space.Self);

      }
      else //snap to desired hex
      {
        //print("snap!");
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetZAngleReal);
      }
    }
  }

  //-----------------------------------------------------------------------------------------
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

  //-----------------------------------------------------------------------------------------
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

  //-----------------------------------------------------------------------------------------
  public void SetIsSelected(bool bSelected)
  {
    m_bSelected = bSelected;
  }
}
