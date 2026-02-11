using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunctionController : JunctionManagerBase
{
    [Header("Assign the end points of routes here")]
    [SerializeField] List<JunctionPoint> JunctionPoints;
    
    [Header("Time each car has before has to give way")]
    [SerializeField] float timeToClearJunction = 5;

   [SerializeField] private Queue<JunctionPoint> orderedPoints = new Queue<JunctionPoint>();

    [Header("Used for deciding which point should be the first. sometimes when at a junction, cars have to join a route rather than start new")]
    [SerializeField] private List<int> startOnPoint;

    private bool isManaging = false;

    private void Start()
    {
        // Subscribe to the event for each JunctionPoint
        foreach (var junctionPoint in JunctionPoints)
        {
            junctionPoint.OnPointReached += OrderPoints;
            junctionPoint.OnChoosingPath += GetPath;
        }
    }

    private void OrderPoints(JunctionPoint jp) 
    { 
       orderedPoints.Enqueue(jp);

        if (!isManaging)
            StartCoroutine(SetJunctionPriority());

    }

    //add coroutine that sets each junction in order to go
    private IEnumerator SetJunctionPriority() 
    {
        isManaging = true;

        while (orderedPoints.Count > 0)
        {
            JunctionPoint point = orderedPoints.Dequeue();

            point.SetState(JunctionPoint.JunctionState.Go);
            yield return new WaitForSeconds(timeToClearJunction);

            point.SetState(JunctionPoint.JunctionState.Stop);
            yield return new WaitForSeconds(1f);
        }

        isManaging = false;
    }


}
