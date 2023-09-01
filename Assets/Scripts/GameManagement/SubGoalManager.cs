using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

//this script should generate subgoals, pass them to the UI, and check their state
public class SubGoalManager
{
    LevelManager levelManager;
    TMP_Text objectiveListString;
    List<Subgoal> subGoals = new List<Subgoal>();
    public class Subgoal
    {
        string goal;
        Func<Action<string>,bool> conditionChecker;
        bool currStatus = false;
        bool statusChanged = false;
        bool overrideStatus = false;
        bool isImportant = false;
        //constructor with the goal string and a lambda that returns whether the completion condition is done

        //condition checker's input function can be used to update the goal string and consequently mark the status of Subgoal as changed
        public Subgoal(string goal, Func<Action<string>, bool> conditionChecker, bool isImportant = false)
        {
            this.isImportant = isImportant;
            this.goal = goal;
            this.conditionChecker = conditionChecker;
        }

        public string getGoalStr()
        {
            if (!isImportant)
            {
                return (currStatus ? "<sprite=1>" : "<sprite=0>") + " " + goal + "\n";

            }
            return (currStatus? "<sprite=1>" : "<sprite=0>") + " " + goal + "\n";
        }

        public void setGoalStr(string str)
        {
            goal = str;
            overrideStatus = true;
        }


        public bool subgoalStatusChanged()
        {
            if (overrideStatus)
            {
                overrideStatus = false;
                return true;
            }
            var condStat = conditionChecker(setGoalStr);
            statusChanged = currStatus != condStat;
            currStatus = condStat;
            return statusChanged;
        }
        public bool isSubGoalComplete()
        {
            return currStatus;
        }
        public bool isImportantMission()
        {
            return isImportant;
        }

    }

    public bool allSubGoalsComplete()
    {
        foreach (var g in subGoals)
        {
            if ((!g.isSubGoalComplete()) && g.isImportantMission())
            {
                return false;
            }
        }
        return true;
    }

    public string getSummary()
    {
        string stat = "Succeeded:\n";
        int succ = 0;
        int fail = 0;
        foreach (Subgoal sgb in subGoals)
        {
            if (sgb.isSubGoalComplete())
            {
                stat += sgb.getGoalStr();
                succ++;
            }
        }
        if (succ == 0)
        {
            stat += "None\n";
        }
        stat += "Failed:\n";
        foreach (Subgoal sgb in subGoals)
        {
            if (!sgb.isSubGoalComplete())
            {
                stat += sgb.getGoalStr();
                fail++;
            }
        }
        if (fail == 0)
        {
            stat += "None\n";
        }
        stat += "Total Complete = " + succ + "/" + (succ + fail) + "\n";
        return stat;
    }


    void generateSubgoalText()
    {
        //set subgoal strings
        objectiveListString.SetText("Objectives:\n");
        foreach (Subgoal sgb in subGoals)
        {
            objectiveListString.SetText(objectiveListString.text += sgb.getGoalStr());
        }
    }

    //generates subgoals TODO



    //bool tempSubgoalStatHolder1 = false, tempSubgoalStatHolder2 = false;


    Dictionary<string, int> packageColorToCount = new Dictionary<string, int>();
    Dictionary<string, List<GameObject>> currentPackageColorToCount = new Dictionary<string, List<GameObject>>();
    Dictionary<string, int> packageColorToCount2 = new Dictionary<string, int>();
    Dictionary<string, int> pkgColorToCount ()
    {
        var packageColorToCount = new Dictionary<string, int>();
        //now identify all the colors of the packages
        foreach (var pkg in GameObject.FindGameObjectsWithTag("Package"))
        {
            var col = FindColor(pkg.GetComponent<Renderer>().material.color);
            if (!currentPackageColorToCount.ContainsKey(col))
            {
                currentPackageColorToCount[col] = new List<GameObject>();
            }

            if (packageColorToCount.ContainsKey(col))
            {

                currentPackageColorToCount[col].Add(pkg);
                packageColorToCount[col] += 1;
                packageColorToCount2[col] += 1;
            }
            else
            {
                currentPackageColorToCount[col].Add(pkg);
                packageColorToCount[col] = 1;
                packageColorToCount2[col] = 1;
            }


        }
        return packageColorToCount;
    }


