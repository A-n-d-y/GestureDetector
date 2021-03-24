
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PDollarGestureRecognizer;

public class HandWritingRecognitionExample : MonoBehaviour
{
    public GestureRecogniser gestureRecogniser; // Gesture Class
    public InputField expectedGestureInputField; // Input of the expected gesture. E.G if set to "4", only the "4" gesture will return positive
    public InputField newGestureName; // Name of new user generated gesture
    public Slider ConfidenceSlider; // Sets the confidence slider. Default is 0.9, scale is 0.0 to 1.0
    public Text identifiedGestureLabel; // Label which displays detected label and confidence score
    public Text minimumRequiredConfidenceScoreLabel; // Text to display minimum confidence score
    public Toggle autoResetToggle; // If true, everything will reset after 2 seconds of successfull classification.


    void Start()
    {
        //Subscribe to listener; Update text label once a gesture is matched.
        gestureRecogniser.OnMatchedGesture += SetIdentifiedGestureLabel;

        // Set defaults from the gestureRecogniser
        ConfidenceSlider.value = gestureRecogniser.minimumRequiredConfidenceScore;
        minimumRequiredConfidenceScoreLabel.text = gestureRecogniser.minimumRequiredConfidenceScore.ToString("F3");

        // Add event listeners
        expectedGestureInputField.onValueChanged.AddListener(delegate { OnExpectedGestureValueChanged(); });
        ConfidenceSlider.onValueChanged.AddListener(delegate { OnConfidenceSliderValueChanged(ConfidenceSlider.value); });
    }

    /// <summary>
    /// onChange event for confidence slider. The higher the value, the higher confidence required to return a classification.
    /// </summary>
    /// <param name="newConfidenceValue">New confidence value, 0 - 1</param>
    public void OnConfidenceSliderValueChanged(float newConfidenceValue)
    {
        gestureRecogniser.minimumRequiredConfidenceScore = newConfidenceValue;
        minimumRequiredConfidenceScoreLabel.text = newConfidenceValue.ToString("F3");
    }

    /// <summary>
    /// onChange for expected gesture 
    /// </summary>
    public void OnExpectedGestureValueChanged()
    {
        gestureRecogniser.expectedGesture = expectedGestureInputField.text;
    }

    /// <summary>
    /// Called when a gesture is recognised and the text label is updated
    /// </summary>
    /// <param name="gesture">Detected gesture / symbol</param>
    /// <param name="confidence">Confidence score of classification</param>
    public void SetIdentifiedGestureLabel(string gesture, float confidence)
    {
        identifiedGestureLabel.text = "Gesture identified: " + gesture + "\n Confidence: " + confidence.ToString(("F3"));

        if (autoResetToggle.isOn)
        {
            StartCoroutine(WaitAndReset(2));
        }
    }

    /// <summary>
    /// Resets the labels 
    /// </summary>
    public void resetPressed()
    {
        StartCoroutine(WaitAndReset(0));
    }

    public IEnumerator WaitAndReset(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        gestureRecogniser.Reset();
        identifiedGestureLabel.text = "Detecting...";
    }

    /// <summary>
    /// Called when user has created a new gesture and presses saved.
    /// </summary>
    public void SaveNewGesture()
    {
        if (gestureRecogniser.points.Count > 0 && newGestureName.text != "")
        {
            string fileName = String.Format("{0}/{1}-{2}.xml", Application.persistentDataPath, newGestureName.text, DateTime.Now.ToFileTime());

            // We can't save to persistentDataPath on webplayer.
            #if !UNITY_WEBPLAYER
                GestureIO.WriteGesture(gestureRecogniser.points.ToArray(), newGestureName.text, fileName);
            #endif

            // Add to the current trainingSet so it will be available on next detection.
            gestureRecogniser.trainingSet.Add(new Gesture(gestureRecogniser.points.ToArray(), newGestureName.text));
            identifiedGestureLabel.text = "Gesture Added!";
        }
        else
        {
            identifiedGestureLabel.text = "No gesture name given, or no gesture drawn!";
            return;
        }
        StartCoroutine(WaitAndReset(1));
    }
}
