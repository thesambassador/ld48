using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public DestructibleTile[] TerrainTiles;
	public TileBase[] OreTiles;
	public DestructibleTile[] ExplosiveTiles;

	public List<DestructibleTilemap> GeneratedChunks;

	public CustomPerlin MainPerlin;
	public CustomPerlin SolidThresholdPerlin;
	public CustomPerlin ExplosivePerlin;

	private Dictionary<LevelFeatureSpawnPool, List<LevelFeature>> _spawnPoolFeatures;
	public LevelFeature[] LevelFeaturePrefabs;

	// Start is called before the first frame update
	void Awake() {
		SeedOffset = Random.Range(0.0f, 5000.0f);
		MainPerlin = new CustomPerlin(SeedOffset, WorldMultiplier);
		SolidThresholdPerlin = new CustomPerlin(SeedOffset, SolidThresholdMultiplier);
		ExplosivePerlin = new CustomPerlin(SeedOffset, ExplosiveSpreadFactor, true);

		_spawnPoolFeatures = new Dictionary<LevelFeatureSpawnPool, List<LevelFeature>>();

		//build level feature list
		if(LevelFeaturePrefabs != null) {
			foreach (LevelFeature levelFeature in LevelFeaturePrefabs) {
				if (!_spawnPoolFeatures.ContainsKey(levelFeature.SpawnPool)) {
					_spawnPoolFeatures[levelFeature.SpawnPool] = new List<LevelFeature>();
				}
				_spawnPoolFeatures[levelFeature.SpawnPool].Add(levelFeature);
			}
		}
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
		
		chunk.transform.position = new Vector3(xStart, yStart, 0);
		for (int x = 0; x < ChunkSize; x++) {
			for (int y = 0; y < ChunkSize; y++) {
				DestructibleTile toSet = null;
				int baseOre = 0;

				float perlin = MainPerlin.GetPerlin(x + xStart, y + yStart);
				float solidThreshold = SolidThresholdPerlin.GetPerlin(x + xStart, y + yStart);
				//if we're generating a solid tile:
				if (perlin > solidThreshold) {
					//check whether it should be explosive
					if (ShouldBeExplosive(x + xStart, y + yStart)) {
						toSet = ExplosiveTiles[0];
						//tilemap.SetTile(new Vector3Int(x, y, 0), newTile);
					}
					else {
						//determine how much ore the tile should have
						float normOre = Mathf.InverseLerp(solidThreshold, 1, perlin);
						normOre = Mathf.InverseLerp(.5f, 1, normOre);
						int oreAmount = Mathf.FloorToInt(normOre / (1.0f / OreTiles.Length));
						oreAmount = Mathf.Clamp(oreAmount, 0, OreTiles.Length);
						baseOre = oreAmount;
						//set the tile to the normal tile thing
						toSet = TerrainTiles[0];
						//tilemap.SetTile(new Vector3Int(x, y, 0), newTile);
					}
				}
				else {
					//tilemap.SetTile(new Vector3Int(x, y, 0), null);
					//chunk.ForegroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
					//chunk.OreAmount[x, y] = 0;
				}

				chunk.SetTile(x, y, toSet, baseOre);
			}
		}
		chunk.ApplyTiles();

		chunk.Caverns = Cavern.GetChunkCaverns(chunk, chunk.MinCavernSize);
		if (chunk.Caverns.Count > 0) {
			chunk.BiggestCavern = chunk.Caverns.Aggregate((i1, i2) => i1.AirTiles.Count > i2.AirTiles.Count ? i1 : i2);
		}
		else {
			chunk.BiggestCavern = null;
		}

		GenerateLevelFeatures(chunk);

	}



	private void GenerateLevelFeatures(Chunk chunk) {
		if(chunk.BiggestCavern == null) {
			print("no biggest cavern at " + chunk.ChunkStart);
			return;
		}

		foreach (LevelFeatureSpawnPool pool in _spawnPoolFeatures.Keys) {
			List<LevelFeature> featurePrefabs = GetValidLevelFeaturesForChunk(_spawnPoolFeatures[pool], chunk);
			int numFeatures = GetNumLevelFeaturesToGen(pool, chunk);
			
			if(featurePrefabs.Count > 0) {
				//print("spawning shit");
				for (int i = 0; i < numFeatures; i++) {
					LevelFeature prefabToUse = featurePrefabs.GetRandomElement();
					chunk.SpawnLevelFeatureInCavern(prefabToUse);
				}

			}
			

		}
	}

	private List<LevelFeature> GetValidLevelFeaturesForChunk(List<LevelFeature> fullList, Chunk chunk) {
		List<LevelFeature> result = new List<LevelFeature>();

		foreach (LevelFeature feature in fullList) {
			bool isValid = true;
			//check if we have any caverns of the min size
			if(chunk.BiggestCavern == null) {
				isValid = false;
			}

			foreach (Cavern cavern in chunk.Caverns) {
				if(cavern.AirTiles.Count < feature.MinCavernSize) {
					isValid = false;
				}
			}

			//check the depth
			if(chunk.ChunkStart.y > -feature.MinDepth || chunk.ChunkStart.y < -feature.MaxDepth) {
				isValid = false;
			}

			if (isValid) {
				result.Add(feature);
			}
		}

		return result;
	}

	private int GetNumLevelFeaturesToGen(LevelFeatureSpawnPool pool, Chunk chunk) {
		switch (pool) {
			case LevelFeatureSpawnPool.Doodads:
				break;
			case LevelFeatureSpawnPool.Enemies:
				break;
			case LevelFeatureSpawnPool.Traps:
				break;
			case LevelFeatureSpawnPool.Powerups:
				break;
			default:
				break;
		}
		return Random.Range(0, 6);
	}

	

	public void Update() {
		
	}
}
