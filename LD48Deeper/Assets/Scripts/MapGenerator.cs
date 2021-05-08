using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour {
	public int ChunkSize = 32;

	public float ExplosiveSpreadFactor = 4;
	public float ExplosiveThreshold = .95f;
	public float SeedOffset;
	public float WorldMultiplier = .25f;
	public float SolidThresholdMultiplier = 16;
	public DestructibleTilemap TilemapPrefab;
	public PlayerController PlayerObject;

	public TileBase[] TerrainTiles;
	public TileBase[] OreTiles;
	public TileBase[] ExplosiveTiles;

	public List<DestructibleTilemap> GeneratedChunks;

	public CustomPerlin MainPerlin;
	public CustomPerlin SolidThresholdPerlin;
	public CustomPerlin ExplosivePerlin;

	// Start is called before the first frame update
	void Awake() {
		SeedOffset = Random.Range(0.0f, 5000.0f);
		MainPerlin = new CustomPerlin(SeedOffset, WorldMultiplier);
		SolidThresholdPerlin = new CustomPerlin(SeedOffset, SolidThresholdMultiplier);
		ExplosivePerlin = new CustomPerlin(SeedOffset, ExplosiveSpreadFactor, true);

		//GeneratedChunks = new List<DestructibleTilemap>();
		//for (int x = -4; x < 4; x++) {
		//	for (int y = -1; y >= -8; y--) {
		//		GeneratedChunks.Add(GenerateChunk(x * 16, y * 16));
		//	}
		//}
	}


	public void DestroyAllChunks() {
		if(GeneratedChunks != null) {
			for (int i = 0; i < GeneratedChunks.Count; i++) {
				Destroy(GeneratedChunks[i].gameObject);
			}
		}
		GeneratedChunks = null;
	}

	bool ShouldBeExplosive(float x, float y) {
		float perlin = ExplosivePerlin.GetPerlin(x, y); //inverting
		if(perlin > ExplosiveThreshold) {
			return true;
		}
		return false;
	}

	public void GenerateChunk(int xStart, int yStart, Chunk chunk) {
		Tilemap tilemap = chunk.ChunkTilemap;
		if(chunk.OreAmount == null) {
			chunk.OreAmount = new int[ChunkSize, ChunkSize];
		}
		
		chunk.transform.position = new Vector3(xStart, yStart, 0);
		for (int x = 0; x < ChunkSize; x++) {
			for (int y = 0; y < ChunkSize; y++) {
				float perlin = MainPerlin.GetPerlin(x + xStart, y + yStart);
				float solidThreshold = SolidThresholdPerlin.GetPerlin(x + xStart, y + yStart);
				//if we're generating a solid tile:
				if (perlin > solidThreshold) {
					//check whether it should be explosive
					if (ShouldBeExplosive(x + xStart, y + yStart)) {
						TileBase newTile = ExplosiveTiles[0];
						tilemap.SetTile(new Vector3Int(x, y, 0), newTile);
					}
					else {
						//determine how much ore the tile should have
						float normOre = Mathf.InverseLerp(solidThreshold, 1, perlin);
						normOre = Mathf.InverseLerp(.5f, 1, normOre);
						int oreAmount = Mathf.FloorToInt(normOre / (1.0f / OreTiles.Length));
						oreAmount = Mathf.Clamp(oreAmount, 0, OreTiles.Length);

						//set the tile to the normal tile thing
						TileBase newTile = TerrainTiles[0];
						tilemap.SetTile(new Vector3Int(x, y, 0), newTile);

						//if there's ore, need to set the foreground tile and ore amount
						if (oreAmount != 0) {
							TileBase oreTile = OreTiles[oreAmount-1];
							chunk.ForegroundTilemap.SetTile(new Vector3Int(x, y, 0), oreTile);
							chunk.OreAmount[x, y] = (int)Mathf.Pow(2, oreAmount);
						}
						else {
							chunk.ForegroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
							chunk.OreAmount[x, y] = 0;
						}
					}
				}
				else {
					tilemap.SetTile(new Vector3Int(x, y, 0), null);
					chunk.ForegroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
					chunk.OreAmount[x, y] = 0;
				}
			}
		}
	}

	Vector3Int[] _positions;
	TileBase[] _tiles;
	public void GenerateChunkBatch(int xStart, int yStart, Chunk chunk) {
		if (_positions == null) {
			_positions = new Vector3Int[ChunkSize * ChunkSize];
			_tiles = new TileBase[_positions.Length];
		}

		Tilemap tilemap = chunk.ChunkTilemap;

		chunk.transform.position = new Vector3(xStart, yStart, 0);
		for (int x = 0; x < ChunkSize; x++) {
			for (int y = 0; y < ChunkSize; y++) {
				float perlin = MainPerlin.GetPerlin(x + xStart, y + yStart);
				float solidThreshold = SolidThresholdPerlin.GetPerlin(x + xStart, y + yStart);
				if (perlin > solidThreshold) {
					if (ShouldBeExplosive(x + xStart, y + yStart)) {
						TileBase newTile = ExplosiveTiles[0];
						_tiles[(y * ChunkSize) + x] = newTile;
						_positions[(y * ChunkSize) + x].Set(x, y, 0);
					}
					else {
						float normOre = Mathf.InverseLerp(solidThreshold, 1, perlin);
						normOre = Mathf.InverseLerp(.5f, 1, normOre);
						int oreAmount = Mathf.FloorToInt(normOre / (1.0f / TerrainTiles.Length));
						oreAmount = Mathf.Clamp(oreAmount, 0, TerrainTiles.Length - 1);
						TileBase newTile = TerrainTiles[oreAmount];
						_tiles[(y * ChunkSize) + x] = newTile;
						_positions[(y * ChunkSize) + x].Set(x, y, 0);
					}
				}
				else {
					_tiles[(y * ChunkSize) + x] = null;
					_positions[(y * ChunkSize) + x].Set(x, y, 0);
				}
			}
		}
		tilemap.SetTiles(_positions, _tiles);
	}

	public void Update() {
		
	}
}
