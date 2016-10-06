using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 649

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour {

	//an array containing everything we want to disable when a player spawns
	[SerializeField]
	Behaviour[] componentsToDisable;
	Camera sceneCamera;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

	void Start()
	{
		//disable everything we do not want
		if (!isLocalPlayer) {

            AssignRemoteLayer();
            DisableComponents();
		
        } else {
            //setup the main camera
			sceneCamera = Camera.main;
			if (sceneCamera != null) {
				sceneCamera.gameObject.SetActive (false);
			}
		}

        GetComponent<Player>().Setup();
	}

    //on login registers player to the game manager dictionary of players
    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID , _player);
    }

    void DisableComponents()
    {
    foreach (Behaviour toDisable in componentsToDisable)
        toDisable.enabled = false;
    }

    //sets the layer to RemotePlayer of all children of this gameObject
    private void AssignRemoteLayer()
    {
        foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (trans.gameObject.layer == LayerMask.NameToLayer(remoteLayerName))
                break;
            trans.gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
        }
    }

    void OnDisable()
	{
		if (sceneCamera != null && isLocalPlayer)
			sceneCamera.gameObject.SetActive (true);

        GameManager.DeRegisterPlayer(transform.name);
	}
}
