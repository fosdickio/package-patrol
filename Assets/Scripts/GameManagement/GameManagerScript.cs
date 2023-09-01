using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.SceneManagement;
public class GameManagerScript : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        GameLoading,
        GamePlaying,
        GamePaused,
        BetweenLevelsMenu,//the menu between two levels that shows score and stuff
        ShowingMap
    };

    Dictionary<GameState,GameObject> gameMenus = new Dictionary<GameState, GameObject>();
     
    static GameManagerScript gameManager;    
    private GameState gameState = GameState.MainMenu;
    int levelCounter = 1;
    private LevelManager level;
    public int scenario = 0;

    public static GameManagerScript get()
    {
        return gameManager;
    }

    public GameState getState()
    {
        return gameState;
    }
    public void setState(GameState gm)
    {
        gameState = gm;
    }

    public int GetLevel()
    {
        return levelCounter;
    }

    private string activeCameraName="vcam_Player";
    public string getActiveCameraName()
    {
        return activeCameraName;
    }
    public void setActiveCameraName(string camName)
    {
        activeCameraName = camName;
    }

    public void showControls()
    {
        controlsAndCredits.SetActive(true);
        GameObject.Find("ControlsAndCreditsText").GetComponent<UnityEngine.UI.Text>().text = "" +
            "Keyboard Controls\n\n" +
            "Move Player: WASD\n" +
            "Shoot Packages/Load Truck: F\n" +
            "Cycle Packages in Bazooka/Unload Truck: C\n" +
            "Spray Water: R\n" +
            "Drop Weapon: X\n" +
            "Display Large Minimap: M\n" +
            "Pause: Esc\n\n" +
            "Gamepad Controls\n\n" +
            "Move Player: Left Stick\n" +
            "Shoot Packages/Load Truck: South Button\n" +
            "Cycle Packages in Bazooka/Unload Truck: West Button\n" +
            "Spray Water: East Button\n" +
            "Drop Weapon: Keypad Down\n" +
            "Display Large Minimap: Select\n" +
            "Pause: Esc\n\n" +
            "";
    }

    public void showCredits()
    {

        controlsAndCredits.SetActive(true);
        GameObject.Find("ControlsAndCreditsText").GetComponent<UnityEngine.UI.Text>().text = "" +
            "Credits\n\n" +
            "The Holy Walkamolies:\n" +
            "Abhijit Singh, Brent Fosdick, James Coleman, Martin Tian\n\n" +
            "Audio Assets From:\n" +
            "Sidearm Studios, Ryding, Noctaro, MisterTood, nikorn69, Dionysuspsi\n\n" +
            "Graphic Assets From:\n" +
            "Synty Studios, Slava Z, polyperfect, Archanor VFX\n\n" +
            "";
    }
    public void closeWindow()
    {
        controlsAndCredits.SetActive(false);
    }

    GameObject controlsAndCredits;
    // Start is called before the first frame update

    GameObject ScenarioCanvas;
    void Start()
    {
        controlsAndCredits = GameObject.Find("ControlsAndCredits");
        controlsAndCredits.SetActive(false);
        level = null;
        gameManager = this;
        gameState = GameState.MainMenu;

        var canvas = this.gameObject.transform.Find("Canvas").gameObject;
        gameMenus.Add(GameState.MainMenu,canvas.transform.Find("MainMenu").gameObject);
        gameMenus.Add(GameState.GamePaused, canvas.transform.Find("PauseMenu").gameObject);
        gameMenus.Add(GameState.BetweenLevelsMenu, canvas.transform.Find("BetweenLevelsMenu").gameObject);
        gameMenus.Add(GameState.GamePlaying, canvas.transform.Find("HUD").gameObject);
        gameMenus.Add(GameState.ShowingMap, canvas.transform.Find("Map").gameObject);

        loadMainMenu();
    }

    // Update is called once per frame

    bool escReleased = false;


    private void toggleMap()
    {
        clearAllMenus();
        if (gameState==GameState.GamePlaying)
        {
            //pause game
            gameState = GameState.ShowingMap;
            getMenuFromState(gameState).SetActive(true);
            toggleHUD(false);
        }
        else
        {
            //resume game
            gameState = GameState.GamePlaying;
            getMenuFromState(GameState.GamePaused).SetActive(false);
            toggleHUD(true);
        }
    }


    void Update()
    {
        if ((InputSystem.devices.Count > 0 && Keyboard.current.mKey.wasReleasedThisFrame) || (Gamepad.all.Count > 0 && Gamepad.current.selectButton.wasReleasedThisFrame))
        {
            toggleMap();
        }
        else if ((InputSystem.devices.Count > 0 && Keyboard.current.escapeKey.wasReleasedThisFrame) || (Gamepad.all.Count >0  && Gamepad.current.startButton.wasReleasedThisFrame ))
        {
            if(gameState == GameState.GamePaused)
            {
                togglePauseMenu(false);
                Time.timeScale = 1;
            }
            else if (gameState == GameState.GamePlaying)
            {
                Time.timeScale = 0;
                togglePauseMenu(true);

            }
        }
        if (level!= null)
        {
            level.Update();
        }

    }

    void Awake()
    {
        if(this && this.gameObject)
            DontDestroyOnLoad(this.gameObject);
    }


    public static void exitGame()
    {
        Application.Quit();
    }

    public void playTutorial() //play tutorial
    {
        FindObjectOfType<AudioManager>().Play("ShootPackage");
        FindObjectOfType<AudioManager>().playThemeMusic();
        clearAllMenus();
        gameState = GameState.GameLoading;
        level = new LevelManager(true);
        ScenarioCanvas = GameObject.Find("ScenarioCanvas");
        toggleHUD(false);
    }

    public void playLevel(bool nextLevel) //if next level is true, next level is loaded, else current level is restarted
    {
        if(SceneManager.GetActiveScene().buildIndex == 3)
        {
            playTutorial();
            return;
        }
        FindObjectOfType<AudioManager>().Play("ShootPackage");
        FindObjectOfType<AudioManager>().playThemeMusic();
        if (nextLevel)
        {
            levelCounter++;
        }
        clearAllMenus();
        gameState = GameState.GameLoading;        
        level = new LevelManager(); 
        toggleHUD(true);
    }

    public bool isPlaytest()
    {
        return scenario != 0;
    }
    public void playtest(int scene)
    {

        scenario = scene;
        playLevel(false);
    }


    void clearAllMenus()
    {
        if (level != null)
        {
            level.clearIndicators();
        }
        foreach ( var kvPair in gameMenus )
        {
            kvPair.Value.SetActive(false);
        }
    }

    public GameObject getMenuFromState(GameState gs)
    {
        return gameMenus[gs];
    }

    private void togglePauseMenu(bool toggleOn)
    {
        if(ScenarioCanvas == null)
        {
            ScenarioCanvas = GameObject.Find("ScenarioCanvas");
        }
        clearAllMenus();
        if (toggleOn)
        {
            //pause game
            gameState = GameState.GamePaused;
            if (level.isTutorial && ScenarioCanvas!= null)
            {
                ScenarioCanvas.SetActive(false);
            }
            getMenuFromState(gameState).SetActive(true);
            if (!level.isTutorial)
            {
                toggleHUD(false);
            }
        }
        else
        {
            //resume game
            gameState = GameState.GamePlaying;
            if (level.isTutorial && ScenarioCanvas != null)
            {
                ScenarioCanvas.SetActive(true);
            }
            getMenuFromState(GameState.GamePaused).SetActive(false);
            if (!level.isTutorial)
            {
                toggleHUD(true);

            }
        }
    }

    public void toggleIntermediateMenu()
    {
        clearAllMenus();
        getMenuFromState(GameState.BetweenLevelsMenu).SetActive(true);

    }


    private void toggleHUD(bool toggleOn)
    {
        getMenuFromState(GameState.GamePlaying).SetActive(toggleOn);
    }


    public void loadMainMenu() // loads main menu
    {

        getMenuFromState(GameState.BetweenLevelsMenu).SetActive(false);
        clearAllMenus();
        SceneManager.LoadScene(2);
        FindObjectOfType<AudioManager>().playThemeMusic();
        gameState = GameState.MainMenu;
        getMenuFromState(gameState).SetActive(true);
    }
}
