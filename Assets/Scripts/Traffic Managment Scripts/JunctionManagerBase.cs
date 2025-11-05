using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides a base class for all junction managers to derive from.
/// </summary>
public abstract class JunctionManagerBase : MonoBehaviour
{
    [Header("Assign Next Possible Routes that can be taken here")]
    [SerializeField] protected List<Path> possibleRoutes;

    /// <summary>
    /// Returns a path that can be taken from the given path.
    /// </summary>
    /// <param name="oldPath">The path from which the next path should be taken.</param>
    /// <returns>A path</returns>
    protected Path GetPath(Path oldPath)
    {
        int randomValue;
        Path chosenPath;

        do
        {
            randomValue = Random.Range(0, possibleRoutes.Count);
            chosenPath = possibleRoutes[randomValue];
        }
        while (chosenPath == oldPath || ArePathsOpposite(chosenPath, oldPath));

        return chosenPath;

    }

    /// <summary>
    /// Determines whether the two paths are opposite or not.
    /// Bare in mind that this compares the start of <paramref name="path1"/> and the end of <paramref name="path2"/>.
    /// And as such it is order sensitive.
    /// </summary>
    /// <param name="path1"> First path</param>
    /// <param name="path2"> Second path</param>
    /// <returns>True if paths are opposite, false otherwise</returns>
    protected bool ArePathsOpposite(Path path1, Path path2)
    {
        Vector3 firstDirection = path1.StartDirection;
        Vector3 secondDirection = path2.EndDirection;

        float dotProduct = Vector3.Dot(firstDirection, secondDirection);

        print($"Chosen: {path1.name} Current Path: {path2.name}  Dot Product:  {dotProduct} Result: {dotProduct >= -1f}");
        return dotProduct <= -0.5;
    }

}

