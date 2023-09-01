using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DriverController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 3.0f;
    //[SerializeField] float runFactor = 1.0f;

    Animator animator;
    Vector3 rawInput;
    CharacterController characterController;
    float rotationFactorPerFrame = 1.0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateAnimation();
        updateRotation();
        characterController.Move(rawInput * walkSpeed * Time.deltaTime);
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        rawInput = new Vector3(movementVector.x, 0, movementVector.y);
    }

    void updateAnimation()
    {
        float maxVector = Mathf.Max(Mathf.Abs(rawInput.x), Mathf.Abs(rawInput.z));
        animator.SetFloat("vely", Mathf.Abs(maxVector));


    }

    void updateRotation()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(rawInput.normalized);

        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);

    }
}
