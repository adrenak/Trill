﻿using UnityEngine;
using System.Linq;
using Adrenak.UniMic;

public class AudioVisualizer : MonoBehaviour {
    [SerializeField]
    Transform[] vizBars;

    [SerializeField]
    Transform absVolBar;

    [SerializeField]
    [Range(1, 1000)]
    public float scale = 100;

    [SerializeField]
    float scaleRate = 1;

    AudioSource m_Source;

    void Start() {
        m_Source = gameObject.AddComponent<AudioSource>();

        var mic = Mic.Instance;
        mic.StartRecording(16000, 100);

        mic.OnSampleReady += (index, segment) => {
			var clip = AudioClip.Create("clip", 1600, mic.Clip.channels, mic.Clip.frequency, false);
			clip.SetData(segment, 0);
			m_Source.clip = clip;
			m_Source.loop = true;
			m_Source.mute = true;
			m_Source.Play();
        };
    }
    
    void Update() {
        var mic = Mic.Instance;

        if (!mic.IsRecording) return;
        
        // Update spectrum bars
        var spectrum = mic.GetSpectrumData(FFTWindow.Rectangular, 512);
		Debug.Log(spectrum.Length);
        // TODO: This is rubbish logic but it looks genuine so ok. In reality, spectrum chunks should be added to get an actual 8-ISO standard spectrum or something
        //  There is a good material on this available on youtube here : https://www.youtube.com/watch?v=4Av788P9stk
        //  Will update the code with that later.
        for (int i = 0; i < vizBars.Length; i++) {
            var desiredHeight = spectrum[i * (512 / vizBars.Length)] * scale;
            vizBars[i].localScale = new Vector3(
                vizBars[i].localScale.x,
                Mathf.Lerp(vizBars[i].localScale.y, desiredHeight, scaleRate),    // Make sure the spectrum doesn't go out
                vizBars[i].localScale.z
            );
        }

		absVolBar.localScale = new Vector3(
			absVolBar.localScale.x,
			mic.GetOutputData(512).Max(),
			absVolBar.localScale.z
		);
    }
}