    void generatePackageGoals()
    {

        currentPackageColorToCount = new Dictionary<string, List<GameObject>>();
        packageColorToCount = pkgColorToCount();

        //for each color, add a subgoal
        foreach (var kvp in packageColorToCount)
        {
            var goalDescription = kvp.Key + " :0/" + kvp.Value;
            Func<Action<string>, bool> checker = (Action<string> act) => {

                //get current package count;
                /*var currCt = pkgColorToCount();

                int countInactives = 0;
                for ()
                {

                }*/

                int packagesDone = kvp.Value;
                /*if (currCt.ContainsKey(kvp.Key))
                {
                    packagesDone = packagesDone - currCt[kvp.Key];
                }*/
                int currCt = 0;
                for (int i =0; i< kvp.Value; i++)
                {
                    if (currentPackageColorToCount[kvp.Key][i])
                    {
                        currCt++;
                        packagesDone--;
                    }
                }


                bool packageCountChanged = false;
                var cur = packageColorToCount2[kvp.Key];
                if (cur != currCt)
                {
                    packageCountChanged = true;
                    levelManager.addScore(10*Math.Abs(packageColorToCount2[kvp.Key] - currCt));
                    packageColorToCount2[kvp.Key] = currCt;
                }

                //packages delivered string
                var goalDescriptionNew = kvp.Key + " :" + packagesDone + "/" + kvp.Value;

                if (packageCountChanged) { 
                    act(goalDescriptionNew);
                }
                return packagesDone == kvp.Value;
            };

            var subg = new Subgoal(goalDescription, checker, true);
            subGoals.Add(subg);
        }
    }

    int startingGoldenPackages;
    int currentGoldenPackages;
    void generateGoldenPackageGoals() {
        startingGoldenPackages = GameObject.FindGameObjectsWithTag("GoldenPackage").Length;
        currentGoldenPackages = startingGoldenPackages;
        Func<Action<string>, bool> checker = (Action<string> act) =>
        {
            var checkLen = 0;
            
            var pkgs = GameObject.FindGameObjectsWithTag("GoldenPackage");

            foreach (var pkg in pkgs)
            {
                if (pkg.active)
                {
                    checkLen++;
                }
            }

            if (checkLen != currentGoldenPackages)
            {
                levelManager.addScore(100 * Math.Abs(currentGoldenPackages - checkLen));
                //update string
                currentGoldenPackages = checkLen;
                var goalStr = "Bonus golden packages: "+(startingGoldenPackages- currentGoldenPackages) +"/" + startingGoldenPackages;
                act(goalStr);
            }

            return checkLen==0;
        };

        var goalStr = "Hidden Bonus packages: 0/" + startingGoldenPackages;
        var subg = new Subgoal(goalStr, checker, false);
        subGoals.Add(subg);
    }

    void generateSubGoals()
    {


        var subg3 = new Subgoal("Take no damage", (Action<string> act) => { return levelManager.getPlayerHealthPercentage() == 100; });
        subGoals.Add(subg3);

        generatePackageGoals();

        generateGoldenPackageGoals();

        generateSubgoalText();
    }

    //updates GUI if subgoals are accomplished
    void evaluateSubGoals()
    {
        bool redraw = false;
        foreach( Subgoal sgb in subGoals)
        {
            if (sgb.subgoalStatusChanged())
            {
                redraw = true;
            }
        }
        if (redraw)
        {
            generateSubgoalText();
        }
    }

