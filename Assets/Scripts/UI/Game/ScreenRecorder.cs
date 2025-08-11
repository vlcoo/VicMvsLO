using NSMB;
using NSMB.Replay;
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
    public int w, h;
    public bool frameLatch;
    public string outPath = "C:\\Users\\Victor\\Projects\\mvlo stuff\\record\\out.mp4";
    public int maxSize = 100;
    
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
        if (!string.IsNullOrEmpty(GlobalController.Instance.cmdTargetSize))
            maxSize = int.Parse(GlobalController.Instance.cmdTargetSize);
        if (!string.IsNullOrEmpty(GlobalController.Instance.cmdReplayName))
            outPath = $"{Path.Combine(Path.GetTempPath(), GlobalController.Instance.cmdReplayName)}.mp4";
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
            _ffmpegInput.Write(data.ToArray());
        });
    }

    private void StopRecording() {
        if (!isRecording) return;
        isRecording = false;
        
        _ffmpegInput.Flush();
        _ffmpegInput.Close();
        _ffmpeg.WaitForExit();
        _ffmpeg.Close();
        
        Application.Quit();
    }

    private void CreateFfmpeg() {
        double maxBits = maxSize * 1024.0 * 1024.0 * 8.0 * 0.95;
        float replayDuration = ActiveReplayManager.Instance.CurrentReplay.Header.ReplayLengthInFrames / 60.0f;
        double targetBitrate = (int) (maxBits / replayDuration / 1000.0);
        var args = $"-y -f rawvideo -pix_fmt rgb24 -video_size {w}x{h} -framerate 30 -i - " +
                   $"-vf vflip -c:v libx264 -pix_fmt yuv420p -threads auto -preset ultrafast " +
                   $"-b:v {targetBitrate}k " +
                   $"\"{outPath}\"";
        UnityEngine.Debug.Log(args);
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = args,
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
