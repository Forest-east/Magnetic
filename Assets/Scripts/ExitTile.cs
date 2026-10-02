using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Renderer))]
public class ExitTile : Tile
{
    /// <summary>
    /// Total switches required to open the exist
    /// </summary>
    int totalSwitches = 0;

    /// <summary>
    /// Total switches enabled
    /// </summary>
    int activeSwitches = 0;

    /// <summary>
    /// True when not all expected switches have been enabled
    /// </summary>
    bool DoorIsLocked
    {
        get { return this.activeSwitches != this.totalSwitches; }
    }

    /// <summary>
    /// Material to display when the exit is still locked
    /// </summary>
    [SerializeField]
    Color lockedColor;

    /// <summary>
    /// Material to display when the exit is unlocked
    /// </summary>
    [SerializeField]
    Color openedColor;

    /// <summary>
    /// Holds a reference to renderer component
    /// </summary>
    [SerializeField]
    Renderer childRenderer;

    /// <summary>
    /// Holds a reference to particle system
    /// </summary>
    [SerializeField]
    ParticleSystem particle;

    /// <summary>
    /// The name of the scene to teleport the player to when this tile is reached
    /// </summary>
    [SerializeField]
    string nextLevelName;

    /// <summary>
    /// True when the player enters this tile after turning the switches on
    /// </summary>
    bool winIsTriggered = false;

    /// <summary>
    /// How long to wait before loading the next scene
    /// </summary>
    [SerializeField]
    float sceneLoadDelay = 1f;

    /// <summary>
    /// Sound to play when the exit is activated
    /// </summary>
    [SerializeField]
    AudioClip activedClip;

    /// <summary>
    /// Sound to play when the exit is deactivated 
    /// </summary>
    [SerializeField]
    AudioClip deactivatedClip;

    /// <summary>
    /// Sound to play when the player is on the exit
    /// </summary>
    [SerializeField]
    AudioClip exitSound;



    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        this.type = Type.Exit;

        this.childRenderer.materials[1].color = this.lockedColor;

        // Fail safe, set it to self if next scene is unknown
        if(string.IsNullOrEmpty(this.nextLevelName)) {
            this.nextLevelName = SceneManager.GetActiveScene().name;
        }

        // Register the switches events to know when a switch is activated/deactivated
        foreach(SwitchTile switchTile in FindObjectsByType<SwitchTile>()) {
            this.totalSwitches++;
            switchTile.SwitchActivatedEvent += this.OnSwitchOn;
            switchTile.SwitchDeactivatedEvent += this.OnSwitchOff;
        }
    }
    
    /// <summary>
    /// Listens for the switch to delegate becoming active
    /// Increases the counter of active switches
    /// </summary>
    public void OnSwitchOn()
    {
        this.activeSwitches++;

        // All switches are active
        if( !this.DoorIsLocked) {
            this.PlaySound(this.activedClip);
            particle.Play();
            this.childRenderer.materials[1].color = this.openedColor;
        }
    }

    /// <summary>
    /// Listens for the switch to delegate becoming unactive
    /// Decreases the counter of active switches
    /// </summary>
    public void OnSwitchOff()
    {
        this.activeSwitches--;
        // All switches are deactive
        if( this.DoorIsLocked) {
            this.PlaySound(this.deactivatedClip);
            particle.Stop();
            this.childRenderer.materials[1].color = this.lockedColor;
        }
    }

    /// <summary>
    /// Only when all switches have been turned is when this becomes enabled
    /// </summary>
    /// <returns></returns>
    public override bool IsWalkable()
    {
        return true;
    }
    
    /// <summary>
    /// Player Entered this tile
    /// Trigger win condition
    /// </summary>
    public void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" && !this.winIsTriggered && !this.DoorIsLocked) {
            this.winIsTriggered = true;
            this.PlaySound(this.exitSound);
            StartCoroutine(this.LoadSceneAfterSeconds(this.nextLevelName, this.sceneLoadDelay));
        }
    }

    /// <summary>
    /// Loads the given scene after the given time has passed
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator LoadSceneAfterSeconds(string sceneName, float time)
    {
        yield return new WaitForSeconds(time);

        if(sceneName == "MainMenu") {
            FindAnyObjectByType<LevelController>().MainMenu();
        } else {
            SceneManager.LoadScene(sceneName);
        }        
    }
}

