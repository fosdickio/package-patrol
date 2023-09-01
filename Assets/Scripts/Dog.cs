using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
{
    
    enum dogSize { small, medium, big }

    [SerializeField] dogSize size;

    public int damage;
    

    bool barking;
    bool mauling;
    bool whining;

    private void Start()
    {
        barking = false;
        mauling = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        Maul(collision);
    }

    void Maul(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            mauling = true;
            //FindObjectOfType<AudioManager>().Play("BigDogMaul");

            switch (size)
            {
                case dogSize.small:
                    FindObjectOfType<AudioManager>().Play("SmallDogMaul");
                    break;
                case dogSize.medium:
                    FindObjectOfType<AudioManager>().Play("MediumDogMaul");
                    break;
                case dogSize.big:
                    FindObjectOfType<AudioManager>().Play("BigDogMaul");
                    break;
                default:
                    break;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        mauling = false;
    }

    public void Chase()
    {
        if (!barking && !mauling)
        {
            StartCoroutine(Bark(size));
        }
        
    }

    IEnumerator Bark(dogSize size)
    {
        barking = true;

        switch (size)
        {
            case dogSize.small:
                FindObjectOfType<AudioManager>().Play("SmallDogBark");
                break;
            case dogSize.medium:
                FindObjectOfType<AudioManager>().Play("MediumDogBark");
                break;
            case dogSize.big:
                FindObjectOfType<AudioManager>().Play("BigDogBark");
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(3f);
        barking = false;
    }


    public void Flee()
    {
        if(!whining)
        {
            StartCoroutine(Whine(size));
        }
        
    }

    IEnumerator Whine(dogSize size)
    {
        whining = true;
        switch (size)
        {
            case dogSize.small:
                FindObjectOfType<AudioManager>().Play("SmallDogWine");
                break;
            case dogSize.medium:
                FindObjectOfType<AudioManager>().Play("MediumDogWine");
                break;
            case dogSize.big:
                FindObjectOfType<AudioManager>().Play("BigDogWine");
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(1.6f);
        whining = false;
    }




}
