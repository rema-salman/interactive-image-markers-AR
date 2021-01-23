/*
 * ImageTargetTracker.cs
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
using Vuforia;                      // import functionalities of the Vuforia Engine Augmented Reality (AR) platform

/// <summary>
/// This class is intended to be attached to an ImageTarget GameObject of the Vuforia Engine AR, and function as an interface in order to keep track of tracking state and to implement custom tracking behaviours.
/// </summary>
public class ImageTargetTracker : MonoBehaviour
{
    #region PROPERTIES

    [Header("Tracking")]
    public bool isTracked;                  // property indicating if the ImageTarget is currently being tracked (true) or not (false)

    [Header("Debug")]
    public bool showDebugMessages = true;   // indicator whether (true) or not (false) to display custom debug messages in the Unity Console (default = true; can also be set in the Unity Inspector)

    #endregion


    #region UNITY_EVENT_FUNCTIONS

    /// <summary>
    /// Instantiation and reference setup.
    /// </summary>
    private void Awake()
    {
        // register automatically Actions to the EventListener of the DefaultTrackableEventHandler component (this could altenatively be done manually via Unity Inspector)
        DefaultTrackableEventHandler imageTargetEventHandler = GetComponent<DefaultTrackableEventHandler>();
        if (imageTargetEventHandler != null)
        {
            imageTargetEventHandler.OnTargetFound.AddListener(OnImageTargetTrackerFound);
            imageTargetEventHandler.OnTargetLost.AddListener(OnImageTargetTrackerLost);
        }
        else Debug.LogError("[ImageTargetTracker] DefaultTrackableEventHandler component could not be found on GameObject with name " + this.name);

        // initiate default state
        isTracked = false;
    }

    #endregion


    #region VUFORIA_TRACKING_BEHAVIOUR_FUNCTIONS

    // Note: Add these functions to the Event Handlers of the DefaultTrackableEventHandler component (of an ImageTarget) via Unity Inspector.

    /// <summary>
    /// Function to be called "OnTargetFound" event in the DefaultTrackableEventHandler component of a Vuforia ImageTarget GameObject.
    /// </summary>
    public void OnImageTargetTrackerFound()
    {
        // Note: This function features the state / behaviour changes in the event of the ImageTarget being found (= detected) by the AR Camera.

        isTracked = true;   // indicate that the ImageTarget is currently tracked

        if (showDebugMessages) Debug.Log("[ImageTargetTracker] Tracking found for " + this.name);    // display debug message in console (if allowed)
    }

    /// <summary>
    /// Function to be called "OnTargetLost" event in the DefaultTrackableEventHandler component of a Vuforia ImageTarget GameObject.
    /// </summary>
    public void OnImageTargetTrackerLost()
    {
        // Note: This function features the state / behaviour changes in the event of the ImageTarget being lost (= undetected while previous state was detected) by the AR Camera.

        isTracked = false;  // indicate that the ImageTarget is currently not tracked

        if (showDebugMessages) Debug.Log("[ImageTargetTracker] Tracking lost for " + this.name);    // display debug message in console (if allowed)
    }

    #endregion
}
