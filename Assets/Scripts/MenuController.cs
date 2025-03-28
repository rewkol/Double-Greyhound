using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public TitlePlayerController greyhound;
    public TitleVikingController vikingRight;
    public TitleVikingController vikingLeft;

    private UIController ui;
    private int state;
    private int timer;
    private bool buttonPressed;

    private Transform title;
    private Transform greyhoundNormal;
    private Transform greyhoundInverted;
    private GameObject start;

    private SpriteRenderer black;

    private Transform backgrounds;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        ui.EnterMenu();

        title = transform.Find("Title");
        greyhoundNormal = transform.Find("GreyhoundNormal");
        greyhoundInverted = transform.Find("GreyhoundInverted");
        start = GameObject.Find("Start");

        title.position = new Vector3(0.0f, 7.0f, -4.25f);
        greyhoundNormal.position = new Vector3(-20.0f, 3.4f, -4.0f);
        greyhoundInverted.position = new Vector3(20.0f, 1.4f, -3.9f);
        // Default colour FDFF7A
        start.SetActive(false);
        black = GameObject.Find("Darkness").GetComponent<SpriteRenderer>();

        backgrounds = transform.Find("BackgroundHolder");


        state = 0;
        // States
        // 0 - Title descends and Greyhounds fly in
        // 1 - Back screen fades and Captain Greyhound begins to "move"
        // 2 - Start text blinks while the attract mode loops in background (fades to black to reset)
        
        // Used to do small animations/pauses in animations
        timer = 25;
        buttonPressed = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (!buttonPressed)
        {
            if (state >= 2 && Input.GetAxis("Fire1") != 0)
            {
                ui.CreateGameState();

                SceneManager.LoadScene("HVHS");
                SceneManager.UnloadSceneAsync("Menu");
            }
            else if (state == 1 && Input.GetAxis("Fire1") != 0)
            {
                start.SetActive(true);
                black.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                state = 2;
                timer = 0;
            }
            else if (state == 0 && Input.GetAxis("Fire1") != 0)
            {
                title.position = new Vector3(0.0f, 1.7f, -4.25f);
                greyhoundNormal.position = new Vector3(0.0f, 3.4f, -4.0f);
                greyhoundInverted.position = new Vector3(0.0f, 1.4f, -3.9f);
                state = 1;
                timer = 10;
            }
            buttonPressed = true;
        }

        if (Input.GetAxis("Fire1") == 0)
        {
            buttonPressed = false;
        }
    }

    void FixedUpdate()
    {
        if (state == 0)
        {
            if (title.position.y > 1.7f)
            {
                title.position += new Vector3(0.0f, -0.028f, 0.0f);
                if (title.position.y < 1.7f)
                {
                    title.position = new Vector3(0.0f, 1.7f, -4.25f);
                }
            }
            else if (timer > 0)
            {
                timer--;
            }
            else if (greyhoundNormal.position.x < 0.0f && greyhoundInverted.position.x > 0.0f)
            {
                greyhoundNormal.position += new Vector3(0.5f, 0.0f, 0.0f);
                greyhoundInverted.position += new Vector3(-0.5f, 0.0f, 0.0f);
                if (greyhoundNormal.position.x > 0.0f || greyhoundInverted.position.x < 0.0f)
                {
                    greyhoundNormal.position = new Vector3(0.0f, 3.4f, -4.0f);
                    greyhoundInverted.position = new Vector3(0.0f, 1.4f, -3.9f);
                }
            }
            else
            {
                title.position = new Vector3(0.0f, 1.7f, -4.25f);
                greyhoundNormal.position = new Vector3(0.0f, 3.4f, -4.0f);
                greyhoundInverted.position = new Vector3(0.0f, 1.4f, -3.9f);
                state = 1;
                timer = 10;
            }
        }
        else if (state == 1)
        {
            if (timer > 0)
            {
                // Do nothing while title animation plays
            }
            else if (timer > -51)
            {
                black.color = new Color(0.0f, 0.0f, 0.0f, 1.0f + (timer / 50.0f));
            }
            timer--;

            if (timer <= -100)
            {
                start.SetActive(true);
                state = 2;
                timer = 0;
            }
        }
        else
        {
            if (start.GetComponent<Text>().color.a < 1.0f)
            {
                start.GetComponent<Text>().color = new Color((253 / 255.0f), 1.0f, (122 / 255.0f), start.GetComponent<Text>().color.a + 0.02f);
                if (start.GetComponent<Text>().color.a > 1.0f)
                {
                    start.GetComponent<Text>().color = new Color((253 / 255.0f), 1.0f, (122 / 255.0f), 1.0f);
                }
            }
            else
            {
                // Modulo for the blinking of the start button
                if (timer % 150 < 75)
                {
                    // FDFF7A
                    start.GetComponent<Text>().color = new Color((253 / 255.0f), 1.0f, (122 / 255.0f), 1.0f);
                }
                else
                {
                    start.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                }

                // Walk on screen
                if (timer == 0)
                {
                    greyhound.FaceRight();
                    greyhound.StartWalk();
                    vikingRight.FaceLeft();
                }
                // Pause for a second
                else if (timer == 75)
                {
                    greyhound.StopWalk();
                }
                // Walk toward centre
                else if (timer == 125)
                {
                    greyhound.StartWalk();
                }
                // Stay in place and start moving the background
                else if (timer == 185)
                {
                    greyhound.SetInPlace(true);
                }
                else if (timer >= 185 && timer < 460)
                {
                    backgrounds.position += new Vector3(-greyhound.GetSpeed(), 0.0f, 0.0f);
                }
                // Lock background and start moving forward slightly
                else if (timer == 460)
                {
                    greyhound.SetInPlace(false);
                }
                else if (timer == 485)
                {
                    greyhound.StopWalk();
                }
                // Turn around and walk back across the centre
                else if (timer == 500)
                {
                    greyhound.FaceLeft();
                    greyhound.StartWalk();
                }
                // Face right and wait for Viking to walk onto screen
                else if (timer == 560)
                {
                    greyhound.FaceRight();
                    greyhound.StopWalk();
                }
                else if (timer == 580)
                {
                    greyhound.StartWalk();
                    greyhound.SetInPlace(true);
                }
                else if (timer >= 580 && timer < 600)
                {
                    greyhound.NudgeUp();
                    vikingRight.NudgeUp();
                }
                else if (timer == 600)
                {
                    greyhound.StopWalk();
                    greyhound.SetInPlace(false);
                }
                else if (timer == 610)
                {
                    vikingRight.StartWalk();
                }
                else if (timer == 620)
                {
                    greyhound.StartWalk();
                    greyhound.SetInPlace(true);
                }
                else if (timer >= 620 && timer < 640)
                {
                    greyhound.NudgeDown();
                    vikingRight.NudgeDown();
                }
                else if (timer == 640)
                {
                    greyhound.StopWalk();
                    greyhound.SetInPlace(false);
                }
                // Walk toward Viking and Viking walk toward Greyhound
                else if (timer == 715)
                {
                    greyhound.StartWalk();
                }
                else if (timer == 720)
                {
                    greyhound.StopWalk();
                    vikingRight.StopWalk();
                    vikingRight.Attack();
                }
                // Viking swings axe and Greyhound walks back
                else if (timer == 723)
                {
                    greyhound.FaceLeft();
                    greyhound.StartWalk();
                }
                else if (timer == 740)
                {
                    greyhound.FaceRight();
                    greyhound.StopWalk();
                }
                else if (timer == 745)
                {
                    vikingRight.StartWalk();
                }
                // Greyhound steps forward
                else if (timer == 750)
                {
                    greyhound.StartWalk();
                }
                else if (timer == 755)
                {
                    vikingRight.StopWalk();
                    vikingRight.Attack();
                }
                // Time 4 punches to kill the Viking
                else if (timer == 763)
                {
                    greyhound.StopWalk();
                    greyhound.Attack();
                }
                else if (timer == 766)
                {
                    vikingRight.Damaged();
                }
                else if (timer == 780)
                {
                    vikingRight.Attack();
                }
                else if (timer == 786)
                {
                    greyhound.Attack();
                }
                else if (timer == 789)
                {
                    vikingRight.Damaged();
                }
                else if (timer == 800)
                {
                    greyhound.StartWalk();
                }
                else if (timer == 803)
                {
                    vikingRight.Attack();
                }
                else if (timer == 810)
                {
                    greyhound.StopWalk();
                    greyhound.Attack();
                }
                // For the record, it was when I got to here that I realized that writing the code to record these inputs and play them back probably would have been smarter and more efficient
                else if (timer == 813)
                {
                    vikingRight.Damaged();
                }
                else if (timer == 827)
                {
                    vikingRight.Attack();
                }
                else if (timer == 832)
                {
                    greyhound.Attack();
                }
                else if (timer == 835)
                {
                    vikingRight.Death(false);
                }
                // Walk toward the centre of the screen again
                else if (timer == 850)
                {
                    greyhound.StartWalk();
                }
                else if (timer == 874)
                {
                    greyhound.SetInPlace(true);
                    // He is "walking" off the screen as a dead body lol
                    vikingRight.StartWalk();
                }
                // Walk in place and move background forward again
                else if (timer >= 874 && timer < 1040)
                {
                    backgrounds.position += new Vector3(-greyhound.GetSpeed(), 0.0f, 0.0f);
                }
                // Lock background and start moving forward slightly
                else if (timer == 1040)
                {
                    greyhound.SetInPlace(false);
                    vikingRight.StopWalk();
                }
                else if (timer >= 1080 && timer < 1090)
                {
                    greyhound.NudgeUp();
                    vikingRight.NudgeUp();
                    vikingLeft.NudgeUp();
                }
                // Wait until Viking appears on stage
                else if (timer == 1090)
                {
                    greyhound.StopWalk();
                }
                else if (timer == 1099)
                {
                    greyhound.Attack();
                }
                else if (timer == 1119)
                {
                    greyhound.Attack();
                    vikingRight.StartWalk();
                }
                else if (timer == 1138)
                {
                    greyhound.FaceLeft();
                    greyhound.StartWalk();
                }
                // Start to run away to screen left
                else if (timer >= 1140 && timer < 1150)
                {
                    greyhound.NudgeDown();
                    vikingRight.NudgeDown();
                    vikingLeft.NudgeDown();
                }
                // Second Viking appears from screen left
                else if (timer == 1180)
                {
                    vikingLeft.StartWalk();
                }
                // Greyhound move to centre while right Viking catches up and starts attacking
                else if (timer == 1208)
                {
                    greyhound.FaceRight();
                }
                else if (timer == 1210)
                {
                    vikingRight.StopWalk();
                    vikingRight.Attack();
                }
                // Greyhound dodges and starts to punch right Viking
                else if (timer == 1218)
                {
                    greyhound.FaceLeft();
                }
                else if (timer == 1233)
                {
                    greyhound.FaceRight();
                }
                // Left Viking catches up and begins to swing
                else if (timer == 1243)
                {
                    greyhound.StopWalk();
                    greyhound.Attack();
                    vikingLeft.StopWalk();
                    vikingLeft.Attack();
                }
                else if (timer == 1245)
                {
                    vikingRight.Attack();
                }
                else if (timer == 1246)
                {
                    vikingRight.Damaged();
                }
                else if (timer == 1255)
                {
                    greyhound.FaceLeft();
                    greyhound.Attack();
                }
                else if (timer == 1258)
                {
                    vikingLeft.Damaged();
                }
                else if (timer == 1260)
                {
                    vikingRight.Attack();
                }
                else if (timer == 1267)
                {
                    greyhound.FaceRight();
                    greyhound.StartWalk();
                }
                else if (timer == 1272)
                {
                    greyhound.StopWalk();
                    greyhound.Attack();
                    vikingLeft.StartWalk();
                }
                else if (timer == 1275)
                {
                    vikingRight.Damaged();
                }
                else if (timer == 1277)
                {
                    vikingLeft.StopWalk();
                    vikingLeft.Attack();
                }
                // Yes I purposefully need to slow down the greyhound here because he is too good (on purpose for the player's sake)
                else if (timer == 1287)
                {
                    greyhound.FaceLeft();
                    greyhound.StartWalk();
                }
                else if (timer == 1289)
                {
                    vikingRight.StartWalk();
                }
                // Oops he didn't punch fast enough and wasted frames walking
                else if (timer == 1291)
                {
                    greyhound.Attack();
                }
                // Greyhound is hit
                else if (timer == 1293)
                {
                    // First strike!
                    greyhound.Damaged();
                }
                else if (timer == 1294)
                {
                    vikingRight.StopWalk();
                    vikingRight.Attack();
                }
                else if (timer == 1305)
                {
                    vikingLeft.Attack();
                }
                // Once again adding a couple frames to make him less perfect
                else if (timer == 1306)
                {
                    greyhound.FaceRight();
                }
                else if (timer == 1308)
                {
                    greyhound.Attack();
                }
                else if (timer == 1310)
                {
                    // Second strike!!
                    greyhound.Damaged();
                }
                else if (timer == 1320)
                {
                    greyhound.FaceLeft();
                }
                // Greyhound dies and vikings walk off screen
                else if (timer == 1321)
                {
                    greyhound.Death();
                }
                else if (timer == 1350)
                {
                    vikingLeft.StartWalk();
                    vikingRight.FaceRight();
                    vikingRight.StartWalk();
                }

                // Fade back to black and reset character positions and background position
                else if (timer >= 1450 && timer < 1600)
                {
                    if (timer < 1500)
                    {
                        black.color = new Color(0.0f, 0.0f, 0.0f, (timer - 1450) / 50.0f);
                    }
                    else if (timer == 1500)
                    {
                        vikingLeft.Death(true);
                        vikingRight.Death(false);
                        greyhound.Reset();
                        backgrounds.position = new Vector3(8.091957f, backgrounds.position.y, backgrounds.position.z);
                    }
                    else if (timer >= 1550)
                    {
                        black.color = new Color(0.0f, 0.0f, 0.0f, (1599 - timer) / 50.0f);
                    }
                }

                // Global timer is used for action in the background
                timer = ++timer % 1600;
            }
        }
    }
}
