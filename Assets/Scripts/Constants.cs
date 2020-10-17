using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {

	// World Settings
	public static readonly int ChunkXZ = 16;
	public static readonly int ChunkY = 128;
    public static readonly int WorldSizeChunks = 100;
	public static readonly int GroundHeightConstant = 40;
    public static readonly int ViewDistanceChunks = 10;
    public static readonly int WorldSizeBlocks = WorldSizeChunks * ChunkXZ;

	// Texture Settings
    public static readonly int TextureSize = 2; // X*X
    public static readonly float NormalizedTextureSize = 1f / (float)TextureSize;

	// Player Settings
	public static readonly float walkSpeed = 2f;
    public static readonly float sprintSpeed = 4f;
    public static readonly float jumpPower = 5f;
    public static readonly float g = -9.807f; //actual gravity power
	public static readonly float playerWidth = 0.15f;
	public static readonly float playerReach = 8f;

	// Voxel Settings
	public static readonly Vector3[] voxelVertices =
	{
		new Vector3(0.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 1.0f, 1.0f),
		new Vector3(0.0f, 1.0f, 1.0f),
	};

	public static readonly int[,] voxelTriangles =
	{
        // Back, Front, Top, Bottom, Left, Right

		// 0 1 2 2 1 3
		{0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6}  // Right Face
	};

	public static readonly Vector3[] voxelFaces =
	{
		new Vector3(0.0f, 0.0f, -1.0f),
		new Vector3(0.0f, 0.0f, 1.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, -1.0f, 0.0f),
		new Vector3(-1.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 0.0f, 0.0f)
	};

	public static readonly CubeT[] cubeTypes =
	{
		new CubeT
		{
			color = "Air",
			isHard = false,
			hardness = 0,
			textureID = 0
		},
		new CubeT
		{
			color = "Grass",
			isHard = true, 
			hardness = 2, 
			textureID = 1
		},
		new CubeT
		{
			color = "Dirt", 
			isHard = true, 
			hardness = 3, 
			textureID = 0
		},
		new CubeT
		{
			color = "Stone", 
			isHard = true, 
			hardness = 5, 
			textureID = 3
		},
		new CubeT
		{
			color = "Ice", 
			isHard = true, 
			hardness = 4, 
			textureID = 2
		}
	};

	public class CubeT 
	{
		public string color;
		public bool isHard;
		public int hardness;
		public int textureID;
	}
}
