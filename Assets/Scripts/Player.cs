using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

#pragma warning disable 649

public class Player : NetworkBehaviour {

    // we want any class to be able to access the bool, but only the protected set to be able to change it
    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get {return _isDead;}
        protected set { _isDead = value; }
    } // this makes sure that any function can fetch isDead but only its children can change it

    [SerializeField]
    private int maxHealth = 100;
    [SyncVar]
    private int currentHealth;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    private Rigidbody rb;

    public void Setup()
    {
        // this function is called at start of PlayerSetup
        rb = gameObject.GetComponent<Rigidbody>(); // this statement is a dependency of SetDefauls() so it must come before
        
        /*wasEnabled remembers which components are enabled (they are different for localPlayer and remote)
        It is used to reenable these components on respawn*/
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }
        SetDefaults();
    }

    void Update()
    {
        CheckSuicide();
    }

    //TODO: Remove Kill button
    //this is a suicide button
    private void CheckSuicide()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.K))
            CmdTakeDamage(999);
    }

    // rpc can only be used by server so I call a command that calls the rpc
    [Command]
    public void CmdTakeDamage (int _amount)
    {
        if (isDead)
            return;
        RpcTakeDamage(_amount);
    }

    [ClientRpc]
    private void RpcTakeDamage (int _amount)
    {

        currentHealth -= _amount;

        if (currentHealth <= 0)
            Die();
    }

    // this function is called on death
    private void Die()
    {
        isDead = true;

        //disables components which control the player movement etc
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }


        Debug.Log(transform.name + " was killed.");

        // this is a coroutine because it waits 5 seconds
        StartCoroutine(Respawn());

        // lets the player model fall over on death
        rb.constraints = RigidbodyConstraints.None;

    }

    private IEnumerator Respawn()
    {
        // wait for 5 seconds
        yield return new WaitForSeconds(GameManager.singleton.matchSettings.respawnTime);

        SetDefaults();

        //besically move me back to spawn
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        //turns back to normal (in opposition to death) look Die()
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // player motor has some variables that need to be changed (jetpack)
        gameObject.GetComponent<PlayerMotor>().setDefaults();
    }
}
