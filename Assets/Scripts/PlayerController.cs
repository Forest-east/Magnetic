using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player movement 
/// Handles attracting and repelling objects
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour, IMagnetic
{
    /// <summary>
    /// How many units per tile helps determine when moving how much to increate the 
    /// move vector to keep the grid-base movement
    /// </summary>
    int unitsPerTile = 2;

    /// <summary>
    /// A reference to the levelController
    /// </summary>
    LevelController levelController;

    /// <summary>
    /// Stores the directions the player wants to move
    /// </summary>
    Vector3 inputVector = Vector3.zero;

    /// <summary>
    /// Where the player wants to move towards
    /// </summary>
    Vector3 destination;

    /// <summary>
    /// How close to the destination before snapping into place
    /// </summary>
    [SerializeField]
    float distancePadding;

    /// <summary>
    /// How fast the player moves
    /// </summary>
    [SerializeField]
    float speed;
    public float Speed { get { return this.speed; } }

    /// <summary>
    /// Total tiles the player can push/repel and object away
    /// </summary>
    [SerializeField]
    int repelTileDistance = 2;
    public int RepelTileDistance
    {
        get
        {
            return this.repelTileDistance;
        }
    }

    /// <summary>
    /// How many "tiles" away from the player can an object be attracted from
    /// </summary>
    [SerializeField]
    int attractionDistance = 2;

    /// <summary>
    /// True for as long as the player is triggering this action
    /// </summary>
    bool isAttracting = false;

    /// <summary>
    /// True when the player is waiting for object to be "attached"
    /// </summary>
    [SerializeField]
    bool waitForAttachables = false;

    /// <summary>
    /// True when the player was waiting for attacheables and they
    /// are all attached
    /// </summary>
  //  [SerializeField]
   // bool doneWaitingForAttachables = false;

    /// <summary>
    /// The last position the player was when they invoked the "attraction" action
    /// This helps prevent the attraction logic from happening more than onces per
    /// tile the player stands on and allows them to move
    /// </summary>
    [SerializeField]
    Vector3 attractedFromPosition = new Vector3(-1, -1f, -1f);

    /// <summary>
    /// A list of objects attracted to the player
    /// </summary>
    List<IAttractable> attractables = new List<IAttractable>();

    /// <summary>
    /// A list of objects the player is pending to have attached to them
    /// </summary>
    List<IAttractable> attractablesPending = new List<IAttractable>();

    /// <summary>
    /// True when the player repelled objects
    /// Remains true while the sound plays
    /// </summary>
    [SerializeField]
    bool isRepelling = false;

    /// <summary>
    /// True when the player has not reached their destination
    /// </summary>
    [SerializeField]
    bool isMoving = false;
    
    /// <summary>
    /// True when the player is allowed 
    /// </summary>
    bool canMove = true;

    /// <summary>
    /// True when the player can no longer be controlled
    /// and is waiting to either respawn or the level to reload
    /// </summary>
    bool isDisabled = false;
    public bool IsDisabled
    {
        get
        {
            return this.isDisabled;
        }
        set
        {
            this.isDisabled = value;
        }
    }

    /// <summary>
    /// The layer to look for when casting the ray during the attract
    /// </summary>
    [SerializeField]
    LayerMask attractableLayer;

    /// <summary>
    /// How high from the pivot point to calculate the raycast from
    /// </summary>
    private float raycastHeight = 0.25f;

    /// <summary>
    /// A reference to the audio source component
    /// </summary>
    protected AudioSource audioSource;

    /// <summary>
    /// Sound played while the player is attracting
    /// </summary>
    [SerializeField]
    AudioClip attractClip;

    /// <summary>
    /// Sound played while the player is holding objects
    /// </summary>
    [SerializeField]
    AudioClip holdClip;

    /// <summary>
    /// Sound played when the player repels objects held
    /// </summary>
    [SerializeField]
    AudioClip repelClip;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start ()
    {
        this.levelController = FindAnyObjectByType<LevelController>();
        this.destination = this.transform.position;
        this.audioSource = GetComponent<AudioSource>();
        this.unitsPerTile = this.levelController.unitsPerTile;
	}

    /// <summary>
    /// Updates the player's input
    /// </summary>
    void Update()
    {
        // Can't do anything
        if(this.isDisabled) {
            return;
        }

        this.SetInputVector();

        // Can perform actions while moving
        if(!this.isMoving) {
            this.SetPlayerAction();
        }

        // Play the sounds that match the new player actions
        this.PlayActionSounds();

        // Disable movement while attracting action is happenig
        if(this.canMove) {
            this.Move();
        }
    }

    /// <summary>
    /// Determines whether the player wants to:
    ///     - Start attracting 
    ///     - Cancel attract
    ///     - Repel objects
    /// </summary>
    void SetPlayerAction()
    {
        // Default to false
        this.canMove = false;
        bool objectsPending = this.attractablesPending.Count > 0;
        this.isAttracting = Input.GetButton("Fire1");

        // Check if we can attrack
        if(this.isAttracting) {
            this.Attract();

        // There are objects still being attached to the player
        // But player has opted to stop this
        } else if(!this.isAttracting && objectsPending) {
            this.CancelAttraction();
        
        // Push any objects away
        } else if(!this.isAttracting) {
            this.Repel();
        }
                
        // Re-evaluate because we may have some now
        objectsPending = this.attractablesPending.Count > 0;

        // No longer waiting on anything
        // Can move even if the is Attracting is enabled
        if(!objectsPending) {
            this.canMove = true;
        }
    }

    /// <summary>
    /// Based on the state of the player plays a sound to match that state
    /// </summary>
    void PlayActionSounds()
    { 
        // Gives priority to this clip
        if(this.isRepelling) {
            this.isRepelling = false;
            this.PlaySound(this.repelClip, true); 
        }

        // Attracting sound is played only when no objects are being held
        if(this.isAttracting && this.attractables.Count < 1) {
            this.PlayLoopSound(this.attractClip);
        }

        // While the player is holding objects play the hold sound
        if(this.isAttracting && this.attractables.Count > 0) {
            this.PlayLoopSound(this.holdClip);            
        }
    }

    /// <summary>
    /// Casts a ray in all direction to find new objects within range to 
    /// attract them to the player 
    /// </summary>
    void Attract()
    {
        // We've calculated from this position so prevent doing so again
        this.attractedFromPosition = this.transform.position;

        // The directions we will fire the raycast in attempts to 
        // catch something that is attractable
        List<Vector3> directions = new List<Vector3>() {
            Vector3.forward,
            Vector3.left,
            Vector3.back,
            Vector3.right,
        };

        // where the ray will start from
        Vector3 origin = new Vector3(this.transform.position.x, this.raycastHeight, this.transform.position.z);

        foreach(Vector3 direction in directions) {
            // Show a line in the editor to see when we are calculating for attraction
            Debug.DrawLine(origin, this.transform.position + direction * (this.attractionDistance* this.unitsPerTile));

            // The ray destination needs to be calculated with units per tile in mind
            int rayDestination = this.attractionDistance * this.unitsPerTile;
            Ray ray = new Ray(origin, direction);
            RaycastHit hitInfo;

            if(Physics.Raycast(ray, out hitInfo, rayDestination, this.attractableLayer)) {

                IAttractable attractable = hitInfo.collider.GetComponent<IAttractable>();
                if(attractable != null) {

                    // Ignore falling objects
                    if(attractable.IsFalling) {
                        continue;
                    }

                    // Found a new attractable item
                    bool isNew = !this.attractables.Contains(attractable) && !this.attractablesPending.Contains(attractable);
                    if(isNew) {
                        this.attractablesPending.Add(attractable);
                        attractable.Attract(this);
                    }
                }
            }
        } // foreach        
    }

    /// <summary>
    /// Called when the had invoke objects to be attracted but cancelled
    /// before they point of "no return" 
    /// </summary>
    void CancelAttraction()
    {
        foreach(IAttractable attractable in this.attractablesPending) {
            attractable.CancelAttract();
        }
    }

    /// <summary>
    /// Pushes all attached objects away
    /// </summary>
    void Repel()
    {
        foreach(IAttractable attractable in this.attractables) {
            this.isRepelling = true;
            this.attractablesPending.Add(attractable);
            attractable.Repel(this);
        }

        this.attractables.Clear();
    }

    /// <summary>
    /// Stores the player's movement input as well as their action input
    /// </summary>
    void SetInputVector()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

         // Horizontal takes prescedence
        if(h != 0 && v != 0) {
            v = 0f;
        }

        // Multiply by the units per tile since the player always moves one unit at a time
        this.inputVector = new Vector3(h, 0f, v) * this.unitsPerTile;
    }

    /// <summary>
    /// Handles moving the player to the desired destination
    /// as well as any and all objects the player is currently attached to
    /// unless the player is attached to a wall, the wall does not move
    /// </summary>
    void Move()
    {
        float distance = Vector3.Distance(this.destination, this.transform.position);

        // Made it, snap to destination and update the destination based on player input
        if(distance <= this.distancePadding) {
            this.isMoving = false;
            this.transform.position = this.destination;

            // Snap the childs too
            foreach(IAttractable attractable in this.attractables) {
                Transform goTransform = attractable.ObjectTransform;
                goTransform.position = this.destination + attractable.FollowDestination;
            }
            
            // Where the player could potentially move
            Vector3 targetDestination = this.transform.position + this.inputVector;

            // If all attached objects can occupy the target destinaiton
            // then the player can move 
            bool childrenCanMove = true;
            foreach(IAttractable attractable in this.attractables) {
                Transform goTransform = attractable.ObjectTransform;
                Vector3 targetPosition = targetDestination + attractable.FollowDestination;

                childrenCanMove = this.levelController.CanAttachableMoveToPosition(targetPosition);

                if(!childrenCanMove) {
                    break;
                }
            }

            if(childrenCanMove && this.levelController.IsPositionAvailable(targetDestination)) {
                this.destination = targetDestination;
            }

        // Still Moving
        } else {
            this.isMoving = true;
            Vector3 targetDestination = Vector3.Lerp(this.transform.position, this.destination, this.speed * Time.deltaTime);
            this.transform.position = targetDestination;

            // Move all attached objects
            foreach(IAttractable attractable in this.attractables) {
                Transform goTransform = attractable.ObjectTransform;
                Vector3 targetPosition = this.destination + attractable.FollowDestination;                
                targetDestination = Vector3.Lerp(goTransform.position, targetPosition, this.speed * Time.deltaTime);
                goTransform.position = targetDestination;
            }
        }
    }

    /// <summary>
    /// Invoked by an attractable that has finished attaching itself to the player
    /// </summary>
    /// <param name="attractable"></param>
    public void Attach(IAttractable attractable)
    {
        this.attractables.Add(attractable);
        this.attractablesPending.Remove(attractable);
    }

    /// <summary>
    /// Detaches the object 
    /// Typically invoked after the player has cancelled the attracting action
    /// or perhaps the object is no longer available
    /// </summary>
    /// <param name="attractable"></param>
    public void Detach(IAttractable attractable)
    {
        this.attractablesPending.Remove(attractable);
    }

     /// <summary>
    /// Plays the given sound clip once
    /// </summary>
    /// <param name="clip"></param>
    void PlaySound(AudioClip clip, bool isHighPrority = false)
    {
        // Wait for current sound to stop
        if(this.audioSource.isPlaying && !isHighPrority) {
            return;
        }

        //this.audioSource.loop = false;

        // Not the current sound
        if( this.audioSource.clip != clip ) {
            this.audioSource.Stop();
            this.audioSource.clip = clip;
        }

        // Play it only if it is not currently playing
        if(!this.audioSource.isPlaying) {
            this.audioSource.Play();
        }
    }

    /// <summary>
    /// Loops the given sound clip
    /// Triggers the play only once
    /// Ensures it is the only clip playing
    /// </summary>
    /// <param name="clip"></param>
    void PlayLoopSound(AudioClip clip)
    {
        this.audioSource.loop = true;

        // Not the current sound
        if( this.audioSource.clip != clip ) {
            this.audioSource.Stop();
            this.audioSource.clip = clip;
        }

        // Play it only if it is not currently playing
        if(!this.audioSource.isPlaying) {
            this.audioSource.Play();
        }
    }    
}
