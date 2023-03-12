using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GeneratorSettings", menuName = "Create Generator Settings")]
public class GeneratorSettings : ScriptableObject
{
    /// <summary>The strength that the resulting noise gets multiplied by</summary>
    public float NoiseStrength;

    /// <summary>The frequency of the noise</summary>
    public float Frequency;

    /// <summary>How often a noise gets layered to get a more varied result</summary>
    public int Octaves;

    /// <summary>Factor that scales each octave by this value down</summary>
    public float Lacunarity;

    /// <summary>Factor that reduces each octaves influence on the resulting noise</summary>
    public float Persistence;

    /// <summary>The hilliness noise decides how much of the base noise gets applied. The hilliness frequency decides how big hilly and flat areas are.</summary>
    public float HillinessFrequency;

    /// <summary>Frequency of the base height noise</summary>
    public float BaseHeightFrequency;

    /// <summary>
    /// The noise for forests returns a value between 0 and 1. This value therefore has to be between these two values and can 
    /// make a tree appear if the calculated value is bigger than this threshhold.
    /// </summary>
    public float ForestThreshhold;

    /// <summary>Frequency of the forest noise</summary>
    public float ForestFrequency;

    /// <summary>Sets how high the base height noise will be</summary>
    public float BaseHeightMultiplier;

    /// <summary>Base height that all other noise gets added to</summary>
    public float BaseHeight;

    /// <summary>Height where the water gets places</summary>
    public float WaterHeight;

    /// <summary>The resolution determines how detailed a chunk mesh will be. The higher the value, the more detailed a chunk is</summary>
    public int ChunkResolution;

    /// <summary>Sets how big a chunk is</summary>
    public int ChunkSize;
}
