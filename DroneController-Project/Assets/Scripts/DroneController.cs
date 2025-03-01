using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsBasedDroneController : MonoBehaviour
{
    public float baseSpeed = 10f;
    public float boostMultiplier = 3f;
    public float horizontalInertia = 0.2f;
    public float verticalSpeed = 5f;
    public float sinkRate = 0.5f;
    public float hoverStiffness = 10f;
    public float hoverDamping = 5f;
    public float mouseSensitivity = 2f;
    public bool invertMouseY = false;
    public float rotationInertia = 0.1f;
    public bool enableBanking = true;
    public float bankAngle = 15f;
    public float bankSpeed = 3f;

    private float targetYaw;
    private float targetPitch;
    private float targetRoll;
    private float targetAltitude;
    private Vector2 horizontalInput;
    private bool isBoosting;
    private bool useFullDirection;
    private float verticalInput;
    private Rigidbody rb;
    private GameInput gameInput;
    private bool previousUseFullDirection = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 euler = transform.eulerAngles;
        targetYaw = euler.y;
        targetPitch = euler.x;
        targetRoll = 0f;

        targetAltitude = transform.position.y;
        gameInput = new GameInput();
        gameInput.Enable();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {

        useFullDirection = gameInput.Player.Lock.ReadValue<float>() > 0.5f;
        bool currentUseFullDirection = gameInput.Player.Lock.ReadValue<float>() > 0.5f;

        if (previousUseFullDirection && !currentUseFullDirection)
        {
            targetAltitude = transform.position.y;
        }

        previousUseFullDirection = currentUseFullDirection;
        useFullDirection = currentUseFullDirection;

        float inputX = gameInput.Player.Move.ReadValue<Vector2>().x;
        float inputZ = gameInput.Player.Move.ReadValue<Vector2>().y;
        horizontalInput = new Vector2(inputX, inputZ);
        isBoosting = gameInput.Player.Dash.ReadValue<float>() > 0.5f;

        if (!useFullDirection)
        {
            verticalInput = 0f;
            if (gameInput.Player.Up.ReadValue<float>() > 0.5f)
                verticalInput = 1f;
            else if (gameInput.Player.Down.ReadValue<float>() > 0.5f)
                verticalInput = -1f;

            if (Mathf.Abs(verticalInput) < 0.01f)
                targetAltitude -= sinkRate * Time.deltaTime;
            else
                targetAltitude += verticalInput * verticalSpeed * Time.deltaTime;
        }

        float mouseX = gameInput.Player.Look.ReadValue<Vector2>().x * mouseSensitivity;
        float mouseY = gameInput.Player.Look.ReadValue<Vector2>().y * mouseSensitivity * (invertMouseY ? 1f : -1f);
        targetYaw += mouseX;
        targetPitch += mouseY;
        targetPitch = Mathf.Clamp(targetPitch, -90f, 90f);

        if (enableBanking)
            targetRoll = -horizontalInput.x * bankSpeed * bankAngle;
        else
            targetRoll = 0f;
    }

    private void FixedUpdate()
    {
        float speed = baseSpeed * (isBoosting ? boostMultiplier : 1f);
        if (useFullDirection)
        {
            Vector3 desiredDirection = (transform.forward * horizontalInput.y + transform.right * horizontalInput.x);
            if (desiredDirection.magnitude > 1f)
                desiredDirection.Normalize();

            Vector3 desiredVelocity = desiredDirection * speed;
            Vector3 velocityError = desiredVelocity - rb.linearVelocity;
            rb.AddForce(velocityError / horizontalInertia, ForceMode.Acceleration);
        }
        else
        {
            Quaternion horizontalRotation = Quaternion.Euler(0f, targetYaw, 0f);
            Vector3 desiredHorizontalVelocity = horizontalRotation * new Vector3(horizontalInput.x, 0f, horizontalInput.y);
            if (desiredHorizontalVelocity.magnitude > 1f)
                desiredHorizontalVelocity.Normalize();
            
            desiredHorizontalVelocity *= speed;

            Vector3 currentHorizontalVelocity = rb.linearVelocity;
            currentHorizontalVelocity.y = 0f;
            Vector3 horizontalVelocityError = desiredHorizontalVelocity - currentHorizontalVelocity;
            rb.AddForce(horizontalVelocityError / horizontalInertia, ForceMode.Acceleration);
            float altitudeError = targetAltitude - transform.position.y;
            float verticalForce = (altitudeError * hoverStiffness) - (rb.linearVelocity.y * hoverDamping);
            rb.AddForce(Vector3.up * verticalForce, ForceMode.Acceleration);
        }

        Quaternion targetRotation = Quaternion.Euler(targetPitch, targetYaw, targetRoll);
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(rb.rotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
            angle -= 360f;

        if (Mathf.Abs(angle) > 0.01f)
        {
            float angleRad = angle * Mathf.Deg2Rad;
            Vector3 desiredAngVel = axis.normalized * (angleRad / rotationInertia);
            Vector3 angVelError = desiredAngVel - rb.angularVelocity;
            Vector3 torque = angVelError / rotationInertia;
            rb.AddTorque(torque, ForceMode.Acceleration);
        }
    }
}