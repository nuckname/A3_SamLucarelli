using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

	public Renderer textureRenderColourMap;
	public Renderer textureRenderNoiseMap;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	
	
	public void DrawTexture(Texture2D texture, MapGenerator.MeshType meshtype) {


		if (meshtype == MapGenerator.MeshType.ColourMap)
		{
			textureRenderColourMap.sharedMaterial.mainTexture = texture;
			textureRenderColourMap.transform.localScale = new Vector3 (texture.width, 1, texture.height);
		}
		
		if (meshtype == MapGenerator.MeshType.NoiseMap)
		{
			textureRenderNoiseMap.sharedMaterial.mainTexture = texture;
			textureRenderNoiseMap.transform.localScale = new Vector3 (texture.width, 1, texture.height);
		}

	}

	public void DrawMesh(MeshData meshData, Texture2D texture)
	{
		meshFilter.sharedMesh = meshData.CreateMesh();
		meshRenderer.sharedMaterial.mainTexture = texture;
	}
	
}
