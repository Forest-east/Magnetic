using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles opening and closing the level menu
/// This is opened either by the player or when
/// the level is completed 
/// </summary>
public class LevelMenu : MonoBehaviour
{
    /// <summary>
    /// A reference to the GO holing the menu
    /// This allows us to disable/enable it
    /// </summary>
    [SerializeField]
    GameObject menuGO;

    /// <summary>
    /// True when the menu is opened
    /// </summary>
    bool menuIsOpened = false;

    /// <summary>
    /// Used to prevent opening and closing the menu too quickly
    /// </summary>
    bool buttonPressed = false;

	/// <summary>
    /// Initialize
    /// </summary>
	void Start ()
    {
        this.menuGO.SetActive(false);
	}
	
	/// <summary>
    /// Listens for the player to open or closed the menu
    /// </summary>
	void Update ()
    {
        if(Input.GetKey(KeyCode.Escape)) {
            
            // Open the menu
            if(!this.buttonPressed && !this.menuIsOpened) {
                this.buttonPressed = true;
                this.OpenMenu();
            }

            // Close the menu
            if(!this.buttonPressed && this.menuIsOpened) {
                this.buttonPressed = true;
                this.CloseMenu();
            }
        } else {
            this.buttonPressed = false;
        }
	}

    /// <summary>
    /// Opens the menu only once
    /// Show Cursor
    /// </summary>
    public void OpenMenu()
    {
        // Done
        if(this.menuIsOpened) {
            return;
        }

        Cursor.visible = true;
        this.menuIsOpened = true;
        this.menuGO.SetActive(true);
        FindAnyObjectByType<PlayerController>().IsDisabled = true;
    }

    /// <summary>
    /// Closes the menu only once
    /// Hides the cursor
    /// </summary>
    public void CloseMenu()
    {
        // Done
        if(!this.menuIsOpened) {
            return;
        }

        Cursor.visible = false;
        this.menuIsOpened = false;
        this.menuGO.SetActive(false);
        FindAnyObjectByType<PlayerController>().IsDisabled = false;
    }
}
