using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DictionarySerializer : MonoBehaviour
{
    [Header("Ensure each element of each list has a pair in the other, otherwise it won't work!")]
    public List<int> screenIndex;
    public List<GameObject> gameObjects;
}
