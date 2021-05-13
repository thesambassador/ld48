using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


using UnityEngine.Tilemaps;

[SelectionBase]
public class Chunk : MonoBehaviour {
	public Chunk LeftChunk;
	public Chunk RightChunk;
	public Chunk DownChunk;
	public Chunk UpChunk;
	public Vector2Int ChunkStart;
	public Tilemap ChunkTilemap;
	public Tilemap ForegroundTilemap;
	public DestructibleTilemap DTilemap;
	public MapGenerator MapGen;
	public int ChunkSize = 32;

	public Renderer NoiseRenderer;
	public TilemapRenderer TileRenderer;

	public Transform NoiseTransform;

	public int[,] OreAmount;
	public int[,] HealthAmount;

	public CustomPerlin CurPerlin;
	public List<Cavern> Caverns;
	public Cavern BiggestCavern;
	public int MinCavernSize = 10;

	private Vector3Int[] _tilePositions;
	private TileBase[] _tiles;
	public List<LevelFeature> SpawnedFeatures;

	// Start is called before the first frame update
	void Start() {
		NoiseRenderer.enabled = false;
		DTilemap.TileChunk = this;
		//ShowSolide();
	}

	public void Initialize(int chunkSize) {
		ChunkSize = chunkSize;
		_tilePositions = new Vector3Int[ChunkSize * ChunkSize];
		for (int x = 0; x < ChunkSize; x++) {
			for (int y = 0; y < ChunkSize; y++) {
				_tilePositions[(y * ChunkSize) + x].Set(x, y, 0);
			}
		}

		_tiles = new TileBase[_tilePositions.Length];
		OreAmount = new int[ChunkSize, ChunkSize];
		HealthAmount = new int[ChunkSize, ChunkSize];


		SpawnedFeatures = new List<LevelFeature>();
	}

	public void SetTile(int x, int y, DestructibleTile tile, int oreSqrRt) {
		int index = (y * ChunkSize) + x;
		_tiles[index] = (tile as TileBase);

		//set ore stuff
		TileBase oreTile = null;
		if(tile != null) {
			//print(oreSqrRt);
			if (oreSqrRt != 0) {
				oreTile = tile.OreForegroundTiles[oreSqrRt-1];
			}
			HealthAmount[x, y] = tile.StartingHealth;
		}
		else {
			HealthAmount[x, y] = 0;
		}
		ForegroundTilemap.SetTile(_tilePositions[index], oreTile);
		if(oreSqrRt == 0) {
			OreAmount[x, y] = 0;
		}
		else {
			OreAmount[x, y] = (int)Mathf.Pow(2, oreSqrRt);
		}

		
		
	}

	public void ApplyTiles() {
		ChunkTilemap.SetTiles(_tilePositions, _tiles);
	}

	public void SetChunkStart(int x, int y) {
		ChunkStart.Set(x, y);
		Generate();

		if (NoiseRenderer.enabled && CurPerlin != null) {
			GenerateTexture(CurPerlin);
		}
	}

	public void Generate() {
		DestroySpawnedLevelFeatures();
		MapGen.GenerateChunk(ChunkStart.x, ChunkStart.y, this);
		
		//ShowCaverns();
	}

	public void DestroySpawnedLevelFeatures() {
		foreach (LevelFeature feature in SpawnedFeatures) {
			if (feature.gameObject.activeInHierarchy) {
				Destroy(feature.gameObject);
			}
		}
		SpawnedFeatures = new List<LevelFeature>();
	}

	public TileBase[] CavernDebugTiles;
	public TileBase OutlineDebugTile;
	[NaughtyAttributes.Button("Show Caverns")]
	public void ShowCaverns() {
		
		int cavernIndex = 0;
		foreach (Cavern cav in Caverns) {
			TileBase toUse = CavernDebugTiles[cavernIndex % CavernDebugTiles.Length];

			foreach (Vector3Int pos in cav.CavernTileCoordinates) {
				ForegroundTilemap.SetTile(pos, toUse);
			}

			foreach (Vector3Int pos in cav.AirTiles) {
				ForegroundTilemap.SetTile(pos, OutlineDebugTile);
			}

			cavernIndex++;
		}
	}

