using UnityEngine;

/// <summary>
/// Represents a path an AI can follow during its navigation.
/// </summary>
public class Path : MonoBehaviour
{
    /// <summary> The normalized starting direction of the path.</summary>
    public Vector3 StartDirection { get => (GetPoint(1) - GetPoint(0)).normalized; }
    /// <summary> The normalized ending direction of the path.</summary>
    public Vector3 EndDirection { get => (GetPoint(Length - 1) - GetPoint(Length - 2)).normalized; }
    /// <summary> The length of the path.</summary>
    public int Length {get => points.Length;}
    [SerializeField]
    private Transform[] points;

    /// <summary>
    /// Returns the <c>Vector3</c> coordinate of the <paramref name="index"/>-th point on the path.
    /// Or <c>Vector3.positiveInfinity</c> if <paramref name="index"/> is out of bounds
    /// </summary>
    public Vector3 GetPoint(int index)
    {
        if (Length <= index || index < 0)
            return points[Length - 1].position;

        return points[index].position;
    }

    /// <summary>
    /// Returns the junction point at the end of this path.
    /// </summary>
    public JunctionPoint GetNextJunction()
    {
        return points[Length - 1].GetComponent<JunctionPoint>();
    }

    /// <summary>
    /// Returns <c>true</c> if the supplied <paramref name="index"/> is the last point on the path,
    /// <c>false</c> otherwise
    /// </summary>
    public bool IsLast(int index) { return Length - 1 <= index; }


#if UNITY_EDITOR
    /// <summary>
    /// Ensures that the path is updated, if the gameObjects representing the waypoints are changed in the Editor
    /// </summary>
    [UnityEditor.CustomEditor(typeof(Path))]
    public class PathEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Path path = (Path)target;

            System.Collections.Generic.List<Transform> transforms = new System.Collections.Generic.List<Transform>();
            foreach (Transform t in path.gameObject.transform.GetComponentInChildren<Transform>())
            {
                transforms.Add(t);
            }
            path.points = transforms.ToArray();
        }
    }
#endif
}

