using UnityEngine;


[CreateAssetMenu(menuName = "Map/MapTerrain", fileName = "TerrainSettings")]
public class MapTerrain : ScriptableObject
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh }
    public DrawMode drawMode = DrawMode.ColourMap;

    [Header("Map Size")]
    [Min(1)] public int mapWidth = 256;
    [Min(1)] public int mapHeight = 256;

    [Header("Noise")]
    [Min(0.0001f)] public float noiseScale = 50f;
    [Min(0)] public int octaves = 4;
    [Range(0, 1)] public float persistance = 0.5f;
    [Min(1)] public float lacunarity = 2f;
    public Vector2 offset;

    [Header("Regions")]
    [Min(1)] public int heightCount = 2;
    public TerrainType[] regions;
    public Gradient regionGradient;
    public bool reverseGradient = false;

    [Header("Blockiness")]
    [Min(1)] public int blockSize = 4;

    [Header("Height / Mesh")]
    [Range(0.1f, 5f)] public float heightPower = 1f;
    public float meshHeightMulti = 1f;
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Seed & Defaults")]
    public bool useSeed = true;
    public int seed = 0;

    [Tooltip("If 'Use Seed' is ON, these defaults are applied at generation time.")]
    public float defaultNoiseScale = 50f;
    public int defaultOctaves = 4;
    [Range(0, 1)] public float defaultPersistance = 0.5f;
    public float defaultLacunarity = 2f;
    public int defaultBlockSize = 10;

    // Make the asset self-consistent when edited
    private void OnValidate()
    {
        if (lacunarity < 1f) lacunarity = 1f;
        if (octaves < 0) octaves = 0;
        if (heightCount < 1) heightCount = 1;

        RebuildRegionsArray();
    }

    public void RebuildRegionsArray()
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

    private Color SampleRegionColor(int index, int count)
    {
        float u = (count <= 1) ? 0f : index / (count - 1f);
        if (reverseGradient) u = 1f - u;
        float s = Mathf.Lerp(0f, 1f, u);
        return regionGradient != null ? regionGradient.Evaluate(s) : Color.white;
    }
}

[System.Serializable]
public struct TerrainType
{
    public float height;
    public Color colour;
}

