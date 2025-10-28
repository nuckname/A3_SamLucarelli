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

    public Vector2 offset;

    [Min(1)] public int heightCount = 2;
    public TerrainType[] regions;

    public Gradient regionGradient;
    public bool reverseGradient = false;

    [Min(1)] public int blockSize = 4;

    [Header("Height")]
    [Range(0.1f, 5f)] public float heightPower = 1f;
    public float meshHeightMulti = 1f;
    public AnimationCurve heightCurve;

    [Header("Seed Settings")]
    public bool useSeed = true;
    public int seed;
    
    [Header("Default Parameters When Using Seed")]
    public float defaultNoiseScale = 50f;
    public int defaultOctaves = 4;
    [Range(0, 1)] public float defaultPersistance = 0.5f;
    public float defaultLacunarity = 2f;
    public int defaultBlockSize = 10;
    
    public void GenerateMap()
    {
        if (useSeed)
        {
            noiseScale = defaultNoiseScale;
            octaves = defaultOctaves;
            persistance = defaultPersistance;
            lacunarity = defaultLacunarity;
            blockSize = defaultBlockSize;
        }

        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapWidth: mapWidth,
            mapHeight: mapHeight,
            seed: seed,
            scale: noiseScale,
            octaves: octaves,
            persistance: persistance,
            lacunarity: lacunarity,
            offset: offset
        );

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

        float[,] snapped = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float h = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (h <= regions[i].height) {
                        snapped[x, y] = regions[i].height;
                        break;
                    }
                }
            }
        }
        noiseMap = snapped;

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
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(noiseMap, Mathf.Pow(meshHeightMulti, heightPower), heightCurve),
                TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight)
            );
        }
    }
    
    void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
        if (heightCount < 1) heightCount = 1;

        RebuildRegionsArray();

        GenerateMap();
    }

    void RebuildRegionsArray()
    {
        if (regions == null || regions.Length != heightCount)
            regions = new TerrainType[heightCount];

        for (int i = 0; i < heightCount; i++) 
        {
            float t = (i + 1f) / heightCount;
            float rounded = Mathf.Round(t * 100f) / 100f;
            if (i == heightCount - 1) rounded = 1f;

            Color color = SampleRegionColor(i, heightCount);

            regions[i] = new TerrainType { height = rounded, colour = color };
        }
    }

    Color SampleRegionColor(int index, int count)
    {
        float u = (count <= 1) ? 0f : index / (count - 1f);
        if (reverseGradient) u = 1f - u;
        float s = Mathf.Lerp(0f, 1f, u);
        return regionGradient.Evaluate(s);
    }
}

[System.Serializable]
public struct TerrainType
{
    public float height;
    public Color colour;
}
