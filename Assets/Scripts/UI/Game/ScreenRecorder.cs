using Quantum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Debug = System.Diagnostics.Debug;

public class ScreenRecorder : MonoBehaviour {
    public RenderTexture outTexture;
    public Texture2D out2d;
    public Camera cam;
    public bool isRecording;
    private Process _ffmpeg;
    private BinaryWriter _ffmpegInput;
    private readonly List<byte[]> _frameBuffers = new();
    public int w, h;
    public bool frameLatch;
    
    public void Start()
    {
        QuantumEvent.Subscribe<EventGameEnded>(this, e => {
            StopRecording();
        });
        QuantumCallback.Subscribe<CallbackSimulateFinished>(this, e => {
            StepRecording();
        });
        Application.runInBackground = true;
        
        w = outTexture.width;
        h = outTexture.height;
    }
    
    public void OnDestroy() {
        if (isRecording) StopRecording();
    }

    public void TakeScreenshot() {
        out2d = new Texture2D(outTexture.width, outTexture.height);
        RenderTexture.active = outTexture;
        cam.Render();
        out2d.ReadPixels(new Rect(0, 0, out2d.width, out2d.height), 0, 0);
        out2d.Apply();
        var bytes = out2d.EncodeToPNG();
        File.WriteAllBytes("C:\\Users\\Victor\\Desktop\\mvlo stuff\\record\\out.png", bytes);
        RenderTexture.active = null;
    }

    private void StartRecording() {
        if (isRecording) return;
        
        out2d = new Texture2D(w, h, TextureFormat.RGB24, false);
        CreateFfmpeg();
        
        isRecording = true;
    }

    private void StepRecording() {
        if (!isRecording) StartRecording();
        frameLatch = !frameLatch;
        if (!frameLatch) return;
        RenderTexture.active = outTexture;
        cam.Render();
        AsyncGPUReadback.Request(outTexture, 0, GraphicsFormat.R8G8B8_UNorm, request => {
            var data = request.GetData<byte>();
            // var copy = new byte[data.Length];
            // data.CopyTo(copy);
            _ffmpegInput.Write(data.ToArray());
        });
        // RenderTexture.active = null;
        // out2d.Apply();
        // byte[] raw = new byte[w * h * 3];
        // Buffer.BlockCopy(out2d.GetRawTextureData(), 0, raw, 0, raw.Length);
        // out2d.ReadPixels(new Rect(0, 0, out2d.width, out2d.height), 0, 0);
        // out2d.Apply();
        // _ffmpegInput.Write(out2d.GetRawTextureData());

        // _frameBuffers.Add(raw);
    }

    private void StopRecording() {
        if (!isRecording) return;
        isRecording = false;
        
        // CreateFfmpeg();

        // foreach (var frameBuffer in _frameBuffers.Where(frameBuffer => frameBuffer.Length == w * h * 3))
        // {
        //     _ffmpegInput.Write(frameBuffer);
        // }
        
        _ffmpegInput.Flush();
        _ffmpegInput.Close();
        _ffmpeg.WaitForExit();
        _ffmpeg.Close();
        
        Application.Quit();
    }

    private void CreateFfmpeg() {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-y -f rawvideo -pix_fmt rgb24 -video_size {w}x{h} -framerate 30 -i - -vf vflip -c:v libx264 -pix_fmt yuv420p -preset ultrafast -crf 32 -tune zerolatency -threads auto \"C:\\Users\\Victor\\Desktop\\mvlo stuff\\record\\out.mp4\"",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        _ffmpeg = new Process();
        _ffmpeg.StartInfo = startInfo;
        _ffmpeg.Start();
        _ffmpeg.ErrorDataReceived += (s, e) => UnityEngine.Debug.LogError($"FFmpeg: {e.Data}");
        _ffmpeg.BeginErrorReadLine();

        _ffmpegInput = new BinaryWriter(_ffmpeg.StandardInput.BaseStream);
    }
}
