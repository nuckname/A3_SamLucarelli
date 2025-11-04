using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public MapTerrain mapTerrain; 

	public enum MeshType { NoiseMap, ColourMap}
    
   public void GenerateMap()
    {
        float noiseScale = mapTerrain.useSeed ? mapTerrain.defaultNoiseScale : mapTerrain.noiseScale;
        int octaves = mapTerrain.useSeed ? mapTerrain.defaultOctaves : mapTerrain.octaves;
        float persistance = mapTerrain.useSeed ? mapTerrain.defaultPersistance : mapTerrain.persistance;
        float lacunarity = mapTerrain.useSeed ? mapTerrain.defaultLacunarity : mapTerrain.lacunarity;
        int blockSize = mapTerrain.useSeed ? mapTerrain.defaultBlockSize : mapTerrain.blockSize;
        int seed = mapTerrain.seed;

        int mapWidth  = Mathf.Max(1, mapTerrain.mapWidth);
        int mapHeight = Mathf.Max(1, mapTerrain.mapHeight);

        mapTerrain.RebuildRegionsArray();
        var regions = mapTerrain.regions;


        float[,] heightMap = Noise.GenerateNoiseMap(
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
                    blocky[x, y] = heightMap[sampleX, sampleY];
                }
            }
            heightMap = blocky;
        }

        float[,] regionMap = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        for (int x = 0; x < mapWidth; x++)
        {
            float h = heightMap[x, y];
            bool matched = false;
            for (int i = 0; i < regions.Length; i++)
            {
                if (h <= regions[i].height)
                {
                    regionMap[x, y] = regions[i].height;
                    matched = true;
                    break;
                }
            }
        }

        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float h = heightMap[x, y];
                Color c = regions[regions.Length - 1].colour;
                for (int i = 0; i < regions.Length; i++)
                {
                    if (h <= regions[i].height) 
                    { 
                        c = regions[i].colour; 
                        break; 
                    }
                }
                colourMap[y * mapWidth + x] = c;
            }
        }

        // Display
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (display == null)
        {
            Debug.LogWarning("MapGenerator: No MapDisplay found in scene.");
            return;
        }

        switch (mapTerrain.drawMode)
        {
            case MapTerrain.DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap), MeshType.NoiseMap);
                break;

            case MapTerrain.DrawMode.ColourMap:
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight), MeshType.ColourMap);
                break;

            case MapTerrain.DrawMode.Mesh:
                display.DrawMesh(
                    MeshGenerator.GenerateTerrainMesh(
                        heightMap,
                        Mathf.Pow(mapTerrain.meshHeightMulti, mapTerrain.heightPower),
                        mapTerrain.heightCurve
                    ),
                    TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight)
                );
                break;
            
            case MapTerrain.DrawMode.All:
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap), MeshType.NoiseMap);
                
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight), MeshType.ColourMap);
                
                display.DrawMesh(
                    MeshGenerator.GenerateTerrainMesh(
                        heightMap,
                        Mathf.Pow(mapTerrain.meshHeightMulti, mapTerrain.heightPower),
                        mapTerrain.heightCurve
                    ),
                    TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight)
                );
                
                break;
            }
        }
    }


    private void OnValidate()
    {
        //Keeps the scene previews responsive while not in playmode
        if (!Application.isPlaying && mapTerrain != null)
            GenerateMap();
    }
}
