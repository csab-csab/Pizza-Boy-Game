using System;
using UnityEngine;

public class JunctionPoint : MonoBehaviour
{
    public enum JunctionState { Stop, Go };
    public JunctionState state = JunctionState.Stop;


    #region Events
    // Define a delegate for the event
    public delegate void PointReachedHandler(JunctionPoint jp);
    public delegate Path ChooseRandomPath(Path oldPath);
    // Define the event using the delegate
    public event PointReachedHandler OnPointReached;
    public event ChooseRandomPath OnChoosingPath;


    #endregion

    [SerializeField] Material stop;
    [SerializeField] Material go;

   //This method gets called by the car as it gets close
    public void PointReached()
    {
        //gives self as arguement
        OnPointReached?.Invoke(this);
    }

    public Path GetNextPath(Path currentPath)
    {

        Path result = OnChoosingPath?.Invoke(currentPath);
        if(result == null)
        {
            Debug.LogError("No possible paths found!");
            throw (new NotImplementedException()); // TODO: Implement dummy object.
        }
        return result;
    }
   

    public void SetState(JunctionState state) 
    { 
        this.state = state;

        if (state == JunctionState.Stop) 
        { 
         gameObject.GetComponent<MeshRenderer>().material = stop;
        }
        else 
        { 
         gameObject.GetComponent<MeshRenderer>().material = go;
        }
    }
}
