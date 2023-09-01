using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryPadManager : MonoBehaviour
{
    public Material[] materials;
    
    private GameObject[] totalPackages;
    private GameObject[] deliveryPads;
    private Stack<Material> packageMaterials;
    
    void Start()
    {
        totalPackages = GameObject.FindGameObjectsWithTag("Package");
        deliveryPads = GameObject.FindGameObjectsWithTag("DeliveryPad");
        packageMaterials = new Stack<Material>();

        LoadPackageMaterials();
        SetDeliveryPadsToInactive();
        //ActivateDeliveryPads();
        ActivateRandomDeliveryPads();
        DeactivatePadsOfMaterialsNotPresent();
    }

    void LoadPackageMaterials()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            packageMaterials.Push(materials[i]);
        }
    }
    
    void SetDeliveryPadsToInactive()
    {
        for (int i = 0; i < deliveryPads.Length; i++)
        {
            deliveryPads[i].SetActive(false);
        }
    }
    
    void ActivateDeliveryPads()
    {
        var random = new System.Random();
        var indices = new List<int>();

        for (int i = 0; i < totalPackages.Length; i++)
        {
            int index = random.Next(0, totalPackages.Length);

            if (!indices.Contains(index))
            {
                indices.Add(index);
                deliveryPads[index].SetActive(true);
                deliveryPads[index].GetComponent<Renderer>().material = packageMaterials.Pop();
            }
            if (indices.Count == 6) break;
        }
    }

    void ActivateRandomDeliveryPads()
    {
        List<int> randomIntegers = new List<int>();

        for (int i = 0; i < packageMaterials.Count; i++)
        {
            var newRandomInt = Random.Range(0, deliveryPads.Length);
            if (!randomIntegers.Contains(newRandomInt))
            {
                randomIntegers.Add(newRandomInt);
                continue;
            }
            i--;
        }      

        foreach (int randomNum in randomIntegers)
        {
            deliveryPads[randomNum].SetActive(true);
            deliveryPads[randomNum].GetComponent<Renderer>().material = packageMaterials.Pop();
        }
    }

    void DeactivatePadsOfMaterialsNotPresent()
    {
        HashSet<string> uniqueMaterials = new HashSet<string>();

        foreach (GameObject package in totalPackages)
        {
            uniqueMaterials.Add(package.GetComponent<Renderer>().material.name.Replace("PackageMat - ", ""));
        }

        foreach (GameObject pad in deliveryPads)
        {
            if (!uniqueMaterials.Contains(pad.GetComponent<Renderer>().material.name))
            {
                pad.SetActive(false);
            }
        }
    }
}
