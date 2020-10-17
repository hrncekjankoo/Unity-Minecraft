using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk 
{
    GameObject chunkObject;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
    World world;

    public ChunkCoordinates coordinates;

	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();

	public byte[,,] voxelMap = new byte[Constants.ChunkXZ, Constants.ChunkY, Constants.ChunkXZ];
   
    public Chunk(ChunkCoordinates _coordinates, World _world) 
    {
        coordinates = _coordinates;
        world = _world;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coordinates.x * Constants.ChunkXZ, 0f, coordinates.z * Constants.ChunkXZ);
        chunkObject.name = "Chunk " + coordinates.x + ", " + coordinates.z;

        // Fill voxel Map
        for(int y = 0; y < Constants.ChunkY; y++) 
        {
			for(int x = 0; x < Constants.ChunkXZ; x++) 
            {
				for(int z = 0; z < Constants.ChunkXZ; z++) 
                {
                    voxelMap[x, y, z] = world.GetVoxelColor(new Vector3(x, y, z) + chunkObject.transform.position);
				}
			}
		}

        UpdateChunk();
    }

	private void UpdateChunk()
    {
        // Clear previour Mesh data
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        // Update Chunk data
		for(int y = 0; y < Constants.ChunkY; y++) 
        {
			for(int x = 0; x < Constants.ChunkXZ; x++)
            {
				for(int z = 0; z < Constants.ChunkXZ; z++) 
                {
                    if (Constants.cubeTypes[voxelMap[x,y,z]].isHard)
					{
                        //Generate Voxel
                        Vector3 pos = new Vector3(x, y, z);

                        for(int p = 0; p < 6; p++) 
                        { 
                            if(!CheckVoxelHardness(pos + Constants.voxelFaces[p])) 
                            {
                                // ID of cube type
                                byte cubeID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                                // Add vertices
                                vertices.Add(pos + Constants.voxelVertices[Constants.voxelTriangles [p, 0]]);
                                vertices.Add(pos + Constants.voxelVertices[Constants.voxelTriangles [p, 1]]);
                                vertices.Add(pos + Constants.voxelVertices[Constants.voxelTriangles [p, 2]]);
                                vertices.Add(pos + Constants.voxelVertices[Constants.voxelTriangles [p, 3]]);

                                //Add Texture
                                float yTex = Constants.cubeTypes[cubeID].textureID / Constants.TextureSize;
                                float xTex = Constants.cubeTypes[cubeID].textureID - (y * Constants.TextureSize);

                                xTex *= Constants.NormalizedTextureSize;
                                yTex *= Constants.NormalizedTextureSize;

                                yTex = 1f - yTex - Constants.NormalizedTextureSize;

                                uvs.Add(new Vector2(xTex, yTex));
                                uvs.Add(new Vector2(xTex, yTex + Constants.NormalizedTextureSize));
                                uvs.Add(new Vector2(xTex + Constants.NormalizedTextureSize, yTex));
                                uvs.Add(new Vector2(xTex + Constants.NormalizedTextureSize, yTex + Constants.NormalizedTextureSize));

                                // Add triangles
                                triangles.Add(vertexIndex);
                                triangles.Add(vertexIndex + 1);
                                triangles.Add(vertexIndex + 2);
                                triangles.Add(vertexIndex + 2);
                                triangles.Add(vertexIndex + 1);
                                triangles.Add(vertexIndex + 3);
                                vertexIndex += 4;
                            }
                        }
                    }
				}
			}
		}

        // Create Mesh
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };

        //Recalculates the normals of the Mesh from the triangles and vertices.
        //After modifying the vertices it is often useful to update the normals to reflect the change. Normals are calculated from all shared vertices.
		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

    public void EditVoxel(Vector3 pos, byte colorID)
    {
        int xInt = Mathf.FloorToInt(pos.x);
        int yInt = Mathf.FloorToInt(pos.y);
        int zInt = Mathf.FloorToInt(pos.z);

        xInt = xInt - Mathf.FloorToInt(chunkObject.transform.position.x);
        zInt = zInt - Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xInt, yInt, zInt] = colorID;

        Vector3 thisVoxel = new Vector3(xInt, yInt, zInt);
        
        for(int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + Constants.voxelFaces[p];

            if(!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(currentVoxel + chunkObject.transform.position).UpdateChunk();
            }
        }

        UpdateChunk();
    }

	private bool CheckVoxelHardness(Vector3 pos)
    {
		int x = Mathf.FloorToInt(pos.x);
		int y = Mathf.FloorToInt(pos.y);
		int z = Mathf.FloorToInt(pos.z);

		if (!IsVoxelInChunk(x, y, z))
        {
            return Constants.cubeTypes[world.GetVoxelColor(pos + chunkObject.transform.position)].isHard;
        }	

		return Constants.cubeTypes[voxelMap [x, y, z]].isHard;
	}

    public bool isChunkActive 
    {
        get 
        { 
            return chunkObject.activeSelf; 
        }
        set 
        { 
            chunkObject.SetActive(value); 
        }
    }

    private bool IsVoxelInChunk(int x, int y, int z) 
    {
        if (x < 0 || x > Constants.ChunkXZ - 1 || y < 0 || y > Constants.ChunkY - 1 || z < 0 || z > Constants.ChunkXZ - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

public class ChunkCoordinates
{
	public int x;
	public int z;

	public ChunkCoordinates(int _x, int _z) 
	{
		x = _x;
		z = _z;
	}

	public bool isSameChunk(ChunkCoordinates otherChunkCoordinates) 
	{
		if(otherChunkCoordinates == null)
		{
			return false;
		}
		else if(otherChunkCoordinates.x == x && otherChunkCoordinates.z == z)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}