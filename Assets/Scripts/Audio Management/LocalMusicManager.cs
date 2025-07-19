using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public enum Song
{
    Temp1,
    Temp2,
    MainMenu,
    Lobby,
    Building,
    Combat
}

public class LocalMusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] _songs;

    [SerializeField] private GameObject initialAudioListener;

    private GameObject songPlayerObject;
    private AudioSource songPlayerObjectAudioSource;

    #region  Singleton implementation

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static LocalMusicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log($"Duplicate LocalMusicManager found, removing...");
            Destroy(gameObject);
        }
    }

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make the object
        songPlayerObject = Instantiate(new GameObject(), initialAudioListener.transform);
        songPlayerObject.name = "SongPlayerObject";
        songPlayerObject.AddComponent<AudioSource>();

        // Edit the audio source
        songPlayerObjectAudioSource = songPlayerObject.GetComponent<AudioSource>();
        songPlayerObjectAudioSource.clip = _songs[0];
        songPlayerObjectAudioSource.loop = true;

        songPlayerObjectAudioSource.Play();
    }

    /// <summary>
    /// Changes the gameobject that the song will play from
    /// </summary>
    /// <param name="parent">The new gameobject the sound will play from</param>
    public void PlayMusicAt(GameObject parent)
    {
        songPlayerObject.transform.SetParent(parent.transform);
        songPlayerObject.transform.position = Vector3.zero;
    }

    /// <summary>
    /// Switch the current song that is playing
    /// </summary>
    /// <param name="songId">The name of the song to play</param>
    public void SwitchSong(Song songID) => SwitchSong((int)songID);

    /// <summary>
    /// Switch the current song that is playing
    /// </summary>
    /// <param name="songId">An index into the "_songs" array</param>
    public void SwitchSong(int songId) => BeginAudioSwap(songId);

    #region --- Handle music Fade-in-out ---

    const float FADE_SPEED = 0.001f;    // The amount the volumn is changed per frame
    short fadingMusic = 0;              // The state of the music manager, 1: fade in, 0: idle, and -1: fade out
    int newSongId = 0;                  // The id is used to pull an audio clip from the "_songs" array
    float volume = 0;                   // The volume of the track, made seperate for accesibility and maluability reasons.
    float maxVolume = 1;                // The maximum volume of the music

    /// <summary>
    /// Begin / manage the fading and swapping sequence.
    /// </summary>
    /// <param name="songId">The index into <see cref="_songs"/> to pull</param>
    void BeginAudioSwap(int songId)
    {
        if (fadingMusic == -1) // The music is being faded out -> Sneaky change last second
        {
            newSongId = songId;
        }
        else if (fadingMusic == 0) // The music is not being faded in or out -> Fade the music out and switch!
        {
            newSongId = songId;
            volume = songPlayerObjectAudioSource.volume;
            maxVolume = volume;
            StartCoroutine(HandleFade());
        }
        else // The music is being faded in -> Fade the music back out and switch!
        {
            newSongId = songId;
            StopCoroutine(HandleFade());
            StartCoroutine(HandleFade());
        }
    }

    /// <summary>
    /// Handle the fading
    /// </summary>
    IEnumerator HandleFade()
    {
        // Fade out
        fadingMusic = -1;
        while (volume > 0)
        {
            volume -= FADE_SPEED;
            songPlayerObjectAudioSource.volume = volume;
            yield return new WaitForNextFrameUnit();

            Debug.Log(volume);
        }

        // Switch out the music
        songPlayerObjectAudioSource.clip = _songs[newSongId];
        songPlayerObjectAudioSource.Play();

        // Fade in
        fadingMusic = 1;
        while (volume < maxVolume)
        {
            volume += FADE_SPEED;
            songPlayerObjectAudioSource.volume = volume;
            yield return new WaitForNextFrameUnit();

            Debug.Log(volume);
        }

    }

    #endregion
}
