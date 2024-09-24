using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float groundDrag = 5f;

    public float jumpForce = 10f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.5f;
    private bool readyToJump;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 15f;
    public float sprintStaminaCost = 20f;
    private float currentStamina;
    private bool isSprinting;

    [Header("UI Elements")]
    public Image staminaBarImage;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private Rigidbody rb;

    // Disable player movement
    public bool isDisabled = false;
    private bool isMoving = false;

    // variable to control stamina regeneration delay after sprinting
    private float staminaRegenDelay = 1.5f;
    private float regenTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        currentStamina = maxStamina;
    }

    private void Update()
    {
        // Disable player input and stamina management when isDisabled is true
        if (isDisabled) return;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        // Handle input
        MyInput();

        // Control speed
        SpeedControl();

        // Handle stamina regeneration or depletion
        HandleStamina();

        // Update the stamina bar
        UpdateStaminaBar();

        // Apply drag when grounded, zero drag when in air
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        
        CheckIfMoving();
    }

    private void FixedUpdate()
    {
        // Disable player movement when isDisabled is true
        if (isDisabled) return;

        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jumping
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Sprinting (only if stamina is above a minimum threshold)
        if (Input.GetKey(sprintKey) && currentStamina > 0.6f && isMoving && !isDisabled) // Requires at least 0.6 stamina to start sprinting
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Use normal speed if stamina is zero or not sprinting
        var appliedSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Move player with Rigidbody force
        if (grounded)
            rb.AddForce(moveDirection.normalized * appliedSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * appliedSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Get player's velocity, excluding the Y axis (vertical axis)
        var flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity to the current movement speed (walk or sprint)
        var currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (flatVel.magnitude > currentSpeed)
        {
            var limitedVel = flatVel.normalized * currentSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset Y velocity before applying jump force
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Apply jump force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void HandleStamina()
    {
        if (isSprinting && currentStamina > 0)
        {
            // Decrease stamina while sprinting
            currentStamina -= sprintStaminaCost * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            // Reset the stamina regeneration timer whenever sprinting
            regenTimer = staminaRegenDelay;

            // If stamina is zero, stop sprinting immediately
            if (currentStamina <= 0)
            {
                isSprinting = false;
            }
        }
        else
        {
            // Only regenerate stamina after the delay when not sprinting
            if (regenTimer <= 0)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            }
            else
            {
                regenTimer -= Time.deltaTime; // Countdown the regeneration delay timer
            }
        }
    }

    private void UpdateStaminaBar()
    {
        // Update the stamina bar fill amount based on current stamina
        staminaBarImage.fillAmount = currentStamina / maxStamina;
    }
    
    private void CheckIfMoving()
    {
        isMoving = horizontalInput != 0 || verticalInput != 0;
    }
}
