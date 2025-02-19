using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * Ostensibly controls the UI, but in reality has sorta become the GameController too
 * <And to think I wrote this comment before implementing high scores...>
 */
public class UIController : MonoBehaviour
{
    private int UIMode;
    private GameObject statusParent;
    private GameObject dialogueParent;
    private GameObject scoreParent;
    private GameObject camera;

    //Score variables
    private long score;
    private Text scoreText;

    //State variables
    private int run;
    private bool firstRun;
    private int lastUnlock;

    //Player Health Variables
    private PlayerController player;
    private float playerHealthMaxWidth;
    private Transform playerHealthBarFill;
    private Transform playerHealthBarDrain;
    private float drain;
    private const float DEFAULT_WIDTH = 188.23f;

    //Boss Health Variables
    private float bossHealthMaxWidth;
    private float bossMaxHealth;
    private Transform bossHealthBarFill;
    private Transform bossHealthBarOutline;
    private Text bossNameText;

    //Dialogue Variables
    private Text dialogueText;
    private string currentChunk;
    private int wordDisplayRate;
    private int charLimit;
    private bool isWriting;
    private bool skipText;
    private bool buttonPressed;
    private bool transitionPrimed;
    private string transitionScene;

    //High Score Variables
    private int cursorPosition;
    private int cursorCooldown;
    private char[] name;
    private Text scoreTitle;
    private Text scoreField;
    private Text scoreEntry;
    private Text scorePreview;
    private Text upCaret;
    private Text downCaret;
    private Text leftCaret;
    private Text rightCaret;
    private bool scoreEntered;
    private float caretDelta;
    private HighScoreList.HighScore currentScore;
    private int scorePosition;
    private HighScoreList highScores;

    //Transparency Variables
    private bool collisionTransparency;
    private bool utilityTransparency;
    private RawImage uiTexture;

    // Specials Variables
    private Transform specialCover;

    // Music Variables
    private GameObject musicController;


