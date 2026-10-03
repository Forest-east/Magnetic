using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Metal crate class
/// </summary>
public class MetalCrate : Attractable
{
    /// <summary>
    /// Sound to play when the crate attaches to the player
    /// </summary>
    [SerializeField]
    AudioClip attachedClip;

    /// <summary>
    /// Sound to play when the crate is falling
    /// </summary>
    [SerializeField]
    AudioClip fallingClip;

    /// <summary>
    /// How fast should the crate we falling before playing the falling sound
    /// </summary>
    [SerializeField]
    float fallSpeedTrigger = -4;

    /// <summary>
    /// Prevents the sound from being played more than once
    /// </summary>
    bool soundPlayed = false;

    void FixedUpdate()
    {
        int speed = (int)this.rigidBody.linearVelocity.y;
        if(speed <= this.fallSpeedTrigger && !this.soundPlayed) {
            this.soundPlayed = true;
            this.PlaySound(this.fallingClip);
        }
    }

    /// <summary>
    /// Plays the connected with player sound
    /// </summary>
    protected override void PlayAttractedSound()
    {
        this.PlaySound(this.attachedClip);
    }

    /// <summary>
    /// Resets the played sound flag
    /// </summary>
    public override void Respawn()
    {
        this.rigidBody.linearVelocity = Vector3.zero;
        this.soundPlayed = false;
        base.Respawn();
    }
}
