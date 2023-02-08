using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StMalachyController : MonoBehaviour
{
    // All of the graphics layers needed for the ascension scene he triggers
    public GameObject schoolLayer;
    public GameObject uptownLayer;
    public GameObject skylineLayer;
    public GameObject heavenLayer;
    public GameObject transitionLayer;

    private PlayerController player;
    private UIController ui;
    private bool triggeredFight;
    private bool triggeredAscension;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        ui = GameObject.FindObjectsOfType<UIController>()[0];

        // Need these triggers because he is inherently tied to the scene by the vertical parallax effect so can't use spawner and prefabs
        triggeredFight = false;
        triggeredAscension = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerPos = player.GetPosition();
        // TODO: Move to be trigger by boss battle later
        if (!triggeredAscension && playerPos.x >= 38.9f)
        {
            triggeredAscension = true;
            player.ForcePosition(new Vector3(38.9f, playerPos.y, playerPos.z));
            StartCoroutine(AscensionRoutine());
        }
    }

    private IEnumerator AscensionRoutine()
    {
        ui.StartManualCutscene();
        for (int i = 0; i < 1100; i++)
        {
            // Accidentally fall toward Hell
            if (i < 25)
            {
                schoolLayer.transform.position += new Vector3(0.0f, ((i + 1) / 25.0f) * 0.08f, 0.0f);
            }
            else if (25 <= i && i < 125)
            {
                schoolLayer.transform.position += new Vector3(0.0f, 0.08f, 0.0f);
            }
            else if (125 <= i && i < 150)
            {
                schoolLayer.transform.position += new Vector3(0.0f, ((149 - i) / 25.0f) * 0.08f, 0.0f);
            }
            // Check Hell out for a second
            else if (150 <= i && i < 220)
            {
                ;// TODO: Maybe play sound effect here later
            }
            // Ascend out of school slowly
            else if (220 <= i && i < 245)
            {
                schoolLayer.transform.position += new Vector3(0.0f, ((i - 219) / 25.0f) * -0.08f, 0.0f);
            }
            else if ( 245 <= i && i < 555 )
            {
                schoolLayer.transform.position += new Vector3(0.0f, -0.08f, 0.0f);
                uptownLayer.transform.position += new Vector3(0.0f, -0.06f, 0.0f);
                skylineLayer.transform.position += new Vector3(0.0f, -0.03f, 0.0f);
            }
            // Slow down to look over city
            else if (555 <= i && i < 580)
            {
                float modifier = ((579 - i) / 25.0f);
                schoolLayer.transform.position += new Vector3(0.0f, modifier *  -0.08f, 0.0f);
                uptownLayer.transform.position += new Vector3(0.0f, modifier * -0.06f, 0.0f);
                skylineLayer.transform.position += new Vector3(0.0f, modifier * -0.03f, 0.0f);
            }
            else if (580 <= i && i < 750)
            {
                ;// TODO: Could put sound effects here too
            }
            // Speed back up toward Heaven
            else if (750 <= i && i < 900)
            {
                float modifier = ((i - 749) / 50.0f);
                schoolLayer.transform.position += new Vector3(0.0f, modifier * -0.12f, 0.0f);
                uptownLayer.transform.position += new Vector3(0.0f, modifier * -0.09f, 0.0f);
                skylineLayer.transform.position += new Vector3(0.0f, modifier * -0.07f, 0.0f);
            }
            // Transition cloud covers screen / move Heaven into place
            else if (i > 900)
            {
                float modifier = 1.0f;
                if (i > 950)
                {
                    modifier = (1100 - i) / 150.0f;
                }
                transitionLayer.transform.position += new Vector3(0.0f, modifier * -0.28f, 0.0f);
                if (i == 965)
                {
                    heavenLayer.transform.position += new Vector3(0.0f, -66.3f, 0.0f);
                }
            }
            yield return new WaitForFixedUpdate();

        }

        // TODO: put this in one line masked by transition cloud

        ui.EndManualCutscene();
    }
}