    // Start is called before the first frame update
    void Start()
    {
        //Get components
        PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
        if (players.Length == 0)
        {
            UIMode = 99;
            return;
        }
        else
        {
            player = players[0];
        }
        statusParent = GameObject.FindGameObjectsWithTag("UIStatus")[0];
        dialogueParent = GameObject.FindGameObjectsWithTag("UITextBox")[0];
        scoreParent = GameObject.FindGameObjectsWithTag("UIScore")[0];
        camera = GameObject.FindGameObjectsWithTag("MainCamera")[0];

        // Music and Sound
        musicController = GameObject.Find("MusicController");

        //Player Health
        playerHealthBarFill = statusParent.transform.Find("PlayerHealthBarRed");
        playerHealthMaxWidth = playerHealthBarFill.GetComponent<RectTransform>().rect.width;
        playerHealthBarDrain = playerHealthBarFill.Find("PlayerHealthBarPink");
        playerHealthBarDrain.localScale = new Vector3(0.0f, 1.0f, 1.0f);
        drain = 0.0f;

        //Boss Health
        bossHealthBarFill = statusParent.transform.Find("BossHealthBarRed");
        bossHealthBarOutline = statusParent.transform.Find("BossHealthBar");
        bossMaxHealth = 1.0f;
        bossHealthMaxWidth = bossHealthBarFill.GetComponent<RectTransform>().rect.width;
        bossNameText = statusParent.transform.Find("BossNameText").GetComponent<Text>();
        bossHealthBarFill.gameObject.SetActive(false);
        bossHealthBarOutline.gameObject.SetActive(false);
        bossNameText.transform.gameObject.SetActive(false);

        //Dialogue
        dialogueText = dialogueParent.transform.Find("DialogueText").GetComponent<Text>();
        currentChunk = "Invalid Message";//Going to use | character as a page break
        wordDisplayRate = 1;
        charLimit = 45;
        isWriting = false;
        skipText = false;
        buttonPressed = false;
        dialogueParent.SetActive(false);
        transitionPrimed = false;

        //Score (as status)
        score = 0;
        scoreText = statusParent.transform.Find("ScoreText").GetComponent<Text>();
        scoreText.text = "SCORE: 000000000000000";

        //Score (for record keeping)
        scoreTitle = scoreParent.transform.Find("ScoreTitle").GetComponent<Text>();
        scoreField = scoreParent.transform.Find("ScoreField").GetComponent<Text>();
        scoreEntry = scoreParent.transform.Find("ScoreEntry").GetComponent<Text>();
        scorePreview = scoreParent.transform.Find("ScoreEntry").Find("ScorePreview").GetComponent<Text>();
        upCaret = scoreParent.transform.Find("ScoreEntry").Find("UpCaret").GetComponent<Text>();
        downCaret = scoreParent.transform.Find("ScoreEntry").Find("DownCaret").GetComponent<Text>();
        leftCaret = scoreParent.transform.Find("ScoreEntry").Find("LeftCaret").GetComponent<Text>();
        rightCaret = scoreParent.transform.Find("ScoreEntry").Find("RightCaret").GetComponent<Text>();
        scoreEntered = false;
        cursorPosition = 0;
        name = new char[12] { 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A' };
        cursorCooldown = 0;
        caretDelta = 52f;
        currentScore = null;
        scorePosition = 0;
        highScores = null;

        scoreParent.SetActive(false);

        collisionTransparency = false;
        utilityTransparency = true;
        uiTexture = GameObject.FindGameObjectsWithTag("UITexture")[0].GetComponent<RawImage>();

        specialCover = statusParent.transform.Find("SpecialCover");
        specialCover.localScale = new Vector3(1.0f, 0.0f, 1.0f);

        //State
        run = 0;
        firstRun = true;
        lastUnlock = 0;

        UIMode = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // This mode is used to load the UIController without it doing anything so we can use the state functions for the CreditsController
        if (UIMode == 99)
        {
            return;
        }

        if (UIMode == 0)
        {
            float alpha = 0.5f + (collisionTransparency ? 0.0f : 0.25f) + (utilityTransparency ? 0.0f : 0.25f);
            uiTexture.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        }
        else
        {
            uiTexture.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        //UIMode == 0 is normal gameplay triggered by data pushed here from the other controllers
        if (UIMode == 1)
        {
            //If is writing, skip the writing and let the player skip
            if (isWriting)
            {
                if (Input.GetAxis("Fire1") > 0 && !buttonPressed)
                {
                    skipText = true;
                    buttonPressed = true;
                }
                else if (Input.GetAxis("Fire1") == 0)
                {
                    buttonPressed = false;
                }
            }
            //If done writing, go to the next chunk or close dialogue
            else
            {
                if (Input.GetAxis("Fire1") > 0 && !buttonPressed)
                {
                    //Done all text
                    if (currentChunk.Length == 0)
                    {
                        dialogueParent.SetActive(false);
                        UIMode = 0;
                        camera.SendMessage("TextOver");
                    }
                    //Do next chunk
                    else
                    {
                        dialogueText.text = "";
                        StartCoroutine(ChunkRoutine());
                        buttonPressed = true;
                    }
                }
                else if (Input.GetAxis("Fire1") == 0)
                {
                    buttonPressed = false;
                }
            }
        }
        //UIMode == 2 has nothing in the update loop and is controlled by CameraRoutine()
        
        //Transition is primed in UIMode 1, once it goes to 0 instantly switch to 2 and prepare to change scenes
        if (transitionPrimed &&UIMode == 0)
        {
            UIMode = 2;
            StartCoroutine(TransitionRoutine());
        }
    }

    void FixedUpdate()
    {
        this.collisionTransparency = false;
        transform.position = camera.transform.position + new Vector3(-5.77f, 3.9f, 110.0f);

        //I want this in fixed update to guarantee the cursor can't spin too fast when selecting letters
        if (UIMode == 3)
        {
            if (!scoreEntered)
            {
                if (Input.GetAxis("Vertical") > 0 && cursorCooldown == 0)
                {
                    int val = (int)name[cursorPosition];
                    val = CharacterMapping(++val);
                    name[cursorPosition] = (char)val;
                    cursorCooldown = 8;
                }
                if (Input.GetAxis("Vertical") < 0 && cursorCooldown == 0)
                {
                    int val = (int)name[cursorPosition];
                    val = CharacterMapping(--val);
                    name[cursorPosition] = (char)val;
                    cursorCooldown = 8;
                }
                if (Input.GetAxis("Horizontal") > 0 && !buttonPressed)
                {
                    cursorPosition++;
                    if (cursorPosition > 11)
                    {
                        cursorPosition = 11;
                    }
                    else
                    {
                        upCaret.GetComponent<RectTransform>().anchoredPosition = new Vector2(caretDelta, 0.0f) + upCaret.GetComponent<RectTransform>().anchoredPosition;
                        downCaret.GetComponent<RectTransform>().anchoredPosition = new Vector2(caretDelta, 0.0f) + downCaret.GetComponent<RectTransform>().anchoredPosition;
                    }
                    buttonPressed = true;
                }
                if (Input.GetAxis("Horizontal") < 0 && !buttonPressed)
                {
                    cursorPosition--;
                    if (cursorPosition < 0)
                    {
                        cursorPosition = 0;
                    }
                    else
                    {
                        upCaret.GetComponent<RectTransform>().anchoredPosition = new Vector2(-caretDelta, 0.0f) + upCaret.GetComponent<RectTransform>().anchoredPosition;
                        downCaret.GetComponent<RectTransform>().anchoredPosition = new Vector2(-caretDelta, 0.0f) + downCaret.GetComponent<RectTransform>().anchoredPosition;
                    }
                    buttonPressed = true;
                }

                if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)
                {
                    buttonPressed = false;
                    cursorCooldown = 0;
                }
                if (cursorCooldown > 0)
                {
                    cursorCooldown--;
                }


                if (cursorPosition == 11)
                {
                    rightCaret.gameObject.SetActive(false);
                }
                else
                {
                    rightCaret.gameObject.SetActive(true);
                }
                if (cursorPosition == 0)
                {
                    leftCaret.gameObject.SetActive(false);
                }
                else
                {
                    leftCaret.gameObject.SetActive(true);
                }

                scoreEntry.text = new string(name);



                if (Input.GetAxis("Fire1") > 0)
                {
                    scoreEntered = true;
                    cursorCooldown = 50;
                    currentScore = new HighScoreList.HighScore(score, new string(name), false);
                    highScores = LoadHighScores();
                    highScores.scores.Add(currentScore);

                    //Save it before displaying so that we can mess with the list now

                    //Remove hidden scores if the player hasn't beaten Mitchell yet
                    if (highScores.hideScores && currentScore.score < 999999999L)
                    {
                        highScores.scores.RemoveAll(x => x.hidden);
                        //But re add Mitchell back in if they beat Shadow Greyhound
                        if (currentScore.score > 17061998L)
                        {
                            highScores.scores.Add(new HighScoreList.HighScore(999999999L, "The Creator ", false));
                        }
                    }
                    //Sort list desc
                    highScores.scores.Sort();
                    scorePosition = highScores.scores.FindIndex(x => x.CompareTo(currentScore) == 0);

                    //Reload scores and decide whether or not to save this new score (Only allow 99 [displayable scores] to be saved)
                    if (scorePosition < 99)
                    {
                        HighScoreList savedScores = LoadHighScores();
                        savedScores.scores.Add(currentScore);
                        savedScores.scores.Sort();
                        if (scorePosition == 0)
                        {
                            savedScores.hideScores = false;
                        }
                        SaveHighScores(savedScores);
                    }

                    cursorPosition = 150;

                    scorePreview.gameObject.SetActive(false);
                    scoreEntry.gameObject.SetActive(false);
                    scoreField.gameObject.SetActive(true);
                }
            }
            else
            {
                cursorPosition--;
                if (cursorPosition == 0)
                {
                    cursorPosition = 150;
                }

                int scoresAbove = 14;
                if (scorePosition < 14)
                {
                    scoresAbove = scorePosition;
                }
                else if (scorePosition > 98)
                {
                    //Need to flag them for embarassment
                    scoresAbove = -1;
                }

                int startIndex = scorePosition - scoresAbove;
                // If didn't get in the top 99, show the last 13, then ... then the player
                if (scoresAbove < 0)
                {
                    startIndex = 86;
                }

                scoreField.text = "";
                for (int i = startIndex; i < startIndex + 15; i++)
                {
                    //If no more high scores get out of here!
                    if (i == highScores.scores.Count)
                    {
                        break;
                    }

                    if (i < 99)
                    {
                        scoreField.text += ((i == scorePosition && cursorPosition < 75) ? " " : "") + (i < 9 ? "0" : "") + (i + 1) + "> " + highScores.scores[i].ToString() + "\n";
                    }
                    else if (i == 99)
                    {
                        scoreField.text += "              ...              \n";
                    }
                    else
                    {
                        scoreField.text += (cursorPosition < 75 ? " " : "") + "XX> " + currentScore.ToString() + "\n";
                    }
                }


                //After 1 second accept a punch to return to menu
                if (cursorCooldown > 0)
                {
                    cursorCooldown--;
                }
                if (cursorCooldown == 0 && Input.GetAxis("Fire1") > 0)
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    SceneManager.LoadScene("Menu");
                    SceneManager.UnloadSceneAsync(sceneName);
                }
            }
        }
    }

