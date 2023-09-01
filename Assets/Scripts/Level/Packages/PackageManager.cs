using System;
using UnityEngine;

public class PackageManager : MonoBehaviour
{
    public GameObject packagePrefab;
    public Material packageMaterial;

    private int totalPackages;
    
    void Awake()
    {
        System.Random rnd = new System.Random();
        int randomNumber = rnd.Next(2, 4);
        Vector3 containerPosition = this.gameObject.transform.position;
        float initialY = 0.5f;
        
        totalPackages = GameObject.FindGameObjectsWithTag("Package").Length;
        
        for (int i = 0; i < randomNumber; i++)
        {
            if (totalPackages < 12)
            {
                Vector3 pos = new Vector3(containerPosition.x, initialY, containerPosition.z);
                GameObject package = Instantiate(packagePrefab, pos, Quaternion.identity, transform);
                package.GetComponent<Renderer>().material = packageMaterial;
                initialY += 1.0f;
                totalPackages++;
            }
        }
    }
}
