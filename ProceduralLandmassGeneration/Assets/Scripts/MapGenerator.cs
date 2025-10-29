using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public MapTerrain mapTerrain; 

    public void GenerateMap()
    {
        float noiseScale = mapTerrain.useSeed ? mapTerrain.defaultNoiseScale   : mapTerrain.noiseScale;
        int octaves = mapTerrain.useSeed ? mapTerrain.defaultOctaves      : mapTerrain.octaves;
        float persistance = mapTerrain.useSeed ? mapTerrain.defaultPersistance  : mapTerrain.persistance;
        float lacunarity = mapTerrain.useSeed ? mapTerrain.defaultLacunarity   : mapTerrain.lacunarity;
        int blockSize = mapTerrain.useSeed ? mapTerrain.defaultBlockSize    : mapTerrain.blockSize;
        int seed = mapTerrain.seed;

        int mapWidth  = Mathf.Max(1, mapTerrain.mapWidth);
        int mapHeight = Mathf.Max(1, mapTerrain.mapHeight);

        mapTerrain.RebuildRegionsArray();
        var regions = mapTerrain.regions;

        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapWidth:   mapWidth,
            mapHeight:  mapHeight,
            seed:       seed,
            scale:      noiseScale,
            octaves:    octaves,
            persistance:persistance,
            lacunarity: lacunarity,
            offset:     mapTerrain.offset
        );

        if (blockSize > 1)
        {
            float[,] blocky = new float[mapWidth, mapHeight];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int sampleX = Mathf.FloorToInt(x / (float)blockSize) * blockSize;
                    int sampleY = Mathf.FloorToInt(y / (float)blockSize) * blockSize;
                    sampleX = Mathf.Clamp(sampleX, 0, mapWidth  - 1);
                    sampleY = Mathf.Clamp(sampleY, 0, mapHeight - 1);
                    blocky[x, y] = noiseMap[sampleX, sampleY];
                }
            }
            noiseMap = blocky;
        }

        // Snap heights to region thresholds
       
        float[,] snapped = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float h = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (h <= regions[i].height)
                    {
                        snapped[x, y] = regions[i].height;
                        break;
                    }
                }
            }
        }
        noiseMap = snapped;
   

        // Build colour map
        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        // Display
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (display == null)
        {
            Debug.LogWarning("MapGenerator: No MapDisplay found in scene.");
            return;
        }

        if (mapTerrain.drawMode == MapTerrain.DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (mapTerrain.drawMode == MapTerrain.DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
        else
        {
            //Mesh
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(
                    noiseMap,
                    Mathf.Pow(mapTerrain.meshHeightMulti, mapTerrain.heightPower),
                    mapTerrain.heightCurve
                ),
                TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight)
            );
        }
    }

    private void OnValidate()
    {
        //Keeps the scene previews responsive while not in playmode
        if (!Application.isPlaying && mapTerrain != null)
            GenerateMap();
    }
}
