using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

using DG.Tweening;

public class DestructibleTilemap : MonoBehaviour
{
	public Tilemap TargetTilemap;
	public OreParticle OrePrefab;
	public PlayerController PlayerObject;
	public Chunk TileChunk;

	public void ProcessBulletHitAt(ContactPoint2D hit) {
		Vector3 hitPosition = Vector3.zero;
		hitPosition.x = hit.point.x - 0.1f * hit.normal.x;
		hitPosition.y = hit.point.y - 0.1f * hit.normal.y;

		Vector3Int tilePos = TargetTilemap.WorldToCell(hitPosition);

		TryDestroyTileAtAllChunks(tilePos);
	}

	public void TryDestroyTileAtAllChunks(Vector3Int target) {
		if(TileChunk != null ) {
			Chunk targetChunk = TileChunk;
			while(target.x < 0 && targetChunk != null) {
				targetChunk = targetChunk.LeftChunk;
				target.x += TileChunk.ChunkSize;
			}
			while (target.x >= TileChunk.ChunkSize && targetChunk != null) {
				targetChunk = targetChunk.RightChunk;
				target.x -= TileChunk.ChunkSize;
			}
			while (target.y < 0 && targetChunk != null) {
				targetChunk = targetChunk.DownChunk;
				target.y += TileChunk.ChunkSize;
			}

			while (target.y >= TileChunk.ChunkSize && targetChunk != null) {
				targetChunk = targetChunk.UpChunk;
				target.y -= TileChunk.ChunkSize;
			}

			if(targetChunk != null) {
				targetChunk.TryDestroyTileAt(target);
			}


		}
		else {
			TryDestroyTileAt(target);
		}
	}

	public void TryDestroyTileAt(Vector3Int target, int ore=0) {
		
		Vector3 tileCenter = CellToTileCenter(target);

		TileBase tile = TargetTilemap.GetTile(target);
		DestructibleTile dTile = tile as DestructibleTile;
		if (dTile != null) {
			if (dTile.Destructible) {
				if (dTile.DestroyEffect != null) {
					GameObject effect = Instantiate(dTile.DestroyEffect);
					effect.transform.position = tileCenter;
				}
				bool toExplode = dTile.Explodes;

				
				TargetTilemap.SetTile(target, null);
				if(ore > 0) {
					for (int i = 0; i < ore; i++) {
						OreParticle newOre = Instantiate(OrePrefab);
						newOre.transform.position = tileCenter;
						newOre.Target = PlayerObject;
					}
				}

				if (toExplode) {
					DoExplosion(target, dTile.ExplodeRadius);
				}

			}
		}
	}

	public void DoExplosion(Vector3Int explosionCenter, float radius, bool doChunks = true) {
		//print("Explosion at " + explosionCenter + " radius of " + radius);
		Vector3 worldCenter = TargetTilemap.GetCellCenterWorld(explosionCenter);
		Camera.main.DOComplete();
		Camera.main.DOShakePosition(.2f, 1);

		int radCeil = Mathf.CeilToInt(radius);

		for(int x = explosionCenter.x - radCeil; x <= explosionCenter.x + radCeil; x++) {
			for (int y = explosionCenter.y - radCeil; y <= explosionCenter.y + radCeil; y++) {
				Vector3Int tilePos = new Vector3Int(x, y, 0);
				Vector3 tileWorldPos = TargetTilemap.GetCellCenterWorld(tilePos);
				//print("trying to destroy tile at " + tilePos);
				if (Vector3.Distance(tileWorldPos, worldCenter) <= radius) {
					
					TryDestroyTileAtAllChunks(tilePos);
				}
			}
		}

	}



	public Vector3 CellToTileCenter(Vector3Int tileCell) {
		return TargetTilemap.CellToWorld(tileCell) + (TargetTilemap.layoutGrid.cellSize * .5f);
	}
}
