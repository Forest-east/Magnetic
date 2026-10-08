using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Handles high level transactions such as:
///     - Keep track of player's progress
///     - Play music based on loaded scene
/// 
/// Saves the levels unlocked
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton
    /// </summary>
    public static GameManager instance = null;

    /// <summary>
    /// A reference to the audio source component
    /// </summary>
    protected AudioSource audioSource;

    /// <summary>
    /// Audio to play on main menu
    /// </summary>
    [SerializeField]
    AudioClip menuMusicClip;

    /// <summary>
    /// Audio to play on each level
    /// </summary>
    [SerializeField]
    AudioClip levelMusicClip;

    /// <summary>
    /// The name of the first level to load
    /// </summary>
    [SerializeField]
    string firstLevelName;

    /// <summary>
    /// Name of the main menu for when the player wants to return
    /// </summary>
    [SerializeField]
    string mainMenuSceneName;

    /// <summary>
    /// A list of levels unlocked
    /// Starts with the fist level minimum
    /// </summary>
    [SerializeField]
    List<string> levelsUnlocked = new List<string>() { "Level_1" };

    /// <summary>
    /// The name/location of the save file
    /// </summary>
    string SaveFilePath;

    /// <summary>
    /// Returns true if the given level name is unlocked
    /// </summary>
    /// <param name="levelName"></param>
    /// <returns></returns>
    public bool IsLevelUnlocked(string levelName)
    {
        return this.levelsUnlocked.Contains(levelName);
    }

    /// <summary>
    /// Prevents more than once instance of the GameManager
    /// Loads last played session
    /// </summary>
    void Awake()
    {
        if(instance == null) {
            instance = this;

            // Fetch the audio source here (not in Start) so that it is ready
            // before any other script in the same frame calls into this singleton.
            // MainMenu.Start(), LevelController.Awake() and PlayerController.Start()
            // all run before GameManager.Start() and would otherwise hit a null.
            this.audioSource = GetComponent<AudioSource>();

            this.SaveFilePath = Application.persistentDataPath + "/magnetic.gd";
            this.LoadGame();
        } else {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        this.PlayMainMenuMusic();
    }

    /// <summary>
    /// Closes the app
    /// </summary>
    public void Exit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    /// <summary>
    /// Tranistions to the first level
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(this.firstLevelName);
        this.PlayLevelMusic();
    }

    /// <summary>
    /// Takes the player back to the main menu
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene(this.mainMenuSceneName);
        this.PlayMainMenuMusic();
    }

    /// <summary>
    /// Plays the music for a level
    /// </summary>
    public void PlayLevelMusic()
    {
        this.PlayLoopSound(this.levelMusicClip);
    }

    /// <summary>
    /// Plays the music for a level
    /// </summary>
    public void PlayMainMenuMusic()
    {
        this.PlayLoopSound(this.menuMusicClip);
    }

    /// <summary>
    /// Loops the given sound clip
    /// Triggers the play only once
    /// Ensures it is the only clip playing
    /// </summary>
    /// <param name="clip"></param>
    void PlayLoopSound(AudioClip clip)
    {
        // Guard: if the prefab has no AudioSource (or it was stripped),
        // skip audio instead of throwing every time a level loads.
        if(this.audioSource == null) {
            return;
        }

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

    /// <summary>
    /// Loads the given level
    /// </summary>
    /// <param name="level"></param>
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
        this.PlayLevelMusic();
    }

    /// <summary>
    /// Called when a level is loaded to add to the list of levels unlocked
    /// </summary>
    public void LevelUnlocked(string levelName)
    {
        // Already know about this level
        if(this.levelsUnlocked.Contains(levelName)) {
            return;
        }

        // Save the new level
        this.levelsUnlocked.Add(levelName);
        this.SaveGame();
    }

    /// <summary>
    /// Saves currently unlocked levels
    /// </summary>
    public void SaveGame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(this.SaveFilePath);
        bf.Serialize(file, this.levelsUnlocked);
        file.Close();
    } // SaveGame


    /// <summary>
    /// Loads unlocked levels
    /// </summary>
    public void LoadGame()
    {
        if( ! this.SaveFileExists() ) {
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(this.SaveFilePath, FileMode.Open);

        this.levelsUnlocked = (List<string>)bf.Deserialize(file);        
        file.Close();
    } // LoadGame


    /// <summary>
    /// Returns TRUE if the save file exists
    /// </summary>
    public bool SaveFileExists()
    {
        return File.Exists(this.SaveFilePath);
    } // SaveFileExists
}
