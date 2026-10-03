using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Metal crate class
/// </summary>
public class MetalBall : Attractable
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

    /// <summary>
    /// How fast to "roll" when the ball is repelled
    /// </summary>
    [SerializeField]
    float rollAcceleration = 3f;

    /// <summary>
    /// How close to get to the current tile before checking for the next tile
    /// </summary>
    [SerializeField]
    float nextTilePadding = 0.05f;

    /// <summary>
    /// How fast can the ball roll
    /// </summary>
    [SerializeField]
    float maxRollSpeed = 5f;

    /// <summary>
    /// Stops the ball from rolling if it is falling
    /// </summary>
    //void FixedUpdate()
    //{
    //    int speed = (int)this.rigidBody.velocity.y;
    //    if(speed <= this.fallSpeedTrigger) {
    //        StopCoroutine("Roll");
    //    }
    //}

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

    /// <summary>
    /// Causes the ball to continuosly move until falling off or hitting
    /// another object
    /// </summary>
    /// <param name="invoker"></param>
    public override void Repel(IMagnetic invoker)
    {
        invoker.Detach(this);
        this.isAttached = false;
        this.isBeingAttracted = false;

        Vector3 destination = new Vector3(
            Mathf.Floor(this.transform.position.x),
            0f,
            Mathf.Floor(this.transform.position.z)
        );

        // Tile can be null or walkable
        // If it is not available then we have a destination
        for(int i = 1; i <= 20; i++) {
            Vector3 targetDestination = this.transform.position + this.positionedAt * i;

            if(this.levelController.IsTileAtPositionAvailable(targetDestination)) {
                destination = targetDestination;

                bool tileIsVoid = this.levelController.IsPositionVoid(destination);
                bool emptyHole = this.levelController.IsEmptyHoleTile(destination);

                // The ball needs to drop soon as it hits a gap - so make it fall
                // if the tile is a hole, the ball needs to drop
                if(tileIsVoid || emptyHole) {
                    break;
                }

            } else {
                break;
            }
        }

        this.destination = this.lastPosition = destination;        
        StartCoroutine("MoveToDestination", this.repelledSpeed);
    }
}
