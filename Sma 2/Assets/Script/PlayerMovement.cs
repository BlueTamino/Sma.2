using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]    private float horizontalMove = 0f;
                        private float VerticalMove = 0f;
    [Range(1f,60f)]     public float runSpeed = 40f;
    [Range(1f, 60f)]    public float ClimbSpeed;
    [SerializeField]    private bool canCroch;
    [SerializeField]    private Joystick joystick;
                        public bool NeedsNetwork = true;
                        public CharacterController2D controller;
                        private Animator anim;
                        bool crouch = false;
                        bool jump = false;
                        private readonly NetworkVariable<int> randomNumber =
                        new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Awake()
    {
        /*joystick = FindFirstObjectByType<Joystick>();
        CinemachineVirtualCamera Vcam = GameObject.FindGameObjectWithTag("Vcam").GetComponent<CinemachineVirtualCamera>();
        Button JumpBtn = GameObject.FindGameObjectWithTag("Jump Btn").GetComponent<Button>();
        JumpBtn.onClick.AddListener(
        delegate
        {
            Jump();
        });
        Vcam.Follow = gameObject.transform;*/
        if (!NeedsNetwork)
        {
            InitializePlayer();
        }
        Application.targetFrameRate = 60;
        //Time.timeScale = 0.1f;
        anim = gameObject.GetComponent<Animator>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            InitializePlayer();
        }
        randomNumber.OnValueChanged += (int previosValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + "; Random Number: " + randomNumber.Value);
        };
    }
    public void InitializePlayer()
    {
        joystick = FindFirstObjectByType<Joystick>();
        CinemachineVirtualCamera Vcam = GameObject.FindGameObjectWithTag("Vcam").GetComponent<CinemachineVirtualCamera>();
        Vcam.Follow = gameObject.transform;
        GameObject.FindGameObjectWithTag("Jump Btn").GetComponent<InputPrefabTransfer>().RegisterPlayer(gameObject);
        /* Old Input System
        Button JumpBtn = GameObject.FindGameObjectWithTag("Jump Btn").GetComponent<Button>();
        JumpBtn.onClick.AddListener(
        delegate
        {
            Jump();
        });*/
    }
    void Update()
    {
        if (!(IsOwner || !NeedsNetwork)) return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = Random.Range(1, 1000);
        }
        if(joystick.Horizontal >= 0.2f)
        {
            horizontalMove = runSpeed;
        }
        else if(joystick.Horizontal <= -0.2f)
        {
            horizontalMove = -runSpeed;
        }
        else
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        }
        if (joystick.Vertical >= 0.2f)
        {
            VerticalMove = ClimbSpeed;
        }
        else if (joystick.Vertical <= -0.2f)
        {
            VerticalMove = ClimbSpeed;
        }
        else
        {
            horizontalMove = Input.GetAxisRaw("Vertical") * ClimbSpeed;
        }
        anim.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetButtonDown("Jump"))
        {   
            jump = true;
        }
        if ((Input.GetButtonDown("Crouch") || Input.GetKeyDown(KeyCode.LeftShift)) && canCroch)
        {
            crouch = true;
        }else if ((Input.GetButtonUp("Crouch") || Input.GetKeyUp(KeyCode.LeftShift)) && canCroch)
        {
            crouch  = false;
        }
    }
    void FixedUpdate()
    {
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, VerticalMove * Time.fixedDeltaTime);
            jump = false;
    }
    public void Jump()
    {
        jump = true;
    }
}
