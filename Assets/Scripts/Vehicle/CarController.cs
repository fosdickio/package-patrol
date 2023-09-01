using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class CarController : MonoBehaviour
{
    //Car "MotorSphere" concept used here based on tutorial by 
    //Spawn Camp Games: https://www.youtube.com/watch?v=TBIYSksI10k

    private Vector3 rawDriveInput;
    private float driveInput;
    private float turnInput;
    private bool isCarGrounded;

    public float maxHealth = 100;
    public float health = 100;
    public float fwdSpeed;
    public float revSpeed;
    public float turnSpeed;
    public LayerMask groundLayer;
    public float airDrag;
    public float groundDrag;

    public Rigidbody motorSphereRB;
    public Rigidbody truckRB;

    public PlayerInput playerInput;
    private PlayerInput truckInput;
    public Transform playerSeat;
    public Transform playerExitPoint;
    public bool playerInEntryZone;
    private AudioManager audioManager;

    [SerializeField] TrailRenderer[] trails;
    [SerializeField] GameObject smokeEffects;

    [SerializeField] bool destroyed;


    private void Awake()
    {
        truckInput = GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        audioManager = FindObjectOfType<AudioManager>();
        //detach rigidbody from car
        motorSphereRB.transform.parent = null;
        truckRB.transform.parent = null;
        destroyed = false;
    }

    // Update is called once per frame
    void Update()
    {
        driveInput = rawDriveInput.z;
        turnInput = rawDriveInput.x;

        //adjusts car speed
        driveInput *= driveInput > 0 ? fwdSpeed: revSpeed;


        //set cars position to sphere
        transform.position = motorSphereRB.transform.position;

        //set cars rotation
        //TODO  Decide on method of rotation
        float newRotation = turnInput * turnSpeed * Time.deltaTime; // * rawDriveInput.z;  //remove *rawDriveInput.z to make super snappy turns
        transform.Rotate(0, newRotation, 0, Space.World);

        //raycast ground check
        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        //rotate car to be parallel to ground
        transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        motorSphereRB.drag = (isCarGrounded) ? groundDrag : airDrag;

        HandleTurnEffects();

        if(health <=0 && !destroyed)
        {
            TruckDestroyed();
        }
    }

    private void HandleTurnEffects()
    {
        if (Mathf.Abs(rawDriveInput.x) >= 0.85f)
        {
            //audioManager.Play("TruckScreech");
            foreach (var trail in trails)
            {
                trail.emitting = true;
            }
        } 
        else
        {
            foreach (var trail in trails)
            {
                trail.emitting = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (isCarGrounded) 
        {
            //move car
            motorSphereRB.AddForce(transform.forward * driveInput, ForceMode.Acceleration);
        } else
        {
            motorSphereRB.AddForce(transform.up * -30f);
        }

        truckRB.MoveRotation(transform.rotation); 
    }

    void OnDrive(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        rawDriveInput = new Vector3(movementVector.x, 0, movementVector.y);
    }

    void OnExitVehicle()
    {

        MinimapGenerator.isInTruck = false;
        if (GameManagerScript.get() != null)
        {
            GameManagerScript.get().setActiveCameraName("vcam_Player");
        }

        CharacterMovement character = playerInput.gameObject.GetComponent<CharacterMovement>();
        Transform player = playerInput.transform;
        Rigidbody playerRB = playerInput.gameObject.GetComponent<Rigidbody>();
        Collider playerCollider = player.GetComponent<Collider>();
        Animator playerAnimator = player.GetComponent<Animator>();
        TruckEntryZoneIndicator entryZoneIndicator = GetComponentInChildren<TruckEntryZoneIndicator>();


        CinemachineVirtualCamera truckCamera = GameObject.Find("vcam_Truck").GetComponent<CinemachineVirtualCamera>();
        truckCamera.Priority = 1;

        truckInput.enabled = false;
        playerCollider.enabled = true;

        player.position = playerExitPoint.position;
        player.parent = null;
        FindObjectOfType<AudioManager>().Play("ExitTruck");
        FindObjectOfType<AudioManager>().Stop("TruckEngine");

        if (playerAnimator) { playerAnimator.SetBool("driving_truck", false); }

        playerRB.isKinematic = false;

        playerInput.enabled = true;
        character.ActivateWeaponSlot();
        character.GrabWeapon();
        

        entryZoneIndicator.activated = true;
        TurnOnCutoutShader();
    }

    private void TurnOnCutoutShader()
    {
        WallCutout wallCutout = GameObject.FindObjectOfType<WallCutout>();
        if (wallCutout) wallCutout.targetObject = playerInput.transform;
    }

    public void TakeDamage(int damageAmount)
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

    void TruckDestroyed()
    {
        destroyed = true;

        fwdSpeed = 0f;
        revSpeed = 0f;
        turnSpeed = 0f;

        smokeEffects.SetActive(true);
        FindObjectOfType<AudioManager>().Play("TruckDestroyed");

    }


}
