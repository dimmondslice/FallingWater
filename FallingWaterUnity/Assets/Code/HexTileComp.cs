using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class HexTileComp : MonoBehaviour
{
  public Transform m_flipPartner;
  public GameObject m_ControlNub;
  public Transform m_turnSignal;

  public Transform m_pointer;
  public GameObject m_ghostHexPrefab;

  public float m_liftDistance;
  public bool m_allowImmediateRotate;
  public float m_rotateSpeed;

  public AudioClip[] m_rotateSounds;
  public AudioClip[] m_flipSounds;
  public AudioClip m_endRotateSound;

  private AudioSource audioSource;
  private GameObject m_currentSpawnedGhost;

  private bool m_bSelected = false;
  private int m_targetSlice;
  private int m_currentSlice;

  private int m_flipDir = 1;

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
    //if (m_ControlNub && !m_ControlNub.activeInHierarchy && GameManagerComp.m_currentLevelIndex == 4)
    {
      //m_ControlNub.SetActive(onOff == 0);
    }
  }

  //-----------------------------------------------------------------------------------------
  public void SelectTile()
  {
    if (!m_ControlNub.activeInHierarchy)
      return;

    transform.Translate(-transform.forward * m_liftDistance, Space.Self);

    //spawn ghost
    if (!m_allowImmediateRotate && m_ghostHexPrefab)
    {
      m_currentSpawnedGhost = Instantiate(m_ghostHexPrefab, transform.position, transform.rotation);
      if (m_currentSpawnedGhost.TryGetComponent<HexTileComp>(out HexTileComp hex))
      {
        hex.SelectTile();
      }
    }

    m_bSelected = true;
    StartCoroutine(WhileSelected_Cor());
  }

  //-----------------------------------------------------------------------------------------
  public void DeselectTile()
  {
    if (!m_ControlNub.activeInHierarchy)
      return;

    m_bSelected = false;
    StopCoroutine(WhileSelected_Cor());

    transform.Translate(transform.forward * m_liftDistance, Space.Self);


    Destroy(m_currentSpawnedGhost);

    //no rotate the tile after the correct position is selected
    if (m_currentSlice != m_targetSlice && !m_bRotate_CorRunning)
    {
      StartCoroutine(Rotate_Cor());
    }
  }

  //------------------------------------------------------------------------------------------
  private IEnumerator Lift()
  {
    while (true)
    {
      yield return null;
    }
  }

  //------------------------------------------------------------------------------------------
  private IEnumerator WhileSelected_Cor()
  {
    while (m_bSelected)
    {
      Vector3 hexPos_ScreenSpace = Camera.main.WorldToScreenPoint(transform.position);
      hexPos_ScreenSpace.z = 0;
      Vector3 mousePos_HexSpace = Input.mousePosition - hexPos_ScreenSpace;

      //update rotation
      {
        float targetAngleCW = Vector3.Angle(mousePos_HexSpace, Vector3.right);
        if (mousePos_HexSpace.y > 0)
        {
          targetAngleCW = 360 - targetAngleCW;
        }

        if ((mousePos_HexSpace.magnitude / Screen.width) > (18.0f / Screen.width)/*est hex screen space size sq*/)
        {
          m_targetSlice = ((Mathf.FloorToInt(targetAngleCW) + 30) % 360) / 60;
        }

        //allow immediate hex rotating based on mouse
        if (m_allowImmediateRotate && m_currentSlice != m_targetSlice && !m_bRotate_CorRunning)
        {
          StartCoroutine(Rotate_Cor());
        }
      }

      //check for flip tile
      {

      }

      yield return null;
    }
  }

  private IEnumerator Rotate_Cor()
  {
    m_bRotate_CorRunning = true;

    //play rotation sound
    if (audioSource && !audioSource.isPlaying)
    {
      int index = Random.Range(0, m_rotateSounds.Length);
      audioSource.pitch = .9f + (index % 3 * .1f);
      audioSource.PlayOneShot(m_rotateSounds[index], .5f);
    }

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

      transform.Rotate(-Vector3.forward, targetRotDelta * m_rotateSpeed, Space.Self);
      m_currentSlice = ((Mathf.FloorToInt(currentAngleCW) + 30) % 360) / 60;

      yield return null;
    }

    //end coroutine
    {
      //snap into correct rotation
      transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetZAngleReal);
      m_currentSlice = m_targetSlice;

      if (audioSource)
      {
        int index = Random.Range(0, 20);
        audioSource.pitch = .9f + (index % 3 * .1f);
        audioSource.PlayOneShot(m_endRotateSound);
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
      audioSource.pitch = .8f + (index % 4 * .1f);
      audioSource.PlayOneShot(m_rotateSounds[index], .5f);
    }
  }

  //-----------------------------------------------------------------------------------------
  public void FlipTile()
  {
    //if (m_flipPartner)
    //{
    //  //make sure to reposition turn signal
    //  Vector3 hexSpaceRelativePos = m_turnSignal.position - transform.position;

    //  Vector3 flipDir = transform.position - m_flipPartner.position;
    //  flipDir = Vector3.Cross(flipDir, -transform.forward);
    //  transform.RotateAround(transform.position, flipDir, 180.0f);
    //  m_flipPartner.RotateAround(m_flipPartner.position, flipDir, 180.0f);

    //  Vector3 tempPos = transform.position;
    //  transform.position = m_flipPartner.position;
    //  m_flipPartner.position = tempPos;

    //  //correct turn signal position
    //  m_turnSignal.position = transform.position + hexSpaceRelativePos;

    //  //sound queue
    //  int index = Random.Range(0, m_flipSounds.Length);
    //  audioSource.pitch = .9f + (index % 3 * .1f);
    //  audioSource.PlayOneShot(m_flipSounds[index], .5f);
    //}


    if(m_flipPartner)
    {

      StartCoroutine(FlipTile_Cor());



    }

  }

  private IEnumerator FlipTile_Cor()
  {
    //Debug.Break();
    //make sure to reposition turn signal
    Vector3 hexSpaceRelativePos = m_turnSignal.position - transform.position;
    Vector3 oldPositionThisHex = transform.position;
    Quaternion oldRotation = transform.localRotation;

    Vector3 flipAxis = transform.position - m_flipPartner.position;
    flipAxis = Vector3.Cross(flipAxis, -m_flipPartner.forward);

    //slide hex around ring
    Vector3 center = ((m_flipPartner.position - transform.position) / 2.0f) + transform.position;
    Vector3 orthogonalRotateAxis = new Vector3(center.y, -center.x);
    int rotateDir = (int)Mathf.Sign(Vector3.Dot(orthogonalRotateAxis, transform.position - center));
    orthogonalRotateAxis *= rotateDir;

    while (Vector3.SignedAngle(transform.position - center, m_flipPartner.position - center, orthogonalRotateAxis) > 0)
    {
      transform.RotateAround(center, orthogonalRotateAxis, 3);
      yield return null;
    }

    m_flipDir *= -1;  //switch flip dir to be the opposite direction next time

    //snap to target position
    transform.position = m_flipPartner.position;
    //correct to our actual rotation
    //transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f + (transform.localRotation.eulerAngles.z / 60) * 60 + 180);
    //transform.localRotation = Quaternion.Euler(0.0f, 0.0f, transform.localRotation.eulerAngles.z );
    transform.RotateAround(transform.position, transform.right, 180);
    //transform.localRotation = Quaternion.Euler(0.0f, 0.0f, transform.localRotation.eulerAngles.z);
    transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f + Mathf.Round(transform.localRotation.eulerAngles.z / 60) * 60); 

    //correct partner position and rotation (assuming it's invisible

    float newAngle = Vector3.SignedAngle(flipAxis, new Vector3(0.0f, 0.0f, oldRotation.eulerAngles.z), Vector3.forward);
    //transform.RotateAround(transform.position, transform.forward, newAngle);
    //transform.RotateAround(transform.position, transform.right, 180);


    //transform.RotateAround(transform.position, flipAxis, 180.0f);
    m_flipPartner.RotateAround(m_flipPartner.position, flipAxis, 180.0f);

    m_flipPartner.position = oldPositionThisHex;


    //correct turn signal position
    m_turnSignal.position = transform.position + hexSpaceRelativePos;

    yield return null;
  }


}
