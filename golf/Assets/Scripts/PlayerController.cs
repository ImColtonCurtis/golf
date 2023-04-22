using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    Vector3 lastSpawnLocation;
    bool inAction;
    public static float rotationOffset, hieghtOffset;
    public static int currentRotation;

    [SerializeField] SoundManagerLogic mySoundManager;

    [SerializeField] AudioSource[] char_getting_up, char_charging, char_club_hit, char_cheering, char_lost, char_wall_hit, ballWallHit;
    [SerializeField] AudioSource ballCharge;

    // char: getting up, charging, club hit, lost, wall_hit
    // club hit (on char: 3d sound), ball charging (on char: 3d sound), ball wall (on char: 3d sound)

    public static bool staticGetUp, staticCharging, staticClubHit, staticLost;
    public static float chargeAmount;
    int chargeInt;

    [SerializeField] Rigidbody myRB;
    float prevVelMag;

    private void Start()
    {
        rotationOffset = 0;
        currentRotation = 0; // 0 = North, 1 = East, 2 = South, 3 = West

        chargeAmount = 0;
        chargeInt = 0;
        prevVelMag = 0;
    }

    private void Update()
    {
        // check for wall hit
        if (prevVelMag - myRB.velocity.magnitude > prevVelMag * 0.1f && prevVelMag > 4f 
            && GameManager.levelStarted && !GameManager.levelFailed && !GameManager.levelPassed)
        {
            int charWallInt = Random.Range(0, char_wall_hit.Length);
            int ballWallInt = Random.Range(0, ballWallHit.Length);
            float volumeLevel = Mathf.Clamp(prevVelMag / 20, 0, 1);
            char_wall_hit[charWallInt].volume = volumeLevel/2f;
            char_wall_hit[charWallInt].Play();
            ballWallHit[ballWallInt].volume = Mathf.Clamp(volumeLevel*1.5f, 0, 1);
            ballWallHit[ballWallInt].Play();
        }
        prevVelMag = myRB.velocity.magnitude;

        // play char sounds
        if (staticLost)
        {
            char_lost[Random.Range(0, char_lost.Length)].Play();
            staticLost = false;
        }

        if (staticGetUp)
        {
            char_getting_up[Random.Range(0, char_getting_up.Length)].Play();
            staticGetUp = false;
        }

        if (staticCharging)
        {
            ballCharge.Play();
            chargeInt = Random.Range(0, char_charging.Length);
            char_charging[chargeInt].Play();
            staticCharging = false;
        }
        
        if (char_charging[chargeInt].isPlaying)
        {
            ballCharge.volume = Mathf.Clamp(chargeAmount / 1.8f, 0, 1);
            ballCharge.pitch = (chargeAmount / 3.3334f) + 0.85f;

            char_charging[chargeInt].volume = Mathf.Clamp(chargeAmount/8.2f, 0, 1);
            char_charging[chargeInt].pitch = (chargeAmount/3.3334f)+0.85f;
        }

        if (staticClubHit)
        {
            if (char_charging[chargeInt].isPlaying)
            {
                StartCoroutine(FadeSound(char_charging[chargeInt], Mathf.Min(22, char_charging[chargeInt].volume * 22)));
                StartCoroutine(FadeSound(ballCharge, Mathf.Min(18, ballCharge.volume*18)));
            }

            mySoundManager.Play("ClubHit"); // ball in hole
            char_club_hit[Random.Range(0, char_club_hit.Length)].Play();
            staticClubHit = false;
        }
    }
    IEnumerator FadeSound(AudioSource myAudio, float total)
    {
        float timer = 0, totalTime = total;

        float startingVolume = myAudio.volume;

        while (timer <= totalTime)
        {
            myAudio.volume = Mathf.Lerp(startingVolume, 0, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }

        myAudio.volume = 0;
        myAudio.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Lost" && ((!GameManager.levelPassed && !GameManager.levelFailed && !miniGameManager.inMiniGame) || (!miniGameManager.levelFailed2 && miniGameManager.inMiniGame)))
        {
            if (!miniGameManager.inMiniGame)
                GameManager.levelFailed = true;
            else
                miniGameManager.levelFailed2 = true;
        }

        if (collision.gameObject.tag == "Course")
        {
            rotationOffset = collision.gameObject.transform.parent.transform.localEulerAngles.y;
            hieghtOffset = collision.gameObject.transform.parent.parent.transform.localPosition.y;

            switch ((int)rotationOffset) // 0 = North, 1 = East, 2 = South, 3 = West
            {
                case 0:
                    currentRotation = 0;
                    break;
                case 90:
                    currentRotation = 1;
                    break;
                case 180:
                    currentRotation = 2;
                    break;
                case 270:
                    currentRotation = 3;
                    break;
                default:
                    Debug.Log("Error: " + rotationOffset);
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Win" && !GameManager.levelFailed && !GameManager.levelPassed && !miniGameManager.inMiniGame)
        {
            mySoundManager.Play("BallInHole"); // ball in hole

            StartCoroutine(DelayJingle());

            GameManager.holCol.enabled = false;
            GameManager.levelPassed = true;
            ShowConfetti.confObj.SetActive(true);
        }
    }

    IEnumerator DelayJingle()
    {
        yield return new WaitForSeconds(0.35f);
        char_cheering[Random.Range(0, char_cheering.Length)].Play(); // char cheering sound effect
        mySoundManager.Play("WinJingle"); // win jingle
    }
}
