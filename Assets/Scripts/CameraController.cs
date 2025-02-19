using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float VOLUME_DELTA = 2.0f;
    private const float DIALOGUE_VOLUME = 0.33333f;

    public PlayerController player;
    private ParallaxBGController[] parallaxes;

    private Transform transform;
    private bool inTextDialogue;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();

        parallaxes = GameObject.FindObjectsOfType<ParallaxBGController>();

        inTextDialogue = false;
    }

    // Update is called once per frame
    void Update()
    {
        int enemiesOnScreen = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int bossOnScreen = GameObject.FindGameObjectsWithTag("Boss").Length;

        int totalEnemies = enemiesOnScreen + bossOnScreen;
        if(totalEnemies == 0 && player.transform.position.x > transform.position.x)
        {
            float dist = player.transform.position.x - transform.position.x;
            transform.position = transform.position + new Vector3(dist, 0.0f, 0.0f);
            for (int i = 0; i < parallaxes.Length; i++)
            {
                parallaxes[i].Move(dist, transform.position.x);
            }
        }
    }

    // WHen someone else controls the camera the parallax can get broken
    public void ForceUpdate(float dist)
    {
        for (int i = 0; i < parallaxes.Length; i++)
        {
            parallaxes[i].Move(dist, transform.position.x);
        }
    }

    public void ForcePosition(float x)
    {
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TextStart()
    {
        this.inTextDialogue = true;
        StartCoroutine(LowerVolumeRoutine());
    }

    public void TextOver()
    {
        this.inTextDialogue = false;
        StartCoroutine(RaiseVolumeRoutine());
    }

    private IEnumerator RaiseVolumeRoutine()
    {
        while (AudioListener.volume < 1.0f && !this.inTextDialogue)
        {
            AudioListener.volume += (VOLUME_DELTA * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        if (AudioListener.volume > 1.0f)
        {
            AudioListener.volume = 1.0f;
        }
    }

    private IEnumerator LowerVolumeRoutine()
    {
        while (AudioListener.volume > DIALOGUE_VOLUME && this.inTextDialogue)
        {
            AudioListener.volume -= (VOLUME_DELTA * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        if (AudioListener.volume < DIALOGUE_VOLUME)
        {
            AudioListener.volume = DIALOGUE_VOLUME;
        }
    }
}
