using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float maxWeaponForce;
    public float minWeaponForce;
    public float weaponForce { get; private set; }
    public GameObject weaponProjection;
    public GameObject defenseSystem;
    public List<Transform> packageSlotList;
    public Vector3 weaponPivotOffset;
    public Transform rightHandGrip;
    //[SerializeField] GameObject pickupParticleEffect;

    private GameObject packageToLaunch;
    private bool firing;
    private int maxSlotIndex;
    public GameObject lastPackageFired { get; private set; }
    ParticleSystem waterFountain;
    bool defenseSystemActive;

    private void Awake()
    {
        waterFountain = defenseSystem.GetComponent<ParticleSystem>();
    }

    void Start()
    {
        firing = false;
        maxSlotIndex = packageSlotList.Count - 1;
        defenseSystemActive = false;
        waterFountain.Stop();
    }

    void Update()
    {
        weaponForce = Mathf.Lerp(minWeaponForce, maxWeaponForce, Mathf.PingPong(Time.time, 1));
    }

    public void FireWeapon()
    {
        if (!firing)
        {
            firing = true;
            StartCoroutine(ShootPackage());
            firing = false;
        }
    }

    IEnumerator ShootPackage()
    {

        if (packageSlotList[maxSlotIndex].childCount > 0)
        {
            GameObject packageToLaunch = packageSlotList[maxSlotIndex].GetChild(0).gameObject;
            Rigidbody packageRB = packageToLaunch.GetComponent<Rigidbody>();
            Collider packageCollider = packageToLaunch.GetComponent<Collider>();
            Package packageTrail = packageToLaunch.GetComponent<Package>();

            packageToLaunch.transform.parent = GameObject.Find("Packages").transform;
            packageCollider.enabled = true;

            //TODO determine if the kinematic stuff is neccessary with no collider;
            packageRB.isKinematic = false;
            packageTrail.playSmokeTrail();
            packageRB.AddForce(packageToLaunch.transform.forward * weaponForce, ForceMode.Impulse);
            StartCoroutine(StartLastPackageDelay(packageToLaunch));
            
            FindObjectOfType<AudioManager>()?.Play("ShootPackage");

            yield return StartCoroutine(ReloadPackages());
            packageToLaunch = null;
        }
    }

    IEnumerator StartLastPackageDelay(GameObject lastPackage)
    {
        lastPackageFired = lastPackage;
        yield return new WaitForSeconds(.2f);
        lastPackageFired = null;
    }

    public IEnumerator ReloadPackages()
    {
        yield return new WaitForSeconds(.1f);
        CyclePackages();
    }

    public void CyclePackages()
    {
        int minSlotUsed = packageSlotList.Count - 1;
        Transform packageToCycle = transform;

        //FindObjectOfType<AudioManager>().Play("PackageCycle");
        //print("firing audio...");

        foreach (Transform packageSlot in packageSlotList)
        {
            int slotIndex = packageSlotList.IndexOf(packageSlot);

            if (packageSlot.childCount > 0 && packageSlot.GetChild(0) != packageToCycle)
            {
                packageToCycle = packageSlot.GetChild(0);
                minSlotUsed = (slotIndex < minSlotUsed) ? slotIndex : minSlotUsed;

                if (slotIndex + 1 < packageSlotList.Count)
                {
                    packageToCycle.parent = packageSlotList[slotIndex + 1].transform;
                    packageToCycle.SetPositionAndRotation(packageSlotList[slotIndex + 1].transform.position,
                                                          packageSlotList[slotIndex + 1].transform.rotation);
                }
                else
                {
                    packageToCycle.parent = packageSlotList[minSlotUsed].transform;
                    packageToCycle.SetPositionAndRotation(packageSlotList[minSlotUsed].transform.position,
                                                          packageSlotList[minSlotUsed].transform.rotation);
                }
            }
        }

    }

    public GameObject GetFirstPackage()
    {
        return packageSlotList[maxSlotIndex].GetChild(0).gameObject;
    }

    public void LoadPackageInWeapon(GameObject package)
    {
        foreach (Transform packageSlot in packageSlotList)
        {
            if (packageSlot.transform.childCount == 0)
            {
                Rigidbody packageRB = package.GetComponent<Rigidbody>();
                packageRB.isKinematic = true;
                Collider packageCollider = package.GetComponent<Collider>();
                packageCollider.enabled = false;

                package.transform.SetPositionAndRotation(packageSlot.transform.position, packageSlot.transform.rotation);
                package.transform.parent = packageSlot.transform;
            }
        }
        FindObjectOfType<AudioManager>()?.Play("PackagePickup");
    }

    public void TransferPackages(GameObject newWeapon)
    {
        List<Transform> newWeaponSlots = newWeapon.GetComponent<Weapon>().packageSlotList;
        foreach (Transform packageSlot in packageSlotList)
        {
            if (packageSlot.transform.childCount != 0)
            {
                GameObject package = packageSlot.transform.GetChild(0).gameObject;
                package.GetComponent<Rigidbody>().isKinematic = false;
                package.GetComponent<Collider>().enabled = true;

                newWeapon.GetComponent<Weapon>().LoadPackageInWeapon(package);
            }
        }
    }

    public void DropPackages()
    {
        foreach (Transform packageSlot in packageSlotList)
        {
            if (packageSlot.transform.childCount != 0)
            {
                GameObject package = packageSlot.transform.GetChild(0).gameObject;
                package.GetComponent<Rigidbody>().isKinematic = false;
                package.GetComponent<Collider>().enabled = true;

                Vector3 offset = new Vector3(0, 0, 4.0f);
                package.transform.position = transform.position + offset;
                package.transform.parent = GameObject.Find("Packages").transform;
            }
        }
    }

    public void StartDefenseSystem()
    {
        if (defenseSystemActive) { return;  }
        defenseSystemActive = true;
        waterFountain.Play();
        FindObjectOfType<AudioManager>().Play("WaterDefense");
    }

    public void StopDefenseSystem()
    {
        if(!defenseSystemActive) { return; }
        waterFountain.Stop();
        FindObjectOfType<AudioManager>().Stop("WaterDefense");
        defenseSystemActive = false;
    }
    

}
