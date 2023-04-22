using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool levelStarted, levelFailed, levelPassed, fastTap, cheatOn;

    [SerializeField]
    Transform levelObjectsFolder;

    [SerializeField]
    Camera myCam;

    [SerializeField] Transform spawnFolder;

    [SerializeField] SpriteRenderer golfTitle, golfBG, instructionsTitle, instructionsBG, whiteSquare, levelPassedSR, levelPassedBG, parBG, parBGDropShadow;
    public static SpriteRenderer staticParBG;

    [SerializeField] Transform[] retryTexts, winTexts;

    bool restartLogic, startLogic, passedLogic;

    [SerializeField] TextMeshPro currentLevel, parText;
    public static TextMeshPro staticParText;

    public static Vector3 holeLocation;
    public static MeshCollider holCol;

    public static int par, startintPar;

    [SerializeField] SkinnedMeshRenderer charMesh;
    [SerializeField] GameObject charTrail;
    [SerializeField] Animator charAnim;

    [SerializeField] Outline charOutline;

    [SerializeField] GameObject[] startingTiles, middleTiles, endingTiles;
    [SerializeField] Transform spawnedObjectsFolder;

    // level spawner literature
    int spawnedTilesCount, tilesToSpawnCount, prevSpawnedTileInt, maxSpecialObjectsToSpawn, holeNum;
    Vector3 spawnPosition;
    float spawnRotation;
    int turnsInARow;
    int objToSpawnInt;

    [SerializeField] Material mainGroundColor, bgMat;
    [SerializeField] Color greenColor, redColor;

    [SerializeField] SpriteRenderer[] soundIcons;

    [SerializeField] SoundManagerLogic mySoundManager;

    // Sounds: GameManager.cs, PlayerController.cs

    [SerializeField] AudioSource mainMenuMusic;
    [SerializeField] GameObject crownObj;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        Physics.gravity = new Vector3(0, -50.2f, 0);

        levelStarted = false;
        levelFailed = false;
        levelPassed = false;

        restartLogic = false;
        startLogic = false;
        passedLogic = false;

        miniGameManager.inMiniGame = false;

        fastTap = false;

        cheatOn = false;

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

        staticParBG = parBG;

        currentLevel.text = "hole " + PlayerPrefs.GetInt("levelCount", 1);

        // level spawner literature
        spawnedTilesCount = 0;
        // determine tile spawn count
        holeNum = PlayerPrefs.GetInt("levelCount", 1) % 18;
        if (holeNum == 0)
            holeNum = 18;

        if (holeNum % 18 == 0 && PlayerPrefs.GetInt("ChangeColor", 0) == 0)
            PlayerPrefs.SetInt("ChangeColor", 1);

        // change bg color
        if (holeNum % 9 == 1)
        {
            if (holeNum % 18 == 1 && PlayerPrefs.GetInt("ChangeColor", 0) == 1)
            {
                PlayerPrefs.SetInt("mainBGInc", (PlayerPrefs.GetInt("mainBGInc", 0) + 1)%6);
                PlayerPrefs.SetInt("ChangeColor", 0);
            }
            
            int bgInc = PlayerPrefs.GetInt("mainBGInc", 0);
            Color groundColor = new Color(0.1455686f, 0.2509804f, 0.1525961f, 1); // green (course 1), matColor;
            Color matColor = new Color(0.2941176f, 0.4241176f, 0.7529412f, 1); // green (course 1)
            switch (bgInc)
            {
                case 0:
                    groundColor = new Color(0.1455686f, 0.2509804f, 0.1525961f, 1); // green (course 1)
                    matColor = new Color(0.2941176f, 0.4241176f, 0.7529412f, 1); // green (course 1)
                    break;
                case 1:
                    groundColor = new Color(0.145098f, 0.2439215f, 0.2509804f, 1); // green (course 2)
                    matColor = new Color(0.7029999f, 0.7029999f, 0.402819f, 1); // green (course 2)
                    break;
                case 2:
                    groundColor = new Color(0.2439216f, 0.2509804f, 0.145098f, 1); // green (course 3)
                    matColor = new Color(0.2941176f, 0.5082353f, 0.7529412f, 1); // green (course 3)
                    break;
                case 3:
                    groundColor = new Color(0.2509804f, 0.1521568f, 0.145098f, 1); // green (course 4)
                    matColor = new Color(0.28296f, 0.72f, 0.4675717f, 1); // green (course 4)
                    break;
                case 4:
                    groundColor = new Color(0.1521568f, 0.145098f, 0.2509804f, 1); // green (course 5)
                    matColor = new Color(0.6533928f, 0.676f, 0.4461599f, 1); // green (course 5)
                    break;
                case 5:
                    groundColor = new Color(0.2509804f, 0.145098f, 0.2439215f, 1); // green (course 6)
                    matColor = new Color(0.3007f, 0.62f, 0.4059238f, 1); // green (course 6)
                    break;
                default:
                    groundColor = new Color(0.1455686f, 0.2509804f, 0.1525961f, 1); // green (course 1)
                    matColor = new Color(0.2941176f, 0.4241176f, 0.7529412f, 1); // green (course 1)
                    break;
            }
            mainGroundColor.color = groundColor;
            bgMat.color = matColor;
        }

        switch (holeNum) 
        {
            case 0:
                mainGroundColor.color = redColor;
                tilesToSpawnCount = Random.Range(4, 7); // for a par of 4-5
                break;
            case 1:
                if (PlayerPrefs.GetInt("levelCount", 1) == 1)
                    tilesToSpawnCount = 2; // for a par of 2
                else
                    tilesToSpawnCount = Random.Range(5, 9); // for a par of 3-4
                break;
            case 2:
                if (PlayerPrefs.GetInt("levelCount", 1) == 2)
                    tilesToSpawnCount = 5; // for a par of 3
                else
                    tilesToSpawnCount = Random.Range(8, 11); // for a par of 4-5
                break;
            case 3:
                if (PlayerPrefs.GetInt("levelCount", 1) == 3)
                    tilesToSpawnCount = 4; // for a par of 3
                else
                    tilesToSpawnCount = Random.Range(5, 7); // for a par of 3
                break;
            case 4:
                tilesToSpawnCount = Random.Range(8, 10); // for a par of 4
                break;
            case 5:
                tilesToSpawnCount = Random.Range(8, 11); // for a par of 4-5
                break;
            case 6:
                tilesToSpawnCount = Random.Range(5, 7); // for a par of 3
                break;
            case 7:
                tilesToSpawnCount = Random.Range(6, 9); // for a par of 3-4
                break;
            case 8:
                tilesToSpawnCount = Random.Range(9, 11); // for a par of 5
                break;
            case 9:
                mainGroundColor.color = redColor;
                tilesToSpawnCount = Random.Range(3, 6); // for a par of 2-3
                break;
            case 10:
                tilesToSpawnCount = Random.Range(7, 9); // for a par of 4
                break;
            case 11:
                tilesToSpawnCount = Random.Range(4, 7); // for a par of 2-3
                break;
            case 12:
                tilesToSpawnCount = Random.Range(10, 11); // for a par of 5
                break;
            case 13:
                tilesToSpawnCount = Random.Range(7, 10); // for a par of 4
                break;
            case 14:
                tilesToSpawnCount = Random.Range(5, 8); // for a par of 3
                break;
            case 15:
                tilesToSpawnCount = Random.Range(8, 10); // for a par of 4
                break;
            case 16:
                tilesToSpawnCount = Random.Range(7, 11); // for a par of 4-5
                break;
            case 17:
                tilesToSpawnCount = Random.Range(8, 10); // for a par of 4
                break;
            case 18:
                mainGroundColor.color = redColor;
                tilesToSpawnCount = Random.Range(4, 7); // for a par of 4-5
                break;
            default:
                tilesToSpawnCount = Random.Range(3, 11);
                break;
        }

        // Set amount of special objects to spawn
        //// MAX 2 special objects per level
        maxSpecialObjectsToSpawn = Mathf.Min(3, Mathf.RoundToInt((float)tilesToSpawnCount/3.22f));

        prevSpawnedTileInt = 920;
        spawnPosition = Vector3.zero;
        spawnRotation = 0;
        turnsInARow = 0;

        // par literature
        startintPar =  Mathf.RoundToInt((float)tilesToSpawnCount / 2.85f) + 1;
        par = startintPar;
        staticParText = parText;
        staticParBG.color = new Color(1, 0.9468394f, 0.73f, 0);
        staticParText.color = new Color(1, 1, 1, 0);
        staticParText.text = "" + par;

        // Spawn Level
        if (PlayerPrefs.GetInt("SpawnNewLevel", 1) == 1)
            SpawnNewLevel();
        else
            RespawnLevel();

        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.SetInt("levelCount", 53);
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

        if (!levelFailed && par == 0 && ControlsLogic.isStable)
        {
            levelFailed = true;
        }

        if (!restartLogic && levelFailed && !passedLogic)
        {
            PlayerController.staticLost = true;
            mySoundManager.Play("LostJingle"); // out of bounds jingle

            Transform tempObj = retryTexts[Random.Range(0, retryTexts.Length)].transform;
            SpriteRenderer retryTitle, retryBg;
            retryTitle = tempObj.GetComponent<SpriteRenderer>();
            retryBg = tempObj.GetComponentsInChildren<SpriteRenderer>()[1];

            charAnim.SetTrigger("cry");
            Camera_Tracker.stopCameraTracking = true;

            if (par != 0)
            {
                StartCoroutine(FadeTextOut(staticParText));
                StartCoroutine(FadeImageOut(parBG));
                StartCoroutine(FadeImageOut(parBGDropShadow));
                charTrail.SetActive(false);
                charMesh.enabled = false;
                charOutline.enabled = false;
            }

            StartCoroutine(RetryLiterature(retryTitle, retryBg));
            StartCoroutine(RestartWait());            

            restartLogic = true;
        }

        if (!startLogic && levelStarted)
        {
            foreach (SpriteRenderer sprite in soundIcons)
            {
                StartCoroutine(FadeImageOut(sprite));
            }

            StartCoroutine(FadeOutAudio(mainMenuMusic));

            StartCoroutine(FadeImageIn(parBG, 24));
            StartCoroutine(FadeImageIn(parBGDropShadow, 24));
            StartCoroutine(FadeTextIn(staticParText));
            StartCoroutine(FadeImageOut(golfTitle));
            StartCoroutine(FadeImageOut(golfBG));
            StartCoroutine(FadeImageOut(instructionsTitle));
            StartCoroutine(FadeImageOut(instructionsBG));
            startLogic = true;
        }

        if (!passedLogic && levelPassed)
        {
            PlayerPrefs.SetInt("SpawnNewLevel", 1);
            PlayerPrefs.SetInt("TilesCount", 0);

            PlayerPrefs.SetInt("Tile_" + 0, 1);
            for (int i = 1; i < 10; i++)
                PlayerPrefs.SetInt("Tile_"+i, 4);
            PlayerPrefs.SetInt("Tile_" + 10, 1);

            // winTexts
            Transform tempObj = winTexts[4].transform;
            //startintPar, par
            // ace: hole in 1, albatross: 3 under par, eagle: 2 under par, birdie: 1 under par, par: par
            // startinpar - par = startin par -> par
            // scenario:
            // 2 - 1 = 1 -> hole in one, 2 - 0 = 2 -> par
            // 3 - 2 = 1 -> hole in one, 3 - 1 = 2 -> birdie, 3 - 0 = 3 -> par
            // 4 - 3 = 1 -> hole in one, 4 - 2 = 2 -> eagle, 4 - 1 = 3 -> birdie, 4 - 0 = 4 -> par
            // 5 - 4 = 1 -> hole in one, 5 - 3 = 2 -> albatross, 5 - 2 = 3 -> eagle, 5 - 1 = 4 -> birdie, 5 - 0 = 5 -> par
            switch (startintPar)
            {
                case 2:
                    switch (par)
                    {
                        case 0:
                            tempObj = winTexts[4].transform; // par
                            break;
                        case 1:
                            tempObj = winTexts[0].transform; // ace
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    switch (par)
                    {
                        case 0:
                            tempObj = winTexts[4].transform; // par
                            break;
                        case 1:
                            tempObj = winTexts[3].transform; // birdie
                            break;
                        case 2:
                            tempObj = winTexts[0].transform; // ace
                            break;
                        default:
                            break;
                    }
                    break;
                case 4:
                    switch (par)
                    {
                        case 0:
                            tempObj = winTexts[4].transform; // par
                            break;
                        case 1:
                            tempObj = winTexts[3].transform; // birdie
                            break;
                        case 2:
                            tempObj = winTexts[2].transform; // eagle
                            break;
                        case 3:
                            tempObj = winTexts[0].transform; // ace
                            break;
                        default:
                            break;
                    }
                    break;
                case 5:
                    switch (par)
                    {
                        case 0:
                            tempObj = winTexts[4].transform; // par
                            break;
                        case 1:
                            tempObj = winTexts[3].transform; // birdie
                            break;
                        case 2:
                            tempObj = winTexts[2].transform; // eagle
                            break;
                        case 3:
                            tempObj = winTexts[1].transform; // albatross
                            break;
                        case 4:
                            tempObj = winTexts[0].transform; // ace
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    Debug.Log("Error: " + startintPar);
                    break;
            }
            SpriteRenderer winTitle, winBG;
            winTitle = tempObj.GetComponent<SpriteRenderer>();
            winBG = tempObj.GetComponentsInChildren<SpriteRenderer>()[1];

            StartCoroutine(RetryLiterature(winTitle, winBG));

            PlayerPrefs.SetInt("levelCount", PlayerPrefs.GetInt("levelCount", 1) + 1); // increment
            StartCoroutine(RestartWait());

            charTrail.SetActive(false);
            charMesh.enabled = false;

            passedLogic = true;
        }
    }
    
    void RespawnLevel()
    {
        // spawn first tile
        SpawnStarterTile(true);
        spawnedTilesCount++;

        // spawn middle tiles
        for (int i = 1; i < PlayerPrefs.GetInt("TilesCount", 0)-1; i++)
        {
            SpawnMiddleTile(true);
            spawnedTilesCount++;
        }

        // spawn end tiles
        SpawnEndingTile(true);
        spawnedTilesCount++;
    }

    void SpawnNewLevel()
    {
        // spawn starter tile
        SpawnStarterTile(false);
        PlayerPrefs.SetInt("Tile_" + spawnedTilesCount, objToSpawnInt);
        spawnedTilesCount++;

        // spawn middle tiles
        while (spawnedTilesCount < tilesToSpawnCount-1)
        {
            SpawnMiddleTile(false);
            PlayerPrefs.SetInt("Tile_" + spawnedTilesCount, objToSpawnInt);
            spawnedTilesCount++;
        }

        // spawn ending tile
        SpawnEndingTile(false);
        PlayerPrefs.SetInt("Tile_" + spawnedTilesCount, objToSpawnInt);
        spawnedTilesCount++;
        PlayerPrefs.SetInt("TilesCount", spawnedTilesCount);
        PlayerPrefs.SetInt("SpawnNewLevel", 0);
    }

    void SpawnStarterTile(bool fromStorage)
    {
        // determine object to spawn
        if (holeNum == 9 || holeNum == 18)
            objToSpawnInt = 1;
        else
            objToSpawnInt = 0;

        if (fromStorage)
            objToSpawnInt = PlayerPrefs.GetInt("Tile_" + spawnedTilesCount, 0);

        // spawn object, set parent
        GameObject tempObj = Instantiate(startingTiles[objToSpawnInt], spawnedObjectsFolder);
        // set position
        tempObj.transform.localPosition = spawnPosition;
        // set rotation
        tempObj.transform.GetChild(0).localEulerAngles = new Vector3(0, spawnRotation, 0);
        tempObj.transform.GetChild(0).localEulerAngles = new Vector3(0, spawnRotation, 0);
        // move spawn position
        spawnPosition += new Vector3(-4, 0, 0);
    }

    void SpawnMiddleTile(bool fromStorage)
    {
        // determine object to spawn
        //// ensure no wall (on 9 & 18) and single wall (5 & 12) obstacles only spawn on rare ocasions
        objToSpawnInt = 4;
        if (holeNum == 9 || holeNum == 18) // no walls
        {
            if (maxSpecialObjectsToSpawn > 0)
            {
                objToSpawnInt = Random.Range(0, (middleTiles.Length - 1) / 3 + 1);
                if (objToSpawnInt == 0)
                    objToSpawnInt = 1; // curve no wall
                else if (objToSpawnInt == 1)
                    objToSpawnInt = 3; // inverted curve no wall
                else if (objToSpawnInt == 2)
                    objToSpawnInt = 6; // straight_no_wall
                else if (objToSpawnInt == 3)
                {
                    objToSpawnInt = 9; // ramp_down_no_wall
                    maxSpecialObjectsToSpawn--;
                    if (spawnPosition.y < 1)
                    {
                        int tempInt = Random.Range(0, 2);
                        if (tempInt == 0)
                        {
                            if (Random.Range(0, 2) == 0)
                                objToSpawnInt = 1; // curve no wall
                            else
                                objToSpawnInt = 3; // inverted curve no wall
                        }
                        else
                            objToSpawnInt = 6; // straight_no_wall
                        maxSpecialObjectsToSpawn++;
                    }
                }
                else if (objToSpawnInt == 4)
                {
                    objToSpawnInt = 12; // ramp_no_wall
                    maxSpecialObjectsToSpawn--;
                }
                else // special object to spawn
                {
                    objToSpawnInt = Mathf.Min(middleTiles.Length - 1, objToSpawnInt * 3);
                    maxSpecialObjectsToSpawn--;
                }
            }
            else
            {
                objToSpawnInt = Random.Range(0, 3);
                if (objToSpawnInt == 0)
                    objToSpawnInt = 1; // curve no wall
                else if (objToSpawnInt == 1)
                    objToSpawnInt = 3; // inverted curve no wall
                else if (objToSpawnInt == 2)
                    objToSpawnInt = 6; // straight_no_wall
            }
        }
        else if (holeNum == 5 || holeNum == 12) // single wall
        {
            if (maxSpecialObjectsToSpawn > 0)
            {
                objToSpawnInt = Random.Range(0, (middleTiles.Length - 1) / 3 + 1);
                if (objToSpawnInt == 0)
                    objToSpawnInt = 0; // curve wall
                else if (objToSpawnInt == 1)
                    objToSpawnInt = 2; // inverted curve wall
                else if (objToSpawnInt == 2)
                    objToSpawnInt = 5; // straight_one_wall
                else if (objToSpawnInt == 3)
                {
                    objToSpawnInt = 8; // ramp_down__one_wall
                    maxSpecialObjectsToSpawn--;
                    if (spawnPosition.y < 1)
                    {
                        int tempInt = Random.Range(0, 2);
                        if (tempInt == 0)
                        {
                            if (Random.Range(0, 2) == 0)
                                objToSpawnInt = 0; // curve wall
                            else
                                objToSpawnInt = 2; // inverted curve wall
                        }
                        else
                            objToSpawnInt = 5; // straight_one_wall
                        maxSpecialObjectsToSpawn++;
                    }
                }
                else if (objToSpawnInt == 4)
                {
                    objToSpawnInt = 11; // ramp_one_wall
                    maxSpecialObjectsToSpawn--;
                }
                else // special object to spawn
                {
                    objToSpawnInt = Mathf.Min(middleTiles.Length - 1, objToSpawnInt * 3) - 1;
                    maxSpecialObjectsToSpawn--;
                }
            }
            else
            {
                objToSpawnInt = Random.Range(0, 3);
                if (objToSpawnInt == 0)
                    objToSpawnInt = 0; // curve wall
                else if (objToSpawnInt == 1)
                    objToSpawnInt = 2; // inverted curve wall
                else if (objToSpawnInt == 2)
                    objToSpawnInt = 5; // straight_one_wall
            }
        }
        else // walls
        {
            if (maxSpecialObjectsToSpawn > 0)
            {
                objToSpawnInt = Random.Range(0, (middleTiles.Length-1)/3+1); // 12
                if (objToSpawnInt == 0)
                    objToSpawnInt = 0; // curve wall
                else if (objToSpawnInt == 1)
                    objToSpawnInt = 2; // inverted curve wall
                else if (objToSpawnInt == 2)
                    objToSpawnInt = 4; // straight_wall
                else if (objToSpawnInt == 3)
                {
                    objToSpawnInt = 7; // ramp_down_wall
                    maxSpecialObjectsToSpawn--;
                    if (spawnPosition.y < 1)
                    {
                        int tempInt = Random.Range(0, 2);
                        if (tempInt == 0)
                        {
                            if (Random.Range(0, 2) == 0)
                                objToSpawnInt = 0; // curve wall
                            else
                                objToSpawnInt = 2; // inverted curve wall
                        }
                        else
                            objToSpawnInt = 4; // straight_wall
                        maxSpecialObjectsToSpawn++;
                    }
                }
                else if (objToSpawnInt == 4)
                {
                    objToSpawnInt = 10; // ramp_wall
                    maxSpecialObjectsToSpawn--;
                }
                else // special object to spawn
                {
                    objToSpawnInt = Mathf.Min(middleTiles.Length - 1, objToSpawnInt * 3)-2;
                    maxSpecialObjectsToSpawn--;
                }
            }
            else
            {
                objToSpawnInt = Random.Range(0, 3);
                if (objToSpawnInt == 0)
                    objToSpawnInt = 0; // curve wall
                else if (objToSpawnInt == 1)
                    objToSpawnInt = 2; // inverted curve wall
                else if (objToSpawnInt == 2)
                    objToSpawnInt = 4; // straight_wall
            }
        }

        /// ENSURE MAX OF TWO SAME KIND TURNS IN A ROW
        if (objToSpawnInt == 0 || objToSpawnInt == 1 || objToSpawnInt == 2 || objToSpawnInt == 3)
            turnsInARow++;
        if (Mathf.Abs(turnsInARow) >= 3)
        {
            if (holeNum == 9 || holeNum == 18) // no walls
                objToSpawnInt = 6;
            else if(holeNum == 5 || holeNum == 12) // single wall
                objToSpawnInt = 5;
            else
                objToSpawnInt = 4;
        }

        // no down ramp if at 0 height (redunancy check)
        while (objToSpawnInt >= 7 && objToSpawnInt <= 9 && spawnPosition.y < 1)
            objToSpawnInt = Random.Range(0, middleTiles.Length);

        if (fromStorage)
            objToSpawnInt = PlayerPrefs.GetInt("Tile_" + spawnedTilesCount, 0);
        prevSpawnedTileInt = objToSpawnInt;
        // spawn object, set parent
        GameObject tempObj = Instantiate(middleTiles[objToSpawnInt], spawnedObjectsFolder);
        // set position
        tempObj.transform.localPosition = spawnPosition;
        // Change rotation? (only if object is a curve)
        if (objToSpawnInt <= 1) // right turn
        {
            spawnRotation += 90;
            if (spawnRotation >= 360) // normalize
                spawnRotation -= 360;
        }
        else if (objToSpawnInt <= 3) // left turn
        {
            spawnRotation -= 90;
            if (spawnRotation < 0) // normalize
                spawnRotation += 360;
        }
        // set rotation
        tempObj.transform.GetChild(0).localEulerAngles = new Vector3(0, spawnRotation, 0);

        // move spawn position (planer)
        switch ((int)spawnRotation) // 0 = North, 1 = East, 2 = South, 3 = West
        {
            case 0:
                spawnPosition += new Vector3(-4, 0, 0);
                break;
            case 90:
                spawnPosition += new Vector3(0, 0, 4);
                break;
            case 180:
                spawnPosition += new Vector3(4, 0, 0);
                break;
            case 270:
                spawnPosition += new Vector3(0, 0, -4);
                break;
            default:
                Debug.Log("Error: " + spawnRotation);
                break;
        }
        // move spawn position height (if ramp spawned)
        if (objToSpawnInt >= 7 && objToSpawnInt <= 9) // down ramp
        {
            spawnPosition += new Vector3(0, -1, 0);
        }
        else if (objToSpawnInt >= 10 && objToSpawnInt <= 12) // up ramp
        {
            spawnPosition += new Vector3(0, 1, 0);
        }
    }

    void SpawnEndingTile(bool fromStorage)
    {
        // determine object to spawn
        if (holeNum == 9 || holeNum == 18)
            objToSpawnInt = 1;
        else
            objToSpawnInt = 0;

        if (fromStorage)
            objToSpawnInt = PlayerPrefs.GetInt("Tile_" + spawnedTilesCount, 0);

        // spawn object and set parent
        GameObject tempObj = Instantiate(endingTiles[objToSpawnInt], spawnedObjectsFolder);
        // set position
        tempObj.transform.localPosition = spawnPosition;
        // set rotation
        tempObj.transform.GetChild(0).localEulerAngles = new Vector3(0, spawnRotation, 0); // spawnRotation
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
                textTransform.localScale = Vector3.Lerp(startingScale*0.1f, startingScale * 1.65f, timer / (totalTime-18));

            if (timer < totalTime * 0.75f)
            {
                mainText.color = Color.Lerp(startingColor1, new Color(startingColor1.r, startingColor1.g, startingColor1.b, 1), timer / (totalTime*0.7f));
                bgText.color = Color.Lerp(startingColor2, new Color(startingColor2.r, startingColor2.g, startingColor2.b, 1), timer / (totalTime*0.7f));
            }

            yield return new WaitForFixedUpdate();
            timer++;
        }

        timer = 0;
        totalTime = 80;
        startingScale = textTransform.localScale;
        while (timer <= totalTime)
        {
            textTransform.localScale = Vector3.Lerp(startingScale, new Vector3(startingScale.x*1.15f, startingScale.y*1.5f, startingScale.z*1.5f), timer / totalTime);
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

        if (levelPassed && PlayerPrefs.GetInt("levelCount", 1) % 9 == 1 && PlayerPrefs.GetInt("levelCount", 1) > 2)
            SceneManager.LoadScene(1, LoadSceneMode.Single); // minigame
        else
            SceneManager.LoadScene(0, LoadSceneMode.Single); // main game
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
