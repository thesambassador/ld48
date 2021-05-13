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

	public int ProcessBulletHitAt(ContactPoint2D hit, BulletProperties props, bool shouldExplode) {
		Vector3 hitPosition = Vector3.zero;
		hitPosition.x = hit.point.x - 0.1f * hit.normal.x;
		hitPosition.y = hit.point.y - 0.1f * hit.normal.y;

		Vector3Int tilePos = TargetTilemap.WorldToCell(hitPosition);

		return TryDestroyTileAtAllChunks(tilePos, props, shouldExplode);
	}

	public int TryDestroyTileAtAllChunks(Vector3Int target, BulletProperties props = null, bool shouldExplode = false) {
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
				return targetChunk.TryDestroyTileAt(target, props, shouldExplode);
			}
		}
		else {
			TryDestroyTileAt(target, 0, props, shouldExplode);
			return 1;
		}
		return 1;
	}

	public void TryDestroyTileAt(Vector3Int target, int ore=0, BulletProperties props = null, bool shouldExplode = false) {
		
		Vector3 tileCenter = CellToTileCenter(target);

		TileBase tile = TargetTilemap.GetTile(target);
		DestructibleTile dTile = tile as DestructibleTile;
		if (dTile != null) {
			if (dTile.Destructible) {
				if (dTile.DestroyEffect != null) {
					GameObject effect = Instantiate(dTile.DestroyEffect);
					effect.transform.position = tileCenter;
				}
				bool tileExplodes = dTile.Explodes;
				float radius = dTile.ExplodeRadius;

				if(props != null) {
					//if this is an explosive tile, use the explosive multiplier
					if (tileExplodes) {
						radius *= props.ExplosiveMultiplier;
					}
					//otherwise, if this is an explosive bullet...
					else if(shouldExplode) {
						tileExplodes = true;
						radius = props.ExplodeRadius;
					}
				}



				
				TargetTilemap.SetTile(target, null);
				if(ore > 0) {
					for (int i = 0; i < ore; i++) {
						OreParticle newOre = Instantiate(OrePrefab);
						newOre.transform.position = tileCenter;
						newOre.Target = PlayerObject;
					}
				}

				if (tileExplodes) {
					DoExplosion(target, radius);
				}

			}
		}
	}

	public void DoExplosion(Vector3Int explosionCenter, float radius, bool doChunks = true) {
		print("Explosion at " + explosionCenter + " radius of " + radius);
		Vector3 worldCenter = TargetTilemap.GetCellCenterWorld(explosionCenter);

		if (radius >= 2) {
			Camera.main.DOComplete();
			Camera.main.DOShakePosition(.2f, radius / 2);
		}

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
