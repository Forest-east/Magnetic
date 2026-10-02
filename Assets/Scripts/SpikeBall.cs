using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spikeballs are enemy objects that will latch onto the player exploding on impact
/// The player may use a metal create to destroy them before without getting hurt
/// </summary>
public class SpikeBall : Attractable
{
    /// <summary>
    /// A reference to the prefab that simulates the explosion
    /// </summary>
    [SerializeField]
    GameObject explosionPrefab;

    /// <summary>
    /// True when the player collided with this 
    /// </summary>
    bool playerCollision = false;

    /// <summary>
    /// Sound to make on explode
    /// </summary>
    [SerializeField]
    AudioClip explosionClip;

    /// <summary>
    /// How fast to rotate this object
    /// </summary>
    [SerializeField]
    float rotationSpeed = 6f;

    /// <summary>
    /// Rotate this object
    /// </summary>
    void LateUpdate()
    {
        this.transform.Rotate(new Vector3(0f, this.rotationSpeed * Time.deltaTime, 0f));
    }

    /// <summary>
    /// Changes the destination to be the invoker's position
    /// This is to allow the bomb to trigger on collision
    /// </summary>
    /// <param name="invoker"></param>
    public override void Attract(IMagnetic invoker)
    {
        base.Attract(invoker);
        this.destination = invoker.gameObject.transform.position;
    }

    /// <summary>
    /// On collision with player or another attractable trigger explosion
    /// If the collider is the player's then disable the player's movement
    /// and wait before restarting the level
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        // Player Death
        if(other.tag == "Player") {
            this.Explode(true);

        // Check if Spikeball is destroyed
        } else {
            Attractable attractable = other.gameObject.GetComponent<Attractable>();
            if(attractable != null) {
                this.Explode();
            }
        }
    }

    /// <summary>
    /// Triggers the explosion animation
    /// Disable colliders and rigid body so that it cannot be manipulated by physics
    /// If collision was with player then:
    ///     - disables the player's control
    ///     - it waits for a short time before restarting the level
    /// </summary>
    /// <param name="collidedWithPlayer"></param>
    void Explode(bool collidedWithPlayer = false)
    {
        this.playerCollision = collidedWithPlayer;

        // Prevent physics from happening
        this.rigidBody.detectCollisions = false;
        this.rigidBody.useGravity = false;
        foreach(Collider collider in GetComponents<Collider>()) {
            collider.enabled = false;
        }

        // Disable player controls
        // Stops the coroutine that keeps moving this towards the invoker
        if(collidedWithPlayer) {
            StopCoroutine("MoveToDestination");
            FindAnyObjectByType<PlayerController>().IsDisabled = true;
        }

        this.PlaySound(this.explosionClip);
        this.animator.SetTrigger("Explode");        
    }

    /// <summary>
    /// Loads the explosion particle effects
    /// </summary>
    public void ShowExplosion()
    {
        if(this.explosionPrefab != null) {
            Instantiate(this.explosionPrefab, this.transform.position, Quaternion.identity, this.transform);
        }
    }

    /// <summary>
    /// Called during the explosion animation to reload the level
    /// </summary>
    public void RestartLevel()
    {
        if(this.playerCollision) {
            this.levelController.RestartLevel();
        }
    }
}
