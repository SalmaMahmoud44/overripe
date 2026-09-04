using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] AudioClip footstepClip;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip meleeClip;
    [SerializeField] AudioClip juiceClip;

   
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

    }

    public void PlayFootstep()
    {
        audioSource.PlayOneShot(footstepClip);
    }

    public void PlayJump()
    {
        audioSource.PlayOneShot(jumpClip);
    }

    public void PlayMelee()
    {
        audioSource.PlayOneShot(meleeClip);
    }
    public void PlayJuiceSound()
    {
        audioSource.PlayOneShot(juiceClip);
    }
}
