#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MidiPlayerTK
{

    /// <summary>@brief
    /// Window editor for the setup of MPTK
    /// </summary>
    public partial class MidiFileSetupWindow : EditorWindow
    {
        static List<string> infoStats = new List<string>();
        static private void CalculateStat()
        {
            infoStats = new List<string>();

            if (IndexEditItem >= 0 && IndexEditItem < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
            {
                string pathMidiFile = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[IndexEditItem];
                MidiLoad midifile = new MidiLoad();
                midifile.MPTK_KeepNoteOff = withNoteOff;
                midifile.MPTK_EnableChangeTempo = true;
                midifile.MPTK_Load(pathMidiFile);
                if (midifile != null)
                {
                    infoStats.Add($"MIDI File Format: {midifile.midifile.FileFormat}" + (midifile.midifile.FileFormat == 0 ? " (converted to format 1)" : ""));
                    infoStats.Add($"MPTK_LoadTime:       {midifile.MPTK_LoadTime} milliseconds");
                    infoStats.Add($"MPTK_TrackCount:     {midifile.MPTK_TrackCount}");
                    infoStats.Add($"MPTK_MidiEvents:     {midifile.MPTK_MidiEvents.Count}");
                    infoStats.Add($"MPTK_Duration:       {midifile.MPTK_Duration} ({midifile.MPTK_Duration.TotalSeconds} seconds)");
                    infoStats.Add($"MPTK_TickLast:       {midifile.MPTK_TickLast}");
                    infoStats.Add($"MPTK_InitialTempo:   {midifile.MPTK_InitialTempo,0:F2} BPM");
                    infoStats.Add($"MPTK_TimeSigNumerator:   {midifile.MPTK_TimeSigNumerator}");
                    infoStats.Add($"MPTK_TimeSigDenominator: {midifile.MPTK_TimeSigDenominator}");
                    infoStats.Add($"MPTK_NumberBeatsMeasure: {midifile.MPTK_NumberBeatsMeasure}");
                    infoStats.Add($"MPTK_NumberQuarterBeat:  {midifile.MPTK_NumberQuarterBeat} (2 pow timesig.Denominator)");
                    infoStats.Add($"MPTK_TicksInMetronomeClick:   {midifile.MPTK_TicksInMetronomeClick}");
                    infoStats.Add($"MPTK_No32ndNotesInQuarterNote:   {midifile.MPTK_No32ndNotesInQuarterNote}");
                    infoStats.Add($"MPTK_KeySigSharpsFlats:  {midifile.MPTK_KeySigSharpsFlats}  number of flats (if negative) or sharps (if positive).");
                    infoStats.Add($"MPTK_KeySigMajorMinor:   " + ((midifile.MPTK_KeySigMajorMinor == 0) ? "Major" : "Minor"));
                    infoStats.Add($"MPTK_DeltaTicksPerQuarterNote:   {midifile.MPTK_DeltaTicksPerQuarterNote}");
                    infoStats.Add($"MPTK_MicrosecondsPerQuarterNote: {midifile.MPTK_MicrosecondsPerQuarterNote} µseconds per quarter");
                    infoStats.Add("");

                    infoStats.Add($"Tempo Change");
                    foreach (MPTKTempo tempo in midifile.MPTK_TempoMap)
                        infoStats.Add("  " + tempo.ToString());
                    infoStats.Add("");

                    infoStats.Add($"Signature Change");
                    foreach (MPTKSignature sign in midifile.MPTK_SignMap)
                        infoStats.Add("  " + sign.ToString());
                    infoStats.Add("");


                    // Using dictionnary would be better than array but the purpose here is further
                    // to demonstrate how computing statistics from a MIDI file in editor mode.
                    int[,] stat_note = new int[16, 128];
                    int[] stat_channel = new int[16];

                    try
                    {
                        // Calculate notes count by channel
                        foreach (MPTKEvent mpekEvent in midifile.MPTK_MidiEvents)
                        {
                            // In editor mode, only the basic structure of MIDI is available (not MPTKEvent)
                            if (mpekEvent.Command == MPTKCommand.NoteOn)
                            {
                                try
                                {
                                    //NoteOnEvent noteon = (NoteOnEvent)trackEvent.Event;
                                    // IndexSection with NAudio start at 1 but start at 0 with MPTK
                                    stat_note[mpekEvent.Channel, mpekEvent.Value]++;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }

                    infoStats.Add("");
                    try
                    {
                        // Display notes count and calculate count by channel
                        for (int channel = 0; channel < 16; channel++)
                            for (int note = 0; note < 128; note++)
                                if (stat_note[channel, note] > 0)
                                {
                                    stat_channel[channel]++;
                                    infoStats.Add($"Channel:{channel} note:{note} count:{stat_note[channel, note]}");
                                }
                    }
                    catch (Exception ex) { Debug.LogWarning(ex); }

                    // Display count by channel
                    infoStats.Add("");
                    for (int channel = 0; channel < 16; channel++)
                        if (stat_channel[channel] > 0)
                            infoStats.Add($"Total notes for channel:{channel} count:{stat_channel[channel]}");
                }
            }
        }


        private void ShowMidiStat(float startX, float width, float height, float nextAreaY)
        {
            if (infoStats != null)
            {
                try
                {
                    // Begin area MIDI events list
                    // --------------------------
                    // Why +30 ? Any idea !
                    GUILayout.BeginArea(new Rect(startX, nextAreaY, width, height - nextAreaY + 30), MPTKGui.stylePanelGrayLight);
                    scrollPosStat = GUILayout.BeginScrollView(scrollPosStat);
                    foreach (string info in infoStats)
                        GUILayout.Label(info, MPTKGui.styleLabelFontCourier);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{ex}");
                }
                finally
                {
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                }
            }
        }
    }
}
#endif
