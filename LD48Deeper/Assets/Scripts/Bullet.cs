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

	public void SetVelocityDirection(Vector2 dir) {
		RB.velocity = dir * BulletSpeed;
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (CanDestroyTiles) {
			if(collision.gameObject.TryGetComponent<DestructibleTilemap>(out DestructibleTilemap tilemap)) {
			
				foreach (ContactPoint2D hit in collision.contacts) {
					tilemap.ProcessBulletHitAt(hit);
				}
			}
		}

		Destroy(this.gameObject);
	}
}
