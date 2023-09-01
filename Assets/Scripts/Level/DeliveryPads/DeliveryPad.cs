using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryPad : MonoBehaviour
{
    Color padColor;
    private bool isColorUpdated;
    [SerializeField] GameObject deliveredPackagePrefab;

    void Start()
    {
        isColorUpdated = false;
    }

    void OnTriggerEnter(Collider other)
    {
        CheckForLatestColorInformation();
        HandlePlayerHeldPackages(other);
        HandlePackages(other);
    }

    private void RemovePackageVelocity(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void CheckForLatestColorInformation()
    {
        if (!isColorUpdated)
        {
            Renderer padRenderer = GetComponent<Renderer>();
            padColor = padRenderer.material.color;
            isColorUpdated = true;
        }
    }

    private void HandlePackages(Collider other)
    {

        if (other.gameObject.tag == "Package")
        {
            Renderer packageMesh = other.gameObject.GetComponent<Renderer>();
            if (packageMesh.material.color == padColor)
            {
                PlayPackageDeliverySound();
                Vector3 packageVelocity = other.GetComponent<Rigidbody>().velocity;
                Vector3 packagePosition = other.transform.position;
                Destroy(other.gameObject);
                GameObject deliveredPackage = Instantiate(deliveredPackagePrefab, packagePosition, Quaternion.identity);
                Rigidbody deliveredPackageRB = deliveredPackage.GetComponent<Rigidbody>();
                deliveredPackageRB.velocity = packageVelocity;
                deliveredPackageRB.drag = 5;


            }
        }
    }

    private void HandlePlayerHeldPackages(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameObject player = other.gameObject;
            Weapon weapon = other.gameObject.GetComponentInChildren<Weapon>();

            if (!weapon)
            {
                if (player.GetComponent<CharacterMovement>().packageSlotList[0].childCount == 0) { return;  }

                Transform packageTransform = player.GetComponent<CharacterMovement>().packageSlotList[0].GetChild(0);
                
                if (PackageColorMatches(packageTransform))
                {
                    Destroy(packageTransform.gameObject);
                    PlayPackageDeliverySound();
                }
                return;
            }

            foreach (Transform packageSlot in weapon.packageSlotList)
            {
                if (packageSlot.transform.childCount > 0)
                {
                    Transform packageTransform = packageSlot.transform.GetChild(0);
                    if (PackageColorMatches(packageTransform))
                    {
                        Destroy(packageTransform.gameObject);
                        //TODO currently packages dont advance if the hand delivered package is
                        //the first one in the slot, fix this.  The statement below is a partial fix
                        //but it prevents multiple packages from being hand delivered at the same time.
                        //player.CyclePackages();
                        PlayPackageDeliverySound();
                        GameObject deliveredPackage = Instantiate(deliveredPackagePrefab, transform.position, Quaternion.identity);
                        Rigidbody deliveredPackageRB = deliveredPackage.GetComponent<Rigidbody>();
                        deliveredPackageRB.drag = 5;
                    }
                }
            }
        }
    }

    private void PlayPackageDeliverySound()
    {
        FindObjectOfType<AudioManager>()?.Play("PackageDelivered");
    }

    private bool PackageColorMatches(Transform package)
    {
        Renderer packageMesh = package.gameObject.GetComponent<Renderer>();
        bool returnVal = (packageMesh.material.color == padColor) ? true : false;
        return returnVal;
    }

    IEnumerator SlowDownPackage(Rigidbody package)
    {
        /*
        while (package.velocity.magnitude >= .1f)
        {
            //package.velocity = package.velocity * 0.95f * Time.deltaTime;
            package.drag = 20f;
            print(package.velocity.magnitude);
            print(package.velocity);
        }

        package.drag = 1f;
        yield return null;
        */
        yield return null;

    }
}
