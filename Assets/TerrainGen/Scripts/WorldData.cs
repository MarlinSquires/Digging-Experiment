using UnityEngine;

public class WorldData : MonoBehaviour
{
  public static readonly int WorldSizeInChunks = 128; // Always keep as an even number
  
  public static readonly int WorldSizeInVoxels = WorldSizeInChunks * ChunkData.ChunkSize;

  public static readonly int RenderDistance = 6;

}

