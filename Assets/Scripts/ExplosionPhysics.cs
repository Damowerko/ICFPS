using UnityEngine;
using UnityEngine.Networking;

public class ExplosionPhysics : NetworkBehaviour {

	private const string PLAYER_TAG = "Player";

	private float power;
	private float radius;
    private float damage;

    public void SetParameters (float _pow, float _rad, float _damage){
		power = _pow;
		radius = _rad;
        damage = _damage;
	}

	void Start () {
		Destroy(gameObject, 5f);
		transform.Rotate (-90f, 0f, 0f);

        RaycastHit _hit;

        Vector3 explosionPos = transform.position;
		//find all colliders within a sphere containing the explosionradius
		Collider[] colliders = Physics.OverlapSphere (explosionPos, radius);

		//add explosion to every rb associated
		foreach (Collider col in colliders) {
            Rigidbody rb = col.transform.GetRigidbody();
			if(rb != null) {
                Physics.Raycast(explosionPos, (rb.position-explosionPos).normalized,out _hit,radius);
                if (_hit.collider == col)
                {
                    rb.AddExplosionForce(power, explosionPos, radius, 0.5f, ForceMode.Impulse);
                    if(rb.tag == PLAYER_TAG)
                    {
                        CmdDamagePlayer(rb.name, Mathf.RoundToInt(damage /(1+(rb.position - explosionPos).magnitude)));
                    }
                }
			}
		}
	}

    [Command]
    private void CmdDamagePlayer(string _playerID, int _damage)
    {
        Player _player = GameManager.GetPlayer(_playerID);
        _player.CmdTakeDamage(_damage);
    }
}
