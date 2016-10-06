using UnityEngine;

[System.Serializable]
public class PlayerWeapon{

    public string name = "Glock";
    public int damage = 30;
    public float initialSpeed = 100f;
    public float bulletMass = 0.002f;
    public float bulletDrag = 5f;
    public float bulletLifetime = .01f;

    public float playerPenetrationVelocityBleed = 0.2f;

    public Transform bulletSpawn;

}
