using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationSounds : MonoBehaviour
{
    AudioSource animationSoundPlayer;
    public List<AudioClip> music;
    // Start is called before the first frame update
    void Start()
    {
        animationSoundPlayer = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlayerFootstepSound()
    {
        animationSoundPlayer.Stop();
        animationSoundPlayer.clip = music[0];
        animationSoundPlayer.Play();
    }
}
