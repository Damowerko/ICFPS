//basically handles everything that the player controller doesn't. All actions objects firing ect, anything that interacts with the world
//the player controller is just input and raycasting etc.

using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour
{

    //just some variables for movement
    [SerializeField]
    private float standHeight = 1.5f;
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float acceleration = 10f;
    [SerializeField]
    private float jumpForce = 10f;

    //camera movement
    [SerializeField]
    private float aimSensetivity = 3f;
    [SerializeField]
    private float cameraRotationLimit = 89f;

    //jetpack variables
    [SerializeField]
    private float maxJetpackTime = 20f;
    [SerializeField]
    private float fuelRegenSpeed = 1f;
    private float currentJetpackTime;
    private bool jetpack = false;
    [SerializeField]
    private float jetpackThrust = 100f;

    Rigidbody rb;
    [SerializeField]
    Camera cam = null;

    private Vector3 movementForce;
    private bool jumpToggle = false;
    private Vector3 rotation;
    private float cameraRotationX;
    private float currentCameraRotationX;
    private Vector3 localVelocity;

    //grenade vars
    [SerializeField]
    private Rigidbody grenade = null;
    [SerializeField]
    private float throwPower = 100f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentJetpackTime = maxJetpackTime;
    }

    void FixedUpdate()
    {
        if (Grounded())
        {
            PerformMovement();
            PerformJump();
        }
        PerformJetpack();
        PerformRotation();


        //HACK: (Torque)
        //When You walk your character spins due to friction, this is a simple work around but might want to get rid of
        rb.AddTorque(-rb.angularVelocity*100);
    }

    //this checks if we are touching the ground
    private bool Grounded()
    {
        return (Physics.Raycast(rb.transform.position, Vector3.down, standHeight));
    }

    //used to pass values from the player controleller
    public void Move(Vector3 _direction)
    {
        localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        if (_direction.x == 0 && localVelocity.x > 0.5f)
            _direction.x = -localVelocity.normalized.x;
        if (_direction.z == 0 && localVelocity.z > 0.5f)
            _direction.z = -localVelocity.normalized.z;
        movementForce = _direction * acceleration;
    }

    //used to perform the movement
    void PerformMovement()
    {
        if (rb.velocity.magnitude < speed)
            rb.AddRelativeForce(movementForce);
    }

    public void Jump()
    {
        jumpToggle = true;
    }

    public void PerformJump()
    {
        if (jumpToggle == true)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpToggle = false;
    }

    public void Rotate(Vector3 _rotation, float _cameraRotationX)
    {
        rotation = _rotation * aimSensetivity;
        cameraRotationX = _cameraRotationX * aimSensetivity;
    }

    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            currentCameraRotationX += cameraRotationX;
            //clamp the rotation within limits
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

            //apply our camera tranformation, use local euler so that we do not rotate left right
            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }

    public void Jetpack(bool _state)
    {
        jetpack = _state;
    }

    private void PerformJetpack()
    {
        if (currentJetpackTime > 0 && jetpack)
        {
            rb.AddForce(Vector3.up * jetpackThrust);
            rb.AddRelativeForce(movementForce);
            currentJetpackTime -= Time.fixedDeltaTime;
        }
        else if (currentJetpackTime < maxJetpackTime)
            currentJetpackTime += Time.fixedDeltaTime * fuelRegenSpeed;
    }

    [Command]
    public void CmdThrowGrenade()
    {
        Rigidbody grenadeClone = (Rigidbody)Instantiate(grenade, cam.transform.position + cam.transform.forward, cam.transform.rotation);
        grenadeClone.velocity = rb.velocity;
        grenadeClone.AddForce(grenadeClone.transform.forward * throwPower, ForceMode.Impulse);

        NetworkServer.Spawn(grenadeClone.gameObject);
    }

    public void setDefaults()
    {
        currentJetpackTime = maxJetpackTime;
    }
}
