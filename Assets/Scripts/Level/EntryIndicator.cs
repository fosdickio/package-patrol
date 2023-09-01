using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryIndicator : MonoBehaviour
{
    public float rotationSpeed = 0.1f;


    private void Start()
    {
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed);
    }
}
