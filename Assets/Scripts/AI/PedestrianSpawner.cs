using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{
    //public GameObject pedestrianPrefab;
    public int pedestiansToSpawn;

    private Waypoint[] waypoints;

    //public GameObject[] vehicles;
    public int totalPopulationMax = 20;
    public float maxDistance = 100f;
    public int totalAi = 0;
    public GameObject[] pedestrianPrefabs;
    public List<GameObject> spawnedAi;

    // Start is called before the first frame update

    void Awake()
    {
        loadArray();
        StartCoroutine(Spawn());
        managePopulation();
    }

    void loadArray()
    {
        waypoints = FindObjectsOfType<Waypoint>();
    }

    IEnumerator Spawn()
    {
        //int count = 0;
        //while (count < pedestiansToSpawn)
        //{
        //    Debug.Log("<color=yellow>Spawning NPC #: </color>" + count);
        //    GameObject obj = Instantiate(pedestrianPrefab);
        //    Debug.Log("<color=yellow>Instantiated object: </color>" + obj);
        //    Transform child = transform.GetChild(Random.Range(0, transform.childCount - 1));
        //    Debug.Log("<color=yellow>Child gotten: </color>" + child);
        //    obj.GetComponent<WaypointNavigator>().currentWaypoint = child.GetComponent<Waypoint>();
        //    obj.transform.position = child.position;

        //    Debug.Log("<color=yellow>Before return: </color>");
        //    yield return new WaitForEndOfFrame();

        //    count++;
        //}
        while (true)
        {
            yield return new WaitForSeconds(20);
            managePopulation();
        }
    }

    void managePopulation()
    {
        for (int i = 0; i < waypoints.Length; i += 3)
        {
            if (spawnedAi.Count <= totalPopulationMax)
                populationLoop(i);
        }

        for (int i = 0; i < spawnedAi.Count; i++)
        {
            if (Vector3.Distance(transform.position, spawnedAi[i].transform.position) >= maxDistance)
            {
                Destroy(spawnedAi[i]);
                spawnedAi.Remove(spawnedAi[i]);
            }
        }
        totalAi = spawnedAi.Count;
    }

    void populationLoop(int index)
    {
        if (Vector3.Distance(transform.position, waypoints[index].transform.position) <= maxDistance)
        {
            spawnPrefab(index);
        }
    }

    void spawnPrefab(int index)
    {
        GameObject i = Instantiate(pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Length - 1)], GameObject.Find("NPCs").transform);
        //i.GetComponent<WaypointNavigator>().currentWaypoint = waypoints[index].GetComponent<Waypoint>();
        i.GetComponent<AICharacterController>().currentWaypoint = waypoints[index].GetComponent<Waypoint>();
        i.transform.position = waypoints[index].transform.position;
        i.transform.Rotate(0, waypoints[index].transform.eulerAngles.y, 0);
        spawnedAi.Add(i);
    }
}