    private int CharacterMapping(int val)
    {
        //For safety move this so I don't overwrite the switched variable
        int storeVal = val;
        switch(storeVal)
        {
            //UP
            case 91: { val = 97; break; }
            case 123: { val = 32; break; }
            case 33: { val = 65; break; }
            //DOWN
            case 64: { val = 32; break; }
            case 96: { val = 90; break; }
            case 31: { val = 122; break; }
            //DEFAULT TO YOURSELF
            default: { val = val; break; }
        }

        return val;
    }

    public void UpdateScore(long points)
    {
        score += points;
        string newText = "SCORE: ";
        string number = score.ToString();

        //Prefix it with 0s
        if (number.Length < 14)
        {
            int difference = 14 - number.Length;
            number = new string('0', difference) + number;
        }

        // Need this check for the credits to be able to load the current score without the other UI elements present
        if (UIMode != 99)
        {
            scoreText.text = newText + number;
        }
    }

    public void UpdateSpecial(int special)
    {
        if(special > 0)
        {
            statusParent.transform.Find("SpecialsBox").Find("SpecialIcon").gameObject.SetActive(true);
        }
        switch (special)
        {
            default:
            {
                statusParent.transform.Find("SpecialsBox").Find("SpecialIcon").gameObject.SetActive(false);
                break;
            }
            case 1:
            {
                statusParent.transform.Find("SpecialsBox").Find("SpecialIcon").GetComponent<Image>().sprite = Resources.Load<Sprite>("AxeIcon");
                break;
            }
            case 2:
            {
                statusParent.transform.Find("SpecialsBox").Find("SpecialIcon").GetComponent<Image>().sprite = Resources.Load<Sprite>("JumpIcon");
                break;
            }
            case 3:
            {
                statusParent.transform.Find("SpecialsBox").Find("SpecialIcon").GetComponent<Image>().sprite = Resources.Load<Sprite>("HaloIcon");
                break;
            }
        }

    }

