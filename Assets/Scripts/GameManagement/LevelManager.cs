using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager
{

    static LevelManager instance;

    long score = 0;
    SubGoalManager subGoalManager = null;
    float startTime = 120;
    public float timerTime = 0;
    TMP_Text TimerString;
    TMP_Text ScoreString;
    Image TruckHealthImg;
    Image PlayerHealthImg;

    float refLen;
    GameObject HUD;
    public bool decreasingTimer = false;

    public void setPlayerHealthBar(int ht)
    {
        PlayerHealthImg.fillAmount = (float)ht / (float)100;
    }
    int playerHealthCache = 100;
    public int getPlayerHealthPercentage()
    {
        var trc = GameObject.Find("root_Player");
        if (trc)
        {
            playerHealthCache = trc.GetComponent<CharacterMovement>().getHealthPercentage();
        }
        setPlayerHealthBar(playerHealthCache);
        return playerHealthCache;

    }
    public void setTruckHealthBar(int ht)
    {
        TruckHealthImg.fillAmount = (float)ht / (float)100;
    }
    public int getTruckHealthPercentage()
    {
        var trc = GameObject.Find("Truck");
        setTruckHealthBar(trc.GetComponent<CarController>().getHealthPercentage());
        return trc.GetComponent<CarController>().getHealthPercentage();
    }
    public void addScore(long val)
    {
        score += val;
        ScoreString.SetText("Score: "+ score);
    }
    public long getScore()
    {
        return score;
    }

    public static LevelManager get()
    {
        return instance;
    }
    public bool isTutorial = false;
    public LevelManager(bool tutorial = false)
    {
        isTutorial = tutorial;
        endState = false;
        instance = this;
        //load game scene
        //level = new LevelManager();
        Time.timeScale = 1;

        timerTime = startTime;
        decreasingTimer = false;//by default make it an increasing timer
        timerTime = 0;
        score = 0;

        if (!isTutorial)
        {

            ScoreString = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.GamePlaying).transform.Find("Score").gameObject.GetComponent<TMP_Text>();
            ScoreString.SetText("Score: " + score);
            TimerString = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.GamePlaying).transform.Find("Timer").gameObject.GetComponent<TMP_Text>();
            TruckHealthImg = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.GamePlaying).transform.Find("TruckHealth").gameObject.transform.Find("TruckHealthBarInside").GetComponent<Image>();
            PlayerHealthImg = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.GamePlaying).transform.Find("PlayerHealth").gameObject.transform.Find("PlayerHealthBarInside").GetComponent<Image>();

        }
        else
        {

            UnityEngine.SceneManagement.SceneManager.LoadScene(3);
            GameManagerScript.get().setState(GameManagerScript.GameState.GamePlaying);
        }

        HUD = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.GamePlaying);
    }

    // Start is called before the first frame update


    bool sceneLoaded = false;



    void showIntermediateMenuFailed()
    {


        var subObjectiveStatus = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("SubObjectivesStatus").gameObject.GetComponent<TMP_Text>();
        subObjectiveStatus.SetText(subGoalManager.getSummary());

        GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("Score").gameObject.GetComponent<TMP_Text>().SetText("Score: " + score);

        GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("StarRating").gameObject.GetComponent<TMP_Text>().SetText("");

        var gmScript = GameManagerScript.get();
        //end game;
        Time.timeScale = 0;

        var menuStatus = gmScript.getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("MissionStatus").gameObject.GetComponent<TMP_Text>();
        menuStatus.SetText("Mission Failed");

        GameManagerScript.get().toggleIntermediateMenu();
    }

    // Update is called once per frame

    public bool endState = false;

    DateTime failedDateTime;
    bool alreadyFailed = false;
    public void Update()
    {


        if (isTutorial)
        {
            return;
        }
            //TODO remove this, just for demo
            /*if (timerTime > 0 && Time.timeScale > 0) { 
                setTruckHealth((int)(TruckHealthMax * timerTime / startTime));
                addScore(1);
            }*/
            //---------------



            var gmScript = GameManagerScript.get();

        if (gmScript.getState() == GameManagerScript.GameState.GameLoading)
        {
            Time.timeScale = 1;
            timerTime = 0;
            score = 0;
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            gmScript.setState(GameManagerScript.GameState.GamePlaying);
        }
        else if (!sceneLoaded && SceneManager.GetSceneByBuildIndex(1).isLoaded)
        {
            subGoalManager = new SubGoalManager(this);
            sceneLoaded = true;
        }
        if (sceneLoaded)
        {
            subGoalManager.Update();


        }
        getPlayerHealthPercentage();
        getTruckHealthPercentage();

        if ( !endState && mainMissionFailed() )
        {
            alreadyFailed = false;
            endState = true;
            failedDateTime = DateTime.Now;
        }
        else if (!endState && mainMissionPassed() && SceneManager.GetActiveScene().buildIndex!= 2)
        {

            endState = true;
            Time.timeScale = 0;
            showMissionSuccess();
        }
        else if(!endState)
        {
            updateTimer();
        }
        else if(!alreadyFailed && endState && mainMissionFailed() && (DateTime.Now-failedDateTime).TotalSeconds > 4)
        {
            alreadyFailed = true;
            Time.timeScale = 0;
            showIntermediateMenuFailed();

        }
    }

    bool hasMainMissionFailed = false;

    public void setMainMissionFailed()
    {
        hasMainMissionFailed = true;
    }
    bool mainMissionFailed()
    {
        if(LevelManager.get()!=null && LevelManager.get().timerTime < 5)
        {
            return false;
        }
        hasMainMissionFailed = getPlayerHealthPercentage() <= 0;
        return hasMainMissionFailed;
    }
    bool mainMissionPassed()
    {
        if (subGoalManager != null) { 
            return subGoalManager.allSubGoalsComplete(); //check if subgoals got completed before timer ran out
        }
        return false;
    }

    string getScoreString()
    {
        int rating = 1;

        string ratingStr = "";

        int timeBonus = 300 - Mathf.FloorToInt(timerTime);
        timeBonus = timeBonus <= 0 ? 0 : timeBonus;

        addScore(timeBonus);
        var score = getScore();

        if(score > 150)
        {
            rating++;
        }
        if (score > 200)
        {
            rating++;
        }
        if (score > 250)
        {
            rating++;
        }
        if (score >= 300)
        {
            rating++;
        }


        for (int i=0; i< rating - 1; i++)
        {
            ratingStr += "<sprite=3> ";
        }
        ratingStr += "<sprite=3>";

        return ratingStr;
    }

    void showMissionSuccess()
    {
        if(SceneManager.GetActiveScene().buildIndex == 3)
        {
            return;
        }
        if (timerTime >= 0)
        {
            long minutes = Mathf.FloorToInt(timerTime / 60);
            long seconds = Mathf.FloorToInt(timerTime % 60);
            //addScore(minutes*60 + seconds);
        }

        //end game as success;
        Time.timeScale = 0;
        var menuStatus = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("MissionStatus").gameObject.GetComponent<TMP_Text>();
        menuStatus.SetText("Mission Success\n" + "Star Rating\n");


        //calculate score

        GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("StarRating").gameObject.GetComponent<TMP_Text>().SetText(getScoreString());


        var subObjectiveStatus = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("SubObjectivesStatus").gameObject.GetComponent<TMP_Text>();
        subObjectiveStatus.SetText(subGoalManager.getSummary());

        GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.BetweenLevelsMenu).transform.Find("Score").gameObject.GetComponent<TMP_Text>().SetText("Score: " + score);
        GameManagerScript.get().toggleIntermediateMenu();
    }

    public void clearIndicators()
    {
        GameObject[] indicators = GameObject.FindGameObjectsWithTag("indicatorPkg");
        for (int i = 0; i < indicators.Length; i++)
        {
            GameObject.Destroy(indicators[i]);
        }
    }
    void updateTimer()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
            if (timerTime > 0 && decreasingTimer)
        {
            timerTime -= Time.deltaTime;
            long minutes = Mathf.FloorToInt(timerTime / 60);
            long seconds = Mathf.FloorToInt(timerTime % 60);
            if(timerTime < 0)
            {
                timerTime = 0;
                minutes = 0;
                seconds = 0;
            }
            TimerString.SetText(string.Format("Time Remaining {0:00}:{1:00}", minutes, seconds));
        }
        else if(!decreasingTimer)
        {
            timerTime += Time.deltaTime;
            long minutes = Mathf.FloorToInt(timerTime / 60);
            long seconds = Mathf.FloorToInt(timerTime % 60);
            
            TimerString.SetText(string.Format("Time {0:00}:{1:00}", minutes, seconds));
        }
        else
        {
            Time.timeScale = 0;
            clearIndicators();
            if (subGoalManager.allSubGoalsComplete()) {
                showMissionSuccess();
            }
            else
            {
                showIntermediateMenuFailed();
            }
        }
    }

}
