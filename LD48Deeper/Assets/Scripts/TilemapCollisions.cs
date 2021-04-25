using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class TilemapCollisions : MonoBehaviour
{
	public Tilemap _tilemap;
    // Start is called before the first frame update
    void Start()
    {
		//_tilemap = GetComponent<Tilemap>();
    }

	private ContactPoint2D[] _contacts = new ContactPoint2D[100];
	private void OnCollisionEnter2D(Collision2D collision) {
		Vector3 hitPosition = Vector3.zero;
		_hitPoints = new List<ContactPoint2D>();
		foreach (ContactPoint2D hit in collision.contacts) {
			print(hit.normal);
			hitPosition.x = hit.point.x - 0.1f * hit.normal.x; //FOR SOME REASON TILEMAP NORMAL IS INVERTED?!
			hitPosition.y = hit.point.y - 0.1f * hit.normal.y;
			TileBase tile = _tilemap.GetTile(_tilemap.WorldToCell(hitPosition));
			print("hit at " + _tilemap.WorldToCell(hitPosition));

			_hitPoints.Add(hit);

			
			if(tile is DestructibleTile) {
				print("yay");
			}
			else {
				print("nah");
			}
		}
	}

	private List<ContactPoint2D> _hitPoints;

	private void OnDrawGizmosSelected() {
		if (_hitPoints != null) {
			foreach (var item in _hitPoints) {
				Gizmos.DrawSphere(item.point, .03f);
				Gizmos.DrawLine(item.point, item.point + (.3f * item.normal));
			}
		}
	}


}
