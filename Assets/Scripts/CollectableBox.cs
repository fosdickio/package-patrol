using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableBox : MonoBehaviour
{
    [SerializeField] float speed;

    void Update()
    {
        float angularSpeed = 10 * speed * Time.deltaTime;
        transform.Rotate(Vector3.up, angularSpeed);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<AudioManager>().Play("CollectablePickup");
            Destroy(gameObject.transform.parent.gameObject);
            if(gameObject!=null)
            Destroy(gameObject);
        }
    }
}
