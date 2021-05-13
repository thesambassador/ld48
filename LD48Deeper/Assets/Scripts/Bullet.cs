using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class Bullet : MonoBehaviour
{
	public bool CanDestroyTiles = false;
	public Rigidbody2D RB;
	public float BulletSpeed = 20;
	public int AmmoCost = 3;

	public BulletProperties Properties;
	private int _bulletHP = 1;
	private bool _wasFullCharge = false;

	public Color FullChargeColor;
	public Color NormalColor;

	public SpriteRenderer SpriteRend;

	public void ShootBullet(Vector2 dir, BulletProperties props = null, bool wasFullCharge = true) {
		RB.velocity = dir * BulletSpeed;
		if (props != null) {
			Properties = props;
			_bulletHP = Properties.ShotPenetration;
			_wasFullCharge = wasFullCharge;

			if (_wasFullCharge) {
				this.transform.localScale *= 1.3f;
				SpriteRend.color = FullChargeColor;
			}
			else {
				this.transform.localScale = Vector3.one;
				SpriteRend.color = NormalColor;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		print("TRIGGERED");
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (CanDestroyTiles) {
			if(collision.gameObject.TryGetComponent<DestructibleTilemap>(out DestructibleTilemap tilemap)) {
				bool shouldExplode = false;
				if(_bulletHP <= 1 && Properties.ExplodeRadius > .5f) {
					shouldExplode = true;
				}
				foreach (ContactPoint2D hit in collision.contacts) {
					int tileHealth = tilemap.ProcessBulletHitAt(hit, Properties, shouldExplode);
					if(tileHealth == 0) {
						_bulletHP--;
					}
					else {
						_bulletHP = 0;
					}
				}
			}
		}

		
		if (_bulletHP <= 0) {
			Destroy(this.gameObject);
		}
	}
}
