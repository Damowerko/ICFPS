using UnityEngine;
using UnityEngine.Networking;

// this class handles all the inputs

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : NetworkBehaviour {

    //Component caching
	private PlayerMotor motor;
    private Animator animator;


	void Start(){
		motor = GetComponent<PlayerMotor> ();
        animator = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update () {

		//fetch movement from axis or keyboard
		float _xMov = Input.GetAxisRaw("Horizontal");
		float _yMov = Input.GetAxisRaw("Vertical");

		//make a vector
		Vector3 _movement = new Vector3 (_xMov, 0f, _yMov).normalized;
		//apply the vector to a function in the motor script
		motor.Move(_movement);

		if (Input.GetButtonDown ("Jump"))
			motor.Jump ();

		float _yRot = Input.GetAxisRaw ("Mouse X");
		Vector3 _rotation = new Vector3 (0f, _yRot, 0f);

		float _xRot = Input.GetAxisRaw ("Mouse Y");

		motor.Rotate(_rotation, -_xRot);

        //aim down sights
        if (Input.GetButton("Fire2"))
            animator.SetBool("Aim", true);
        else
            animator.SetBool("Aim", false);

		//jetpack script
		if (Input.GetButton ("Jump"))
			motor.Jetpack (true);
		else
			motor.Jetpack (false);

		//throw grenade
		if (Input.GetButtonDown ("Grenade"))
			motor.CmdThrowGrenade ();

	}
}
