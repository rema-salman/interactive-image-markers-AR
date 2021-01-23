/*
 * ImageTargetDistanceScaleManager.cs
 *
 * Project: 4ME306 - Cross Media Design and Production; AR workshop 2
 *
 * Supported Unity version: 2020.2.1f1 Personal (tested)
 *
 * Author: Rema Salman
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for the interaction between multiple Augmented Reality (AR) Markers (implemented as ImageTarget component using Vuforia Engine AR).
/// </summary>
public class ImageTargetDistanceScaleManager : MonoBehaviour
{
    #region PROPERTIES

    // references to image target (= AR marker) game objects (set in the Unity Inspector)
    // (specifically: references to ImageTargetTracker script components of these game objects)
    [Header("ImageTargetTracker References")]
    public ImageTargetTracker imageTargetContent;
    public ImageTargetTracker imageTargetPlus;
    public ImageTargetTracker imageTargetMinus; // imported and decleared imageTargetMinus (marker minus) 

    [Header("Virtual Object of Content AR Marker")]
    public Transform contentVirtualObject;              // reference to content virtual object of the CONTENT AR marker (set in the Unity Inspector)
    Vector3 contentVirtualObjectOriginalScale;  // helper value to keep track of the CONTENT AR marker's virtual object original scale
    static readonly float maxScale = 2.0f;      // helper value indicating the maximum scaling of our CONTENT AR marker's virtual object
    static readonly float minScale = 0.2f; //minimume value scale

    [Header("Debug")]
    public bool showDebugMessages = true;   // indicator whether (true) or not (false) to display custom debug messages in the Unity Console (default = true; can also be set in the Unity Inspector)

    #endregion


    #region UNITY_EVENT_FUNCTIONS

    /// <summary>
    /// Instantiation and reference setup.
    /// </summary>
    private void Awake()
    {
        // check if references are set up (via Unity Inspector) correctly (and print error message to the Unity Console if not)
        if (!imageTargetContent)   Debug.LogError("[ImageTargetDistanceManager] Reference not set: Image Target Content");
        if (!imageTargetPlus)      Debug.LogError("[ImageTargetDistanceManager] Reference not set: Image Target Plus");
        if (!contentVirtualObject) Debug.LogError("[ImageTargetDistanceManager] Reference not set: Content Virtual Object");
        if (!imageTargetMinus)      Debug.LogError("[ImageTargetDistanceManager] Reference not set: Image Target Minus"); 
        // keep track of (= "remember") the original scale of the CONTENT AR marker's virtual object at the start of the application / scene
        contentVirtualObjectOriginalScale = contentVirtualObject.transform.localScale;
    }

    /// <summary>
    /// General update routine (called once per frame).
    /// </summary>
    private void Update()
    {
        // check if the CONTENT AR marker is currently tracked
        if (imageTargetContent.isTracked)
        {
            // while CONTENT AR marker is tracked, check also if PLUS AR marker is currently tracked
            if (imageTargetPlus.isTracked)
            {
                // CONTENT and PLUS AR markers are tracked at the same time: calculate distance and scale up the CONTENT AR marker's virtual object accordingly
                float distance = distanceContentToPlus();
                if (showDebugMessages) Debug.Log("[ImageTargetDistanceManager] Distance: Content <-> Plus = " + distance);
                scaleUpContent(distance);
            }

            // target mins is detected/tracked: calculate distance and shrink scale accordingly.
            if (imageTargetMinus.isTracked)
            {
                float distance = distanceContentToMinus();
                if (showDebugMessages) Debug.Log("[ImageTargetDistanceManager] Distance: Content <-> Minus = " + distance);
                scaleDownContent(distance);
            }
            // if both markers Plus and Minus were detected, then
            // execute the corresponding function based on the nearer to content (with the shorter distance)
            // else (if both distances are equal) then ignore/continue.
            if (imageTargetPlus.isTracked && imageTargetMinus.isTracked)
            {
                float distanceP = distanceContentToPlus();
                float distanceM = distanceContentToMinus();
                if (distanceP > distanceM)
                {
                    scaleDownContent(distanceM);
                }
                if (distanceP < distanceM)
                {
                    scaleUpContent(distanceP);
                } 
            } 
        }
    }
    #endregion


    #region DISTANCE

    /// <summary>
    /// Function to calculate the distance between the CONTENT and PLUS AR markers.
    /// </summary>
    /// <returns>Float value representing the distance between the two markers. Note: Float value returns 0.0f if one or both AR markers are not detected.</returns>
    private float distanceContentToPlus()
    {
        float distance = 0.0f;
        if (imageTargetContent.isTracked && imageTargetPlus.isTracked)
        {
            Vector3 contentPosition = imageTargetContent.transform.position;
            Vector3 plusPosition    = imageTargetPlus.transform.position;
            distance = Vector3.Distance(contentPosition, plusPosition);
        }
        
        return distance;
    }

    // calculate distance between content and minus markers.
    private float distanceContentToMinus()
    {
        float distance = 0.0f;
        if (imageTargetContent.isTracked && imageTargetMinus.isTracked)
        {
            Vector3 contentPosition = imageTargetContent.transform.position;
            Vector3 minusPosition    = imageTargetMinus.transform.position;
            distance = Vector3.Distance(contentPosition, minusPosition);
        }
        return distance;
    }
    #endregion


    #region SCALING

    /// <summary>
    /// Helper function to calculate a scale factor based on a provided distance value and applying simple quantization.
    /// </summary>
    /// <param name="distance">Float value representing the distance for the scale calculation. A smaller distance value results in a larger scale value, and vice verse.</param>
    /// <returns>Float value representing the calculated scale factor.</returns>
    private float calculateScaleMultiplier(float distance)
    {
        float scaleMultiplier = 0.0001f;
        if (distance >= 3.5f) scaleMultiplier = 0.0001f;
        else if (distance >= 3.0f && distance < 3.5f) scaleMultiplier = 0.0005f;
        else if (distance >= 2.5f && distance < 3.0f) scaleMultiplier = 0.001f;
        else if (distance >= 2.0f && distance < 2.5f) scaleMultiplier = 0.005f;
        else if (distance >= 1.5f && distance < 2.0f) scaleMultiplier = 0.01f;
        else if (distance >= 1.0f && distance < 1.5f) scaleMultiplier = 0.02f;
        else if (distance < 1.0f) scaleMultiplier = 0.03f;

        return scaleMultiplier;
    }


    /// <summary>
    /// Function to scale up (= increase the size / scaling) of the virtual object attached to the CONTENT AR marker based on a provided distance value. 
    /// </summary>
    /// <param name="distance">Float value representing the distance between the CONTENT and PLUS AR markers.</param>
    private void scaleUpContent(float distance)
    {
        // get scale multiplier based on distance
        float scaleMultiplier = calculateScaleMultiplier(distance);

        // check for upper scaling maximum (to restrict the virtual object from growing any further)
        if((contentVirtualObject.transform.localScale.x < contentVirtualObjectOriginalScale.x * maxScale) &&
           (contentVirtualObject.transform.localScale.y < contentVirtualObjectOriginalScale.y * maxScale) &&
           (contentVirtualObject.transform.localScale.z < contentVirtualObjectOriginalScale.z * maxScale))
        {
			// if max scaling is not yet reached: apply scaling
			contentVirtualObject.transform.localScale += new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);         
        }

        // reposition / align virtual object to AR marker
        repositionVirtualObject();
    }

// scaling down the content object: using the same scale factor and reposition methods (as provided in the example code)
// making sure it is not transormation won't go beond the defined minScale (.2)
    private void scaleDownContent(float distance)
    {
        float scaleMultiplier = calculateScaleMultiplier(distance);
        if((contentVirtualObject.transform.localScale.x > contentVirtualObjectOriginalScale.x * minScale) &&
           (contentVirtualObject.transform.localScale.y > contentVirtualObjectOriginalScale.y * minScale) &&
           (contentVirtualObject.transform.localScale.z > contentVirtualObjectOriginalScale.z * minScale)) 
        {
			contentVirtualObject.transform.localScale -= new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);         
        }
        repositionVirtualObject();
    }
   
   
    /// <summary>
    /// Helper function to reposition the virtual object on top of the CONTENT AR marker after change of scale.
    /// </summary>
    private void repositionVirtualObject()
    {
        // reposition virtual object to "float" (align) slightly over the the physical CONTENT AR marker
        contentVirtualObject.transform.localPosition = new Vector3(contentVirtualObject.transform.localPosition.x,
                                                                    contentVirtualObject.transform.localScale.y * 0.5f + 0.25f,
                                                                    contentVirtualObject.transform.localPosition.z);
    }

    #endregion
}
