using System.Collections;
using System.Collections.Generic;
using Unity.Services.Mediation.Samples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsLogic : MonoBehaviour
{
    bool touchedDown, primed;
    public static bool isStable, flyOverFastTap;
    float powerConstraint, powerMultiplier, arrowAngle;
    Vector3 startingTapLoc, force;

    [SerializeField] Rigidbody rb;
    [SerializeField] Transform arrowRot;

    [SerializeField] Color arrowColor0, arrowColor1;
    [SerializeField] MeshRenderer arrowMesh, outlineMesh;
    [SerializeField] Transform arrowPos, arrowSize, myTransform;
    Color outlineMeshColor;
    Vector3 startingArrowSize = new Vector3(1.656536f, 0.8701671f, 0), endingArrowSize = new Vector3(1.656536f, 0.8701671f, 0.9719138f);

    [SerializeField] Animator charAnim, camAnim, parCountAnim;
    int ChargeSpeed;

    [SerializeField] SphereCollider noTrigger;

    float prevX = 0, prevZ = 0;
    bool shakeOnWin;

    [SerializeField] Color startingParColor, endingParColor;

    [SerializeField] GroundHeightFinder getTheGroundHeight;

    [SerializeField] GameObject noIcon;

    [SerializeField] Animator soundAnim;

    int cheatCounter;

    void Awake()
    {
        touchedDown = false;
        isStable = true;
        primed = false;
        shakeOnWin = false;
        flyOverFastTap = false;

        powerConstraint = 0.075f;
        powerMultiplier = 320; // was 241

        ChargeSpeed = 0;
        cheatCounter = 0;

        outlineMeshColor = outlineMesh.material.GetColor("_Color");
        outlineMesh.material.SetColor("_Color", new Color(outlineMeshColor.r, outlineMeshColor.g, outlineMeshColor.b, 0));
        arrowMesh.material.SetColor("_Color", new Color(arrowColor0.r, arrowColor0.g, arrowColor0.b, 0));

        if (PlayerPrefs.GetInt("SoundStatus", 1) == 1)
        {
            noIcon.SetActive(false);
            AudioListener.volume = 1;
        }
        else
        {
            noIcon.SetActive(true);
            AudioListener.volume = 0;
        }
    }
    
    private void Update()
    {

        if (Mathf.Abs(rb.velocity.x - prevX) > 10 || Mathf.Abs(rb.velocity.z - prevZ) > 8)
            camAnim.SetTrigger("shake");

        if (!shakeOnWin && GameManager.levelPassed && !miniGameManager.inMiniGame)
        {
            camAnim.SetTrigger("shake");
            shakeOnWin = true;
        }

        prevX = rb.velocity.x;
        prevZ = rb.velocity.z;

        // decrease to 0
        if (!isStable && rb.velocity.magnitude <= 1.8f)
        {
            if (rb.velocity.magnitude <= 1.8f && rb.velocity.magnitude > 0.025f)
                rb.velocity = rb.velocity * 0.9f;
            else if (rb.velocity.magnitude <= 0.025 && primed && ((!GameManager.levelFailed && !miniGameManager.inMiniGame) || (!miniGameManager.levelFailed2 && miniGameManager.inMiniGame)))
            {
                PlayerController.staticGetUp = true;
                charAnim.SetTrigger("endRoll");
                isStable = true;
                primed = false;
                rb.velocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeAll;

                if (!GameManager.levelFailed && !GameManager.levelPassed && !miniGameManager.inMiniGame)
                    StartCoroutine(MoveToZero());
            }
        }
    }

    IEnumerator MoveToZero()
    {
        float timer = 0, totalTime = Random.Range(14, 28);
        Vector3 startingVector = myTransform.localPosition;

        Vector3 endingVector = new Vector3(myTransform.localPosition.x, Mathf.Max(getTheGroundHeight.GetGroundHeight().y + 0.01f, -6.14f), myTransform.localPosition.z);

        Vector3 startingRot = myTransform.localEulerAngles;
        Vector3 firstPos = new Vector3(myTransform.position.x, 0, myTransform.position.z);
        Vector3 secondPos = new Vector3(GameManager.holeLocation.x, 0, GameManager.holeLocation.z);

        float turnAngle = Vector3.Angle(firstPos, secondPos);

        if (myTransform.position.x < GameManager.holeLocation.x && myTransform.position.z < GameManager.holeLocation.z)
        {
            if (myTransform.position.x < 0 && myTransform.position.z >= 0)
                turnAngle += 33.75f;
            else if (myTransform.position.x >= 0 && myTransform.position.z < 0)
                turnAngle -= 22.5f;
            else if (myTransform.position.x < 0 && myTransform.position.z < 0)
                turnAngle -= 33.75f;
            else if(myTransform.position.x > 0 && myTransform.position.z > 0)
                turnAngle += 33.75f;
        }
        else if (myTransform.position.x < GameManager.holeLocation.x && myTransform.position.z > GameManager.holeLocation.z)
        {
            if (myTransform.position.x > 0 && GameManager.holeLocation.z > 0)
                turnAngle += 78.75f;
            else if (myTransform.position.x < 0)
                turnAngle += 67.5f;
            else
                turnAngle += 88.75f;
        }
        else if (myTransform.position.x > GameManager.holeLocation.x && myTransform.position.z > GameManager.holeLocation.z)
        {
            turnAngle += 202.5f;
        }
        else if (myTransform.position.x > GameManager.holeLocation.x && myTransform.position.z < GameManager.holeLocation.z)
        {
            turnAngle += 292.5f;
        }
        Vector3 endingRot = new Vector3(0, turnAngle, 0);

        noTrigger.enabled = false;

        while (timer <= totalTime)
        {
            myTransform.localPosition = Vector3.Lerp(startingVector, endingVector, timer / totalTime);
            myTransform.localEulerAngles = Vector3.Lerp(startingRot, endingRot, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        myTransform.localPosition = endingVector;
        myTransform.localEulerAngles = endingRot;

        noTrigger.enabled = true;
    }

    void OnTouchDown(Vector3 point)
    {
        if (!touchedDown && !GameManager.inLoading)
        {
            if (ShowAds.poppedUp)
            {
                if (point.x <= 0)
                    ShowAds.shouldShowRewardedAd = true;
                else
                    ShowAds.dontShow = true;
            }
            else if (ShowAds.skipPoppedUp)
            {
                if (point.x <= 0)
                    ShowAds.shouldShowRewardedAd = true;
                else
                    ShowAds.dontShow = true;
            }
            else if (GameManager.preGame)
            {
                flyOverFastTap = true;
            }
            else
            {
                // cheat: top-right, top-right, top-left, bottom-right
                // top right tap
                if (!GameManager.levelStarted && (cheatCounter == 0 || cheatCounter == 1) && point.x >= 0.03f && point.y >= 8f)
                {
                    cheatCounter++;
                }
                // top left tap
                else if (!GameManager.levelStarted && (cheatCounter == 2) && point.x <= -0.03f && point.y >= 8f)
                {
                    cheatCounter++;
                }
                // bottom right tap
                else if (!GameManager.levelStarted && (cheatCounter == 3) && point.x >= 0.03f && point.y <= 7.92f)
                {
                    cheatCounter = 0;
                    if (!GameManager.cheatOn)
                        GameManager.cheatOn = true;
                    else
                        GameManager.cheatOn = false;
                }

                else if (!GameManager.levelStarted && point.x <= -0.01f && point.y <= 7.92f) // bottom left button clicked
                {
                    if (PlayerPrefs.GetInt("SoundStatus", 1) == 1)
                    {
                        PlayerPrefs.SetInt("SoundStatus", 0);
                        noIcon.SetActive(true);
                        AudioListener.volume = 0;
                    }
                    else
                    {
                        PlayerPrefs.SetInt("SoundStatus", 1);
                        noIcon.SetActive(false);
                        AudioListener.volume = 1;
                    }
                    soundAnim.SetTrigger("Blob");
                }
                else
                {
                    if (isStable && ((GameManager.par > 0 && !miniGameManager.inMiniGame && !GameManager.levelPassed && !GameManager.levelFailed)  || (miniGameManager.inMiniGame && !miniGameManager.levelFailed2)))
                    {
                        startingTapLoc = point;

                        if (!GameManager.levelFailed && !GameManager.levelStarted && !miniGameManager.inMiniGame)
                        {
                            GameManager.levelStarted = true;
                        }
                        else if (!miniGameManager.levelStarted2 && miniGameManager.inMiniGame)
                            miniGameManager.levelStarted2 = true;
                        PlayerController.staticCharging = true;

                        arrowSize.localScale = startingArrowSize;

                        StartCoroutine(FadeArrowIn());
                        StartCoroutine(FadeArrowOutlineIn());

                        touchedDown = true;
                    }
                }
            }
            if ((GameManager.levelFailed || GameManager.levelPassed) && !miniGameManager.inMiniGame && !GameManager.fastTap)
                GameManager.fastTap = true;
            if (miniGameManager.levelFailed2 && miniGameManager.inMiniGame && !miniGameManager.fastTap)
                miniGameManager.fastTap = true;
        }
    }

    void OnTouchStay(Vector3 point)
    {
        if (isStable && ((!GameManager.levelFailed && !GameManager.levelPassed && !miniGameManager.inMiniGame) || (!miniGameManager.levelFailed2 && miniGameManager.inMiniGame)))
        {
            if (touchedDown)
            {
                Vector3 distance = startingTapLoc - point;
                force = new Vector3(Mathf.Clamp(distance.x, -powerConstraint, powerConstraint), 0, Mathf.Clamp(distance.y, -powerConstraint, powerConstraint));
                PlayerController.chargeAmount = Mathf.Clamp(force.magnitude, 0, powerConstraint) / powerConstraint;
                // alter force based on direction
                Vector3 tempFroce = force;
                switch (PlayerController.currentRotation)  // 0 = North, 1 = East, 2 = South, 3 = West
                {                    
                    case 0:
                        break;
                    case 1:
                        force.x = tempFroce.z;
                        force.z = tempFroce.x * -1;
                        break;
                    case 2:
                        force *= -1;
                        break;
                    case 3:
                        force.x = tempFroce.z * -1;
                        force.z = tempFroce.x;
                        break;
                    default:
                        Debug.Log("Error: " + PlayerController.currentRotation);
                        break;
                }

                // UPDATE FORCE LINE      
                if (force.x > 0 && force.z > 0)
                    arrowAngle = (Mathf.Rad2Deg * Mathf.Atan(force.x / force.z));
                else if (force.x < 0 && force.z > 0)
                    arrowAngle = (Mathf.Rad2Deg * Mathf.Atan(force.x / force.z));
                else if (force.x < 0 && force.z < 0)
                    arrowAngle = (Mathf.Rad2Deg * Mathf.Atan(force.x / force.z)) - 180;
                else if (force.x > 0 && force.z < 0)
                    arrowAngle = (Mathf.Rad2Deg * Mathf.Atan(force.x / force.z)) - 180;
                arrowPos.localEulerAngles = new Vector3(0, arrowAngle, 0);

                // Charge Speed
                if (Mathf.Clamp(force.magnitude, 0, powerConstraint) / powerConstraint >= 0.75f && ChargeSpeed != 3)
                {
                    ChargeSpeed = 3;
                    charAnim.SetTrigger("fastCharge");
                }
                else if (Mathf.Clamp(force.magnitude, 0, powerConstraint) / powerConstraint < 0.8f && Mathf.Clamp(force.magnitude, 0, powerConstraint) / powerConstraint >= 0.3f && ChargeSpeed != 2)
                {
                    ChargeSpeed = 2;
                    charAnim.SetTrigger("charge");
                }
                else if (Mathf.Clamp(force.magnitude, 0, powerConstraint) / powerConstraint < 0.3f && ChargeSpeed != 1)
                {
                    ChargeSpeed = 1;
                    charAnim.SetTrigger("slowCharge");
                }                

                //// Color
                Color tempColor = Color.Lerp(arrowColor0, arrowColor1, Mathf.Clamp(force.magnitude, 0, powerConstraint) / powerConstraint);
                arrowMesh.material.SetColor("_Color", tempColor);

                arrowSize.localScale = Vector3.Lerp(startingArrowSize, endingArrowSize, Mathf.Clamp(force.magnitude, 0, powerConstraint) / powerConstraint); // max = 0.9719138f;
            }
        }
    }

    void OnTouchUp()
    {
        if (touchedDown)
        {
            if ((GameManager.levelStarted && !GameManager.levelFailed && !GameManager.levelPassed && !miniGameManager.inMiniGame) || miniGameManager.inMiniGame)
            {
                StartCoroutine(ChargePrime());

                PlayerController.staticClubHit = true;

                charAnim.SetTrigger("startRoll");
                StartCoroutine(FadeArrowOut());
                StartCoroutine(FadeArrowOutlineOut());
                float miniMult = 1;
                if (miniGameManager.inMiniGame)
                    miniMult = 1.25f;

                rb.AddForce(force * powerMultiplier * miniMult, ForceMode.Impulse);
                rb.constraints = RigidbodyConstraints.None;

                if (!miniGameManager.inMiniGame)
                {
                    GameManager.par--;
                    GameManager.staticParBG.color = Color.Lerp(endingParColor, startingParColor, (float)GameManager.par / (float)GameManager.startintPar);
                    GameManager.staticParText.text = "" + GameManager.par;
                    parCountAnim.SetTrigger("shake");
                }

                isStable = false;
            }
            touchedDown = false;        
        }
    }

    IEnumerator ChargePrime()
    {
        yield return new WaitForSeconds(1f / 6f);
        primed = true;
    }

    void OnTouchExit()
    {
        if (touchedDown)
        {
            touchedDown = false;          
        }
    }

    IEnumerator FadeArrowIn()
    {
        arrowPos.position = new Vector3(rb.transform.position.x, Mathf.Max(rb.transform.position.y+0.01f, -6.139f), rb.transform.position.z); // set arrow position
        float timer = 0, totalTime = 8;
        Color startingColor = new Color(arrowColor0.r, arrowColor0.g, arrowColor0.b, 0);
        while (timer <= totalTime)
        {
            Color tempColor1 = Color.Lerp(arrowColor0, arrowColor1, Mathf.Clamp(force.magnitude, 0, 0.075f) / 0.075f);
            Color tempColor = Color.Lerp(startingColor, tempColor1, timer / totalTime);
            arrowMesh.material.SetColor("_Color", tempColor);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeArrowOut()
    {
        float timer = 0, totalTime = 6;
        Color startingColor = arrowMesh.material.GetColor("_Color");
        Color endingColor = new Color(startingColor.r, startingColor.g, startingColor.b, 0);
        while (timer <= totalTime)
        {
            Color tempColor = Color.Lerp(startingColor, endingColor, timer / totalTime);
            arrowMesh.material.SetColor("_Color", tempColor);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeArrowOutlineIn()
    {
        arrowPos.position = new Vector3(rb.transform.position.x, Mathf.Max(rb.transform.position.y + 0.01f, -6.139f), rb.transform.position.z); // set arrow position
        float timer = 0, totalTime = 8;
        Color startingColor = new Color(outlineMeshColor.r, outlineMeshColor.g, outlineMeshColor.b, 0);
        Color endingColor = new Color(outlineMeshColor.r, outlineMeshColor.g, outlineMeshColor.b, 1);
        while (timer <= totalTime)
        {
            Color tempColor = Color.Lerp(startingColor, endingColor, timer / totalTime);
            outlineMesh.material.SetColor("_Color", tempColor);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeArrowOutlineOut()
    {
        arrowPos.position = new Vector3(rb.transform.position.x, Mathf.Max(rb.transform.position.y + 0.01f, -6.139f), rb.transform.position.z); // set arrow position
        float timer = 0, totalTime = 6;
        Color startingColor = new Color(outlineMeshColor.r, outlineMeshColor.g, outlineMeshColor.b, 0);
        Color endingColor = new Color(outlineMeshColor.r, outlineMeshColor.g, outlineMeshColor.b, 1);
        while (timer <= totalTime)
        {
            Color tempColor = Color.Lerp(endingColor, startingColor, timer / totalTime);
            outlineMesh.material.SetColor("_Color", tempColor);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }
}
