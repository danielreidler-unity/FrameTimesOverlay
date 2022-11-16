using System;
using System.Text;
using UnityEngine;
using TMPro;

public class FrameTimes : MonoBehaviour
{
    private FrameTiming[] frameTimings;
    [SerializeField, Range(3, 240)] private int updateInterval = 30;
    [SerializeField, Range(10, 200)] private int mainTextSize = 32;
    [SerializeField, Range(10, 200)] private int secondaryTextSize = 26;

    private bool hasTextfield = false;
    [SerializeField] private TextMeshProUGUI textField;

    private int frameCounter;
    private StringBuilder stringBuilder;
    
    void Start()
    {
        frameTimings = new FrameTiming[updateInterval];
        frameCounter = 0;
        stringBuilder = new StringBuilder(1024);

        hasTextfield = textField != null;
        if(!hasTextfield)
            Debug.LogError("No textfield provided, frame times will be logged instead.", this);
    }

    void Update()
    {
        FrameTimingManager.CaptureFrameTimings();
        frameCounter++;
        if (frameCounter < updateInterval)
            return;
        
        frameCounter = 0;
        var frameTimingsCount = FrameTimingManager.GetLatestTimings((uint)frameTimings.Length, frameTimings);
        if (frameTimingsCount <= 0) 
            return;
            
        double cpuMin, cpuMax, cpuAvg, gpuMin, gpuMax, gpuAvg;
        cpuMin = cpuMax = cpuAvg = frameTimings[0].cpuFrameTime;
        gpuMin = gpuMax = gpuAvg = frameTimings[0].gpuFrameTime;
            
        for (int i = 1; i < frameTimingsCount; i++)
        {
            var frame = frameTimings[i];
            cpuMin = Math.Min(cpuMin, frame.cpuFrameTime);
            cpuMax = Math.Max(cpuMax, frame.cpuFrameTime);
            cpuAvg += frame.cpuFrameTime;
            gpuMin = Math.Min(gpuMin, frame.gpuFrameTime);
            gpuMax = Math.Max(gpuMax, frame.gpuFrameTime);
            gpuAvg += frame.gpuFrameTime;
        }
            
        // Remove min and max values from the average calculation
        if (frameTimingsCount > 2)
        {
            cpuAvg -= cpuMin;
            cpuAvg -= cpuMax;
            gpuAvg -= gpuMin;
            gpuAvg -= gpuMax;
            frameTimingsCount -= 2;
        }
            
        cpuAvg /= frameTimingsCount;
        gpuAvg /= frameTimingsCount;

        stringBuilder.Clear();
        stringBuilder.Append($"<size={mainTextSize}><b>CPU:</b> {cpuAvg:00.00}</size><size={secondaryTextSize}>ms <i>min:{cpuMin:00.00} max:{cpuMax:00.00}</i></size>\n");
        stringBuilder.Append($"<size={mainTextSize}><b>GPU:</b> {gpuAvg:00.00}</size><size={secondaryTextSize}>ms <i>min:{gpuMin:00.00} max:{gpuMax:00.00}</i></size>");

        if (hasTextfield)
            textField.text = stringBuilder.ToString();
        else
            Debug.Log(stringBuilder.ToString());
    }
}
