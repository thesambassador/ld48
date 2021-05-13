using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelFeatureSpawnSurface {
	Air, //no adjacent solids
	Wall, //adjacent solid on left/right
	Ceiling, //adjacent solid above
	Floor, //adjacent solid below
	Outline, //adjacent solid anywhere (wall/ceiling/floor)
	InTerrain //inside of a solid object
}

public enum LevelFeatureSpawnPool {
	Doodads,
	Enemies,
	Traps,
	Powerups
}

public class LevelFeature : MonoBehaviour
{
	//definition stuff
	public LevelFeatureSpawnSurface SpawnSurface; //what type of surface this spawns on 
	public LevelFeatureSpawnPool SpawnPool; //what type of object this should be put into (and limited by)
	public int MinCavernSize; //won't spawn in caverns smaller than this
	public int MinDepth = 0; //doesn't spawn above this depth
	public int MaxDepth = int.MaxValue; //doesn't spawn below this depth
	public bool SpawnOnSurface = true; //if true, spawns on the empty square attached to the surface, if false, spawns embedded into the ground

	[NaughtyAttributes.HorizontalLine]
	//instance stuff?
	public Vector3Int SpawnCoordinate; //when spawned, this is where it spawned

    
	public void SpawnAt(Vector3Int spawnPosition) {
		SpawnCoordinate = spawnPosition;
		OnSpawn();
	}

	protected virtual void OnSpawn() {}
}
