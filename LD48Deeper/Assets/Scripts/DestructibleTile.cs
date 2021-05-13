﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Tile")]
public class DestructibleTile : RuleTile
{
	public bool Destructible = false;
	public bool Explodes = false;
	public float ExplodeRadius = 2;
	public int OreAmount = 0;
	public int StartingHealth = 1;
	public TileBase[] OreForegroundTiles;
	public GameObject DestroyEffect;

}
