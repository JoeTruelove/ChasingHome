using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationSounds : MonoBehaviour
{
    AudioSource animationSoundPlayer;
    public List<AudioClip> music;

    void Start()
    {
        animationSoundPlayer = GetComponent<AudioSource>();
    }
    private void PlayerFootstepSound()
    {
        animationSoundPlayer.Stop();
        animationSoundPlayer.clip = music[0];
        animationSoundPlayer.Play();
    }
}
