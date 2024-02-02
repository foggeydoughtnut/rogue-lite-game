using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public bool IsFacingRight = true;


    [Header("Jumpp")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpTime = 0.5f;


    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;


    [Header("Camera Stuff")]
    [SerializeField] private GameObject _cameraFollowGO;

    [Header("Ground Check")]
    [SerializeField] private float extraHeight = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    private CameraFollowObject _cameraFollowObject;

    private float _fallSpeedYDampingChangeThreshold;

    private Collider2D coll;

    private GameActions gameActions; 
    private bool isJumping;
    private bool isFalling;
    private float jumpTimeCounter;

    private RaycastHit2D groundHit;



    private void Awake()
    {
        gameActions = new GameActions();
    }

    private void OnEnable()
    {
        gameActions.Enable();
    }

    private void OnDisable()
    {
        gameActions.Disable();
    }

    private void Start()
    {
        _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;

        coll = GetComponent<Collider2D>();
    }

    private void Update()
    {
        HandleMovement();
        Jump();

        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }   

    private void Jump()
    {
        if (gameActions.Player.Jump.WasPressedThisFrame() && IsGrounded())
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        if (gameActions.Player.Jump.IsPressed())
        { 
            if (jumpTimeCounter > 0 && isJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }
        if (gameActions.Player.Jump.WasReleasedThisFrame())
        {
            isJumping = false;
        }

        DrawGroundCheck();
    }

    private void HandleMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (horizontal > 0 || horizontal < 0)
        {
            TurnCheck();
        }
    }

    private bool IsGrounded()
    {
        /*        return Physics2D.BoxCast(groundCheck.position, 0.2f, groundLayer);
         *        */
        groundHit = Physics2D.CapsuleCast(coll.bounds.center, coll.bounds.size, CapsuleDirection2D.Vertical, 0f, Vector2.down, extraHeight, groundLayer);

        if (groundHit.collider != null)
        {
            return true;
        }
        return false;
    }


    private void TurnCheck()
    {
        if (horizontal > 0 && !IsFacingRight)
        {
            Turn();
        }
        else if (horizontal < 0 && IsFacingRight)
        {
            Turn();
        }
    }
    private void Turn()
    {
        if (IsFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
            _cameraFollowObject.CallTurn();


        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;       
            _cameraFollowObject.CallTurn();

        }
    }


    #region Debug Functions
    private void DrawGroundCheck()
    {
        Color rayColor;

        if (IsGrounded())
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }

       Debug.DrawRay(coll.bounds.center, Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);

        /*Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, coll.bounds.extents.y + extraHeight), Vector2.right * (coll.bounds.extents.x * 2), rayColor);*/



    }

    #endregion
}