    int totalPackages;
    int currLen;
    GameObject[] allpkgsStart;
    public SubGoalManager(LevelManager mg)
    {
        levelManager = mg;

        //clear out existing goals in the GUI
        objectiveListString = GameManagerScript.get().getMenuFromState(GameManagerScript.GameState.GamePlaying).transform.Find("Objectives").gameObject.GetComponent<TMP_Text>();


        totalPackages = GameObject.FindGameObjectsWithTag("Package").Length;
        allpkgsStart = GameObject.FindGameObjectsWithTag("Package");
        if (GameManagerScript.get().isPlaytest())
        {
            if (GameManagerScript.get().scenario==1)
            {
                var subg3 = new Subgoal("Deliver 5 packages to win:\ncurrent = 0", (Action<string> act) => {
                    currLen = totalPackages;

                    foreach(var v in allpkgsStart)
                    {
                        if (v==null)
                        {
                            currLen--;
                        }
                    }


                    act("Deliver 5 packages to win:\ncurrent = " + (totalPackages-currLen));
                    return currLen <= totalPackages-5;
                }, true);
                subGoals.Add(subg3);
            }
            else if (GameManagerScript.get().scenario == 2)
            {
                var subg3 = new Subgoal("Deliver all packages to win:\ncurrent = 0/"+ totalPackages, (Action<string> act) => {
                    currLen = totalPackages;

                    foreach (var v in allpkgsStart)
                    {
                        if (v == null)
                        {
                            currLen--;
                        }
                    }
                    act("Deliver all packages to win:\ncurrent = " + (totalPackages-currLen) + "/" + totalPackages);
                    return currLen == 0;
                }, true);
                subGoals.Add(subg3);

            }
            else
            {
                //no subgoals, maximise score
                var subg3 = new Subgoal("Current rating = 5/5", (Action<string> act) => {
                    currLen = totalPackages;

                    foreach (var v in allpkgsStart)
                    {
                        if (v == null)
                        {
                            currLen--;
                        }
                    }
                    //act("Deliver all packages: current = " + (totalPackages - currLen) + "/" + totalPackages);
                    var currSeconds = levelManager.timerTime;
                    int stars = 1;

                    if (currSeconds <= 180)
                    {
                        stars = 2;
                    }
                    if (currSeconds <= 120)
                    {
                        stars = 3;
                    }
                    if (currSeconds <= 90)
                    {
                        stars = 4;
                    }
                    if (currSeconds <=60)
                    {
                        stars = 5;
                    }
                    act("Current rating = "+ stars + "/5");
                    return currLen == 0;
                }, true);
                subGoals.Add(subg3);
            }
        }
        else
        {
            generateSubGoals();
            if (levelManager.decreasingTimer)
            {
            }
            else
            {/*
                //no subgoals, maximise score
                var subg3 = new Subgoal("Current rating = 5/5", (Action<string> act) => {
                    currLen = totalPackages;

                    foreach (var v in allpkgsStart)
                    {
                        if (v == null)
                        {
                            currLen--;
                        }
                    }
                    //act("Deliver all packages: current = " + (totalPackages - currLen) + "/" + totalPackages);
                    var currSeconds = levelManager.timerTime;
                    int stars = 1;

                    if (currSeconds <= 180)
                    {
                        stars = 2;
                    }
                    if (currSeconds <= 120)
                    {
                        stars = 3;
                    }
                    if (currSeconds <= 90)
                    {
                        stars = 4;
                    }
                    if (currSeconds <= 60)
                    {
                        stars = 5;
                    }
                    act("Current rating = " + stars + "/5"
                        );
                    return currLen == 0;
                }, true);
                subGoals.Add(subg3);

                */
            }
        }

        //todo generate subgoals and push GUI notifications

    }

    // Update is called once per frame
    public void Update()
    {
        //check if subgoals have been accomplished and check off GUI notifications
        evaluateSubGoals();
    }



    private static Dictionary<Vector3, string> _colors = new Dictionary<Vector3, string>();



    private static Dictionary<string, Color> _colors2 = new Dictionary<string, Color>()
    {
        ["Red"] = new Color(1f, 0.278431373f, 0.298039216f),

        ["Orange"] = new Color(0.941176471f, 0.580392157f, 0.301960784f),

        ["Yellow"] = new Color(0.933333333f, 0.862745098f, 0.356862745f),

        ["Blue"] = new Color(0.0509803922f, 0.458823529f, 0.97254902f),

        ["Green"] = new Color(0.337254902f, 0.682352941f, 0.341176471f),

        ["Purple"] = new Color(0.768627451f, 0.556862745f, 0.992156863f),
    };


    
    public static string FindColor(Color color)
    {
        float nearest = 99;
        string nameColor = "";
        Vector3 cin = new Vector3(color.r, color.g, color.b);

        foreach (KeyValuePair<string, Color> entry in _colors2)
        {
            // do something with entry.Value or entry.Key
            Vector3 found = new Vector3(entry.Value.r, entry.Value.g, entry.Value.b);

            if (Vector3.Distance(found, cin) < nearest)
            {
                nameColor = entry.Key;
                nearest = Vector3.Distance(found, cin);
            }
        }
        return (nameColor);
    }
    /*
    public static string FindColor(Color color)
    {
        if (_colors.Count == 0)
        {
            foreach(var kvp in _colors2)
            {
                _colors[new Vector3(kvp.Value.r, kvp.Value.g, kvp.Value.b)] = kvp.Key;
            }
        }
        return _colors[new Vector3(color.r,color.g,color.b)];
    }
    */
}


