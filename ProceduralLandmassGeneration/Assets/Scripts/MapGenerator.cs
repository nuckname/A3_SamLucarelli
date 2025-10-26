using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
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
    [Min(1)] public int blockSize = 4;
    [Range(0.1f, 8f)] public float contrastPower = 1f;
    
    public bool scaleBlockSizeByHeight = true;
    [Min(1)] public int blockSizeMin = 2;
    [Min(1)] public int blockSizeMax = 16;
    [Range(0.1f, 5f)] public float blockSizeCurve = 1f;
    public bool invertBlockSize = false;

    public float meshHeightMulti = 1f;

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
		
		Color[] colourMap = new Color[mapWidth * mapHeight];
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions [i].height) {
						colourMap [y * mapWidth + x] = regions [i].colour;
						break;
					}
				}
			}
		}

	    MapDisplay display = FindObjectOfType<MapDisplay>();
	    if (drawMode == DrawMode.NoiseMap) {
	        display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
	    } else if (drawMode == DrawMode.ColourMap) {
	        display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
	    }
	    else if (drawMode == DrawMode.Mesh) {
	        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMulti), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
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
		
		if (autoUpdate) {
			GenerateMap();
		}
	}
}

[System.Serializable]
public struct TerrainType {
	public float height;
	public Color colour;
}