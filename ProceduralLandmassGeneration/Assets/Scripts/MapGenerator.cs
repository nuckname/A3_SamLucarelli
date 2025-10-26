using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh };
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    [Min(1)] public int regionCount = 2;
    public TerrainType[] regions;

    public Gradient regionGradient;
    public bool reverseGradient = false;

    [Min(1)] public int blockSize = 4;

    public float meshHeightMulti = 1f;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        if (blockSize > 1) {
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
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colourMap[y * mapWidth + x] = regions[i].colour;
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
        } else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMulti), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
    }

    void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
        if (regionCount < 1) regionCount = 1;

        RebuildRegionsArray();

        if (autoUpdate) GenerateMap();
    }

    void RebuildRegionsArray()
    {
        if (regions == null || regions.Length != regionCount)
            regions = new TerrainType[regionCount];

        for (int i = 0; i < regionCount; i++) 
        {
            float t = (i + 1f) / regionCount;
            float rounded = Mathf.Round(t * 100f) / 100f;
            if (i == regionCount - 1) rounded = 1f;

            Color color;
          
            color = SampleRegionColor(i, regionCount);

            regions[i] = new TerrainType { height = rounded, colour = color };
        }
    }

    Color SampleRegionColor(int index, int count)
    {
        float u = index / (count - 1f);
        if (reverseGradient) u = 1f - u;
        float s = Mathf.Lerp(0, 1, u);
        return regionGradient.Evaluate(s);
    }
}

[System.Serializable]
public struct TerrainType
{
    public float height;
    public Color colour;
}
