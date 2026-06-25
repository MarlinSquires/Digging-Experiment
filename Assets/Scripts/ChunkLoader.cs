using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
   
    [SerializeField] private Material material;
    
    [SerializeField] private BlockType blockType;
}


[System.Serializable]
public class BlockType
{
    public string name;
    public bool opaque;

}