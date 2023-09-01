using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] float animationSpeed;
    [SerializeField] float rootMovementSpeed;
    [SerializeField] float throwForce;
    [SerializeField] GameObject weaponSlot;
    [SerializeField] GameObject playerGeometry;

    public float maxHealth = 100;
    public float health = 100;

    [SerializeField] LayerMask targetMask;
    [SerializeField] Transform currentTarget;
    [SerializeField] float targetLockTime;
    [SerializeField] float targetLockDistance;
    [SerializeField] bool throwButtonPressed;

    private PlayerInput playerInput;
    public PlayerInput truckInput;
    public bool inVehicleEntryZone;

    private Vector3 input;
    private Animator animator;
    private Rigidbody rigidBody;
    private int velocityHash;
    private int driving_truckHash;
    private int diedHash;

    public List<Transform> packageSlotList;
    private GameObject collisionGO;
    private GameObject packageToLaunch;
    private CharacterIK characterIK;

    private Weapon weapon;
    public GameObject lastPackageDropped;

    bool playerDied;
    bool takingDamage;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        velocityHash = Animator.StringToHash("velocity");
        driving_truckHash = Animator.StringToHash("driving_truck");
        diedHash = Animator.StringToHash("died");
        rigidBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        characterIK = GetComponent<CharacterIK>();
    }

    void Start()
    {
        health = maxHealth;
        takingDamage = false;
        playerDied = false;
    }

    private void Update()
    {
        if (throwButtonPressed) { DetectTarget(); }
        if (health <= 0 && !playerDied) { PlayerDeath(); }
    }

    void FixedUpdate()
    {
        animator.speed = animationSpeed;
        Vector3 direction = input.normalized;

        if(currentTarget)
        {
            direction = (currentTarget.position - transform.position).normalized;
            rigidBody.MoveRotation(Quaternion.LookRotation(direction, Vector3.up));
        }

        if (direction != Vector3.zero)
        {
            rigidBody.MoveRotation(Quaternion.LookRotation(direction, Vector3.up));
        }

        //The below selects the max stick deflection, and uses it control the animator.
        //TODO This might not be the best way to do this.
        float maxVelocity = Mathf.Max(Mathf.Abs(input.x), Mathf.Abs(input.z));
        animator.SetFloat(velocityHash, maxVelocity);
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        input = new Vector3(movementVector.x, 0, movementVector.y);
    }

    void OnAnimatorMove()
    {
        Vector3 newRootPosition;

        newRootPosition = new Vector3(animator.rootPosition.x, this.transform.position.y, animator.rootPosition.z);
        newRootPosition = Vector3.LerpUnclamped(this.transform.position, newRootPosition, rootMovementSpeed);

        rigidBody.MovePosition(newRootPosition);
    }

    void OnThrowPress()
    {
        throwButtonPressed = true;

        if (weapon && !inVehicleEntryZone)
        {
            weapon.weaponProjection.SetActive(true);
        }
        
        if (inVehicleEntryZone)
        {
            //this is a very hacky way to get a reference to the truck entry zone, fix
            EntryZone truckEntryZone = GameObject.Find("TruckEntryZone").GetComponent<EntryZone>();

            //Choose the package to load, begin with manually held packages then moved to those in weapons
            GameObject packageToLoad = null;
            if (packageSlotList[0].childCount > 0)
            {
                packageToLoad = packageSlotList[0].GetChild(0).gameObject;
            }
            else
            {
                if (weapon)
                {
                    packageToLoad = weapon.packageSlotList[weapon.packageSlotList.Count - 1].GetChild(0).gameObject;
                }
            }

            if (!packageToLoad) { return; }

            truckEntryZone.LoadPackage(packageToLoad);
            FindObjectOfType<AudioManager>().Play("LoadTruck");
            if (weapon) { StartCoroutine(weapon.ReloadPackages()); }
            return;
        }

        if (!weapon)
        {
            DropPackage();
        }
    }

    void OnThrowRelease()
    {
        throwButtonPressed = false;

        if (inVehicleEntryZone) { return; }

        if(weapon)
        {
            weapon.weaponProjection.SetActive(false);
        }

        if (weapon)
        {
            weapon = weaponSlot.GetComponentInChildren<Weapon>();
            weapon.FireWeapon();
        } 
        /*else
        {
            DropPackage();
        } */  
    }

    void DropPackage()
    {
        if (packageSlotList[0].childCount > 0)
        {
            GameObject packageToDrop = packageSlotList[0].GetChild(0).gameObject;

            packageToDrop.transform.parent = GameObject.Find("Packages").transform;
            Vector3 offset = new Vector3(0, 0, -0.5f);
            packageToDrop.transform.position = transform.localPosition + offset;

            packageToDrop.GetComponent<Collider>().enabled = true;
            packageToDrop.GetComponent<Rigidbody>().isKinematic = false;
            StartCoroutine(StartLastPackageDelay(packageToDrop));
        }
        
    }

    IEnumerator StartLastPackageDelay(GameObject lastPackage)
    {
        //There more simple way to do this.  Just offset the package more than currently.
        lastPackageDropped = lastPackage;
        yield return new WaitForSeconds(.2f);
        lastPackageDropped = null;
    }

    void OnCyclePackages()
    {
        weapon = weaponSlot.GetComponentInChildren<Weapon>();

        //TO DO disable cycle packages when nothing is in the launcher
        if (inVehicleEntryZone)
        {
            UnloadPackages();
            return;
        }

        if(weapon)
        {
            weapon.CyclePackages();
            FindObjectOfType<AudioManager>().Play("PackageCycle");
        }
        
    }

    void OnDropWeapon()
    {
        if(weapon)
        {
            DropWeapon(null);
        }
    }

    void UnloadPackages()
    {
        EntryZone truckEntryZone = GameObject.Find("TruckEntryZone").GetComponent<EntryZone>();

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

        if (highestSpot)
        {
            Transform packageToReceive = highestSpot.GetChild(0);
            Collider packageCollider = packageToReceive.GetComponent<Collider>();
            Rigidbody packageRB = packageToReceive.GetComponent<Rigidbody>();

            packageCollider.enabled = true;
            packageRB.isKinematic = false;
            packageToReceive.parent = null;

            collisionGO = null;

            Vector3 offset = Vector3.up;
            packageToReceive.transform.position = transform.position + offset;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject collisionGO = collision.gameObject;

        switch(collisionGO.tag)
        {
            case "Package":
                HandlePackages(collision);
                break;
            case "Weapon":
                HandleWeapons(collision);
                break;
            case "Dog":
                HandleAttack(collision);
                break;
            case "AICar":
                HandleCarCrash(collision);
                break;
            default:
                break;

        }
    }

    void HandleCarCrash(Collision collision)
    {
        if (takingDamage || playerDied) { return; }
        takingDamage = true;
        TakeDamage(40);
        FindObjectOfType<AudioManager>().Play("CarHonk");
        Renderer playerRenderer = playerGeometry.GetComponent<Renderer>();
        StartCoroutine(FlashColor(playerRenderer, Color.red, 1.5f));

    }

    private void HandleAttack(Collision collision)
    {
        if(takingDamage || playerDied) { return;  }
        takingDamage = true;

        Dog dog = collision.gameObject.GetComponent<Dog>();
        Renderer playerRenderer = playerGeometry.GetComponent<Renderer>();
        
        TakeDamage(dog.damage);
        StartCoroutine(FlashColor(playerRenderer, Color.red, .2f));
    }

    IEnumerator FlashColor(Renderer renderer, Color color, float time)
    {
        Color originalColor = renderer.material.color;
        renderer.material.color = color;
        yield return new WaitForSeconds(time);
        renderer.material.color = originalColor;
        takingDamage = false;
    }

    void HandleWeapons(Collision collision)
    {
        GameObject newWeapon = collision.gameObject;

        //If there is already a weapon in the player's posession, discard it.
        if (weapon)
        {
            DropWeapon(newWeapon);
        }
        //Transfer packages if the player is holding any
        if (packageSlotList[0].childCount > 0)
        {
            newWeapon.GetComponent<Weapon>().LoadPackageInWeapon(packageSlotList[0].GetChild(0).gameObject);
        }

        //Place new weapon in the correct position.

        Transform weaponPivotPoint = weaponSlot.transform.Find("PivotPoint");
        Vector3 weaponOffset = newWeapon.GetComponent<Weapon>().weaponPivotOffset;
        weaponPivotPoint.localPosition = weaponOffset;

        newWeapon.transform.SetPositionAndRotation(weaponPivotPoint.transform.position,
                                                   weaponPivotPoint.transform.rotation);
        newWeapon.transform.parent = weaponPivotPoint;

        weapon = newWeapon.GetComponent<Weapon>();
        weapon.GetComponent<Collider>().enabled = false;

        GrabWeapon();
        FindObjectOfType<AudioManager>().Play("WeaponPickup");
    }

    private void DropWeapon(GameObject newWeapon)
    {
        if(!weapon) { return;  }
        Vector3 dropOffset = new Vector3(0, 0.5f, -2.0f);
        Vector3 dropPoint = transform.localPosition + dropOffset;
        weapon.transform.position = dropPoint;
        weapon.transform.parent = null;
        if (newWeapon) { weapon.TransferPackages(newWeapon); }
        ReleaseWeapon();

        weapon.GetComponent<Collider>().enabled = true;
        weapon.StopDefenseSystem();
        weapon.DropPackages();
        
    }

    void HandlePackages(Collision collision)
    {
        //remove last package fired and reinstate packagetolaunch on weapon.cs
        //if (packageToLaunch != null && packageToLaunch == collision.gameObject) { return; }
        //if (collisionGO == collision.gameObject) { return; }

        collisionGO = collision.gameObject;


        if (collisionGO.tag == "Package")
        {
            

            if (!weapon) 
            {
                if(collisionGO == lastPackageDropped) { return; }
                HoldPackage(collisionGO);
                return; 
            }

            if (collisionGO == weapon.lastPackageFired) { return; }
            weapon = weaponSlot.GetComponentInChildren<Weapon>();
            if (weapon) { weapon.LoadPackageInWeapon(collisionGO); }

            //collisionGO = null;
        }
    }

    void HoldPackage(GameObject package)
    {
        if (packageSlotList[0].childCount != 0) { return; }
        package.GetComponent<Rigidbody>().isKinematic = true;
        package.GetComponent<Collider>().enabled = false;
        package.transform.parent = packageSlotList[0].transform;
        package.transform.SetPositionAndRotation(packageSlotList[0].position, 
                                                 packageSlotList[0].rotation);

    }

    void OnEnterVehicle()
    {
        CarController truck = truckInput.gameObject.GetComponent<CarController>();
        TruckEntryZoneIndicator entryZoneIndicator = truck.gameObject.GetComponentInChildren<TruckEntryZoneIndicator>();
        CinemachineVirtualCamera truckCamera = GameObject.Find("vcam_Truck").GetComponent<CinemachineVirtualCamera>();
        Transform playerSeat = truck.playerSeat;
        
        if (truck.playerInEntryZone)
        {
            if (GameManagerScript.get() != null)
            {
                GameManagerScript.get().setActiveCameraName("vcam_Truck");
            }
            Collider playerCollider = GetComponent<Collider>();
            playerCollider.enabled = false;

            playerInput.enabled = false;
            rigidBody.isKinematic = true;

            StartCoroutine(SeatPlayer(playerSeat));

            transform.parent = playerSeat;
            transform.SetPositionAndRotation(playerSeat.position, playerSeat.rotation);
            animator.SetBool(driving_truckHash, true);
            FindObjectOfType<AudioManager>().Play("EnterTruck");
            FindObjectOfType<AudioManager>().Play("TruckEngine");

            truckInput.enabled = true;
            truckCamera.Priority = 11;

            entryZoneIndicator.activated = false;

            ReleaseWeapon();
            DeactivateWeaponSlot();
            TurnOffCutoutShader();
        }
    }

    private void TurnOffCutoutShader()
    {
        WallCutout wallCutout = GameObject.FindObjectOfType<WallCutout>();
        if (wallCutout) wallCutout.targetObject = null;
    }

    IEnumerator SeatPlayer(Transform playerSeat)
    {
        //This coroutine prevents the player from sitting outside the vehicle becuase
        //the run walk animation continues after their position has been changed.

        //There might be a snappier way to do this.  Just stop the animation
        //right before the teleport takes place.
        playerGeometry.SetActive(false);

        float waitTime = (animator.GetCurrentAnimatorStateInfo(0).IsName("Running") || animator.GetCurrentAnimatorStateInfo(0).IsName("Walking")) 
                         ? animator.GetCurrentAnimatorStateInfo(0).length : 0.0f;

        yield return new WaitForSeconds(waitTime);
        transform.parent = playerSeat;
        transform.SetPositionAndRotation(playerSeat.position, playerSeat.rotation);
        animator.SetBool(driving_truckHash, true);
        playerGeometry.SetActive(true);

    }

    public void GrabWeapon()
    {
        if(!weaponSlot.GetComponentInChildren<Weapon>()) { return; }
        GameObject weaponToGrip = weaponSlot.GetComponentInChildren<Weapon>().gameObject;
        
        if (weaponToGrip)
        {
            characterIK.rightHandObj = weaponToGrip.transform.Find("RightHandGrip").transform;
            characterIK.ikActive = true;
        }

    }

    public void ReleaseWeapon() { characterIK.ikActive = false; }
    public void ActivateWeaponSlot() { weaponSlot.SetActive(true); }
    public void DeactivateWeaponSlot() { weaponSlot.SetActive(false); }

    void Step() 
    {
        if(gameObject.tag == "Citizen") { return; }
        FindObjectOfType<AudioManager>()?.Play("StepsConcrete");
    }

    void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        health = health < 0 ? 0 : health;
    }

    void AddHealth(int healthAmount)
    {
        health += healthAmount;
    }

    public int getHealthPercentage()
    {
        return (int)Math.Ceiling(100 * health / maxHealth);
    }

    void DetectTarget()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, targetLockDistance, targetMask))
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
            StartCoroutine(SetTarget(hitInfo.transform));
        } else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * targetLockDistance, Color.green);
        }
    }

    IEnumerator SetTarget(Transform target)
    {
        if(!currentTarget)
        {
            currentTarget = target;
            yield return new WaitForSeconds(targetLockTime);
            currentTarget = null;
        }

    }

    void PlayerDeath()
    {
        playerDied = true;
        DropWeapon(null);
        animator.SetBool(diedHash, true);
        FindObjectOfType<AudioManager>().Stop("ThemeMusic");
        FindObjectOfType<AudioManager>().Play("PlayerDeath");
        playerInput.enabled = false;
    }

    void OnSprayPress()
    {
        if (weapon && !inVehicleEntryZone)
        {
            weapon.StartDefenseSystem();
        }
    }

    void OnSprayRelease()
    {
        if (weapon && !inVehicleEntryZone)
        {
            weapon.StopDefenseSystem();
        }
    }
}
