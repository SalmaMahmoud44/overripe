using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip levelMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Boss SFX")]
    public AudioClip mangoSmash;
    public AudioClip mangoDeath;

    [Header("Player SFX")]
    public AudioClip footstepClip;
    public AudioClip jumpClip;
    public AudioClip meleeClip;


    [Header("Interaction SFX")]
    public AudioClip juiceClip;
    public AudioClip juiceHit;
    public AudioClip artifactCollect;
    public AudioClip artifactAppearClip;

    [Header("UI SFX")]
    public AudioClip buttonHoverClip;
    public AudioClip buttonClickClip;

    [Header("Music")]
    [SerializeField] private AudioClip appleBossMusic;   

    [Header("Player SFX")]
    public AudioClip dashClip;        
    public AudioClip hitPlayerClip;   
    public AudioClip shootClip;
    public AudioClip laserClip;

    [Header("Enemy SFX")]
    public AudioClip hitEnemyClip;      
    public AudioClip soldierAttackClip;  
    public AudioClip soldierWalkClip;     

    [Header("Boss SFX")]
    public AudioClip rollClip;        
    public AudioClip appleSeedClip;  
    public AudioClip appleStickClip;  
    public AudioClip appleWalkClip;
    public AudioClip dustPuffClip;
    public AudioClip explosionClip;

    [Header("Timer SFX")]
    public AudioClip addTimeClip;     

    [Header("Music Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("SFX Settings")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public void PlayAppleBossMusic() => PlayMusic(appleBossMusic);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.playOnAwake = false;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu")
            PlayMainMenuMusic();
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null)
            return;

        if (musicSource.clip == music && musicSource.isPlaying)
            return;

        musicSource.clip = music;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayLevelMusic()
    {
        PlayMusic(levelMusic);
    }

    public void PlayBossMusic()
    {
        PlayMusic(bossMusic);
    }



    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayFootstep()
    {
        sfxSource.PlayOneShot(footstepClip);
    }

    public void PlayJump()
    {
        sfxSource.PlayOneShot(jumpClip);
    }

    public void PlayMelee()
    {
        sfxSource.PlayOneShot(meleeClip);
    }
    public void PlayJuiceSound()
    {
        sfxSource.PlayOneShot(juiceClip);
    }

    public void PlayButtonHover()
    {
        PlaySFX(buttonHoverClip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
   
   
}