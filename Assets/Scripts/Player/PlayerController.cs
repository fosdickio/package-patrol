using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float throwForce;

    public List<Transform> packageSlotList;
    private Vector3 input;
    private GameObject collisionGO;
    private GameObject packageToLaunch;
    private bool throwing;

    public PlayerInput truckInput;
    private PlayerInput playerInput;

    private Rigidbody rigidBody;

    public bool inVehicleEntryZone;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        throwing = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 direction = input.normalized;
        Vector3 velocity = direction * speed;
        Vector3 moveAmount = velocity * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        //perhaps change to rigidbody.moveposition / rigidbody.moverotation 
        //See https://mediaspace.gatech.edu/media/Unity+Interactive+Animation/1_35msp0bs 
        //minute 23:15
        transform.Translate(moveAmount,Space.World);    
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        input = new Vector3(movementValue.Get<Vector2>().x, 0, movementValue.Get<Vector2>().y);
    }

    void OnThrow()
    {
        if(!throwing)
        {   
            throwing = true;
            StartCoroutine(ThrowPackage());
            throwing = false;
        }
    }

    IEnumerator ThrowPackage()
    {
        if (packageSlotList[2].childCount > 0)
        {
                       
            Transform package = packageSlotList[2].GetChild(0);
            packageToLaunch = package.gameObject;
            Rigidbody packageRB = package.gameObject.GetComponent<Rigidbody>();

          

            if (inVehicleEntryZone)
            {
                //this is a very hacky way to get a reference to the truck entry zone, fix
                TruckEntryZone truckEntryZone = GameObject.Find("TruckEntryZone").GetComponent<TruckEntryZone>();
                print(truckEntryZone);
                truckEntryZone.LoadPackage(packageToLaunch);
                packageToLaunch = null;
                yield return StartCoroutine(ReloadPackages());
                yield break;
            }

            package.parent = GameObject.Find("Packages").transform;
            packageRB.isKinematic = false;
            packageRB.AddForce(package.forward * throwForce, ForceMode.Impulse);
            
            yield return StartCoroutine(ReloadPackages());
            packageToLaunch = null;
        }

    }

    IEnumerator ReloadPackages()
    {
        yield return new WaitForSeconds(.1f);

        foreach (Transform packageSlot in packageSlotList)
        {
            int slotIndex = packageSlotList.IndexOf(packageSlot);

            if (packageSlot.childCount > 0 && (slotIndex + 1 < packageSlotList.Count)) //&& packageSlotList[slotIndex + 1].childCount == 0
            {
                Transform reloadPackage = packageSlot.GetChild(0);

                reloadPackage.parent = packageSlotList[slotIndex + 1].transform;
                reloadPackage.position = packageSlotList[slotIndex + 1].transform.position;
                reloadPackage.rotation = packageSlotList[slotIndex + 1].transform.rotation;
            }   
        }
    }

    void OnCyclePackages()
    {
        //TO DO disable cycle packages when nothing is in the launcher
        if (inVehicleEntryZone)
        {
            UnloadPackages();
            return;
        }

        CyclePackages();
    }

    void CyclePackages()
    {
        int maxSlotUsed = packageSlotList.Count - 1; 
        foreach (Transform packageSlot in packageSlotList)
        {
            if(packageSlot.childCount > 0)
            {
                int slotIndex = packageSlotList.IndexOf(packageSlot);
                Transform packageToCycle = packageSlot.GetChild(0);

                maxSlotUsed = (slotIndex < maxSlotUsed) ? slotIndex : maxSlotUsed;

                if (slotIndex + 1 < packageSlotList.Count)
                {
                    packageToCycle.parent = packageSlotList[slotIndex + 1].transform;
                    packageToCycle.position = packageSlotList[slotIndex + 1].transform.position;
                    packageToCycle.rotation = packageSlotList[slotIndex + 1].transform.rotation;

                }
                else
                {
                    packageToCycle.parent = packageSlotList[maxSlotUsed].transform;
                    packageToCycle.position = packageSlotList[maxSlotUsed].transform.position;
                    packageToCycle.rotation = packageSlotList[maxSlotUsed].transform.rotation;
                }
            }   
        }
    }

    void UnloadPackages()
    {
        TruckEntryZone truckEntryZone = GameObject.Find("TruckEntryZone").GetComponent<TruckEntryZone>();

        Transform closestStack = null;
        float currClosestDistance = Mathf.Infinity;

        if (truckEntryZone.stacks.Count == 0) { return; }

        foreach (Transform stack in truckEntryZone.stacks)
        {
            float distance = (stack.transform.position - transform.position).magnitude;

            if (distance < currClosestDistance)
            {
                closestStack = stack;
                currClosestDistance = distance;
            }
        }

        Transform highestSpot = null;
        float highY = -Mathf.Infinity;

        foreach (Transform spot in closestStack)
        {
            if (spot.childCount == 0) { continue; }
            if (spot.position.y > highY)
            {
                highestSpot = spot;
                highY = spot.position.y;
            }
        }

        Transform packageToReceive = (highestSpot.GetChild(0) != null) ? highestSpot.GetChild(0) : null;
        Rigidbody packageRB = packageToReceive.GetComponent<Rigidbody>();

        
        packageRB.isKinematic = false;
        packageToReceive.parent = null;

        collisionGO = null;
        packageToReceive.transform.position = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(packageToLaunch != null && packageToLaunch == collision.gameObject) { return; }
        if (collisionGO == collision.gameObject) { return; }

        collisionGO = collision.gameObject;
        if (collisionGO.tag == "Package")
        {
            foreach (Transform packageSlot in packageSlotList)
            {
                if (packageSlot.transform.childCount == 0)
                {
                    Rigidbody otherRB = collisionGO.GetComponent<Rigidbody>();
                    otherRB.isKinematic = true;

                    collisionGO.transform.position = packageSlot.transform.position;
                    collisionGO.transform.rotation = packageSlot.transform.rotation;
                    collisionGO.transform.parent = packageSlot.transform;   
                }
            }
            collisionGO = null;

        }

    }

    void OnEnterVehicle()
    {
        MinimapGenerator.isInTruck = true;
        CarController truck = truckInput.gameObject.GetComponent<CarController>();
        CinemachineVirtualCamera truckCamera = GameObject.Find("vcam_Truck").GetComponent<CinemachineVirtualCamera>();
        Transform playerSeat = truck.playerSeat;
        //CameraController cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();

        if (truck.playerInEntryZone)
        {
            playerInput.enabled = false;
            rigidBody.isKinematic = true;
            transform.position = playerSeat.position;
            transform.rotation = playerSeat.rotation;
            transform.parent = playerSeat;

            truckInput.enabled = true;
            truckCamera.Priority = 11;
        }
    }
}
