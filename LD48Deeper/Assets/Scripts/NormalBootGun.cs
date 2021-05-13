using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class NormalBootGun : BootGun
{
	public Bullet BulletPrefab;
	public float FullBootImpulse = 5;
	public float BootMinImpulse = 1;
	public float BootFullChargeTime = .3f;
	public float BootMinChargeTime = .15f;

	private float _timeSinceLastFired = 0;

	public Color UnchargedPlayerColorMin;
	public Color UnchargedPlayerColorMax;
	public Color ChargedPlayerColor;

	public SpriteRenderer PlayerSprite;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		_timeSinceLastFired += Time.deltaTime;

		Color playerColor = ChargedPlayerColor;
		if(_timeSinceLastFired < BootFullChargeTime) {
			float t = Mathf.InverseLerp(BootMinChargeTime, BootFullChargeTime, _timeSinceLastFired);
			playerColor = Color.Lerp(UnchargedPlayerColorMin, UnchargedPlayerColorMax, t);
		}
		PlayerSprite.color = playerColor;

	}

	public override bool TryShootGun() {
		if (AmmoRef.TrySpendAmmo(BulletPrefab.AmmoCost)) {
			if (PlayerRef.RB.velocity.y < 0) {
				Vector3 newVel = PlayerRef.RB.velocity;
				newVel.y *= .5f;
				PlayerRef.RB.velocity = newVel; //half downward velocity
			}
			PlayerRef.RB.AddForce(Vector2.up * GetBootImpulse(), ForceMode2D.Impulse);
			bool wasFullCharge = false;
			if(_timeSinceLastFired > BootFullChargeTime) {
				wasFullCharge = true;
			}

			_timeSinceLastFired = 0;
			Bullet newBullet = Instantiate(BulletPrefab, PlayerRef.BootBulletSpawnPoint.position, Quaternion.identity);
			newBullet.ShootBullet(Vector2.down, BulletProps, wasFullCharge);

			Vector3 punch = Vector3.down * .2f + Vector3.left * .1f;
			PlayerRef.SpriteTransform.DOComplete();
			PlayerRef.SpriteTransform.DOPunchScale(punch, .2f);
			return true;
		}
		return false;
	}

	public float GetBootImpulse() {
		float impulseFactor = Mathf.InverseLerp(BootMinChargeTime, BootFullChargeTime, _timeSinceLastFired);
		float result = Mathf.Lerp(BootMinImpulse, FullBootImpulse, impulseFactor);
		print("boots: " + result);
		return result;
	}
}