    public void PlayerHealthBar(int health)
    {
        //Change health bar size as player health changes
        float playerHealthScale = ((float) health) / 20.0f;
        float diff = 0.0f;
        bool doHitStun = false;
        bool doHealStun = false;
        if (playerHealthScale < playerHealthBarFill.localScale.x && playerHealthBarFill.GetComponent<RectTransform>().sizeDelta.x > 0.0f)
        {
            doHitStun = true;
            diff = playerHealthBarFill.localScale.x - playerHealthScale;
        }
        else if (playerHealthScale > playerHealthBarFill.localScale.x)
        {
            doHealStun = true;
            diff = playerHealthScale - playerHealthBarFill.localScale.x;
        }
        else if (playerHealthBarFill.GetComponent<RectTransform>().sizeDelta.x <= 0.0f)
        {
            doHealStun = true;
            diff = playerHealthScale;
        }

        if (playerHealthScale < 0.0f)
        {
            playerHealthScale = 0.0f;
        }
        else if (playerHealthScale > 1.0f)
        {
            playerHealthScale = 1.0f;
        }

        if (playerHealthBarFill.GetComponent<RectTransform>().sizeDelta.x > 0.0f)
        {
            this.drain = playerHealthBarFill.localScale.x - playerHealthScale;
        }
        else
        {
            this.drain = -playerHealthScale;
        }
        
        // If 0 need to scale back up to support the drain child object having a scale
        if (playerHealthScale <= 0.0f)
        {
            RectTransform rect = playerHealthBarFill.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.0f, rect.sizeDelta.y);
            playerHealthBarFill.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        else
        {
            RectTransform rect = playerHealthBarFill.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(DEFAULT_WIDTH, rect.sizeDelta.y);
            playerHealthBarFill.localScale = new Vector3(playerHealthScale, 1.0f, 1.0f);
        }

        playerHealthBarDrain.localScale = new Vector3(this.drain * (1.0f / playerHealthBarFill.localScale.x), 1.0f, 1.0f);

        // Calls afterward to make sure all values calculated properly
        if (doHitStun)
        {
            StartCoroutine(HitstunRoutine(diff));
        }
        else if (doHealStun)
        {
            // invert the drain to be positive
            this.drain = -this.drain;
            StartCoroutine(HealStunRoutine(diff));
        }
    }

