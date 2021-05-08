using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteMap : MonoBehaviour
{
	public PlayerController PlayerObject;
	public MapGenerator MapGen;

	public Chunk TopLeft;
	public Chunk TopRight;
	public Chunk BottomLeft;

	public int ActiveChunksX = 8; //number of active chunks in width (half to each side)
	public int ActiveChunksY = 8;

	public Chunk BaseChunkPrefab;
	public float LeftSideWindow; //shift left when this is crossed
	public float RightSideWindow; //shift right when this is crossed
	public float BottomSideWindow; //shift down when this is crossed
	

    void Start()
    {
		GenerateInitial();
		//put player in the center?
		float playerX = (ActiveChunksX * MapGen.ChunkSize) / 2;
		float playerY = MapGen.ChunkSize + 10;
		BottomSideWindow = -MapGen.ChunkSize * (ActiveChunksY / 2) - (MapGen.ChunkSize / 2);
		LeftSideWindow = playerX - (MapGen.ChunkSize * 1.5f);
		RightSideWindow = LeftSideWindow + (MapGen.ChunkSize * 3);

		PlayerObject.transform.position = new Vector3(playerX, playerY);

		StartCoroutine(ChunkGenerationCoroutine());
    }

	void GenerateInitial() {
		Chunk[,] initialChunks = new Chunk[ActiveChunksX, ActiveChunksY];

		for (int x = 0; x < ActiveChunksX; x++) {
			for (int y = 0; y < ActiveChunksY; y++) {
				Chunk newChunk = Instantiate(BaseChunkPrefab, this.transform);
				newChunk.MapGen = MapGen;
				newChunk.Initialize(MapGen.ChunkSize);
				newChunk.SetChunkStart(x * MapGen.ChunkSize, -y * MapGen.ChunkSize);
				newChunk.DTilemap.PlayerObject = PlayerObject;
				initialChunks[x, y] = newChunk;
			}
		}

		TopLeft = initialChunks[0, 0];
		TopRight = initialChunks[ActiveChunksX - 1, 0];
		BottomLeft = initialChunks[0, ActiveChunksY - 1];

		//link em up
		for (int x = 0; x < ActiveChunksX; x++) {
			for (int y = 0; y < ActiveChunksY; y++) {
				//left
				if(x == 0) {
					initialChunks[x, y].LeftChunk = null;
				}
				else {
					initialChunks[x, y].LeftChunk = initialChunks[x - 1, y];
				}

				//right
				if(x == (ActiveChunksX - 1)) {
					initialChunks[x, y].RightChunk = null;
				}
				else {
					initialChunks[x, y].RightChunk = initialChunks[x + 1, y];
				}

				//up
				if(y == 0) {
					initialChunks[x, y].UpChunk = null;
				}
				else {
					initialChunks[x, y].UpChunk = initialChunks[x,y-1];
				}

				//down
				if(y == ActiveChunksY - 1) {
					initialChunks[x, y].DownChunk = null;
				}
				else {
					initialChunks[x, y].DownChunk = initialChunks[x,y+1];
				}
			}
		}
	}

	[NaughtyAttributes.Button("ShiftLeft")]
	void ShiftLeft() {
		//shave off the right edge and put it onto the left, regenerating
		// a b c becomes c a b
		Chunk topOfColumn = TopRight;
		TopRight = topOfColumn.LeftChunk;

		//slide down new right-most side and set rights to null
		Chunk rightSideChunk = TopRight;
		while(rightSideChunk != null) {
			rightSideChunk.RightChunk = null;
			rightSideChunk = rightSideChunk.DownChunk;
		}

		Chunk currentLeftColumn = TopLeft;
		Chunk currentNewColumn = topOfColumn;

		while(currentLeftColumn != null) {
			currentLeftColumn.LeftChunk = currentNewColumn;
			currentNewColumn.RightChunk = currentLeftColumn;
			currentNewColumn.LeftChunk = null;

			int newX = currentLeftColumn.ChunkStart.x - MapGen.ChunkSize; //x is one to the left of the current left
			int newY = currentNewColumn.ChunkStart.y; //y doesn't change


			QueueChunkGeneration(newX, newY, currentNewColumn);
			//currentNewColumn.SetChunkStart(newX, newY);

			currentNewColumn = currentNewColumn.DownChunk;
			currentLeftColumn = currentLeftColumn.DownChunk;
		}

		BottomLeft = BottomLeft.LeftChunk;
		TopLeft = topOfColumn;
		LeftSideWindow -= MapGen.ChunkSize;
		RightSideWindow -= MapGen.ChunkSize;
	}

	[NaughtyAttributes.Button("ShiftRight")]
	void ShiftRight() {
		//shave off the left edge and put it onto the right, regenerating
		// a b c becomes b c a
		Chunk topOfColumn = TopLeft;
		TopLeft = topOfColumn.RightChunk;
		BottomLeft = BottomLeft.RightChunk;

		//slide down new left-most side and set lefts to null
		Chunk leftSideChunk = TopLeft;
		while (leftSideChunk != null) {
			leftSideChunk.LeftChunk = null;
			leftSideChunk = leftSideChunk.DownChunk;
		}

		Chunk currentRightColumn = TopRight;
		Chunk currentNewColumn = topOfColumn;

		while (currentRightColumn != null) {
			currentRightColumn.RightChunk = currentNewColumn;
			currentNewColumn.LeftChunk = currentRightColumn;
			currentNewColumn.RightChunk = null;

			int newX = currentRightColumn.ChunkStart.x + MapGen.ChunkSize; //x is one to the left of the current left
			int newY = currentNewColumn.ChunkStart.y; //y doesn't change

			//currentNewColumn.SetChunkStart(newX, newY);
			QueueChunkGeneration(newX, newY, currentNewColumn);

			currentNewColumn = currentNewColumn.DownChunk;
			currentRightColumn = currentRightColumn.DownChunk;
		}

		TopRight = topOfColumn;
		LeftSideWindow += MapGen.ChunkSize;
		RightSideWindow += MapGen.ChunkSize;
	}

	[NaughtyAttributes.Button("ShiftDown")]
	void ShiftDown() {
		//shave off the top edge and put it onto the bottom, regenerating
		// a           b
		// b  becomes  c
		// c           a

		Chunk startOfRow = TopLeft;
		TopLeft = TopLeft.DownChunk;
		TopRight = TopRight.DownChunk;

		Chunk startOfBottomRow = BottomLeft;
		BottomLeft = startOfRow;

		//remove reference to removed top row
		Chunk curTop = TopLeft;
		while(curTop != null) {
			curTop.UpChunk = null;
			curTop = curTop.RightChunk;
		}

		//zip together new bottom row and original bottom row
		Chunk curBottomRow = startOfBottomRow;
		Chunk curNewRow = startOfRow;
		while(curBottomRow != null) {
			curBottomRow.DownChunk = curNewRow;
			curNewRow.UpChunk = curBottomRow;
			curNewRow.DownChunk = null;

			int newX = curBottomRow.ChunkStart.x; //x doesn't change
			int newY = curBottomRow.ChunkStart.y - MapGen.ChunkSize; //y goes down more

			//curNewRow.SetChunkStart(newX, newY);
			QueueChunkGeneration(newX, newY, curNewRow);

			curBottomRow = curBottomRow.RightChunk;
			curNewRow = curNewRow.RightChunk;
		}
		BottomSideWindow -= MapGen.ChunkSize;
	}

	public void CheckOnPlayer() {
		if (PlayerObject != null && PlayerObject.gameObject.activeInHierarchy) {
			if (PlayerObject.transform.position.x < LeftSideWindow) {
				ShiftLeft();
			}
			else if (PlayerObject.transform.position.x > RightSideWindow) {
				ShiftRight();
			}
			else if (PlayerObject.transform.position.y < BottomSideWindow) {
				ShiftDown();
				//ShiftDown();
			}
		}
	}

	// Update is called once per frame
	void Update()
    {
		CheckOnPlayer();
    }

	public void QueueChunkGeneration(int x, int y, Chunk chunk) {
		if(ChunkQueue == null) {
			ChunkQueue = new Queue<(int, int, Chunk)>();
		}
		chunk.ChunkStart.x = x;
		chunk.ChunkStart.y = y;

		ChunkQueue.Enqueue((x, y, chunk));
	}

	private Queue<(int, int, Chunk)> ChunkQueue;

	bool _gameRunning = true;
	//for doing chunks in multiple frames
	public IEnumerator ChunkGenerationCoroutine() {
		while (_gameRunning) {
			if(ChunkQueue != null && ChunkQueue.Count > 0) {
				var chunkInfo = ChunkQueue.Dequeue();
				chunkInfo.Item3.SetChunkStart(chunkInfo.Item1, chunkInfo.Item2);
			}
			yield return null;
		}
	}
}
 