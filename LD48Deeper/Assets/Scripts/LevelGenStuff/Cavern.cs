using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Cavern
{
	public List<Vector3Int> CavernTileCoordinates; //tiles that make up the cavern
	public List<Vector3Int> CavernOutlineCoordinates; //tiles on the outline
	public List<Vector3Int> WallTiles; //tiles against a wall
	public List<Vector3Int> FloorTiles; //tiles above a solid tile
	public List<Vector3Int> CeilingTiles; //tiles below a solid tile
	public List<Vector3Int> AirTiles; //tiles without any solid tiles around them
	

	public int FurthestLeft { get; private set; }
	public int FurthestRight { get; private set; }
	public int FurthestDown { get; private set; }
	public int FurthestUp { get; private set; }
	public int Width{
		get	{
			return FurthestRight - FurthestLeft;
		}
	}
	public int Height{
		get	{
			return FurthestUp - FurthestDown;
		}
	}



	public Cavern() {
		CavernTileCoordinates = new List<Vector3Int>();
		CavernOutlineCoordinates = new List<Vector3Int>();
		WallTiles = new List<Vector3Int>();
		FloorTiles = new List<Vector3Int>();
		CeilingTiles = new List<Vector3Int>();
		AirTiles = new List<Vector3Int>();
	}

	public void AddTile(Vector3Int newTile) {
		CavernTileCoordinates.Add(newTile);
		//update furthest stuff?
	}

	public static List<Cavern> GetChunkCaverns(Chunk chunk, int minCavernSize = 0) {
		List<Cavern> result = new List<Cavern>();

		Tilemap tilemap = chunk.ChunkTilemap;
		int[,] visitedTiles = new int[chunk.ChunkSize, chunk.ChunkSize];

		for (int x = 0; x < chunk.ChunkSize; x++) {
			for (int y = 0; y < chunk.ChunkSize; y++) {
				//if we haven't visited this yet, check it
				if(visitedTiles[x,y] == 0) {
					Vector3Int coord = new Vector3Int(x, y, 0);
					TileBase tile = tilemap.GetTile(coord);
					//empty tile
					if (tile == null) {
						Cavern newCavern = FloodfillCavern(coord, tilemap, ref visitedTiles);
						if (newCavern.AirTiles.Count > minCavernSize) {
							result.Add(newCavern);
						}
					}
					else {
						visitedTiles[x, y] = 1; //mark the tile if it isn't an empty one
					}
				}
				//don't need to do anything if we have visited, just continue
				
			}
		}
		
		return result;
	}

	private static Cavern FloodfillCavern(Vector3Int start, Tilemap tilemap, ref int[,] visitedTiles) {
		Cavern result = new Cavern();
		Queue<Vector3Int> toCheck = new Queue<Vector3Int>();
		toCheck.Enqueue(start);
		while(toCheck.Count > 0) {
			Vector3Int curCoord = toCheck.Dequeue();
			result.AddTile(curCoord);
			visitedTiles[curCoord.x, curCoord.y] = 1;

			Vector3Int nextLoc = curCoord;

			bool addToOutline = false;
			bool addToWall = false;

			//can check left
			if(curCoord.x > 0) {
				nextLoc.x = curCoord.x - 1;

				if (tilemap.GetTile(nextLoc) == null) {
					//check if not visited
					if (visitedTiles[nextLoc.x, nextLoc.y] == 0) {
						toCheck.Enqueue(nextLoc);
					}
				}
				else {
					addToOutline = true;
					addToWall = true;
				}
				visitedTiles[nextLoc.x, nextLoc.y] = 1;
			}

			//can check right
			if (curCoord.x < visitedTiles.GetUpperBound(0)) {
				nextLoc.x = curCoord.x + 1;

				if (tilemap.GetTile(nextLoc) == null) {
					//check if not visited
					if (visitedTiles[nextLoc.x, nextLoc.y] == 0) {
						toCheck.Enqueue(nextLoc);
					}
				}
				else {
					addToOutline = true;
					addToWall = true;
				}
				visitedTiles[nextLoc.x, nextLoc.y] = 1;
			}

			//can check above
			if (curCoord.y < visitedTiles.GetUpperBound(0)) {
				nextLoc.x = curCoord.x;
				nextLoc.y = curCoord.y + 1;
				if (tilemap.GetTile(nextLoc) == null) {
					//check if not visited
					if (visitedTiles[nextLoc.x, nextLoc.y] == 0) {
						toCheck.Enqueue(nextLoc);
					}
				}
				else {
					addToOutline = true;
					result.CeilingTiles.Add(curCoord);
				}
				visitedTiles[nextLoc.x, nextLoc.y] = 1;
			}

			//can check below
			if (curCoord.y > 0) {
				nextLoc.x = curCoord.x;
				nextLoc.y = curCoord.y - 1;
				if (tilemap.GetTile(nextLoc) == null) {
					//check if not visited
					if (visitedTiles[nextLoc.x, nextLoc.y] == 0) {
						toCheck.Enqueue(nextLoc);
					}
				}
				else {
					addToOutline = true;
					result.FloorTiles.Add(curCoord);
				}
				visitedTiles[nextLoc.x, nextLoc.y] = 1;
			}

			if (addToOutline) {
				result.CavernOutlineCoordinates.Add(curCoord);
			}
			else {
				result.AirTiles.Add(curCoord);
			}

			if (addToWall) {
				result.WallTiles.Add(curCoord);
			}
		}

		return result;
	}


}
