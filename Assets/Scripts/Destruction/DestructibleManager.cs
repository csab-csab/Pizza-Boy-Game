using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestructibleManager : MonoBehaviour
{
    public static DestructibleManager instance { get; private set; }

    private List<Destructible> DestroyedObjects = new List<Destructible>();

    [SerializeField]private float ResetDistance = 100f;

    private Coroutine checkRoutine;

    [SerializeField] float currentDistance;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        print(this.gameObject.name);
    }

    IEnumerator CheckDestroyedObjectsRoutine() 
    {
        while (DestroyedObjects.Count > 0) 
        {
            for (int i = DestroyedObjects.Count - 1; i >= 0; i--) 
            {
                Destructible obj = DestroyedObjects[i];

                if (obj == null)
                {
                    DestroyedObjects.RemoveAt(i);
                    continue;
                }

                currentDistance = Vector3.Distance(GetPlayerPos(), obj.transform.position);

                if (Vector3.Distance(GetPlayerPos(), obj.transform.position) >= ResetDistance)
                { 
                    obj.ResetAfterDestruction();
                    RemovefromDestroyedObject(i);
                }  
            }
            yield return new WaitForSecondsRealtime(2f);

            if (DestroyedObjects.Count == 0)
            {
                break;
            }

        }

        checkRoutine = null;
    }


    public void RegisterDestruction(Destructible obj)
    {
        //if list doesnt already contain the object we are trying to add
        if (!DestroyedObjects.Contains(obj))
        { 
            DestroyedObjects.Add(obj);

            if (checkRoutine == null)
            {
                checkRoutine = StartCoroutine(CheckDestroyedObjectsRoutine());
            }
        }
    }

    private void RemovefromDestroyedObject(int index) 
    {
        if (index > -1 && index < DestroyedObjects.Count) 
        { 
            DestroyedObjects.RemoveAt(index);
        }
    }

    private Vector3 GetPlayerPos() 
    {
        return GameManager.instance.ReturnCarPosition();
    }
}
