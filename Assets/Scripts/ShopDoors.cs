using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopDoors : MonoBehaviour
{
    public Animator leftDoorAnimator;
    public Animator rightDoorAnimator;

    // Update is called once per frame


    void OnTriggerEnter(Collider other)
    {
        leftDoorAnimator.SetBool("Open", true);
        rightDoorAnimator.SetBool("Open", true);
        FindObjectOfType<AudioManager>().Play("ShopDoorChime");
    }

    void OnTriggerExit(Collider other)
    {
        leftDoorAnimator.SetBool("Open", false);
        rightDoorAnimator.SetBool("Open", false);

    }
}
