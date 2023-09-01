using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] popups;
    private int _popupIndex;

    private Dictionary<String, int> _totalPackages;

    void Start()
    {
        _popupIndex = 0;

        // Turn off UI text
        for (int i = 0; i < popups.Length; i++)
        {
            popups[_popupIndex].SetActive(false);
        }

        CountPackages();
    }
    
    void Update()
    {
        CountPackages();
        
        if (_popupIndex == 0)    // Tutorial just started
        {
            popups[_popupIndex].SetActive(true);

            if (!_totalPackages.ContainsKey("Red"))
            {
                popups[_popupIndex].SetActive(false);
                _popupIndex++;
            }
        }
        else if (_popupIndex == 1)
        {
            popups[_popupIndex].SetActive(true);
            
            if (!_totalPackages.ContainsKey("Green") && !_totalPackages.ContainsKey("Orange"))
            {
                popups[_popupIndex].SetActive(false);
                _popupIndex++;
            }
        }
        else if (_popupIndex == 2)
        {
            popups[_popupIndex].SetActive(true);
            
            if (!_totalPackages.ContainsKey("Blue"))
            {
                popups[_popupIndex].SetActive(false);
                _popupIndex++;
            }
        }
        else if (_popupIndex == 3)
        {
            popups[_popupIndex].SetActive(true);
        }
    }

    private void CountPackages()
    {
        _totalPackages = new Dictionary<string, int>();
        
        GameObject[] packages = GameObject.FindGameObjectsWithTag("Package");
        foreach (GameObject package in packages)
        {
            Renderer padRenderer = package.GetComponent<Renderer>();
            char[] separator = " ".ToCharArray();
            String padColor = padRenderer.material.name.Split(separator)[0];

            _totalPackages[padColor] = _totalPackages.ContainsKey(padColor) ? _totalPackages[padColor] + 1 : 1;
        }
    }
}
