using UnityEngine;
using UnityEngine.Networking;

public class GrenadeController : NetworkBehaviour {

#pragma warning disable 649
    [SerializeField]
	Grenade grenade;

	[SerializeField]
	private GameObject explosion;

	void Start () {
		//when we istantiate we resease the fuse
		GameObject.Destroy (gameObject, 5.0f);
	}

	//this will activate when the fuse end
	void OnDestroy () {
        CmdInstantiateExplosion();
	}

	[Command]
	void CmdInstantiateExplosion(){
		//create the object on the server
		GameObject explosionClone = (GameObject)Instantiate (explosion, transform.position, transform.rotation);
		explosionClone.GetComponent<ExplosionPhysics>().SetParameters (grenade.power, grenade.radius,grenade.maxDamage);
        //spawn it on all clients
        NetworkServer.Spawn(explosionClone);
	}
	
}
