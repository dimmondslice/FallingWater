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
  private int m_currentSlice;

  private bool m_bRotate_CorRunning = false;

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
    if (false)
    {
      Vector3 hexPos_ScreenSpace = Camera.current.WorldToScreenPoint(transform.position);
      hexPos_ScreenSpace.z = 0;
      Vector3 mousePos_HexSpace = Input.mousePosition - hexPos_ScreenSpace;

      //float currentAngleCW = ((transform.rotation.eulerAngles.z + 360) % 360);
      //currentAngleCW = (int)(-currentAngleCW) % 360;
      float currentAngleCW = -transform.rotation.eulerAngles.z;
      if (currentAngleCW < 0)
      {
        currentAngleCW = 360 - currentAngleCW;
      }

      float targetAngleCW = Vector3.Angle(mousePos_HexSpace, Vector3.right);
      if (mousePos_HexSpace.y > 0)
      {
        targetAngleCW = 360 - targetAngleCW;
      }

      
      if (m_bSelected ) //&& mousePos_HexSpace.magnitude/Screen.width > (30 / Screen.width)/*est hex screen space size sq*/)
      {
        m_targetSlice = ((Mathf.FloorToInt(targetAngleCW) + 30) % 360) / 60;
      }

      float targetZAngleCW = m_targetSlice * 60;
      float targetZAngleReal = -1 * (targetZAngleCW);
      //if(m_bSelected)
        //print(/*"currentAngleCW:" + currentAngleCW +*/ "\ttargetHexSlice: " + m_targetSlice + "\ttargetAngleCW: " + targetAngleCW + "\ttargetZAngleReal: " + targetZAngleReal);

      Vector3 hexRot2D = new Vector3(Mathf.Cos(currentAngleCW * Mathf.Deg2Rad), Mathf.Sin(currentAngleCW * Mathf.Deg2Rad), 0.0f);
      Vector3 ZRotNormalVec = new Vector3(Mathf.Sin((targetZAngleReal) * Mathf.Deg2Rad), -Mathf.Cos((targetZAngleReal) * Mathf.Deg2Rad), 0.0f);
      int rotDir = (int)Mathf.Sign((int)Vector3.Dot(hexRot2D, ZRotNormalVec));

      float targetRotDelta = rotDir * Mathf.DeltaAngle(targetZAngleReal, currentAngleCW);
      
      if (Mathf.Abs(targetRotDelta) > 5)
      {
        transform.Rotate(-Vector3.forward, targetRotDelta * .3f, Space.Self);

      }
      else if(m_bSelected) //snap to desired hex slice
      {
        //print("snap!");
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetZAngleReal);
        //int index = Random.Range(0, m_rotateSounds.Length);
        //audioSource.pitch = .9f + (index % 3 * .1f);
        //audioSource.PlayOneShot(m_rotateSounds[index], .5f);
      }
    }
  }

  //-----------------------------------------------------------------------------------------
  public void SelectTile()
  {
    m_bSelected = true;
    StartCoroutine(WhileSelected_Cor());
  }

  //-----------------------------------------------------------------------------------------
  public void DeselectTile()
  {
    m_bSelected = false;
    StopCoroutine(WhileSelected_Cor());
  }

  //------------------------------------------------------------------------------------------
  private IEnumerator WhileSelected_Cor()
  {
    while (m_bSelected && Camera.main)
    {
      Vector3 hexPos_ScreenSpace = Camera.main.WorldToScreenPoint(transform.position);
      hexPos_ScreenSpace.z = 0;
      Vector3 mousePos_HexSpace = Input.mousePosition - hexPos_ScreenSpace;



      float targetAngleCW = Vector3.Angle(mousePos_HexSpace, Vector3.right);
      if (mousePos_HexSpace.y > 0)
      {
        targetAngleCW = 360 - targetAngleCW;
      }


      if ((mousePos_HexSpace.magnitude/Screen.width) > (18.0f / Screen.width)/*est hex screen space size sq*/)
      {
        m_targetSlice = ((Mathf.FloorToInt(targetAngleCW) + 30) % 360) / 60;
      }


      if(m_currentSlice != m_targetSlice && !m_bRotate_CorRunning)
      {
        StartCoroutine(Rotate_Cor());
      }

      yield return null;
    }
  }

  private IEnumerator Rotate_Cor()
  {
    m_bRotate_CorRunning = true;

    float targetZAngleCW = (m_targetSlice * 60) % 360;
    float targetZAngleReal = -1 * (targetZAngleCW);

    int rotDir = 0; //set only on the first iteration
    float targetRotDelta = Mathf.Infinity;
    while (Mathf.Abs(targetRotDelta) > 7)
    {
      float currentAngleCW = -transform.rotation.eulerAngles.z;
      if (currentAngleCW < 0)
      {
        currentAngleCW = 360 - currentAngleCW;
      }

      Vector3 hexRot2D = new Vector3(Mathf.Cos(currentAngleCW * Mathf.Deg2Rad), Mathf.Sin(currentAngleCW * Mathf.Deg2Rad), 0.0f);
      Vector3 ZRotNormalVec = new Vector3(Mathf.Sin((targetZAngleReal) * Mathf.Deg2Rad), -Mathf.Cos((targetZAngleReal) * Mathf.Deg2Rad), 0.0f);

      //if (Mathf.Abs(targetRotDelta) < 120)
        rotDir = (int)Mathf.Sign((int)Vector3.Dot(hexRot2D, ZRotNormalVec));
      targetRotDelta = rotDir * Mathf.DeltaAngle(targetZAngleReal, currentAngleCW);

      transform.Rotate(-Vector3.forward, targetRotDelta * .3f, Space.Self);
      m_currentSlice = ((Mathf.FloorToInt(currentAngleCW) + 30) % 360) / 60;

      yield return null;
    }
    
    //end coroutine
    {
      //snap into correct rotation
      transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetZAngleReal);
      m_currentSlice = m_targetSlice;

      //play rotation sound
      if (!audioSource.isPlaying)
      {
        int index = Random.Range(0, m_rotateSounds.Length);
        audioSource.pitch = .9f + (index % 3 * .1f);
        audioSource.PlayOneShot(m_rotateSounds[index], .5f);
      }
    }
    m_bRotate_CorRunning = false;
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
}
