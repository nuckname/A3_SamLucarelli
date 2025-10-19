using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap};
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public bool autoUpdate;

	public TerrainType[] regions;
	
	[Header("Block options")]
	public bool useSpatialBlocks = false; 
	public int blockSize = 4;
	public float contrastPower = 1f; 

	private MapDisplay display;


	public void GenerateMap() 
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

	    if (useSpatialBlocks && blockSize > 1) {
	        float[,] blocky = new float[mapWidth, mapHeight];
	        for (int y = 0; y < mapHeight; y++) {
	            for (int x = 0; x < mapWidth; x++) {
	                int sampleX = Mathf.FloorToInt(x / (float)blockSize) * blockSize;
	                int sampleY = Mathf.FloorToInt(y / (float)blockSize) * blockSize;
	                sampleX = Mathf.Clamp(sampleX, 0, mapWidth - 1);
	                sampleY = Mathf.Clamp(sampleY, 0, mapHeight - 1);
	                blocky[x, y] = noiseMap[sampleX, sampleY];
	            }
	        }
	        noiseMap = blocky;
	    }

	    MapDisplay display = FindObjectOfType<MapDisplay>();
	    if (drawMode == DrawMode.NoiseMap) {
	        display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
	    } else if (drawMode == DrawMode.ColourMap) {
	        display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
	    }
}


	void OnValidate() {
		if (mapWidth < 1) {
			mapWidth = 1;
		}
		if (mapHeight < 1) {
			mapHeight = 1;
		}
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}
}

[System.Serializable]
public struct TerrainType {
	public float height;
	public Color colour;
}