    public void PlayerHealthBarNoStun(int health)
    {
        //Change health bar size as player health changes
        float playerHealthScale = ((float)health) / 20.0f;
        if (playerHealthScale < 0.0f)
        {
            playerHealthScale = 0.0f;
        }
        playerHealthBarFill.localScale = new Vector3(playerHealthScale, 1.0f, 1.0f);
    }

    public void SetPlayerInvincible(bool vince)
    {
        if (vince)
        {
            player.MakeInvincible();
        }
        else
        {
            player.MakeVincible();
        }
    }

    public void BossHealthBar(int health)
    {
        //Change health bar size as Boss health changes
        float bossHealthScale = ((float)health) / bossMaxHealth;
        if ( bossHealthScale < 0.0f)
        {
            bossHealthScale = 0.0f;
        }
        bossHealthBarFill.localScale = new Vector3(bossHealthScale, 1.0f, 1.0f);

        if (health < (bossMaxHealth / 4))
        {
            if (musicController != null)
            {
                musicController.SendMessage("BossHalfway");
            }
        }
    }

    //Sets up a Boss entrance
    public void BossEntrance(float health, string name)
    {
        //Activate Boss Health bar with all variables required (width/max health/name)
        bossMaxHealth = health;
        bossNameText.text = name;
        bossNameText.transform.gameObject.SetActive(true);
        bossHealthBarOutline.gameObject.SetActive(true);
        bossHealthBarFill.gameObject.SetActive(true);
        bossHealthBarFill.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        if (musicController != null)
        {
            musicController.SendMessage("BossAppear");
        }
    }

    public void BossExit()
    {
        //Deactivate Boss Health bar
        bossNameText.transform.gameObject.SetActive(false);
        bossHealthBarOutline.gameObject.SetActive(false);
        bossHealthBarFill.gameObject.SetActive(false);
    }

    public void DisplayDialogue(string pictureName, string fullText)
    {
        UIMode = 1;
        dialogueParent.transform.Find("CharacterHeadshot").GetComponent<Image>().sprite = Resources.Load<Sprite>(pictureName);
        dialogueText.text = "";
        currentChunk = fullText;
        dialogueParent.SetActive(true);
        StartCoroutine(ChunkRoutine());

        camera.SendMessage("TextStart");
    }

