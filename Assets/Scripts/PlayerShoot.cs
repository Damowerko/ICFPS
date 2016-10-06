using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UltimateFracturing;

#pragma warning disable 649


public class PlayerShoot : NetworkBehaviour {

    private const string PLAYER_TAG = "Player";

    // the weapon equiped
    public PlayerWeapon weapon;
#pragma warning disable 169
    //a mask to control what the shoot raycast will hit
    [SerializeField]
    private LayerMask mask;

    void Start()
    {
        // check if the there is a camera assigned to the script
        if (weapon.bulletSpawn == null)
            Debug.LogError("PlayerShoot: No bullet spawn point referenced!");
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            CmdShoot();

            //TODO: Shoot Animation
        }
    }

    // this will calculate a trajectory for the bullet - frame by frame
    [Command]
    void CmdShoot()
    {
        StartCoroutine(Ballisitcs());
    }

    [Command]
    void CmdPlayerShot(string _playerID, int _damage)
    {
        Player _player = GameManager.GetPlayer(_playerID);
        _player.CmdTakeDamage(_damage);
    }

    //function which calculates weapon balistics frame by frame

    #region Ballistics

    public BallisticsProperties balProps;

    private IEnumerator Ballisitcs()
    {
        Vector3 oldPos = weapon.bulletSpawn.position;
        Vector3 currentPos = weapon.bulletSpawn.position;
        Vector3 velocity = weapon.bulletSpawn.forward * weapon.initialSpeed;
        float initialTime = Time.time;

        RpcStartTracerBallistics(velocity,currentPos);

        RaycastHit _hit;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            //the coroutine will terminate when the velocity is to small to do damage or bullet lifetime has expired
            if (velocity.magnitude < 20 || Time.time - initialTime > weapon.bulletLifetime)
                yield break;

            oldPos = currentPos;
            //moves the next cast point
            currentPos += velocity * Time.deltaTime;

            //to prevent drawing a line to itself
            if (currentPos != oldPos)
            {
                //collision detection
                //TODO: Add system to prevent self damage infliction.
                if (Physics.Linecast(oldPos, currentPos, out _hit))
                {
                    if (_hit.collider.tag == PLAYER_TAG)
                    {
                        CmdPlayerShot(_hit.collider.transform.parent.name,
                            Mathf.RoundToInt(weapon.damage * velocity.magnitude / weapon.initialSpeed));

                        Vector3 newVelocity = velocity * weapon.playerPenetrationVelocityBleed;

                        Rigidbody colRigidbody = _hit.transform.GetRigidbody();
                        colRigidbody.AddForceAtPosition(-(newVelocity-velocity) * weapon.bulletMass, _hit.point,ForceMode.Impulse);
                        
                        RpcStartTracerBallistics(velocity, currentPos);
                    }
                    else if (Vector3.Angle(velocity, _hit.normal) < balProps.ricochetAngle) // max angle for ricochet
                    {
                        Vector3 newVelocity = Vector3.Reflect(velocity, _hit.normal);
                        newVelocity -= new Vector3(
                            velocity.x * Random.Range(0f, balProps.ricochetUncertainty),
                            velocity.y * Random.Range(0f, balProps.ricochetUncertainty),
                            velocity.z * Random.Range(0f, balProps.ricochetUncertainty));
                        newVelocity = velocity * balProps.ricochetVelocityBleed;

                        //checks if there is a rigidbody attached to the collider and if so then changes its momentum
                        Rigidbody colRigidbody = _hit.transform.GetRigidbody();
                        if (colRigidbody != null)
                            colRigidbody.AddForceAtPosition(-(newVelocity - velocity) * weapon.bulletMass, _hit.point,ForceMode.Impulse);

                        velocity = newVelocity;
                        currentPos = _hit.point;
                        RpcStartTracerBallistics(velocity, currentPos);
                        
                    }
                    else // if the bullet stops dead
                    {
                        
                        //adding force to fragments of destructible objects
                        // checks if the object is a fragment
                        FracturedChunk chunk = _hit.collider.GetComponent<FracturedChunk>();
                        //if there is a fragment adds force to it
                        if (chunk != null)
                        {

                            //chunk.Impact(_hit.point, velocity.magnitude * weapon.bulletMass / Time.fixedDeltaTime, 0.05f, false);
                            RpcChunkHit(chunk.gameObject, _hit.point, velocity.magnitude * weapon.bulletMass / Time.fixedDeltaTime, 0.05f);

                        }
                        else // check if there is a rigidbody (at least)
                        {
                            Rigidbody colRigidbody = _hit.transform.GetRigidbody();
                            if (colRigidbody != null)
                                colRigidbody.AddForceAtPosition(velocity * weapon.bulletMass, _hit.point, ForceMode.Impulse);
                        }
                        yield break; //give up and die
                    }
                        
                }
            }

            velocity += Physics.gravity * Time.deltaTime;

            velocity -= velocity.normalized * velocity.sqrMagnitude * weapon.bulletDrag * Time.deltaTime;
        }
    }

    [ClientRpc]
    void RpcChunkHit(GameObject chunkObject, Vector3 hitPoint, float force, float radius)
    {
        chunkObject.GetComponent<FracturedChunk>().Impact(hitPoint, force, radius, false);
    }

    [ClientRpc]
    void RpcStartTracerBallistics(Vector3 _velocity, Vector3 _startPosition)
    {
        StartCoroutine(TracerBallistics(_velocity, _startPosition));
    }

    [SerializeField]
    private GameObject trailRenderer;

	//TODO: Streamline tracers.
    /// <summary>
    /// Similar to Ballistics(), but runs on clients and manages ballistics. Does not calculate richochets so after every collision it must be called again.
    /// </summary>
    private IEnumerator TracerBallistics(Vector3 _velocity, Vector3 _startPosition)
    {
        if (trailRenderer == null)
            Debug.Log("No trail renderer attached");

        Vector3 oldPos = _startPosition;
        Vector3 currentPos = _startPosition;
        float initialTime = Time.time;

        RaycastHit _hit;

        GameObject tracerClone = (GameObject)Instantiate(trailRenderer, currentPos, Quaternion.Euler(0f,0f,0f));

        tracerClone.transform.position = _startPosition - 0.2f * _velocity.normalized;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            tracerClone.transform.position = currentPos;
            //the coroutine will terminate when the velocity is to small to do damage or bullet lifetime has expired
            if (_velocity.magnitude < 20 || Time.time - initialTime > weapon.bulletLifetime)
            {   
                yield return new WaitForSeconds(0.5f);
                Destroy(tracerClone);
                yield break;
            }

            oldPos = currentPos;
            //moves the next cast point
            currentPos += _velocity * Time.deltaTime;

            //to prevent drawing a line to itself
            if (currentPos != oldPos)
            {
                //collision detection
                //TODO: Add system to prevent self damage infliction.
                if (Physics.Linecast(oldPos, currentPos, out _hit))
                {
                    yield return new WaitForSeconds(0.5f);
                    Destroy(tracerClone);
                    yield break;
                }
            }

            _velocity += Physics.gravity * Time.deltaTime;

            _velocity -= _velocity.normalized * _velocity.sqrMagnitude * weapon.bulletDrag * Time.deltaTime;

            yield return null;
        }
    }

    #endregion
}
