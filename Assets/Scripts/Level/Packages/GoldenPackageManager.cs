using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenPackageManager : MonoBehaviour
{
    public int numberOfGoldenPackages;
    
    private GameObject[] goldenPackages;
    
    void Start()
    {
        goldenPackages = GameObject.FindGameObjectsWithTag("GoldenPackage");

        SetGoldenPackagesToInactive();
        ActivateRandomGoldenPackages(numberOfGoldenPackages);
    }

    private void SetGoldenPackagesToInactive()
    {
        for (int i = 0; i < goldenPackages.Length; i++)
        {
            goldenPackages[i].SetActive(false);
        }
    }
    
    private void ActivateRandomGoldenPackages(int numberOfPackages)
    {
        List<int> randomIntegers = new List<int>();

        for (int i = 0; i < numberOfPackages; i++)
        {
            var newRandomInt = Random.Range(0, goldenPackages.Length);
            if (!randomIntegers.Contains(newRandomInt))
            {
                randomIntegers.Add(newRandomInt);
                continue;
            }
            i--;
        }      

        foreach (int randomNum in randomIntegers)
        {
            goldenPackages[randomNum].SetActive(true);
        }
    }
}