    private IEnumerator ChunkRoutine()
    {
        //Add a few characters every frame to the displayed text
        isWriting = true;
        while(dialogueText.text.Length < charLimit)
        {
            int remainder = charLimit - dialogueText.text.Length;
            //If this is the last chunk, just add it without any calculations
            if (currentChunk.Length <= wordDisplayRate)
            {
                dialogueText.text = dialogueText.text + currentChunk + new string(' ', remainder + wordDisplayRate);
                currentChunk = "";
            }
            else if (skipText)
            {
                //Get until charLimit
                string chunk = currentChunk.Substring(0, currentChunk.Length > charLimit ? remainder : currentChunk.Length);
                if (chunk.Contains("|"))
                {
                    //If page break inside current chunk, stop chunk at page break and fill chunk with spaces to reach charLimit
                    chunk = currentChunk.Substring(0, currentChunk.IndexOf("|")) + new string(' ', remainder + wordDisplayRate);
                    //Start next chunk beyond the page break character
                    currentChunk = currentChunk.Substring(currentChunk.IndexOf("|") + 1);
                }
                else
                {
                    currentChunk = currentChunk.Substring(currentChunk.Length > charLimit ? remainder : currentChunk.Length);
                }
                dialogueText.text = dialogueText.text + chunk;
                skipText = false;
            }
            else
            {
                //Get next chunk
                string chunk = currentChunk.Substring(0, wordDisplayRate);
                if (chunk.Contains("|"))
                {
                    //If page break inside current chunk, stop chunk at page break and fill chunk with spaces to reach charLimit
                    chunk = currentChunk.Substring(0, currentChunk.IndexOf("|")) + new string(' ', remainder + wordDisplayRate);
                    //Start next chunk beyond the page break character
                    currentChunk = currentChunk.Substring(currentChunk.IndexOf("|") + 1);
                }
                else
                {
                    currentChunk = currentChunk.Substring(wordDisplayRate);
                }
                dialogueText.text = dialogueText.text + chunk;
            }

            //Wait for fixed update to make it slower like the old days, otherwise this would be lightning quick!
            yield return new WaitForFixedUpdate();
        }
        isWriting = false;
    }

    public void PanCamera()
    {
        UIMode = 2;
        StartCoroutine(CameraRoutine());
    }

    public void PanCameraReverse()
    {
        UIMode = 2;
        StartCoroutine(CameraReverseRoutine());
    }

    //Allows other controllers to force UIMode 2 so they can create their own custom cutscenes
    public void StartManualCutscene()
    {
        UIMode = 2;
    }

    public void EndManualCutscene()
    {
        UIMode = 0;
    }

    private IEnumerator CameraRoutine()
    {
        for (int i = 0; i < 50; i++)
        {
            camera.transform.position = camera.transform.position + new Vector3(0.072f, 0.0f, 0.0f);
            camera.gameObject.SendMessage("ForceUpdate", 0.07f);
            yield return new WaitForFixedUpdate();
        }
        UIMode = 0;
    }

    private IEnumerator CameraReverseRoutine()
    {
        for (int i = 0; i < 100; i++)
        {
            camera.transform.position = camera.transform.position + new Vector3(-0.072f, 0.0f, 0.0f);
            camera.gameObject.SendMessage("ForceUpdate", -0.07f);
            yield return new WaitForFixedUpdate();
        }
        UIMode = 0;
    }

    //Tells if the player and enemies are allowed to move
    public bool GameActive()
    {
        return UIMode == 0;
    }

    public long GetScore()
    {
        return this.score;
    }


