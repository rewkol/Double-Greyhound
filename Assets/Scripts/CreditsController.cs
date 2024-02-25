using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    private string creditsStr;
    private string victoryText;

    private UIController ui;

    private GameObject messageParent;
    private GameObject creditsParent;
    private GameObject scoreParent;
    private Text creditsText;
    private Text messageText;
    private Text scoreText;

    private bool buttonPressed;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.FindObjectsOfType<UIController>()[0];

        TextAsset credits = (TextAsset)Resources.Load("credits");
        creditsStr = credits.text;

        // Being cheeky and reusing these tags
        messageParent = GameObject.FindGameObjectsWithTag("UIStatus")[0];
        creditsParent = GameObject.FindGameObjectsWithTag("UITextBox")[0];
        scoreParent = GameObject.FindGameObjectsWithTag("UIScore")[0];
        messageParent.SetActive(false);
        scoreParent.SetActive(false);

        buttonPressed = false;

        creditsText = creditsParent.GetComponent<Text>();
        creditsText.text = creditsStr;

        // Chunked with | again
        victoryText = "Congratulations!|You have defeated the Shadow Greyhound and recovered the school's trophies!|You have befriended the Viking Chief, the Queen Seabee, and the Supreme Saint - all of them valuable allies for the work ahead: to restore peace between the schools!|You have what it takes to live a Vita Vitalis!|Keeping playing for a High Score";
        messageText = messageParent.GetComponent<Text>();

        scoreText = scoreParent.transform.Find("ScoreField").GetComponent<Text>();


        StartCoroutine(CreditsScrollRoutine());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // TODO: On fire1 press, chunk each part of the victory text

        // TODO: Once text is done displaying, hide and load High Scores like the UI does with current score highlighted in list under name "CURRENT"

        // TODO: After high score screen load HVHS scene again to replay game
    }

    private IEnumerator CreditsScrollRoutine()
    {
        for (int i = 0; i < 2500; i++)
        {
            if (Input.GetAxis("Fire1") > 0 && !buttonPressed)
            {
                buttonPressed = true;
                break;
            }
            creditsParent.transform.position += new Vector3(0.0f, 1.85f, 0.0f);
            yield return new WaitForFixedUpdate();
            if (i == 2499)
            {
                for (int j = 0; j < 200; j++)
                {
                    yield return new WaitForFixedUpdate();
                }
                for (int j = 0; j < 50; j++)
                {
                    creditsText.color = new Color(1.0f, 1.0f, 1.0f, (1.0f - (j / 50.0f)));
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        creditsParent.SetActive(false);
        messageParent.SetActive(true);

        StartCoroutine(MessageChunkRoutine());
    }

    private IEnumerator MessageChunkRoutine()
    {
        int waitTime = buttonPressed ? 50 : 200;
        bool skipChunks = false;
        while (victoryText.Length > 0)
        {
            if (!skipChunks)
            {
                for (int i = 0; i < waitTime; i++)
                {
                    if (Input.GetAxis("Fire1") > 0 && !buttonPressed)
                    {
                        skipChunks = true;
                        break;
                    }
                    else if (Input.GetAxis("Fire1") == 0 && buttonPressed)
                    {
                        buttonPressed = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                waitTime = 200;
            }

            if (victoryText.Contains("|"))
            {
                string toAdd = victoryText.Substring(0, victoryText.IndexOf("|")) + "\n\n";
                messageText.text = messageText.text + toAdd;
                victoryText = victoryText.Substring(victoryText.IndexOf("|") + 1);
            }
            else
            {
                messageText.text = messageText.text + victoryText;
                victoryText = "";
            }
        }

        for (int j = 0; j < 100; j++)
        {
            yield return new WaitForFixedUpdate();
        }
        for (int j = 0; j < 100; j++)
        {
            messageText.color = new Color(1.0f, 1.0f, 1.0f, (1.0f - (j / 50.0f)));
            yield return new WaitForFixedUpdate();
        }
        messageParent.SetActive(false);
        for (int j = 0; j < 50; j++)
        {
            yield return new WaitForFixedUpdate();
        }

        scoreParent.SetActive(true);

        StartCoroutine(HighScoreRoutine());
    }

    private IEnumerator HighScoreRoutine()
    {
        // Most of this code is copied from UIController
        ui.LoadGameState();
        long score = ui.GetScore();
        HighScoreList scores = ui.LoadHighScores();
        HighScoreList.HighScore currentScore = new HighScoreList.HighScore(score, "CURRENT     ", false);
        scores.scores.Add(currentScore);
        scores.scores.Sort();
        int scorePosition = scores.scores.FindIndex(x => x.CompareTo(currentScore) == 0);
        int scoresAbove = 14;
        if (scorePosition < 14)
        {
            scoresAbove = scorePosition;
        }
        else if (scorePosition > 98)
        {
            scoresAbove = -1;
        }

        int startIndex = scorePosition - scoresAbove;
        if (scoresAbove < 0)
        {
            startIndex = 86;
        }

        int cooldown = 50;
        int cursorPosition = 150;

        while (cooldown > 0 || Input.GetAxis("Fire1") == 0)
        {
            if (cooldown > 0)
            {
                cooldown--;
            }

            cursorPosition--;
            if (cursorPosition <= 0)
            {
                cursorPosition = 150;
            }

            scoreText.text = "";
            for (int i = startIndex; i < startIndex + 15; i++)
            {
                //If no more high scores get out of here!
                if (i == scores.scores.Count)
                {
                    break;
                }

                if (i < 99)
                {
                    scoreText.text += ((i == scorePosition && cursorPosition < 75) ? " " : "") + (i < 9 ? "0" : "") + (i + 1) + "> " + scores.scores[i].ToString() + "\n";
                }
                else if (i == 99)
                {
                    scoreText.text += "              ...              \n";
                }
                else
                {
                    scoreText.text += (cursorPosition < 75 ? " " : "") + "XX> " + currentScore.ToString() + "\n";
                }
            }
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForFixedUpdate();

        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("HVHS");
        SceneManager.UnloadSceneAsync(sceneName);
    }
}
