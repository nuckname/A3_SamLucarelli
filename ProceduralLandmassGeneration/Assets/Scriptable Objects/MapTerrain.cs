using UnityEngine;


[CreateAssetMenu(menuName = "Map/MapTerrain", fileName = "TerrainSettings")]
public class MapTerrain : ScriptableObject
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh }

	[Tooltip("Changes the display of the map")]
	public DrawMode drawMode = DrawMode.ColourMap;

    [Header("Map Size & Detail")]
        [InspectorName("Map Width")]
        [Tooltip("How wide the map is")]
        [Min(1)] public int mapWidth = 256;
    
	    [InspectorName("Map Height")]
        [Tooltip("How long the map is")]
        [Min(1)] public int mapHeight = 256;
    
        [Space(6)]
        [InspectorName("Map Zoom")]
        [Tooltip("How zoomed-in the terrain pattern is. Lower = bigger hills (chunky). Higher = finer details.")]
        [Min(0.0001f)] public float noiseScale = 50f;
    
    // ---------- Terrain Pattern (Noise) ----------
    [Header("Terrain Pattern")]
        [InspectorName("Layers")]
        [Tooltip("How many layers of noise are stacked. More layers = richer, more varied terrain (slower).")]
        [Min(0)] public int octaves = 4;
    
        [InspectorName("Smoothness")]
        [Tooltip("How quickly detail fades across layers. Lower = smoother, Higher = rougher textures.")]
        [Range(0, 1)] public float persistance = 0.5f;
    
        [InspectorName("Lacunarity")]
        [Tooltip("How much each layer zooms in compared to the previous. Higher = more tiny features.")]
        [Min(1)] public float lacunarity = 2f;
    
        [InspectorName("Offset (Pan)")]
        [Tooltip("Moves the noise pattern left/right/up/down without changing its shape.")]
        public Vector2 offset;
    
    // ---------- Regions & Colours ----------
    [Header("Regions & Colours")]
        [InspectorName("Number of Regions")]
        [Tooltip("How many color/elevation bands (e.g., water, sand, grass, rock, snow).")]
        [Min(1)] public int heightCount = 2;
    
        [InspectorName("Height")]
        [Tooltip("Each region has a height threshold (0..1) and a color.")]
        public TerrainType[] regions;
    
        [InspectorName("Region Gradient")]
        [Tooltip("Auto-generate region colors from this gradient (used when you rebuild regions).")]
        public Gradient regionGradient;
    
        [InspectorName("Reverse Gradient")]
        [Tooltip("Flip the gradient from top-to-bottom when generating region colors.")]
        public bool reverseGradient = false;

        [InspectorName("Block Size")]
        [Tooltip("How chunky the map looks. Higher = bigger square blocks.")]
        [Min(1)] public int blockSize = 4;
    
        [Header("Height")]
        [InspectorName("Height Power")]
        [Tooltip("Bends the height curve. <1 flattens low areas; >1 exaggerates peaks.")]
        [Range(0.1f, 5f)] public float heightPower = 1f;
    
        [InspectorName("Mesh Height Multiplier")]
        [Tooltip("How tall the mesh is.")]
        public float meshHeightMulti = 1f;
        
        [InspectorName("Height Curve")]
        [Tooltip("Fine-tune how raw noise (0..1) maps to final height. Left=low, Right=high.")]
    	public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    // ---------- Seed ----------
    [Header("Seed")]
        [InspectorName("Use Seed")]
        [Tooltip("Turn ON to get repeatable maps. Turn OFF for a fresh random map each time.")]
        public bool useSeed = true;
    
        [InspectorName("Seed Value")]
        [Tooltip("Type any number. The same seed + settings = the same map.")]
        public int seed = 0;
    
        [Header("Defaults")]
        [Tooltip("If 'Use Seed' is ON, these default values are used instead of the sliders above.")]
        [InspectorName("Default Noise Scale")] public float defaultNoiseScale = 50f;
    
        [InspectorName("Default Octaves")] public int defaultOctaves = 4;
        
        [InspectorName("Default Persistence")]
        [Range(0, 1)] public float defaultPersistance = 0.5f;
        
        [InspectorName("Default Lacunarity")] public float defaultLacunarity = 2f;
    
    	[InspectorName("Default Block Size")] public int defaultBlockSize = 10;

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

