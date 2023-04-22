using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class miniGameManager : MonoBehaviour
{
    public static bool levelFailed2, fastTap, cheatOn, inMiniGame;

    [SerializeField]
    Transform levelObjectsFolder;

    [SerializeField]
    Camera myCam;

    int miniGameScore;

    [SerializeField] Transform spawnFolder, charTransform;

    [SerializeField] SpriteRenderer golfTitle, golfBG, instructionsTitle, instructionsBG, whiteSquare, levelPassedSR, levelPassedBG, parBG, parBGDropShadow;

    [SerializeField] Transform[] retryTexts;

    bool restartLogic, startTrackingScore;

    [SerializeField] TextMeshPro currentLevel;

    [SerializeField] SkinnedMeshRenderer charMesh;
    [SerializeField] GameObject charTrail;
    [SerializeField] Animator charAnim;

    [SerializeField] Outline charOutline;
    [SerializeField] Transform spawnedObjectsFolder;

    // level spawner literature

    [SerializeField] Material mainGroundColor, bgMat;

    [SerializeField] SpriteRenderer[] soundIcons;

    [SerializeField] SoundManagerLogic mySoundManager;

    // Sounds: GameManager.cs, PlayerController.cs

    [SerializeField] AudioSource mainMenuMusic;
    [SerializeField] GameObject crownObj;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        levelFailed2 = false;

        restartLogic = false;

        fastTap = false;

        startTrackingScore = false;

        miniGameScore = 0;

        cheatOn = false;
        inMiniGame = true;

        Physics.gravity = new Vector3(0, 0, -50.2f);

        if (PlayerPrefs.GetInt("eggoEnabled", 0) == 0) // is off
        {
            crownObj.SetActive(false);
            cheatOn = false;
        }
        else if (PlayerPrefs.GetInt("eggoEnabled", 0) == 1) // is on
        {
            crownObj.SetActive(true);
            cheatOn = true;
        }

        currentLevel.text = "highscore: " + PlayerPrefs.GetInt("HighScore", 0) + "m";

        StartCoroutine(FadeAndChangeScore());
        SpawnLevel();
    }

    IEnumerator FadeAndChangeScore()
    {
        yield return new WaitForSecondsRealtime(3);
        StartCoroutine(FadeTextOut(currentLevel));
        yield return new WaitForSecondsRealtime(0.5f);
        currentLevel.text = miniGameScore + "m";

        StartCoroutine(FadeTextIn(currentLevel));

        yield return new WaitForSecondsRealtime(0.5f);
        startTrackingScore = true;
    }

    IEnumerator FadeOutAudio(AudioSource myAudio)
    {
        float timer = 0, totalTime = 24;
        float startingLevel = myAudio.volume;
        while (timer <= totalTime)
        {
            myAudio.volume = Mathf.Lerp(startingLevel, 0, timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    private void Start()
    {
        StartCoroutine(StartLogic());
    }

    private void Update()
    {
        if (cheatOn && PlayerPrefs.GetInt("eggoEnabled", 0) == 0) // turn on
        {
            crownObj.SetActive(true);
            PlayerPrefs.SetInt("eggoEnabled", 1);
        }
        else if (!cheatOn && PlayerPrefs.GetInt("eggoEnabled", 0) == 1) // turn off
        {
            crownObj.SetActive(false);
            PlayerPrefs.SetInt("eggoEnabled", 0);
        }

        if (!restartLogic && levelFailed2)
        {
            PlayerController.staticLost = true;
            mySoundManager.Play("LostJingle"); // out of bounds jingle

            Transform tempObj = retryTexts[Random.Range(0, retryTexts.Length)].transform;
            SpriteRenderer retryTitle, retryBg;
            retryTitle = tempObj.GetComponent<SpriteRenderer>();
            retryBg = tempObj.GetComponentsInChildren<SpriteRenderer>()[1];

            charAnim.SetTrigger("cry");
            Camera_Tracker.stopCameraTracking = true;

            charTrail.SetActive(false);
            charMesh.enabled = false;
            charOutline.enabled = false;

            // set highscore
            if (miniGameScore > PlayerPrefs.GetInt("HighScore", 0))
                PlayerPrefs.SetInt("HighScore", miniGameScore);

            StartCoroutine(FadeOutAudio(mainMenuMusic));

            StartCoroutine(RetryLiterature(retryTitle, retryBg));
            StartCoroutine(RestartWait());

            restartLogic = true;
        }

        if (!levelFailed2 && startTrackingScore)
        {
            // charTransform
            // base os -3.5f
            int baseScore = (int)Mathf.Max(0,(charTransform.transform.localPosition.z + 3.5f));
            if (baseScore > miniGameScore)
                miniGameScore = baseScore;

            currentLevel.text = miniGameScore + "m";
        }
    }

    void SpawnLevel()
    {

    }

    IEnumerator StartLogic()
    {
        whiteSquare.enabled = true;
        whiteSquare.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FadeImageOut(whiteSquare));
    }

    IEnumerator RetryLiterature(SpriteRenderer mainText, SpriteRenderer bgText)
    {
        float timer = 0, totalTime = 40;
        Color startingColor1 = mainText.color;
        Color startingColor2 = bgText.color;
        Transform textTransform = mainText.gameObject.transform.parent.transform;

        Vector3 startingScale = textTransform.localScale;

        while (timer <= totalTime)
        {
            if (timer <= 18)
                textTransform.localScale = Vector3.Lerp(startingScale * 0.1f, startingScale * 1.65f, timer / (totalTime - 18));

            if (timer < totalTime * 0.75f)
            {
                mainText.color = Color.Lerp(startingColor1, new Color(startingColor1.r, startingColor1.g, startingColor1.b, 1), timer / (totalTime * 0.7f));
                bgText.color = Color.Lerp(startingColor2, new Color(startingColor2.r, startingColor2.g, startingColor2.b, 1), timer / (totalTime * 0.7f));
            }

            yield return new WaitForFixedUpdate();
            timer++;
        }

        timer = 0;
        totalTime = 80;
        startingScale = textTransform.localScale;
        while (timer <= totalTime)
        {
            textTransform.localScale = Vector3.Lerp(startingScale, new Vector3(startingScale.x * 1.15f, startingScale.y * 1.5f, startingScale.z * 1.5f), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator RestartWait()
    {
        for (int i = 0; i < 35; i++)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (fastTap)
                break;
        }
        StartCoroutine(RestartLevel(whiteSquare));
    }

    IEnumerator RestartLevel(SpriteRenderer myImage)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }



    IEnumerator FadeImageOut(SpriteRenderer myImage)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 1), new Color(startingColor.r, startingColor.g, startingColor.b, 0), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
        myImage.enabled = false;
    }

    IEnumerator FadeImageIn(SpriteRenderer myImage, float totalTime)
    {
        float timer = 0;
        Color startingColor = myImage.color;
        myImage.enabled = true;
        while (timer <= totalTime)
        {
            myImage.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeTextOut(TextMeshPro myTtext)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myTtext.color;
        while (timer <= totalTime)
        {
            myTtext.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 1), new Color(startingColor.r, startingColor.g, startingColor.b, 0), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }

    IEnumerator FadeTextIn(TextMeshPro myTtext)
    {
        float timer = 0, totalTime = 24;
        Color startingColor = myTtext.color;
        while (timer <= totalTime)
        {
            myTtext.color = Color.Lerp(new Color(startingColor.r, startingColor.g, startingColor.b, 0), new Color(startingColor.r, startingColor.g, startingColor.b, 1), timer / totalTime);
            yield return new WaitForFixedUpdate();
            timer++;
        }
    }
}
