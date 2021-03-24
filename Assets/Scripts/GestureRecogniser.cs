

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PDollarGestureRecognizer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GestureRecogniser : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
{
    private bool touchWithinTouchArea; // True if touch is within bounds of touch area
    private int strokeId = -1; // Stroke ID is the touch event. E.G "T" has two strokes. "3" has one
    private int vertexCount = 0; // LineRenderer vertex count
    private LineRenderer currentGestureLineRenderer; // Current or most recent line renderer
    private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>(); // List of linerenders drawn in touch area
    public float recognitionDelay = 1; // How long to wait to count the next interaction as part of the same drawing
    public float minimumRequiredConfidenceScore = 0.90f; // Range: 0.0f - 1.0f
    public string expectedGesture; // If set, the recogniser will only return a match for the expected gesture
    public bool limitedArea = true; // Whether to constrain the space the user can draw to the box
    public List<Gesture> trainingSet = new List<Gesture>(); // Trainingset contains the XML data of the symbols
    public List<Point> points = new List<Point>(); // Stores X,Y and stroke ID variables
    public LineRenderer gestureLineRenderer; // Transform of the linerenderer
    
    // A class can subscribe to these events.
    public delegate void MatchedGesture(string symbol, float confidence);
    public delegate void UnmatchedGesture();
    public delegate void PointerDown(PointerEventData e);
    public delegate void PointerUp(PointerEventData e);
    public event MatchedGesture OnMatchedGesture;
    public event UnmatchedGesture OnUnmatchedGesture;
    public event PointerDown OnInteractionStart;
    public event PointerUp OnInteractionEnd;


    public void Start()
    {
        // Read the XML gestures from resources
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/Numeric");
        foreach (TextAsset gestureXml in gesturesXml)
        {
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        }

        // If a gesture has been created but not yet moved to resources, we check and load any from the persistent path
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
            Debug.LogWarning(string.Format("Gesture {0} detected in persistentDataPath. Move to resources if required.", filePath));
        }
    }

    /// <summary>
    /// Called when a user presses down or clicks within the touch area
    /// </summary>
    /// <param name="eventData">Touch event data </param>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Treat as a continuation of recent touch events
        StopCoroutine("DelayRecognition");

        // We add a new stroke and reset the vertex count
        ++strokeId;
        vertexCount = 0;

        // When the user taps or clicks within the touch area, we create a line renderer
        currentGestureLineRenderer = Instantiate(gestureLineRenderer, transform.position, transform.rotation, transform) as LineRenderer;
        gestureLinesRenderer.Add(currentGestureLineRenderer);

        // We now add the touch position to the points array and add the coordinates to the line renderer
        Vector2 pointerPosition = eventData.position;
        points.Add(new Point(pointerPosition.x, -pointerPosition.y, strokeId));
        currentGestureLineRenderer.positionCount = ++vertexCount;
        currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(pointerPosition.x, pointerPosition.y, 10)));

        OnInteractionStart?.Invoke(eventData);
    }

    /// <summary>
    /// Called when a drag event is detected within the touch area
    /// </summary>
    /// <param name="eventData">Drag event data</param>
    public void OnDrag(PointerEventData eventData)
    {
        // We don't want the line renderer appearing outside of the touch area, so we return
        if ((limitedArea && !touchWithinTouchArea) || currentGestureLineRenderer == null)
        {
            return;
        }
        points.Add(new Point(eventData.position.x, -eventData.position.y, strokeId));
        currentGestureLineRenderer.positionCount = ++vertexCount;
        currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10)));
    }

    /// <summary>
    /// Called at the end of a drag event
    /// </summary>
    /// <param name="eventData">Drag event data</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        // Delay added for multiple stroke gestures, so we don't classify too early
        StartCoroutine("DelayRecognition");
        OnInteractionEnd?.Invoke(eventData);
    }

    /// <summary>
    /// Checks for classification after a set period
    /// </summary>
    /// <returns></returns>
    public IEnumerator DelayRecognition()
    {
        yield return new WaitForSeconds(recognitionDelay);
        // Classify the gesture from our points array and training set
        Gesture candidate = new Gesture(points.ToArray());
        Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

        // Only return data if the confidence exceeds the required minimum confidence score
        if (gestureResult.Score <= minimumRequiredConfidenceScore)
        {
            OnUnmatchedGesture?.Invoke();
            yield break;
        }

        // If a gesture is expected but another gesture is detected, "Incorrect Symbol" will be returned with a confidence score of 1
        if (!String.IsNullOrEmpty(expectedGesture) && expectedGesture != gestureResult.GestureClass)
        {
            OnMatchedGesture?.Invoke("Incorrect Symbol", 1);
            yield break;
        }

        // gestureResult.GestureClass represents the matched gesture
        GetComponentInChildren<Text>().text = gestureResult.GestureClass;
        
        // Send off the results to other classes subscribed to the event
        OnMatchedGesture?.Invoke(gestureResult.GestureClass, gestureResult.Score);

        DetroyLineRenderers(gameObject);
    }

    /// <summary>
    /// Called when touch enters within the touch area
    /// </summary>
    /// <param name="eventData">Touch event data</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        touchWithinTouchArea = true;
    }

    /// <summary>
    /// Called when touch exits the touch area
    /// </summary>
    /// <param name="eventData">Touch event data</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        touchWithinTouchArea = false;
        if (eventData.dragging)
        {
            gestureLinesRenderer.Clear();
            currentGestureLineRenderer = Instantiate(gestureLineRenderer, transform.position, transform.rotation, transform) as LineRenderer;
            gestureLinesRenderer.Add(currentGestureLineRenderer);
            vertexCount = 0;
        }
    }

    /// <summary>
    /// Resets all data
    /// </summary>
    public void Reset()
    {
        // Reset the text value back
        DetroyLineRenderers(gameObject);
        gameObject.GetComponentInChildren<Text>().text = "";
        strokeId = -1;
        points.Clear();
        gestureLinesRenderer.Clear();
    }

    /// <summary>
    /// Loops through the a game object and removes all line renderers 
    /// </summary>
    /// <param name="gameObject"></param>
    private void DetroyLineRenderers(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.GetComponent<LineRenderer>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}

