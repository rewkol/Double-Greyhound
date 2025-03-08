using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * The SFXController class acitvely manages a pool of AudioSource objects and allows calling objects to request
 * a sound effect to be played. Depending on the state of the pool, the sound may not be played based on a passed
 * priority with the request
 * 
 * E.G., If the pool is full of priority 5 sounds, but a request comes in for a priority 10 sound, it wil not play,
 *       but a request for a priority 1 sound will kick the audio source out that has the longest current runtime
 */
public class SFXController : MonoBehaviour
{
    // Singleton
    static SFXController instance;

    private const int POOL_SIZE = 24;
    private const float POLL_FREQUENCY = 5.0f;

    private List<GameObject> pool;
    private List<GameObject> free;
    private float pollTime;
    private List<Transform> trackingTransforms;
    private List<Vector3> trackingOffsets;
    private List<GameObject> trackedSources;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            // This singleton object and all its children exist here
            DontDestroyOnLoad(this.gameObject);

            InitializeSources();
            this.trackingTransforms = new List<Transform>(5);
            this.trackingOffsets = new List<Vector3>(5);
            this.trackedSources = new List<GameObject>(5);
            CacheCommonSounds();
        }
    }

    void Update()
    {
        if (this.pollTime > POLL_FREQUENCY)
        {
            // Refresh the list of free audio sources
            this.free.Clear();
            foreach (GameObject source in this.pool)
            {
                // Literally just checks playing status and not anything else
                if (!source.GetComponent<AudioSource>().isPlaying)
                {
                    this.free.Add(source);
                }
            }
            this.pollTime = 0.0f;
        }
        else
        {
            this.pollTime += Time.deltaTime;
        }

        // Track the tracking sources
        for (int i = 0; i < this.trackedSources.Count; i++)
        {
            GameObject source = this.trackedSources[i];
            source.transform.position = this.trackingTransforms[i].position + this.trackingOffsets[i];
        }
    }

    /**
     * Initializes each AudioSource GameObject and adds it to the sourceArray
     */
    private void InitializeSources()
    {
        // The actual pool of audio sources
        this.pool = new List<GameObject>(POOL_SIZE);
        // A list of free sources periodically refreshed to save on list iterations on the pool
        this.free = new List<GameObject>(POOL_SIZE);

        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject source = new GameObject("SFX_Source_" + i, typeof(AudioSource));
            source.GetComponent<Transform>().SetParent(this.transform);
            this.pool.Add(source);
            this.free.Add(source);
        }
    }
    
    /**
     * Loads many of the common sound effects so that the loading in game doesn't cause any hitches
     */
    private void CacheCommonSounds()
    {
        Resources.LoadAll("SFX", typeof(AudioClip));
    }

    /**
     * Finds and returns an AudioSource object that is ready to play a new AudioClip
     * 
     * @param priority the priority requested for the available AudioSource
     */
    private GameObject FindAvailableSource(int priority)
    {
        GameObject foundSource = null;
        if (this.free.Count > 0)
        {
            foundSource = this.free[0];
            this.free.RemoveAt(0);
        }
        else
        {
            // this.free may not be up to date if something ended since last polling period
            foreach (GameObject source in this.pool)
            {
                if (!source.GetComponent<AudioSource>().isPlaying)
                {
                    foundSource = source;
                    break;
                }
            }

            if (foundSource == null)
            {
                // Try any effect that has 0.0f volume. These should not be playing anyway because it is pointless but best to remove any that are
                foreach (GameObject source in this.pool)
                {
                    if (source.GetComponent<AudioSource>().volume <= 0.0f)
                    {
                        foundSource = source;
                        break;
                    }
                }

                // If still nothing, have to kill an existing priority if there are any of a lower level
                if (foundSource == null)
                {
                    List<GameObject> contenders = new List<GameObject>(POOL_SIZE);

                    foreach (GameObject source in this.pool)
                    {
                        if (source.GetComponent<AudioSource>().priority < priority)
                        {
                            contenders.Add(source);
                        }
                    }

                    // If there are no contenders at this point then sorry bro that sound ain't playing
                    if (contenders.Count > 0)
                    {
                        // Sort contenders by lowest priority and then by highest time in case of ties
                        contenders.Sort((s1, s2) =>
                        {
                            AudioSource as1 = s1.GetComponent<AudioSource>();
                            AudioSource as2 = s2.GetComponent<AudioSource>();

                            // Want to compare the priorities in reverse order
                            int priorityComparison = as2.priority.CompareTo(as1.priority);
                            if (priorityComparison != 0)
                            {
                                return priorityComparison;
                            }
                            // Times in reverse order as well
                            return as2.time.CompareTo(as1.time);
                        });
                        foundSource = contenders[0];
                    }
                }
            }

        }

        return foundSource;
    }

    public void CancelAllSounds()
    {
        this.pollTime = 0.0f;
        this.free.Clear();
        foreach (GameObject source in this.pool)
        {
            // Stop all audio sources
            source.GetComponent<AudioSource>().Stop();
        }
        this.trackingTransforms.Clear();
        this.trackingOffsets.Clear();
        this.trackedSources.Clear();
    }

    public void PlaySFX2D(string sfx)
    {
        this.PlaySFX2D(sfx, 1.0f, 128, 0.0f, false);
    }

    public void PlaySFX2D(string sfx, float volume)
    {
        this.PlaySFX2D(sfx, volume, 128, 0.0f, false);
    }

    public void PlaySFX2D(string sfx, float volume, int priority)
    {
        this.PlaySFX2D(sfx, volume, priority, 0.0f, false);
    }

    public void PlaySFX2D(string sfx, float volume, int priority, float pitchDeadband)
    {
        this.PlaySFX2D(sfx, volume, priority, pitchDeadband, false);
    }

    /**
     * Attempts to play the given SFX on one of the pooled AudioSources based on the priority in 2D space. If there are no pooled AudioSources available
     * to play a song at this priority, it will not play
     * 
     * @param sfx the path in the "SFX" resource directory to the AudioClip
     * @param volume the volume the source should play at
     * @param priority the priority the source should play at
     * @param pitchDeadband the variability in pitches to play at (e.g, a deadband of 0.1 allows the pitch to be 0.9 - 1.1, but a deadband of 0 always plays at same pitch of 1.0)
     * @param ignoreVolume whether to ignore the listener volume (i.e., for UI effects like text progression when the listener is partially muted)
     */
    public void PlaySFX2D(string sfx, float volume, int priority, float pitchDeadband, bool ignoreVolume)
    {
        GameObject source = FindAvailableSource(priority);
        if (source != null)
        {
            AudioSource audioSource = source.GetComponent<AudioSource>();
            audioSource.volume = volume;
            audioSource.priority = priority;
            audioSource.ignoreListenerVolume = ignoreVolume;
            if (pitchDeadband != 0.0f)
            {
                audioSource.pitch = 1.0f + (((Random.value * 2) - 1.0f) * pitchDeadband);
            }
            else
            {
                audioSource.pitch = 1.0f;
            }

            audioSource.clip = Resources.Load<AudioClip>("SFX/" + sfx);
            audioSource.loop = false;
            audioSource.spatialBlend = 0.0f;
            audioSource.Play();
        }
    }

    public void PlaySFX3D(string sfx, Vector3 position)
    {
        this.PlaySFX3D(sfx, position, 3.0f, 14.0f, 1.0f, 128, 0.0f, false);
    }

    public void PlaySFX3D(string sfx, Vector3 position, float minDistance, float maxDistance)
    {
        this.PlaySFX3D(sfx, position, minDistance, maxDistance, 1.0f, 128, 0.0f, false);
    }

    public void PlaySFX3D(string sfx, Vector3 position, float minDistance, float maxDistance, float volume)
    {
        this.PlaySFX3D(sfx, position, minDistance, maxDistance, volume, 128, 0.0f, false);
    }

    public void PlaySFX3D(string sfx, Vector3 position, float minDistance, float maxDistance, float volume, int priority)
    {
        this.PlaySFX3D(sfx, position, minDistance, maxDistance, volume, priority, 0.0f, false);
    }

    public void PlaySFX3D(string sfx, Vector3 position, float minDistance, float maxDistance, float volume, int priority, float pitchDeadband)
    {
        this.PlaySFX3D(sfx, position, minDistance, maxDistance, volume, priority, pitchDeadband, false);
    }

    /**
     * Attempts to play the given SFX on one of the pooled AudioSources based on the priority in 3D space at the given location. 
     * If there are no pooled AudioSources available to play a song at this priority, it will not play
     * 
     * @param sfx the path in the "SFX" resource directory to the AudioClip
     * @param position the position in 3D space to place the AudioSource
     * @param minDistance the minimum audio attenuation distance
     * @param maxDistance the maximum audio attenuation distance
     * @param volume the volume the source should play at
     * @param priority the priority the source should play at
     * @param pitchDeadband the variability in pitches to play at (e.g, a deadband of 0.1 allows the pitch to be 0.9 - 1.1, but a deadband of 0 always plays at same pitch of 1.0)
     * @param ignoreVolume whether to ignore the listener volume (i.e., for UI effects like text progression when the listener is partially muted)
     */
    public void PlaySFX3D(string sfx, Vector3 position, float minDistance, float maxDistance, float volume, int priority, float pitchDeadband, bool ignoreVolume)
    {
        GameObject source = FindAvailableSource(priority);
        if (source != null)
        {
            Transform transform = source.GetComponent<Transform>();
            transform.position = position;

            AudioSource audioSource = source.GetComponent<AudioSource>();
            audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.volume = volume;
            audioSource.priority = priority;
            audioSource.ignoreListenerVolume = ignoreVolume;
            if (pitchDeadband != 0.0f)
            {
                audioSource.pitch = 1.0f + (((Random.value * 2) - 1.0f) * pitchDeadband);
            }
            else
            {
                audioSource.pitch = 1.0f;
            }
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;

            audioSource.clip = Resources.Load<AudioClip>("SFX/" + sfx);
            audioSource.loop = false;
            audioSource.spatialBlend = 1.0f;
            audioSource.Play();
        }
    }

    public void PlaySFXLooping3D(string sfx, Vector3 offset, float minDistance, float maxDistance, float volume, int priority, float pitchDeadband, bool ignoreVolume, Transform trackingParent)
    {
        // TODO: Do not change the AudioSource transform parent, because that will remove it from the DontDestroyOnLoad space. Simply have it update its position based on trackingParent and then in the loop we can handle tracking sounds
        // TODO: Offset is the positional offset from the trackingParent.position
        GameObject source = FindAvailableSource(priority);
        if (source != null)
        {
            Transform transform = source.GetComponent<Transform>();
            transform.position = trackingParent.position + offset;

            AudioSource audioSource = source.GetComponent<AudioSource>();
            audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.volume = volume;
            audioSource.priority = priority;
            audioSource.ignoreListenerVolume = ignoreVolume;
            if (pitchDeadband != 0.0f)
            {
                audioSource.pitch = 1.0f + (((Random.value * 2) - 1.0f) * pitchDeadband);
            }
            else
            {
                audioSource.pitch = 1.0f;
            }
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;

            audioSource.clip = Resources.Load<AudioClip>("SFX/" + sfx);
            audioSource.loop = true;
            audioSource.spatialBlend = 1.0f;
            audioSource.dopplerLevel = 0.5f;
            audioSource.Play();

            // Could be an object but I'm lazy
            this.trackingTransforms.Add(trackingParent);
            this.trackingOffsets.Add(offset);
            this.trackedSources.Add(source);
        }
    }
}
