﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

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

	public CustomPerlin CurPerlin;

	private Vector3Int[] _tilePositions;
	private TileBase[] _tiles;

	// Start is called before the first frame update
	void Start() {
		NoiseRenderer.enabled = false;
		DTilemap.TileChunk = this;
		//ShowSolide();
	}

	public void Initialize(int chunkSize) {
		ChunkSize = chunkSize;
		_tilePositions = new Vector3Int[ChunkSize * ChunkSize];
		_tiles = new TileBase[_tilePositions.Length];
		OreAmount = new int[ChunkSize, ChunkSize];
	}

	public void SetTile(int x, int y, DestructibleTile tile, int oreSqrRt) {
		int index = (y * ChunkSize) + x;
		_tiles[index] = (tile as TileBase);

		//set ore stuff
		ForegroundTilemap.SetTile(_tilePositions[index], tile.OreForegroundTiles[oreSqrRt]);
		OreAmount[x, y] = (int)Mathf.Pow(2, oreSqrRt);
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
		MapGen.GenerateChunk(ChunkStart.x, ChunkStart.y, this);
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

	public void TryDestroyTileAt(Vector3Int pos) {
		int ore = 0;
		if(pos.x >= 0 && pos.x < ChunkSize && pos.y >= 0 && pos.y < ChunkSize) {
			ore = OreAmount[pos.x, pos.y];
		}
		DTilemap.TryDestroyTileAt(pos, ore);
		ForegroundTilemap.SetTile(pos, null);
		//print("destroyed tile at " + pos + " with ore " + ore);
	}

}
