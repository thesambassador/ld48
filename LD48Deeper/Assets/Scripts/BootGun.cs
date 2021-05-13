using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletProperties {
	public int ShotPenetration = 1;
	public int ShotStrength = 1;
	public float ExplodeRadius = .1f; //how big "charged" shots explode on their own
	public float ExplosiveMultiplier = 1; //how much bigger normal explosives blow up when hit
}


public class BootGun : MonoBehaviour
{
	public BulletProperties BulletProps;

	public PlayerController PlayerRef;
	public AmmoController AmmoRef;
	public void Awake() {

	}
	public virtual bool TryShootGun() { return true; }
	
}
