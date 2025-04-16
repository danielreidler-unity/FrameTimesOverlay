using System;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class FrameTimes : MonoBehaviour
{
    // Rung buffer cache size, used to calculate Min/Max/Average values
    private const int VALUES_CACHE_SIZE = 60;
    
    [Header("Text")]
    [FormerlySerializedAs("mpdateInterval")][SerializeField, Range(3, 240)] private int m_UpdateInterval = 30;
    [FormerlySerializedAs("mainTextSize")] [SerializeField, Range(10, 200)] private int m_MainTextSize = 32;
    [FormerlySerializedAs("secondaryTextSize")] [SerializeField, Range(10, 200)] private int m_SecondaryTextSize = 26;
    [FormerlySerializedAs("textField")] [SerializeField] private TextMeshProUGUI m_TextField;

    private FrameTiming[] m_FrameTiming; // Timings for every frame
    private FrameTiming[] m_FrameValuesCache; // Ring buffer
    private int m_CacheIndex;
    private int m_FrameCounter;
    private bool m_HasTextfield;
    private StringBuilder m_StringBuilder;
    
    void Start()
    {
        m_FrameTiming = new FrameTiming[1];
        m_FrameValuesCache = new FrameTiming[VALUES_CACHE_SIZE];
        
        m_CacheIndex = 0;
        m_FrameCounter = 0;
        m_StringBuilder = new StringBuilder(1024);

        m_HasTextfield = m_TextField != null;
        if(!m_HasTextfield)
            Debug.LogError("No textfield provided, frame times will be logged instead.", this);
    }

    void Update()
    {
        // Read values every frame since some devices don't support more than that
        FrameTimingManager.CaptureFrameTimings();
        FrameTimingManager.GetLatestTimings(1, m_FrameTiming);
        
        // Adds the last read value to our ring buffer cache
        m_FrameValuesCache[m_CacheIndex] = m_FrameTiming[0];
        m_CacheIndex = (m_CacheIndex + 1) % VALUES_CACHE_SIZE;

        // Only update text on screen when needed
        m_FrameCounter++;
        if (m_FrameCounter <= m_UpdateInterval)
            return;
            
        m_FrameCounter = 0;
        double cpuMin, cpuMax, cpuAvg, gpuMin, gpuMax, gpuAvg;
        cpuMin = cpuMax = cpuAvg = m_FrameValuesCache[0].cpuFrameTime;
        gpuMin = gpuMax = gpuAvg = m_FrameValuesCache[0].gpuFrameTime;
            
        for (int i = 1; i < VALUES_CACHE_SIZE; i++)
        {
            var frame = m_FrameValuesCache[i];
            cpuMin = Math.Min(cpuMin, frame.cpuFrameTime);
            cpuMax = Math.Max(cpuMax, frame.cpuFrameTime);
            cpuAvg += frame.cpuFrameTime;
            gpuMin = Math.Min(gpuMin, frame.gpuFrameTime);
            gpuMax = Math.Max(gpuMax, frame.gpuFrameTime);
            gpuAvg += frame.gpuFrameTime;
        }
            
        cpuAvg /= VALUES_CACHE_SIZE;
        gpuAvg /= VALUES_CACHE_SIZE;

        m_StringBuilder.Clear();
        m_StringBuilder.Append($"<size={m_MainTextSize}><b>CPU:</b> {cpuAvg:00.00}</size><size={m_SecondaryTextSize}>ms <i>min:{cpuMin:00.00} max:{cpuMax:00.00}</i></size>\n");
        m_StringBuilder.Append($"<size={m_MainTextSize}><b>GPU:</b> {gpuAvg:00.00}</size><size={m_SecondaryTextSize}>ms <i>min:{gpuMin:00.00} max:{gpuMax:00.00}</i></size>");

        if (m_HasTextfield)
            m_TextField.text = m_StringBuilder.ToString();
        else
            Debug.Log(m_StringBuilder.ToString());
    }
}
