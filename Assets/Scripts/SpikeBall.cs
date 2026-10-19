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
    /// How fast to hover this object
    /// </summary>
    [SerializeField]
    float hoverSpeed = 6f;

    /// <summary>
    /// The object currently attracting this ball (normally the player)
    /// </summary>
    IMagnetic attractedBy;

    /// <summary>
    /// True once the explosion sequence has been started
    /// Prevents the bomb from exploding twice
    /// </summary>
    bool explosionStarted = false;

    /// <summary>
    /// How close this ball has to get to the object pulling it before it counts
    /// as a hit. Matches roughly the sum of the two collider half sizes.
    /// </summary>
    const float contactDistance = 1.7f;

    /// <summary>
    /// Rotate this object.
    /// While being attracted, make sure the bomb actually reaches the attractor.
    /// Relying on the physics trigger alone is not reliable: the ball only starts
    /// moving after the "Engaged" animation event, so if the player stops holding
    /// (or the animation is interrupted) it never travels, ends up parked next to
    /// the player and can be pushed away harmlessly. That made grabbing a spike
    /// ball sometimes safe and sometimes fatal.
    /// </summary>
    void LateUpdate()
    {
        this.transform.Rotate(new Vector3(0f, this.rotationSpeed * Time.deltaTime, 0f));

        if(this.explosionStarted || !this.isBeingAttracted || this.attractedBy == null) {
            return;
        }

        Vector3 attractorPosition = this.attractedBy.gameObject.transform.position;
        bool touching = Vector3.Distance(this.transform.position, attractorPosition) <= contactDistance;

        // Reached the attractor, or ended up attached to it instead of exploding
        if(touching || this.IsAttached) {
            this.Explode(true);
        }
    }

    /// <summary>
    /// Changes the destination to be the invoker's position
    /// This is to allow the bomb to trigger on collision
    /// </summary>
    /// <param name="invoker"></param>
    public override void Attract(IMagnetic invoker)
    {
        this.attractedBy = invoker;
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
        // Only ever explode once. LateUpdate may also fire the explosion, so this
        // guards against a double trigger when the physics collider reports first.
        if(this.explosionStarted) {
            return;
        }
        this.explosionStarted = true;

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
            FindObjectOfType<PlayerController>().IsDisabled = true;
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
    /// Called after the explosion animation is done by the animator
    /// To either restart the level if it collided with the player or
    /// disable itself if it was destroyed by anything else
    /// </summary>
    public void ExplosionCompleted()
    {
        if(this.playerCollision) {
            this.levelController.RestartLevel();
        } else {
            this.gameObject.SetActive(false);
        }
    }
}
