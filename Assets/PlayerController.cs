using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    NavMeshAgent agent;
    Camera cam;
    Animator anim;

    private PlayerInputActions playerActions;
    private PlayerInput _playerInput;
    private InputAction _click;
    bool UIOpen = false;

    int velocityZHash;//animator optimization
    int velocityXHash;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        _playerInput = GetComponent<PlayerInput>();
        cam = Camera.main;

        playerActions = new PlayerInputActions();
        SetupInputActions();
        velocityXHash = Animator.StringToHash("VelocityX");
        velocityZHash = Animator.StringToHash("VelocityZ");
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            UIOpen = pauseMenu.activeSelf;
        }

    }
    private void FixedUpdate()
    {
        if (_click.IsPressed() && !UIOpen)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 1000f))
            {
                agent.SetDestination(hit.point);
            }
        }
        if (agent.hasPath) AnimateMovement(transform.InverseTransformDirection(agent.velocity));
        else AnimateMovement(Vector3.zero);
        cam.transform.position = Vector3.Lerp(cam.transform.position, transform.position, .1f);
    }

    //Input Methods----------------------------------------------------------------
    private void SetupInputActions()
    {
        _click = _playerInput.actions["Click"];
    }
    private void AnimateMovement(Vector3 velocity)
    {
        anim.SetFloat(velocityXHash, velocity.x);
        anim.SetFloat(velocityZHash, velocity.z);
    }
}
