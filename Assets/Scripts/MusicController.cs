using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    private const float MAIN_DELTA = 1.25f;
    private const float MAIN_DELTA_DOWN = 0.75f;
    private const float BOSS_DELTA = 0.5f;

    public float mainThemeVolume = 1.0f;
    public float mainSecondVolume = 1.0f;
    public AudioClip mainThemeIntro;
    public AudioClip mainSecondIntro;
    public AudioClip mainTheme;
    public AudioClip mainSecond;

    public float bossThemeVolume = 1.0f;
    public float bossSecondVolume = 1.0f;
    public AudioClip bossThemeIntro;
    public AudioClip bossSecondIntro;
    public AudioClip bossTheme;
    public AudioClip bossSecond;

    public int bosses = 2;

    private bool introPlaying;
    private bool enemyPresent;
    private bool bossStarted;
    private AudioSource sourceMain;
    private AudioSource sourceSecond;
    private AudioSource sourceMainIntro;
    private AudioSource sourceSecondIntro;

    public float introLength = 5.0f;
    private float runTime;
    private float secondTrackVolume;

    private AudioClip deathClip;
    private AudioClip victoryClip;

    // Start is called before the first frame update
    void Start()
    {
        deathClip = Resources.Load<AudioClip>("JINGLES_DEATH");
        victoryClip = Resources.Load<AudioClip>("JINGLES_VICTORY");

        introPlaying = false;
        runTime = 0.0f;
        enemyPresent = false;
        bossStarted = false;

        AudioSource[] sources = GetComponents<AudioSource>();
        sourceMain = sources[0];
        sourceSecond = sources[1];
        sourceMainIntro = sources[2];
        sourceSecondIntro = sources[3];


        sourceMain.clip = mainTheme;
        sourceMain.loop = true;
        sourceSecond.clip = mainSecond;
        sourceSecond.loop = true;


        if (mainThemeIntro != null)
        {
            sourceMainIntro.clip = mainThemeIntro;
            sourceMainIntro.loop = false;
            sourceMainIntro.volume = mainThemeVolume;
            sourceMain.volume = 0.0f;

            sourceMainIntro.Play();
            introPlaying = true;
        }
        else
        {
            sourceMain.volume = mainThemeVolume;
        }

        if (mainSecondIntro != null)
        {
            sourceSecondIntro.clip = mainSecondIntro;
            sourceSecondIntro.loop = false;
            sourceSecondIntro.volume = 0.0f;
            sourceSecondIntro.Play();
            introPlaying = true;
        }
        // SourceSecond always starts with volume 0
        sourceSecond.volume = 0.0f;
        secondTrackVolume = 0.0f;

        sourceMain.Play();
        sourceSecond.Play();
    }

    void Update()
    {
        // Bring in second track when enemy is on screen but not during boss fight
        if (GameObject.FindGameObjectsWithTag("Enemy").Length > 0 && !bossStarted)
        {
            secondTrackVolume += (MAIN_DELTA * Time.deltaTime);
            if (secondTrackVolume > mainSecondVolume)
            {
                secondTrackVolume = mainSecondVolume;
            }
        }
        else if (!bossStarted)
        {
            secondTrackVolume -= (MAIN_DELTA_DOWN * Time.deltaTime);
            if (secondTrackVolume < 0.0f)
            {
                secondTrackVolume = 0.0f;
            }
        }

        // Independently track the correct secondSource to the volume
        if (introPlaying)
        {
            sourceSecondIntro.volume = secondTrackVolume;
        }
        else
        {
            sourceSecond.volume = secondTrackVolume;
        }


        // If intro is playing, countdown and then start crossfade
        if (introPlaying)
        {
            runTime += Time.deltaTime;
            if (runTime > introLength)
            {
                sourceMain.volume += (MAIN_DELTA * Time.deltaTime);
                sourceMainIntro.volume -= (MAIN_DELTA * Time.deltaTime);
                if (!bossStarted && sourceMain.volume > mainThemeVolume)
                {
                    sourceMain.volume = mainThemeVolume;
                }
                if (bossStarted && sourceMain.volume > bossThemeVolume)
                {
                    sourceMain.volume = mainThemeVolume;
                }
                if (sourceMainIntro.volume < 0.0f)
                {
                    sourceMainIntro.volume = 0.0f;
                }

                // Because the volume of the second track is in flux, this gets a bit wonkier
                sourceSecond.volume += (MAIN_DELTA * Time.deltaTime);
                sourceSecondIntro.volume = secondTrackVolume - sourceSecond.volume;
                if (sourceSecond.volume > secondTrackVolume)
                {
                    sourceSecond.volume = secondTrackVolume;
                }
                if (sourceSecondIntro.volume < 0.0f)
                {
                    sourceSecondIntro.volume = 0.0f;
                }

                // If both volumes are where they need to be, shut down the old ones
                if (!bossStarted && (sourceMain.volume == mainThemeVolume && sourceSecond.volume == secondTrackVolume))
                {
                    introPlaying = false;
                    sourceMainIntro.Stop();
                    sourceSecondIntro.Stop();
                }
                else if (bossStarted && (sourceMain.volume == bossThemeVolume && sourceSecond.volume == secondTrackVolume))
                {
                    introPlaying = false;
                    sourceMainIntro.Stop();
                    sourceSecondIntro.Stop();
                }
            }
        }
    }

    public void BossAppear()
    {
        bosses--;

        if (bosses <= 0)
        {
            // Restart the sources for the boss clips
            enemyPresent = false;
            bossStarted = true;
            StartCoroutine(BossTransitionRoutine());
        }
    }

    private IEnumerator BossTransitionRoutine()
    {
        while (sourceMain.volume > 0.0f || sourceSecond.volume > 0.0f)
        {
            sourceMain.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceMain.volume < 0.0f)
            {
                sourceMain.volume = 0.0f;
            }
            sourceSecond.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceSecond.volume < 0.0f)
            {
                sourceSecond.volume = 0.0f;
            }
            yield return new WaitForEndOfFrame();
        }
        sourceMain.Stop();
        sourceSecond.Stop();
        if (introPlaying)
        {
            sourceMainIntro.Stop();
            sourceSecondIntro.Stop();
        }

        introPlaying = false;
        runTime = 0.0f;
        // Make extra sure
        sourceMain.volume = 0.0f;
        sourceSecond.volume = 0.0f;
        sourceMainIntro.volume = 0.0f;
        sourceSecondIntro.volume = 0.0f;

        sourceMain.clip = bossTheme;
        sourceMain.loop = true;
        sourceSecond.clip = bossSecond;
        sourceSecond.loop = true;


        if (bossThemeIntro != null)
        {
            sourceMainIntro.clip = bossThemeIntro;
            sourceMainIntro.loop = false;
            sourceMainIntro.volume = bossThemeVolume;
            sourceMain.volume = 0.0f;

            sourceMainIntro.Play();
            introPlaying = true;
        }
        else
        {
            sourceMain.volume = 0.0f;
        }

        if (bossSecondIntro != null)
        {
            sourceSecondIntro.clip = bossSecondIntro;
            sourceSecondIntro.loop = false;
            sourceSecondIntro.volume = 0.0f;
            sourceSecondIntro.Play();
            introPlaying = true;
        }
        // SourceSecond always starts with volume 0
        sourceSecond.volume = 0.0f;
        secondTrackVolume = 0.0f;

        sourceMain.Play();
        sourceSecond.Play();

        if (introPlaying)
        {
            while (sourceMainIntro.volume < bossThemeVolume)
            {
                sourceMainIntro.volume += (BOSS_DELTA * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (sourceMain.volume < bossThemeVolume)
            {
                sourceMain.volume += (BOSS_DELTA * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }

        if (sourceMain.volume > bossThemeVolume)
        {
            sourceMain.volume = bossThemeVolume;
        }
    }

    public void BossHalfway()
    {
        if (bossStarted && sourceSecond.volume == 0)
        {
            StartCoroutine(RaiseVolumeRoutine());
        }
    }

    private IEnumerator RaiseVolumeRoutine()
    {
        while (secondTrackVolume < bossSecondVolume)
        {
            secondTrackVolume += (BOSS_DELTA * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        if (secondTrackVolume > bossSecondVolume)
        {
            secondTrackVolume = bossSecondVolume;
        }
    }

    public void Kill()
    {
        sourceMain.Stop();
        sourceSecond.Stop();
        sourceMainIntro.Stop();
        sourceSecondIntro.Stop();
    }

    public void Death()
    {
        Kill();
        sourceMain.clip = deathClip;
        sourceMain.loop = false;
        sourceMain.volume = 1.0f;
        sourceMain.Play();
    }

    public void Victory()
    {
        Kill();
        sourceMain.clip = victoryClip;
        sourceMain.loop = false;
        sourceMain.volume = 1.0f;
        sourceMain.Play();
    }
}
