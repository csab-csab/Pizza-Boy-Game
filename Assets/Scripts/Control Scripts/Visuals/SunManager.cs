using JetBrains.Annotations;
using UnityEngine;

public class SunManager : MonoBehaviour
{
    // Reference to the SpriteRenderer component of the sun
    [SerializeField] Transform sunTransform;
    private SpriteRenderer spriteRenderer;

    [SerializeField] Vector3[] SunPositions;
   
    [SerializeField]float[] SunRotations;

    [SerializeField] int wayPointIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void UpdateSunColour(float timeOfDay, Gradient sunColour)
    {
        Color colour =  sunColour.Evaluate(timeOfDay);
        // Set sun's color
        spriteRenderer.color = colour;
    }

    //make 24 points for every hour dirty fix I know
    public void UpdateSunPosition(float timeOfDay, float secondsFromLastHour, float durationBetweenHour, bool isForcedTimeChange = false)
    {
     
        // Calculate the index of the current waypoint based on the time of day
        float timeFraction = timeOfDay / 24f;
        int waypointIndex = Mathf.FloorToInt(timeFraction * SunPositions.Length);

        wayPointIndex = waypointIndex;

        //Ensure waypoint index loops around, avoiding an error
        if (waypointIndex >= SunPositions.Length - 1)
        {
            waypointIndex = 0;
        }

        //force sun position to forced time
        if (isForcedTimeChange) 
        { 
         sunTransform.position = SunPositions[waypointIndex];
        }

        // Move towards the current waypoint
        sunTransform.position = Vector3.Lerp(sunTransform.position, SunPositions[waypointIndex], secondsFromLastHour/durationBetweenHour * Time.deltaTime);

        //Rotate the sun based on the time of day
        Quaternion targetRotation = Quaternion.Euler(new Vector3(SunRotations[waypointIndex], 0, 0));
        sunTransform.rotation = Quaternion.Lerp(sunTransform.rotation, targetRotation, durationBetweenHour);
    }
}
