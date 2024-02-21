using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GunHandler[] items;
    [SerializeField] TMP_Text ammoCountText;
    public GameObject spawner;
    public GameObject pauseMenu;
    [HideInInspector] public bool isPaused;
    public bool isScopeToggle = true;
    int itemIndex;
    int previousItemIndex = -1;
    public bool isScoped;
    [SerializeField] Transform cameraHolder; 
    
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public bool sprinting;
    public float groundDrag;
    public float leanAmount;
    public float leanSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public bool crouching;
    public float startPosition;
    public float crouchPosition;

    [Header("Wallrunning")]
    public bool wallrunning;
    public float wallrunSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Hitmarkers")]
    public RectTransform hitmarker;
    public CanvasGroup hitmarkerCanvas;
    public bool hitmarkerRight;
    public float hitmarkerRot; 
    public AudioSource hitMarkerAudioSource;
    public AudioClip hitSound;
    public GameObject killmarker;
    public AudioSource killMarkerAudioSource;
    public AudioClip killSound;
    public float hitmarkerLastingTime = 0.5f;
    public float killmarkerLastingTime = 1f;
    

    public Transform orientation;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        isPaused = false;
        crouching = false;
        sprinting = false;
        hitmarkerRight = false;
        startYScale = transform.localScale.y;
        startPosition = cameraHolder.localPosition.y;

        EquipItem(0);

        //Instantiate(spawner, Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!isPaused)
            {
                isPaused = true;
                Cursor.lockState = CursorLockMode.None;
                pauseMenu.SetActive(true);
            }
            else
            {
                isPaused = false;
                Cursor.lockState = CursorLockMode.Locked;
                pauseMenu.SetActive(false);
            }
        }

        if(!isPaused)
        {
            MyInput();
            SpeedControl();
            StateHandler();
            HandleWeaponSwitching();
            HandleShooting();
            HandleReload();

            if(isScopeToggle)
                HandleScopingToggle();
            else
                HandleScoping();
        }

        ShowAmmo();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    public void EquipItem(int _index)
    {
        if(_index == previousItemIndex)
            return;
        
        itemIndex = _index;
        items[itemIndex].gunObject.SetActive(true);
        //items[itemIndex].SetActive(true);

        if(previousItemIndex != -1)
        {
            items[previousItemIndex].gunObject.SetActive(false);
            //items[previousItemIndex].SetActive(false);
        }

        previousItemIndex = itemIndex;
    }

    void HandleShooting()
    {
        if(state == MovementState.sprinting)
            return;
        
        if(Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Shoot();
        }
        else if(Input.GetMouseButton(0))
        {
            if(!items[itemIndex].isAutomatic)
                return;

            items[itemIndex].Shoot();
        }
    }

    public void HandleScopeToggledOrNot(bool toggle)
    {
        isScopeToggle = !toggle;
    }

    void HandleScopingToggle()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if(!isScoped)
                isScoped = true;
            else
                isScoped = false;
        }
    }

    void HandleScoping()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isScoped = true;
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            isScoped = false;
        }
    }

    void HandleWeaponSwitching()
    {
        for(int i = 0; i < items.Length; i++)
        {
            if(Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }
    }

    void HandleReload()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(items[itemIndex].isReloading)
                return;
            
            StartCoroutine(items[itemIndex].Reload());
            items[itemIndex].audioSource.PlayOneShot(items[itemIndex].reloadSound);
        }
    }

    void ShowAmmo()
    {
        ammoCountText.text = items[itemIndex].currentAmmo + "/" + items[itemIndex].magSize;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            if(!crouching)
            {
                crouching = true;
                cameraHolder.localPosition = Vector3.Lerp(new Vector3(cameraHolder.localPosition.x, startPosition, cameraHolder.localPosition.z), new Vector3(cameraHolder.localPosition.x, crouchPosition, cameraHolder.localPosition.z), crouchSpeed * Time.deltaTime);
                //transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
            else
            {
                crouching = false;
                cameraHolder.localPosition = Vector3.Lerp(new Vector3(cameraHolder.localPosition.x, crouchPosition, cameraHolder.localPosition.z), new Vector3(cameraHolder.localPosition.x, startPosition, cameraHolder.localPosition.z), crouchSpeed * Time.deltaTime);
                //transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
            
        }
    }

    private void StateHandler()
    {
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            moveSpeed = wallrunSpeed;
        }
        
        // Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if(grounded && Input.GetKey(sprintKey))
        {
            if(Input.GetMouseButtonDown(0) || Input.GetAxisRaw("Vertical") < 1)
            {
                state = MovementState.walking;
                moveSpeed = wallrunSpeed;
            }                
            else
            {
                sprinting = true;
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        sprinting = false;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
