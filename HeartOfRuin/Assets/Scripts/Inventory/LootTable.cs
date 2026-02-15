using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum LootSamplingMode { PerEntry, WeightedPicks }

[Serializable]
public class LootEntry
{
    [Tooltip("Reference to the item asset")]
    public Item item;

    [Tooltip("Relative weight when using WeightedPicks mode")]
    public float weight = 1f;

    [Tooltip("Chance to drop this entry when using PerEntry mode (0-1)")]
    [Range(0f, 1f)]
    public float chance = 1f;

    [Tooltip("Minimum amount spawned when this entry is selected")]
    public int minCount = 1;

    [Tooltip("Maximum amount spawned when this entry is selected")]
    public int maxCount = 1;

    [Tooltip("If true, once this entry is picked it will not be picked again (WeightedPicks mode)")]
    public bool unique = true;
}

public class LootTable : ScriptableObject
{
    public LootSamplingMode samplingMode = LootSamplingMode.PerEntry;

    [Tooltip("If samplingMode == WeightedPicks, this is how many picks will be made.")]
    public int picks = 1;

    [SerializeField]
    public LootEntry[] entries = Array.Empty<LootEntry>();

    // Basic validation called from editor or at runtime
    public void EnsureValid()
    {
        if (entries == null) entries = Array.Empty<LootEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].minCount < 0) entries[i].minCount = 0;
            if (entries[i].maxCount < entries[i].minCount) entries[i].maxCount = entries[i].minCount;
            if (entries[i].weight < 0f) entries[i].weight = 0f;
            entries[i].chance = Mathf.Clamp01(entries[i].chance);
        }
        if (picks < 0) picks = 0;
    }

    // Sample the table.
    public List<(Item item, int qty)> Sample(System.Random? rng = null)
    {
        EnsureValid();
        List<(Item, int)> outList = new List<(Item, int)>();

        if (rng == null) rng = new System.Random();

        if (samplingMode == LootSamplingMode.PerEntry)
        {
            foreach (LootEntry e in entries)
            {
                if (e.item == null) continue;
                // roll chance
                if (UnityEngine.Random.value <= e.chance)
                {
                    int qty = e.minCount;
                    if (e.maxCount > e.minCount)
                    {
                        
                        qty = UnityEngine.Random.Range(e.minCount, e.maxCount + 1);
                    }
                    outList.Add((e.item, qty));
                }
            }
        }
        else // WeightedPicks
        {
            // clone array and weights, optionally support unique
            List<LootEntry> pool = new List<LootEntry>(entries.Where(x => x.item != null && x.weight > 0f));
            if (pool.Count == 0) return outList;

            for (int p = 0; p < picks; p++)
            {
                float total = pool.Sum(x => x.weight);
                if (total <= Mathf.Epsilon) break;

                // pick a random value in [0,total)
                double r = rng.NextDouble() * total;
                float acc = 0f;
                LootEntry? chosen = null;
                int chosenIndex = -1;
                for (int i = 0; i < pool.Count; i++)
                {
                    acc += pool[i].weight;
                    if (r < acc)
                    {
                        chosen = pool[i];
                        chosenIndex = i;
                        break;
                    }
                }

                if (chosen == null) break;

                int qty = chosen.minCount;
                if (chosen.maxCount > chosen.minCount)
                    qty = UnityEngine.Random.Range(chosen.minCount, chosen.maxCount + 1);

                outList.Add((chosen.item, qty));

                if (chosen.unique)
                {
                    pool.RemoveAt(chosenIndex);
                    if (pool.Count == 0) break;
                }
            }
        }

        return outList;
    }
}