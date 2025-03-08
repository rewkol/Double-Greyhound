using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SJHSMusicController : MonoBehaviour
{
    protected const float BOSS_DELTA = 0.75f;

    public float levelThemeVolume = 1.0f;
    public float greyhoundThemeVolume = 1.0f;
    public float mitchellThemeVolume = 1.0f;
    public float bossThemeVolume = 1.0f;
    public float chiefThemeVolume = 1.0f;
    public float queenThemeVolume = 1.0f;
    public float saintThemeVolume = 1.0f;
    public AudioClip levelTheme;
    public AudioClip greyhoundTheme;
    public AudioClip mitchellTheme;
    public AudioClip bossTheme;
    public AudioClip greyhoundChiefTheme;
    public AudioClip greyhoundQueenTheme;
    public AudioClip greyhoundSaintTheme;

    private bool chiefPresent;
    private bool queenPresent;
    private bool saintPresent;

    private bool stopStart;

    private AudioSource sourceMain;
    private AudioSource sourceChief;
    private AudioSource sourceQueen;
    private AudioSource sourceSaint;

    private int bosses;

    private AudioClip deathClip;
    private AudioClip victoryClip;

    // Start is called before the first frame update
    void Start()
    {
        deathClip = Resources.Load<AudioClip>("JINGLES_DEATH");
        victoryClip = Resources.Load<AudioClip>("JINGLES_FINAL");

        bosses = 2;
        // True means song is playing, false means it is stopping
        stopStart = true;

        AudioSource[] sources = GetComponents<AudioSource>();
        sourceMain = sources[0];
        sourceChief = sources[1];
        sourceQueen = sources[2];
        sourceSaint = sources[3];


        sourceMain.clip = levelTheme;
        sourceMain.loop = true;

        sourceChief.clip = greyhoundChiefTheme;
        sourceChief.loop = true;
        sourceQueen.clip = greyhoundQueenTheme;
        sourceQueen.loop = true;
        sourceSaint.clip = greyhoundSaintTheme;
        sourceSaint.loop = true;

        sourceMain.volume = levelThemeVolume;

        sourceChief.volume = 0.0f;
        sourceQueen.volume = 0.0f;
        sourceSaint.volume = 0.0f;

        sourceMain.Play();
        // The other three tracks don't need to play yet :)
    }

    void Update()
    {
        if (chiefPresent)
        {
            sourceChief.volume += (BOSS_DELTA * Time.deltaTime);
            if (sourceChief.volume > chiefThemeVolume)
            {
                sourceChief.volume = chiefThemeVolume;
            }
        }
        else
        {
            sourceChief.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceChief.volume < 0.0f)
            {
                sourceChief.volume = 0.0f;
            }
        }

        if (queenPresent)
        {
            sourceQueen.volume += (BOSS_DELTA * Time.deltaTime);
            if (sourceQueen.volume > queenThemeVolume)
            {
                sourceQueen.volume = queenThemeVolume;
            }

        }
        else
        {
            sourceQueen.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceQueen.volume < 0.0f)
            {
                sourceQueen.volume = 0.0f;
            }
        }


        if (saintPresent)
        {
            sourceSaint.volume += (BOSS_DELTA * Time.deltaTime);
            if (sourceSaint.volume > saintThemeVolume)
            {
                sourceSaint.volume = saintThemeVolume;
            }
        }
        else
        {
            sourceSaint.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceSaint.volume < 0.0f)
            {
                sourceSaint.volume = 0.0f;
            }
        }
    }

    public void StartNextSong()
    {
        if (!stopStart)
        {
            stopStart = true;
            if (bosses == 2)
            {
                StartCoroutine(StartNextRoutine(greyhoundTheme, greyhoundThemeVolume, true));
            }
            else if (bosses == 1)
            {
                StartCoroutine(StartNextRoutine(mitchellTheme, mitchellThemeVolume, false));
            }
            else if (bosses == 0)
            {
                StartCoroutine(StartNextRoutine(bossTheme, bossThemeVolume, false));
            }
            bosses--;
        }
    }

    public void StopCurrentSong()
    {
        if (stopStart)
        {
            stopStart = false;
            this.chiefPresent = false;
            this.queenPresent = false;
            this.saintPresent = false;
            StartCoroutine(StopCurrentRoutine());
        }
    }

    private IEnumerator StartNextRoutine(AudioClip nextTheme, float nextThemeVolume, bool backingStart)
    {
        sourceMain.volume = 0.0f;

        sourceMain.clip = nextTheme;
        sourceMain.loop = true;

        if (backingStart)
        {
            sourceChief.Play();
            sourceQueen.Play();
            sourceSaint.Play();
        }
        else
        {
            sourceChief.Stop();
            sourceQueen.Stop();
            sourceSaint.Stop();
        }

        sourceMain.Play();
        while (sourceMain.volume < nextThemeVolume)
        {
            if (!stopStart)
            {
                break;
            }

            sourceMain.volume += (BOSS_DELTA * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        if (sourceMain.volume > nextThemeVolume)
        {
            sourceMain.volume = nextThemeVolume;
        }
    }

    private IEnumerator StopCurrentRoutine()
    {
        // TODO: The volume for the backing track stayed up despite the main theme dropping but I can't see how that would happen in this while condition?
        while (sourceMain.volume > 0.0f || sourceChief.volume > 0.0f || sourceQueen.volume > 0.0f || sourceSaint.volume > 0.0f)
        {
            if (stopStart)
            {
                break;
            }

            sourceMain.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceMain.volume < 0.0f)
            {
                sourceMain.volume = 0.0f;
            }
            sourceChief.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceChief.volume < 0.0f)
            {
                sourceChief.volume = 0.0f;
            }
            sourceQueen.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceQueen.volume < 0.0f)
            {
                sourceQueen.volume = 0.0f;
            }
            sourceSaint.volume -= (BOSS_DELTA * Time.deltaTime);
            if (sourceSaint.volume < 0.0f)
            {
                sourceSaint.volume = 0.0f;
            }
            yield return new WaitForEndOfFrame();
        }

        if (!stopStart)
        {
            sourceMain.Stop();
            sourceChief.Stop();
            sourceQueen.Stop();
            sourceSaint.Stop();
        }
    }

    public void Kill()
    {
        sourceMain.Stop();
        sourceChief.Stop();
        sourceQueen.Stop();
        sourceSaint.Stop();

        stopStart = false;
    }

    public void ChiefAppear()
    {
        this.chiefPresent = true;
    }

    public void QueenAppear()
    {
        this.queenPresent = true;
    }

    public void SaintAppear()
    {
        this.saintPresent = true;
    }

    public void ChiefExit()
    {
        this.chiefPresent = false;
    }

    public void QueenExit()
    {
        this.queenPresent = false;
    }

    public void SaintExit()
    {
        this.saintPresent = false;
    }

    public void BossAppear()
    {
        // do nothing
    }

    public void BossHalfway()
    {
        // do nothing
    }

    public void Death()
    {
        Kill();
        stopStart = false;
        sourceMain.clip = deathClip;
        sourceMain.loop = false;
        sourceMain.volume = 1.0f;
        sourceMain.Play();
    }

    public void Victory()
    {
        Kill();
        stopStart = true;
        sourceMain.clip = victoryClip;
        sourceMain.loop = false;
        sourceMain.volume = 1.0f;
        sourceMain.Play();
    }
}