    //File stuff
    /**
     * Call at the end of a level to store state
     */
    public void SaveGameState(bool incrementRun, int specialUnlock)
    {
        if (incrementRun)
        {
            firstRun = false;
            run++;
        }

        GameState state = new GameState(score, Mathf.Min(player.GetHealth() + 5, 20), 
            specialUnlock > player.GetSpecialUnlocked() ? specialUnlock : player.GetSpecial(), 
            specialUnlock > lastUnlock ? specialUnlock : lastUnlock, 
            run, firstRun);
        //Serialize state
        string path = Application.persistentDataPath + "/state.tmp"; //AppData/LocalLow
        FileStream file;
        if (File.Exists(path))
        {
            file = File.OpenWrite(path);
        }
        else
        {
            file = File.Create(path);
        }

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, state);
        file.Close();
    }

    /**
     * Create new game state from scratch
     */
    public void CreateGameState()
    {
        GameState state = new GameState(0L, 20, 0, 0, 0, true);
        //Serialize state
        string path = Application.persistentDataPath + "/state.tmp"; 
        FileStream file;
        if (File.Exists(path))
        {
            file = File.OpenWrite(path);
        }
        else
        {
            file = File.Create(path);
        }

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, state);
        file.Close();
    }

    /**
     * Call at the beginning of a level to restore state
     */
    public void LoadGameState()
    {
        string path = Application.persistentDataPath + "/state.tmp";
        FileStream file;

        if (File.Exists(path))
        {
            file = File.OpenRead(path);
        }
        else
        {
            CreateGameState();
            file = File.OpenRead(path);
        }

        BinaryFormatter bf = new BinaryFormatter();
        GameState state = (GameState) bf.Deserialize(file);
        file.Close();

        UpdateScore(state.score);
        // Need this check for the credits controller again
        // I know I could do the old fashioned trick of just having all these elements off screen but we aren't heathens are we
        if (UIMode != 99)
        {
            this.run = state.run;
            this.firstRun = state.firstRun;
            player.SetHealth(state.health);
            PlayerHealthBarNoStun(state.health);
            if (state.special == 0 && state.unlocked > 0)
            {
                player.SetSpecial(1);
                UpdateSpecial(1);
            }
            else
            {
                player.SetSpecial(state.special);
                UpdateSpecial(state.special);
            }
            this.lastUnlock = state.unlocked;
            player.SetSpecialUnlocked(lastUnlock);
        }
    }

    /**
     * Locks out all gameplay control until end of time
     */
    public void EnterMenu()
    {
        UIMode = 99;
    }

    public void SaveHighScore()
    {
        UIMode = 2;
        StartCoroutine(WaitForDeathRoutine());
    }

    /**
     * Wait for death animation to finish before displaying score stuff
     */
    private IEnumerator WaitForDeathRoutine()
    {
        if (musicController != null)
        {
            musicController.SendMessage("Death");
        }

        for(int i = 0; i < 50; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        UIMode = 3;
        scoreParent.SetActive(true);
        //Activate UI elements and controls for inputting name, and then display nearest 15 scores (+/- 12 on both sides if possible)
        string number = score.ToString();

        //Prefix it with 0s
        if (number.Length < 14)
        {
            int difference = 14 - number.Length;
            number = new string('0', difference) + number;
        }

        scorePreview.text = number;

        scoreField.gameObject.SetActive(false);
        scoreEntry.gameObject.SetActive(true);
    }

    private void CreateHighScores()
    {
        //TODO: Create a default high score list
        HighScoreList scoreList = new HighScoreList();
        scoreList.scores.Add(new HighScoreList.HighScore(4000L, "Valkyrie    ", false));
        scoreList.scores.Add(new HighScoreList.HighScore(7500L, "Viking Chief", false));
        scoreList.scores.Add(new HighScoreList.HighScore(789L, "Seabee Swarm", false));
        scoreList.scores.Add(new HighScoreList.HighScore(101112L, "Queen Seabee", false));
        scoreList.scores.Add(new HighScoreList.HighScore(131415L, "St Malachy  ", false));
        scoreList.scores.Add(new HighScoreList.HighScore(161718L, "SupremeSaint", false));
        scoreList.scores.Add(new HighScoreList.HighScore(192021L, "CapGreyhound", false));
        scoreList.scores.Add(new HighScoreList.HighScore(212223L, "Nick Balcomb", false));
        scoreList.scores.Add(new HighScoreList.HighScore(212223L, "John R G    ", false));
        scoreList.scores.Add(new HighScoreList.HighScore(242526L, "Clifford S  ", false));
        scoreList.scores.Add(new HighScoreList.HighScore(272829L, "Monet Comeau", false));
        scoreList.scores.Add(new HighScoreList.HighScore(303132L, "Patrick H   ", false));
        scoreList.scores.Add(new HighScoreList.HighScore(333435L, "Robert Scott", false));
        scoreList.scores.Add(new HighScoreList.HighScore(363738L, "Alex Harper ", false));
        scoreList.scores.Add(new HighScoreList.HighScore(394041L, "Brandi C    ", false));
        scoreList.scores.Add(new HighScoreList.HighScore(17061998L, "Mitchell G  ", false));
        // Hide scores from the post game
        scoreList.scores.Add(new HighScoreList.HighScore(999999999L, "The Creator ", true));
        scoreList.hideScores = true;

        string path = Application.persistentDataPath + "/scores.lst";
        FileStream file;
        if (File.Exists(path))
        {
            file = File.OpenWrite(path);
        }
        else
        {
            file = File.Create(path);
        }

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, scoreList);
        file.Close();
    }

    public HighScoreList LoadHighScores()
    {
        string path = Application.persistentDataPath + "/scores.lst";
        FileStream file;

        if (File.Exists(path))
        {
            file = File.OpenRead(path);
        }
        else
        {
            CreateHighScores();
            file = File.OpenRead(path);
        }

        BinaryFormatter bf = new BinaryFormatter();
        HighScoreList scoreList = (HighScoreList)bf.Deserialize(file);
        file.Close();

        return scoreList;
    }

    private void SaveHighScores(HighScoreList scoreList)
    {
        string path = Application.persistentDataPath + "/scores.lst";
        FileStream file;
        if (File.Exists(path))
        {
            file = File.OpenWrite(path);
        }
        else
        {
            file = File.Create(path);
        }

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, scoreList);
        file.Close();
    }

    public void PrimeTransition(string nextScene)
    {
        transitionScene = nextScene;
        transitionPrimed = true;
    }

    private IEnumerator TransitionRoutine()
    {
        player.Celebrate();
        if (musicController != null)
        {
            musicController.SendMessage("Victory");
        }

        for (int i = 0; i < 250; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // Credits needs more time for the longer jingle!
        if (transitionScene == "Credits")
        {
            for (int i = 0; i < 250; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(transitionScene);
        SceneManager.UnloadSceneAsync(sceneName);
    }

    private IEnumerator HitstunRoutine(float duration)
    {
        this.utilityTransparency = false;
        float timeStep = 0.01f;
        Time.timeScale = 0.0f;
        // Yes I could just use max lol
        float remaining = duration > this.drain ? duration : this.drain;
        while (remaining > 0.0f)
        {
            if (remaining <= this.drain)
            {
                playerHealthBarDrain.localScale = new Vector3(remaining * (1.0f / playerHealthBarFill.localScale.x), 1.0f, 1.0f);
            }
            remaining -= timeStep;
            yield return new WaitForSecondsRealtime(timeStep);
        }
        Time.timeScale = 1.0f;

        // Just make sure the scale is 0 by the end
        playerHealthBarDrain.localScale = new Vector3(0.0f, 1.0f, 1.0f);

        yield return new WaitForSecondsRealtime(0.5f);
        this.utilityTransparency = true;
    }

    // I believe mine is the first game to have heal stun lolol
    private IEnumerator HealStunRoutine(float duration)
    {
        this.utilityTransparency = false;
        float timeStep = 0.01f;
        float fullBar = playerHealthBarFill.localScale.x;

        if (this.drain == fullBar)
        {
            this.drain -= timeStep;
        }
        playerHealthBarFill.localScale = new Vector3(fullBar - this.drain, 1.0f, 1.0f);
        Time.timeScale = 0.0f;
        // Yes I could just use max lol
        float remaining = duration > this.drain ? duration : this.drain;
        while (remaining > 0.0f)
        {
            if (remaining <= this.drain)
            {
                playerHealthBarFill.localScale = new Vector3(fullBar - remaining, 1.0f, 1.0f);
                playerHealthBarDrain.localScale = new Vector3(remaining * (1.0f / playerHealthBarFill.localScale.x), 1.0f, 1.0f);
            }
            remaining -= timeStep;
            yield return new WaitForSecondsRealtime(timeStep);
        }
        Time.timeScale = 1.0f;

        // Just make sure the scale is 0 by the end
        playerHealthBarFill.localScale = new Vector3(fullBar, 1.0f, 1.0f);
        playerHealthBarDrain.localScale = new Vector3(0.0f, 1.0f, 1.0f);

        yield return new WaitForSecondsRealtime(0.5f);
        this.utilityTransparency = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        this.collisionTransparency = true;
    }

    public void TriggerSpecialCover(int frames)
    {
        StartCoroutine(SpecialCooldownRoutine(frames));
    }

    private IEnumerator SpecialCooldownRoutine(int frames)
    {
        for (int i = frames; i > 0; i--)
        {
            this.specialCover.localScale = new Vector3(1.0f, ((float) i) / frames, 1.0f);
            yield return new WaitForFixedUpdate();
        }
        this.specialCover.localScale = new Vector3(1.0f, 0.0f, 1.0f);
    }
}
