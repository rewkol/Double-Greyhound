using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private int UIMode;
    private GameObject statusParent;
    private GameObject dialogueParent;
    private GameObject camera;

    //Score variables
    private long score;
    private Text scoreText;

    //Player Health Variables
    private PlayerController player;
    private float playerHealthMaxWidth;
    private Transform playerHealthBarFill;

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

    // Start is called before the first frame update
    void Start()
    {
        //Get components
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        statusParent = GameObject.FindGameObjectsWithTag("UIStatus")[0];
        dialogueParent = GameObject.FindGameObjectsWithTag("UITextBox")[0];
        camera = GameObject.FindGameObjectsWithTag("MainCamera")[0];

        //Player Health
        playerHealthBarFill = statusParent.transform.Find("PlayerHealthBarRed");
        playerHealthMaxWidth = playerHealthBarFill.GetComponent<RectTransform>().rect.width;

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

        //Score
        score = 0;
        scoreText = statusParent.transform.Find("ScoreText").GetComponent<Text>();
        scoreText.text = "SCORE: 000000000000000";

        //TODO: Load saved values from previous level

        UIMode = 0;
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    public void UpdateScore(long points)
    {
        score += points;
        string newText = "SCORE: ";
        string number = score.ToString();

        //Prefix it with 0s
        if (number.Length < 15)
        {
            int difference = 15 - number.Length;
            number = new string('0', difference) + number;
        }

        scoreText.text = newText + number;
    }

    public void PlayerHealthBar(int health)
    {
        //Change health bar size as player health changes
        float playerHealthScale = ((float) health) / 20.0f;
        if (playerHealthScale < 0.0f)
        {
            playerHealthScale = 0.0f;
        }
        playerHealthBarFill.localScale = new Vector3(playerHealthScale, 1.0f, 1.0f);
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

    private IEnumerator CameraRoutine()
    {
        for (int i = 0; i < 50; i++)
        {
            camera.transform.position = camera.transform.position + new Vector3(0.07f, 0.0f, 0.0f);
            camera.gameObject.SendMessage("ForceUpdate", 0.07f);
            yield return new WaitForFixedUpdate();
        }
        UIMode = 0;
    }

    //Tells if the player and enemies are allowed to move
    public bool GameActive()
    {
        return UIMode == 0;
    }
}
