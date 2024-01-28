using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementScript : MonoBehaviour
{
    [Header("ExternalReferences")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] private Camera playerCamera;

    private PlayerInputActions playerActions;
    private PlayerInput _playerInput;
    private InputAction _move;
    private InputAction _run;
    private InputAction _jump;

    private Rigidbody rb;
    private Animator anim;


    [Header("Control Configuration")]
    [SerializeField] private float moveForce = 1f;
    [SerializeField] private float runForce = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = .5f;
    [SerializeField] private float maxRunSpeed = 2f;
    private Vector3 forceDirection;
    private Vector2 moveInput;


    int velocityZHash;//animator optimization
    int velocityXHash;
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        velocityXHash = Animator.StringToHash("VelocityX");
        velocityZHash = Animator.StringToHash("VelocityZ");

        rb = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        playerActions = new PlayerInputActions();
    }
    private void OnEnable()
    {
        SetupInputActions();
        playerActions.Game.Enable();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) pauseMenu.SetActive(!pauseMenu.activeSelf);
    }
    private void FixedUpdate()
    {
        moveInput = _move.ReadValue<Vector2>();
        if (_run.IsPressed())
        {
            forceDirection += moveInput.x * GetCameraRight(playerCamera) * runForce;
            forceDirection += moveInput.y * GetCameraForward(playerCamera) * runForce;
        }
        else
        {
            forceDirection += moveInput.x * GetCameraRight(playerCamera) * moveForce;
            forceDirection += moveInput.y * GetCameraForward(playerCamera) * moveForce;
        }

        transform.forward = GetCameraForward(playerCamera);
        rb.AddForce(forceDirection, ForceMode.Impulse);

        //accelerating fall
        if (rb.velocity.y < 0f)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }

        //clamping velocity
        Vector3 horVelocity = rb.velocity;
        horVelocity.y = 0f;
        if(horVelocity.sqrMagnitude > maxSpeed * maxSpeed && !_run.IsPressed())
        {
            rb.velocity = horVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }else if(horVelocity.sqrMagnitude > maxRunSpeed * maxRunSpeed && _run.IsPressed())
        {
            rb.velocity = horVelocity.normalized * maxRunSpeed + Vector3.up * rb.velocity.y;
        }

        //convert velocity to local space, instead of world space

        SetAnimatorVariables(transform.InverseTransformDirection(rb.velocity));
        forceDirection = Vector3.zero;
    }


    //Input Methods----------------------------------------------------------------
    private void SetupInputActions()
    {
        _move = _playerInput.actions["Movement"];
        _jump = _playerInput.actions["Jump"];
        _run = _playerInput.actions["Run"];

    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }
    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f)) 
        {
            return true; 
        }
        else return false;
    }


    //Camera Methods-----------------------------------------------------------------
    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0f;
        return right.normalized;
    }
    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }
    //Animation control--------------------------------------------------------------
    private void SetAnimatorVariables(Vector3 velocity)
    {
        anim.SetFloat(velocityXHash, velocity.x);
        anim.SetFloat(velocityZHash, velocity.z);
        
        anim.SetBool("IsRunning", playerActions.Game.Run.IsPressed());
    }


    
}