	[NaughtyAttributes.Button("Show Explosive")]
	public void ShowExplosive() {
		GenerateTexture(MapGen.ExplosivePerlin);
		NoiseRenderer.enabled = true;
		TileRenderer.enabled = false;
	}

	[NaughtyAttributes.Button("Show Solid")]
	public void ShowSolide() {
		GenerateTexture(MapGen.SolidThresholdPerlin);
		NoiseRenderer.enabled = true;
	}

	[NaughtyAttributes.Button("Show Main")]
	public void ShowMain() {
		GenerateTexture(MapGen.MainPerlin);
		NoiseRenderer.enabled = true;
	}

	public void GenerateTexture(CustomPerlin toUse) {
		CurPerlin = toUse;
		NoiseTransform.localScale = new Vector3(ChunkSize, ChunkSize, 1);
		NoiseRenderer.material.mainTexture = toUse.OutputToTexture(ChunkSize, ChunkSize, ChunkStart.x, ChunkStart.y);

	}

	public int TryDestroyTileAt(Vector3Int pos, BulletProperties props = null, bool shouldExplode = false) {
		int ore = 0;
		int tileHealth = 1;
		if(pos.x >= 0 && pos.x < ChunkSize && pos.y >= 0 && pos.y < ChunkSize) {
			ore = OreAmount[pos.x, pos.y];
			if (props != null) {
				HealthAmount[pos.x, pos.y] -= props.ShotStrength;
			}
			else {
				HealthAmount[pos.x, pos.y] -= 4; //if props is null, this is probably being destroyed by another explosion
			}
			tileHealth = HealthAmount[pos.x, pos.y];
		}

		if (tileHealth <= 0) {
			DTilemap.TryDestroyTileAt(pos, ore, props, shouldExplode);
			ForegroundTilemap.SetTile(pos, null);
		}
		//print("destroyed tile at " + pos + " with ore " + ore);
		//print(tileHealth);
		return tileHealth;
	}

	public bool SpawnLevelFeatureInCavern(LevelFeature feature) {

		List<Vector3Int> coordsToUse = BiggestCavern.AirTiles;
		Vector3Int offset = Vector3Int.zero;

		switch (feature.SpawnSurface) {
			case LevelFeatureSpawnSurface.Air:
				break;
			case LevelFeatureSpawnSurface.Wall:
				coordsToUse = BiggestCavern.WallTiles;
				break;
			case LevelFeatureSpawnSurface.Ceiling:
				coordsToUse = BiggestCavern.CeilingTiles;
				if (!feature.SpawnOnSurface) {
					offset.y = 1;
				}
				break;
			case LevelFeatureSpawnSurface.Floor:
				coordsToUse = BiggestCavern.FloorTiles;
				if (!feature.SpawnOnSurface) {
					offset.y = -1;
				}
				break;
			case LevelFeatureSpawnSurface.Outline:
				coordsToUse = BiggestCavern.CavernOutlineCoordinates;
				//can't use spawn in for this yet....?
				break;
			case LevelFeatureSpawnSurface.InTerrain:
				//not implemented yet
				break;
			default:
				break;
		}

		if(coordsToUse == null || coordsToUse.Count == 0) {
			print("no valid spawn for " + feature.name + " at " + this.ChunkStart);
			return false;
		}
		Vector3Int coord = coordsToUse.GetRandomElement();

		if(!feature.SpawnOnSurface) {
			if (feature.SpawnSurface == LevelFeatureSpawnSurface.Wall) {
				offset.x = GetWallOffset(coord);
			}
			coord += offset;

			//if we're embedding into the wall, need to remove the tile there
			ChunkTilemap.SetTile(coord, null);
			ForegroundTilemap.SetTile(coord, null);
		}

		Vector3 worldPos = ChunkTilemap.GetCellCenterWorld(coord);

		LevelFeature newFeature = Instantiate(feature);
		newFeature.transform.position = worldPos;
		newFeature.SpawnAt(coord);
		SpawnedFeatures.Add(newFeature);

		return true;
	}

	public int GetWallOffset(Vector3Int airTileToCheck) {
		return 1;
	}

}
