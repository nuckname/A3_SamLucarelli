using UnityEngine;

public class GradientMapTerrain : MonoBehaviour
{
    public MapTerrain mapTerrain;

    public void RebuildKeepColours()
    {
        RebuildRegionsArray(mapTerrain, keepColours: true);
    }

    public void ApplyGradientToRegions()
    {
        ApplyGradient(mapTerrain);
    }
    
    public void RebuildRegionsArray(MapTerrain mt, bool keepColours = true)
    {
        Color[] old = null;

        if (keepColours && mt.regions != null)
            old = CopyColours(mt.regions);

        if (mt.regions == null || mt.regions.Length != mt.heightCount)
            mt.regions = new TerrainType[mt.heightCount];

        for (int i = 0; i < mt.heightCount; i++)
        {
            mt.regions[i].height = ThresholdFor(i, mt.heightCount);

            if (keepColours && old != null && i < old.Length)
                mt.regions[i].colour = old[i];
        }
    }


    public void ApplyGradient(MapTerrain mt)
    {
        if (mt.regions == null || mt.regions.Length == 0) return;

        for (int i = 0; i < mt.regions.Length; i++)
            mt.regions[i].colour = SampleRegionColor(mt, i, mt.regions.Length);
    }
    
    private float ThresholdFor(int index, int count)
    {
        if (count <= 1) return 1f;
        if (index == count - 1) return 1f;

        float t = (index + 1f) / count;
        return Mathf.Round(t * 100f) / 100f;
    }

    private Color[] CopyColours(TerrainType[] src)
    {
        Color[] outCols = new Color[src.Length];
        for (int i = 0; i < src.Length; i++)
            outCols[i] = src[i].colour;

        return outCols;
    }

    private Color SampleRegionColor(MapTerrain mt, int index, int count)
    {
        float u = (count <= 1) ? 0f : index / (count - 1f);
        if (mt.reverseGradient) u = 1f - u;
        float s = Mathf.Lerp(0f, 1f, u);
        return mt.regionGradient != null ? mt.regionGradient.Evaluate(s) : Color.white;
    }
}
