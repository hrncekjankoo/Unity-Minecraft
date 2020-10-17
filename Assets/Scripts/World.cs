using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class World : MonoBehaviour 
{
    public Transform player;
    public Vector3 spawn;

    public Material material;

    Chunk[,] chunks = new Chunk[Constants.WorldSizeChunks, Constants.WorldSizeChunks];

    List<ChunkCoordinates> activeChunks = new List<ChunkCoordinates>();
    ChunkCoordinates playerLastChunkCoordinates;
    public ChunkCoordinates playerChunkCoordinates;
    
    public GameObject lDebugScreen;
    public GameObject rDebugScreen;

    private void Start() 
    {
        spawn = new Vector3((Constants.WorldSizeChunks * Constants.ChunkXZ) / 2f, Constants.ChunkY - 50f, (Constants.WorldSizeChunks * Constants.ChunkXZ) / 2f);

        // Generate Ground
        for(int x = Constants.WorldSizeChunks / 2 - Constants.ViewDistanceChunks / 2; x < Constants.WorldSizeChunks / 2 + Constants.ViewDistanceChunks / 2; x++) 
        {
            for(int z = Constants.WorldSizeChunks / 2 - Constants.ViewDistanceChunks / 2; z < Constants.WorldSizeChunks / 2 + Constants.ViewDistanceChunks / 2; z++) 
            {
                CreateChunk(new ChunkCoordinates(x, z));
            }
        }

        player.position = spawn;

        playerLastChunkCoordinates = GetChunkCoordinatesFromVector3(player.transform.position);
    }

    private void Update() 
    {
        playerChunkCoordinates = GetChunkCoordinatesFromVector3(player.transform.position);

        // If chunks are not same recalculate chunks
        if (!GetChunkCoordinatesFromVector3(player.transform.position).isSameChunk(playerLastChunkCoordinates))
        {
            ChunkCoordinates coordinates = GetChunkCoordinatesFromVector3(player.position);
            playerLastChunkCoordinates = playerLastChunkCoordinates;

            int chunkX = Mathf.FloorToInt(player.position.x / Constants.ChunkXZ);
            int chunkZ = Mathf.FloorToInt(player.position.z / Constants.ChunkXZ);

            List<ChunkCoordinates> previouslyActiveChunks = new List<ChunkCoordinates>(activeChunks);

            for(int x = chunkX - Constants.ViewDistanceChunks / 2; x < chunkX + Constants.ViewDistanceChunks / 2; x++) 
            {
                for(int z = chunkZ - Constants.ViewDistanceChunks / 2; z < chunkZ + Constants.ViewDistanceChunks / 2; z++) 
                {
                    // If the chunk is within the world
                    if ((x > 0) && (x < Constants.WorldSizeChunks - 1) && (z > 0) && (z < Constants.WorldSizeChunks - 1)) 
                    {
                        ChunkCoordinates thisChunk = new ChunkCoordinates(x, z);

                        if (chunks[x, z] == null)
                        {
                            CreateChunk(thisChunk);
                        }  
                        else if (!chunks[x, z].isChunkActive) 
                        {
                            chunks[x, z].isChunkActive = true;
                            activeChunks.Add(thisChunk);
                        }
                        // Check if this chunk was already in the active chunks list.
                        for(int i = 0; i < previouslyActiveChunks.Count; i++) 
                        {
                            if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                                previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }

            foreach (ChunkCoordinates coordinatesXZ in previouslyActiveChunks)
            {
                chunks[coordinatesXZ.x, coordinatesXZ.z].isChunkActive = false;
            }
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            lDebugScreen.SetActive(!lDebugScreen.activeSelf);
            rDebugScreen.SetActive(!rDebugScreen.activeSelf);
        }
    }

    private ChunkCoordinates GetChunkCoordinatesFromVector3(Vector3 pos) 
    {
        int x = Mathf.FloorToInt(pos.x / Constants.ChunkXZ);
        int z = Mathf.FloorToInt(pos.z / Constants.ChunkXZ);

        return new ChunkCoordinates(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos) 
    {
        int x = Mathf.FloorToInt(pos.x / Constants.ChunkXZ);
        int z = Mathf.FloorToInt(pos.z / Constants.ChunkXZ);

        return chunks[x, z];
    }

    private void CreateChunk(ChunkCoordinates coordinates) 
    {
        chunks[coordinates.x, coordinates.z] = new Chunk(new ChunkCoordinates(coordinates.x, coordinates.z), this);

        activeChunks.Add(new ChunkCoordinates(coordinates.x, coordinates.z));
    }
    
    public bool IsVoxelHard(float _x, float _y, float _z) 
    {
        int xInt = Mathf.FloorToInt(_x);
        int yInt = Mathf.FloorToInt(_y);
        int zInt = Mathf.FloorToInt(_z);

        int xChunk = xInt / Constants.ChunkXZ;
        int zChunk = zInt / Constants.ChunkXZ;

        xInt = xInt - (xChunk * Constants.ChunkXZ);
        zInt = zInt - (zChunk * Constants.ChunkXZ);

        return Constants.cubeTypes[chunks[xChunk, zChunk].voxelMap[xInt, yInt, zInt]].isHard;
    }

    public int GetVoxelHardness(float _x, float _y, float _z) 
    {
        int xInt = Mathf.FloorToInt(_x);
        int yInt = Mathf.FloorToInt(_y);
        int zInt = Mathf.FloorToInt(_z);

        int xChunk = xInt / Constants.ChunkXZ;
        int zChunk = zInt / Constants.ChunkXZ;

        xInt = xInt - (xChunk * Constants.ChunkXZ);
        zInt = zInt - (zChunk * Constants.ChunkXZ);

        return Constants.cubeTypes[chunks[xChunk, zChunk].voxelMap[xInt, yInt, zInt]].hardness;
    }

    // Return Voxel Type based on height
    public byte GetVoxelColor(Vector3 pos) 
    {
        int y = Mathf.FloorToInt(pos.y);
        
        // 0 - Air
        // 1 - Grass
        // 2 - Dirt
        // 3 - Stone
        // 4 - Ice 

        // Generate height using Perlin Noise
        int groungHeight = Mathf.FloorToInt(Constants.GroundHeightConstant * Mathf.PerlinNoise((pos.x + 0.1f) / Constants.ChunkXZ * 0.3f, (pos.z + 0.1f) / Constants.ChunkXZ * 0.3f));

        // Return air if voxel is outside of world
        if((pos.x < 0) || (pos.x >= Constants.WorldSizeBlocks) || (pos.y < 0) || (pos.y >= Constants.ChunkY) || (pos.z < 0) || (pos.z >= Constants.WorldSizeBlocks))
        {
            return 0;
        }

        // return ice most down 
        if(y == 0)
        {
            return 4;
        }
        
        // Grass on Top
        if(y == groungHeight)
        {
            return 1;
        }
        // Dirt Under grass
        else if(y < groungHeight && y > groungHeight - 4)
        {
            return 2;
        }
        // Air above the groungHeight
        else if(y > groungHeight)
        {
            return 0;
        }
        // Else stone
        else 
        {
            return 3;
        }
    }
}
