#if UNITY_EDITOR
//#define MPTK_PRO
using System;
using UnityEditor;
using UnityEngine;
using static MidiPlayerTK.MidiFilePlayer;

namespace MidiPlayerTK
{
    /// <summary>@brief
    /// Inspector for the MIDI global player component
    /// </summary>
    public class MidiCommonEditor : ScriptableObject
    {
        private TextArea taSequence;
        private TextArea taProgram;
        private TextArea taInstrument;
        private TextArea taText;
        private TextArea taCopyright;
        private SerializedProperty CustomEventSynthAwake;
        private SerializedProperty CustomEventSynthStarted;

        //                                         Level=0            1           2           4             8             
        static private string[] popupQuantization = { "None", "Quarter Note", "Eighth Note", "16th Note", "32th Note", "64th Note", "128th Note" };
        string[] synthRateLabel = new string[] { "Default", "24000 Hz", "36000 Hz", "48000 Hz", "60000 Hz", "72000 Hz", "84000 Hz", "96000 Hz" };
        int[] synthRateIndex = { -1, 0, 1, 2, 3, 4, 5, 6 };

        string[] synthBufferSizeLabel = new string[] { "Default", "64", "128", "256", "512", "1024", "2048" };
        int[] synthBufferSizeIndex = { -1, 0, 1, 2, 3, 4, 5 };

        string[] synthInterpolationLabel = new string[] { "None - efficient but low quality", "Linear - good quality", "Cubic", "7th Order" };
        int[] synthInterpolationIndex = { 0, 1, 2, 3 };



        public void DrawAlertOnDefault()
        {
            if (MPTKGui.myStyle == null) MPTKGui.myStyle = new CustomStyle();
            EditorGUILayout.LabelField(
                "Changing properties here are without any guarantee!" +
                " To activate full stat, define these scripting symbols: " +
                "DEBUG_PERF_AUDIO, DEBUG_PERF_MIDI, DEBUG_STATUS_STAT"
                , MPTKGui.myStyle.LabelAlert);
        }

        static public bool DrawFoldoutAndHelp(bool state, string title, string urlHelp)
        {
            EditorGUILayout.BeginHorizontal();
            state = EditorGUILayout.Foldout(state, title);
            MidiCommonEditor.DrawHelp(urlHelp);
            EditorGUILayout.EndHorizontal();
            return state;
        }

        static public void DrawLabelAndHelp(string title, string urlHelp)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, MPTKGui.myStyle.LabelGreen);
            MidiCommonEditor.DrawHelp(urlHelp);
            EditorGUILayout.EndHorizontal();
        }

        static public void DrawHelp(string urlHelp)
        {
            if (GUILayout.Button(
                new GUIContent(MPTKGui.IconHelpBlack, "Get some help on MPTK web site"),
                EditorStyles.helpBox,
                GUILayout.Width(16f), GUILayout.Height(16f)))
                Application.OpenURL(urlHelp);
        }

        static public void DrawHelpAPI(string urlAPI)
        {
            if (GUILayout.Button(new GUIContent("API", "Get some help on MPTK web site"),
                EditorStyles.helpBox,
                GUILayout.Width(32f), GUILayout.Height(16f)))
                Application.OpenURL("https://mptkapi.paxstellar.com/" + urlAPI); // "http://autogam.free.fr/MPTK/html/"
        }

        public void DrawCaption(string title, string urlHelp, string urlAPI)
        {
            if (MPTKGui.myStyle == null) MPTKGui.myStyle = new CustomStyle();
            EditorGUILayout.BeginVertical(MPTKGui.myStyle.InfoInspectorBackground);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(title);

            DrawHelp(urlHelp);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                EditorGUILayout.LabelField(new GUIContent("SoundFont: " + MidiPlayerGlobal.ImSFCurrent.SoundFontName + " loaded", MidiPlayerGlobal.HelpDefSoundFont));
            }
            else if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
            {
                EditorGUILayout.LabelField(new GUIContent("SoundFont: " + MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name, MidiPlayerGlobal.HelpDefSoundFont));
            }
            else
            {
                ErrorNoSoundFont();
            }
            DrawHelpAPI(urlAPI);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            //EditorGUILayout.Separator();
        }

        public void AllPrefab(MidiSynth instance)
        {
            float volume = EditorGUILayout.Slider(new GUIContent("Volume", "Set global volume for this MIDI playing"), instance.MPTK_Volume, 0f, 1f);
            if (instance.MPTK_Volume != volume)
                instance.MPTK_Volume = volume;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Transpose");
            instance.MPTK_Transpose = EditorGUILayout.IntSlider(instance.MPTK_Transpose, -24, 24);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Channel Exception", "Apply transpose on all channels except this one. -1 to apply all. Default is 9 because generally it's the drum channel and we don't want to transpose drum instruments!"));
            instance.MPTK_TransExcludedChannel = EditorGUILayout.IntSlider(instance.MPTK_TransExcludedChannel, -1, 15);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            instance.MPTK_EnablePresetDrum = EditorGUILayout.Toggle(new GUIContent("Drum Preset Change", "Enable or disable preset/bank change on the channel 9 (drum). When enabled, could create unexpected music with MIDI files not compliant with the MIDI standard."), instance.MPTK_EnablePresetDrum);
            instance.MPTK_LogEvents = EditorGUILayout.Toggle(new GUIContent("Log MIDI Events Played", "Log information about each MIDI events played.\nIt's recommended to enable \"Monospace font\" in the setting of the console (three vertical dot in the panel)."), instance.MPTK_LogEvents);

            //Debug.Log(instance.GetType());
            string foldoutTitle = $"Show Spatialization Parameters - {instance.MPTK_Spatialize} {Math.Round(instance.distanceToListener, 2)}/{instance.MPTK_MaxDistance}";
            instance.showSpatialization = DrawFoldoutAndHelp(instance.showSpatialization, foldoutTitle, "https://paxstellar.fr/setup-mptk-sound-spatialization/");
            if (instance.showSpatialization)
            {
                EditorGUI.indentLevel++;
                GUIContent labelSpatialization = new GUIContent("Spatialization", "Enable spatialization effect");
#if MPTK_PRO
                if (!(instance is MidiSpatializer))
                {
#endif
                    bool spatialize = EditorGUILayout.Toggle(labelSpatialization, instance.MPTK_Spatialize);
                    if (instance.MPTK_Spatialize != spatialize)
                        instance.MPTK_Spatialize = spatialize;
#if MPTK_PRO
                }
                else
                {
                    // Need to be forced to true, here to check.
                    EditorGUILayout.LabelField(labelSpatialization, new GUIContent(instance.MPTK_Spatialize ? "True" : "False"));
                    bool mode = EditorGUILayout.Toggle(new GUIContent("Channel Spatialization", "Enable channel spatialization effect"), instance.MPTK_ModeSpatializer == MidiSynth.ModeSpatializer.Channel);
                    instance.MPTK_ModeSpatializer = mode == true ? MidiSynth.ModeSpatializer.Channel : MidiSynth.ModeSpatializer.Track;

                    mode = EditorGUILayout.Toggle(new GUIContent("Track Spatialization", "Enable track spatialization effect"), instance.MPTK_ModeSpatializer == MidiSynth.ModeSpatializer.Track);
                    instance.MPTK_ModeSpatializer = mode == true ? MidiSynth.ModeSpatializer.Track : MidiSynth.ModeSpatializer.Channel;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(new GUIContent("Max Spatial Synth", ""));

                    if (!Application.isPlaying)
                        instance.MPTK_MaxSpatialSynth = EditorGUILayout.IntSlider(instance.MPTK_MaxSpatialSynth, 16, 50);
                    else
                        EditorGUILayout.LabelField($"{instance.MPTK_MaxSpatialSynth} (Can't be modified when running)");

                    EditorGUILayout.EndHorizontal();
                }
#endif

                //EditorGUILayout.BeginHorizontal();
                string tooltipDistance = "Playing is paused if distance between AudioListener and this component is greater than MaxDistance";
                float distance = EditorGUILayout.IntField(new GUIContent("Max Distance", tooltipDistance), (int)instance.MPTK_MaxDistance);
                if (instance.MPTK_MaxDistance != distance)
                    instance.MPTK_MaxDistance = distance;
                //float distanceToListener = MidiPlayerGlobal.MPTK_DistanceToListener(instance.transform);
                if (instance.distanceToListener > instance.MPTK_MaxDistance)
                    EditorGUILayout.LabelField(new GUIContent($"Midi sequencer is paused, current distance to AudioListener: {Math.Round(instance.distanceToListener, 2)}",
                        tooltipDistance), MPTKGui.myStyle.LabelAlert);
                else
                    //Debug.Log("Camera: " + instance.distanceEditorModeOnly);
                    EditorGUILayout.LabelField(new GUIContent($"Current distance to AudioListener: {Math.Round(instance.distanceToListener, 2)}", tooltipDistance));
                //EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        public void MidiFileParameters(MidiFilePlayer instance)
        {
            string tooltip = "Start playing Midi when the application starts";
            instance.MPTK_PlayOnStart = EditorGUILayout.Toggle(new GUIContent("Automatic MIDI Start", tooltip), instance.MPTK_PlayOnStart);

            tooltip = "Start playing Midi from the first note found in the MIDI. OF course, previous MIDI event are also send to the MIDI synthesizer but immediately (preset change, meta event, ...\n";
            instance.MPTK_StartPlayAtFirstNote = EditorGUILayout.Toggle(new GUIContent("Start Playing at the First Note", tooltip), instance.MPTK_StartPlayAtFirstNote);

            tooltip = "Pause when application loss the focus";
            instance.MPTK_PauseOnFocusLoss = EditorGUILayout.Toggle(new GUIContent("Pause When Focus Loss", tooltip), instance.MPTK_PauseOnFocusLoss);

            instance.MPTK_DirectSendToPlayer = EditorGUILayout.Toggle(new GUIContent("Send MIDI to the Synth", "Midi events are send to the MIDI player directly"), instance.MPTK_DirectSendToPlayer);

            instance.MPTK_MidiAutoRestart = EditorGUILayout.Toggle(new GUIContent("Automatic MIDI Restart", "The current MIDI playing is restarted when it reaches the end"), instance.MPTK_MidiAutoRestart);

            EditorGUILayout.BeginHorizontal();
            tooltip = "If set, play stop at the last note found, the note is played but the MIDI sequencer don't wait the end of the note to stop.\n";
            tooltip += "If not set, play stop at the last note found but when all playing notes are released.";
            EditorGUILayout.PrefixLabel(new GUIContent("Mode Stop At Last Note", tooltip));
            if (EditorGUILayout.DropdownButton(new GUIContent(MidiFilePlayer.ModeStopPlayLabel[(int)instance.MPTK_ModeStopVoice], tooltip), FocusType.Passive)) //,new GUIStyle( "MiniPopup")))
            {
                var dropDownMenu = new GenericMenu();
                foreach (ModeStopPlay mode in Enum.GetValues(typeof(ModeStopPlay)))
                    dropDownMenu.AddItem(
                        new GUIContent(MidiFilePlayer.ModeStopPlayLabel[(int)mode], ""),
                        instance.MPTK_ModeStopVoice == mode, () => { instance.MPTK_ModeStopVoice = mode; EditorUtility.SetDirty(instance); });
                dropDownMenu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Separator();

                string infotime = "Real time from the start of the playing. Tempo or speed change have no impact.";
                TimeSpan times = TimeSpan.FromMilliseconds(instance.MPTK_RealTime);
                string playTime = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", times.Hours, times.Minutes, times.Seconds, times.Milliseconds);
                EditorGUILayout.LabelField(new GUIContent("Real Time", infotime), new GUIContent(playTime, infotime));

                infotime = "Time from start and total duration regarding the current tempo and the position in the MIDI file";
                EditorGUILayout.LabelField(new GUIContent("MIDI Time", infotime), new GUIContent(instance.playTimeEditorModeOnly + " / " + instance.durationEditorModeOnly, infotime));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Position", "Set real time position since the startup regarding the current tempo"));
                float currentPosition = (float)Math.Round(instance.MPTK_Position);
                float newPosition = (float)Math.Round(EditorGUILayout.Slider(currentPosition, 0f, instance.MPTK_DurationMS));
                if (currentPosition != newPosition)
                {
                    // Avoid event as layout triggered when duration is changed
                    if (Event.current.type == EventType.Used)
                    {
                        //Debug.Log("New position " + currentPosition + " --> " + newPosition + " " + Event.current.type);
                        instance.MPTK_Position = newPosition;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();
                string infotick = "Tick count for start and total duration regardless the current tempo";
                EditorGUILayout.LabelField(new GUIContent("Ticks", infotick), new GUIContent(instance.MPTK_TickCurrent + " / " + instance.MPTK_TickLast, infotime));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Position", "Set tick position since the startup regardless the current tempo"));
                long currenttick = instance.MPTK_TickCurrent;
                long ticks = Convert.ToInt64(EditorGUILayout.Slider(currenttick, 0f, (float)instance.MPTK_TickLast));
                if (currenttick != ticks)
                {
                    // Avoid event as layout triggered when duration is changed
                    if (Event.current.type == EventType.Used)
                    {
                        //Debug.Log("New tick " + currenttick + " --> " + ticks + " " + Event.current.type);
                        instance.MPTK_TickCurrent = ticks;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Time Signature", ""));
                if (instance.MPTK_MidiLoaded != null)
                {
                    GUILayout.Label($"{instance.MPTK_MidiLoaded.MPTK_NumberBeatsMeasure} / {instance.MPTK_MidiLoaded.MPTK_NumberQuarterBeat}\t");
#if MPTK_PRO

                    GUILayout.Label($"Measure: {instance.MPTK_MidiLoaded.MPTK_CurrentMeasure}.{instance.MPTK_MidiLoaded.MPTK_CurrentBeat}");
#else
                    GUILayout.Label($"Measure: pro version");
#endif
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();

                //if (GUILayout.Button(new GUIContent($"Start AudioSource {instance.CoreAudioSource.isPlaying}", "")))
                //    if (instance.CoreAudioSource.isPlaying)
                //        instance.CoreAudioSource.Stop();
                //    else
                //        instance.CoreAudioSource.Play();

                if (instance.MPTK_IsPlaying && !instance.MPTK_IsPaused)
                    GUI.color = MPTKGui.ButtonColor;
                if (GUILayout.Button(new GUIContent("Play", "")))
                    instance.MPTK_Play();
                GUI.color = Color.white;

                if (instance.MPTK_IsPaused)
                    GUI.color = MPTKGui.ButtonColor;
                if (GUILayout.Button(new GUIContent("Pause", "")))
                    // No need to explicitly pause when pause on focus lost
                    if (!instance.MPTK_PauseOnFocusLoss)
                        if (instance.MPTK_IsPaused)
                            instance.MPTK_UnPause();
                        else
                            instance.MPTK_Pause();
                    else
                        Debug.Log("Paused because focus lost, refocusing your app to unpause");
                GUI.color = Color.white;

                if (GUILayout.Button(new GUIContent("Stop", "")))
                    instance.MPTK_Stop();

                if (GUILayout.Button(new GUIContent("Restart", "")))
                    instance.MPTK_RePlay();
                EditorGUILayout.EndHorizontal();
#if MPTK_PRO
                if (!(instance is MidiExternalPlayer))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Previous", "")))
                        instance.MPTK_Previous();
                    if (GUILayout.Button(new GUIContent("Next", "")))
                        instance.MPTK_Next();
                    EditorGUILayout.EndHorizontal();
                }
#endif
            }

            instance.showMidiParameter = DrawFoldoutAndHelp(instance.showMidiParameter, "Show Midi Parameters", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Midi-Parameters");
            if (instance.showMidiParameter)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Quantization", ""), GUILayout.Width(150));
                int newLevel = EditorGUILayout.Popup(instance.MPTK_Quantization, popupQuantization);
                if (newLevel != instance.MPTK_Quantization && newLevel >= 0 && newLevel < popupQuantization.Length)
                    instance.MPTK_Quantization = newLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Speed");
                float speed = EditorGUILayout.Slider(instance.MPTK_Speed, 0.1f, 10f);
                //          Debug.Log("New speed " + instance.MPTK_Speed + " --> " + speed + " " + Event.current.type);
                if (speed != instance.MPTK_Speed)
                {
                    //Debug.Log("New speed " + instance.MPTK_Speed + " --> " + speed + " " + Event.current.type);
                    instance.MPTK_Speed = speed;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                instance.MPTK_EnableChangeTempo = EditorGUILayout.Toggle(new GUIContent("Enable Tempo Change", "Enable MIDI events tempo change from the MIDI file when playing. If disabled, only the first tempo change found in the MIDI will be applied (or 120 if not tempo change). Disable it when you want to change tempo by your script when the MIDI is playing."), instance.MPTK_EnableChangeTempo);
                if (EditorApplication.isPlaying && instance.MPTK_IsPlaying)
                {
                    float tempo = EditorGUILayout.IntSlider((int)instance.MPTK_Tempo, 1, 1000);
                    if (tempo != (int)instance.MPTK_Tempo)
                    {
                        //Debug.Log("New tempo " + instance.CurrentTempo + " --> " + tempo + " " + Event.current.type);
                        instance.MPTK_Tempo = tempo;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Moved to common parameters (V2.10.1)
                //instance.MPTK_EnablePresetDrum = EditorGUILayout.Toggle(new GUIContent("Drum Preset Change", "Enable or disable preset change on the channel 9 (drum). When enabled, could create unexpected music with MIDI files not compliant with the MIDI standard."), instance.MPTK_EnablePresetDrum);

                // Moved to synth parameters (V2.85)
                //instance.MPTK_ReleaseSameNote = EditorGUILayout.Toggle(new GUIContent("Release Same Note", "Enable release note if the same note is hit twice on the same channel."), instance.MPTK_ReleaseSameNote);
                //instance.MPTK_KillByExclusiveClass = EditorGUILayout.Toggle(new GUIContent("Kill By Exclusive Class", "Find the exclusive class of the voice. If set, kill all voices that match the exclusive class and are younger than the first voice process created by this noteon event."), instance.MPTK_KillByExclusiveClass);
                instance.MPTK_KeepNoteOff = EditorGUILayout.Toggle(new GUIContent("Keep MIDI NoteOff", "Keep MIDI NoteOff and NoteOn with Velocity=0 (need to restart the playing Midi)"), instance.MPTK_KeepNoteOff);
                instance.MPTK_KeepEndTrack = EditorGUILayout.Toggle(new GUIContent("Keep MIDI EndTrack", "When set to true, meta MIDI event End Track are keep and these MIDI events are taken into account for calculate the full duration of the MIDI."), instance.MPTK_KeepEndTrack);

                EditorGUI.indentLevel--;
            }
        }

        public void MidiFileInfo(MidiFilePlayer instance)
        {
            instance.showMidiInfo = DrawFoldoutAndHelp(instance.showMidiInfo, "Show MIDI Info", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Midi-Info");
            if (instance.showMidiInfo)
            {
                EditorGUI.indentLevel++;

                if (!string.IsNullOrEmpty(instance.MPTK_SequenceTrackName))
                {
                    if (taSequence == null) taSequence = new TextArea("Sequence");
                    taSequence.Display(instance.MPTK_SequenceTrackName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_ProgramName))
                {
                    if (taProgram == null) taProgram = new TextArea("Program");
                    taProgram.Display(instance.MPTK_ProgramName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_TrackInstrumentName))
                {
                    if (taInstrument == null) taInstrument = new TextArea("Instrument");
                    taInstrument.Display(instance.MPTK_TrackInstrumentName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_TextEvent))
                {
                    if (taText == null) taText = new TextArea("TextEvent");
                    taText.Display(instance.MPTK_TextEvent);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_Copyright))
                {
                    if (taCopyright == null) taCopyright = new TextArea("Copyright");
                    taCopyright.Display(instance.MPTK_Copyright);
                }
                EditorGUI.indentLevel--;
            }
        }

        public void SynthParameters(MidiSynth instance, SerializedObject sobject)
        {
            instance.showSynthParameter = DrawFoldoutAndHelp(instance.showSynthParameter, "Show Synth Parameters", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Synth-Parameters");
            if (instance.showSynthParameter)
            {
                EditorGUI.indentLevel++;

                GUIContent labelCore = new GUIContent("Core Player", "Play music with a non Unity thread. Change this properties only when not running");
                string titleCore = (instance.MPTK_CorePlayer ? "Core" : "Non Core");
                titleCore += " - ";
                titleCore += (instance.MPTK_AudioSettingFromUnity ? "Unity Audio Setting" : "MPTK Audio Setting");
                if (EditorApplication.isPlaying)
                    titleCore += " - Rate " + instance.OutputRate + " Hz - Buffer " + (instance.DspBufferSize > 0 ? instance.DspBufferSize.ToString() : "");
                string foldoutTitle = $"Show Unity Audio Parameters - {titleCore}";
                instance.showUnitySynthParameter = DrawFoldoutAndHelp(instance.showUnitySynthParameter, foldoutTitle, "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Audio-Parameters");
                if (instance.showUnitySynthParameter)
                {
                    EditorGUI.indentLevel++;
                    if (MPTKGui.myStyle == null) MPTKGui.myStyle = new CustomStyle();
                    EditorGUILayout.LabelField(
                        "With Core Player checked (fluidsynth mode), the synthesizer is working on a thread apart from the main Unity thread. " +
                        "Accuracy is much better. The legacy mode which is using many AudioSource will be removed with the next major version.", MPTKGui.myStyle.LabelGreen);
                    if (!EditorApplication.isPlaying)
                        instance.MPTK_CorePlayer = EditorGUILayout.Toggle(labelCore, instance.MPTK_CorePlayer);
                    else
                        EditorGUILayout.LabelField(labelCore, new GUIContent(instance.MPTK_CorePlayer ? "True" : "False"));

                    if (NoErrorValidator.CantChangeAudioConfiguration)
                    {
                        EditorGUILayout.LabelField("Warning: Audio configuration change is disabled on this platform.", MPTKGui.myStyle.LabelAlert);
                    }
                    else if (instance.MPTK_CorePlayer)
                    {
                        EditorGUILayout.Space(20);

                        DrawLabelAndHelp(
                             "If checked then synth rate and buffer size will be automatically defined by Unity in accordance of the capacity of the hardware. " +
                             "Look at Unity menu 'Edit / Project Settings...' and select between best latency and best performance. ",
                             "https://paxstellar.fr/2021/01/01/get-an-accurate-generated-music/"
                             );
                        DrawLabelAndHelp(
                             "If not checked, then rate and buffer size can be defined manually ... but with the risk of bad audio quality.",
                             "https://paxstellar.fr/2020/09/06/performance//"
                             );
                        EditorGUILayout.LabelField("Setting 'best latency' in Unity Audio could produce weird sounds.", MPTKGui.myStyle.LabelAlert);

                        // Setting from Unity or from Maestro
                        instance.MPTK_AudioSettingFromUnity = EditorGUILayout.Toggle(new GUIContent("Unity Audio Setting",
                            "If true then synth rate and buffer size will be automatically defined by Unity in accordance of the capacity of the hardware."), instance.MPTK_AudioSettingFromUnity);


                        {
                            EditorGUI.indentLevel++;
                            GUI.enabled = !instance.MPTK_AudioSettingFromUnity;
                            EditorGUILayout.LabelField("Changing synthesizer rate and buffer size can produce unexpected effect according to the hardware. Save your work before!", MPTKGui.myStyle.LabelGreen);
                            if (EditorApplication.isPlaying)
                                EditorGUILayout.LabelField("Changing these setting when playing is not recommmended. it's only for test purpose because weird sounds can occurs", MPTKGui.myStyle.LabelAlert);
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Increase the rate to get a better sound but with a cost on performance.", MPTKGui.myStyle.LabelGreen);

                            instance.MPTK_EnableFreeSynthRate = EditorGUILayout.Toggle(new GUIContent("Synth Rate Free", "Allow free setting of the Synth Rate."), instance.MPTK_EnableFreeSynthRate);
                            if (!instance.MPTK_EnableFreeSynthRate)
                            {
#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
                                synthRateLabel[0] = "Default: 36000 Hz";
#else
                                synthRateLabel[0] = "Default: " + AudioSettings.outputSampleRate + " Hz";
#endif
                                int indexrate = EditorGUILayout.IntPopup("Synth Rate", instance.MPTK_IndexSynthRate, synthRateLabel, synthRateIndex);
                                if (indexrate != instance.MPTK_IndexSynthRate)
                                    instance.MPTK_IndexSynthRate = indexrate;
                            }
                            else
                            {
                                int rate = (int)EditorGUILayout.Slider(new GUIContent("Synth Rate", ""), (float)instance.MPTK_SynthRate, 12000, 96000);
                                if (rate != instance.MPTK_SynthRate)
                                    instance.MPTK_SynthRate = rate;
                            }
                            EditorGUILayout.Space();

                            EditorGUILayout.LabelField("Decrease the buffer size to get a more accurate playing.", MPTKGui.myStyle.LabelGreen);
                            int bufferLenght;
                            int numBuffers;
#if MPTK_PRO && UNITY_ANDROID && UNITY_OBOE
                            bufferLenght = 128;
#else
                            AudioSettings.GetDSPBufferSize(out bufferLenght, out numBuffers);
#endif
                            synthBufferSizeLabel[0] = "Default: " + bufferLenght;
                            int indexBuffSize = EditorGUILayout.IntPopup("Buffer Synth Size", instance.MPTK_IndexSynthBuffSize, synthBufferSizeLabel, synthBufferSizeIndex);
                            if (indexBuffSize != instance.MPTK_IndexSynthBuffSize)
                                instance.MPTK_IndexSynthBuffSize = indexBuffSize;
                            EditorGUILayout.Space();
                            GUI.enabled = true;
                            EditorGUI.indentLevel--;

                        }
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Interpolation is the core of the synth process. Linear is a good balacing between quality and performance", MPTKGui.myStyle.LabelGreen);
                        instance.InterpolationMethod = (fluid_interp)EditorGUILayout.IntPopup("Interpolation Method", (int)instance.InterpolationMethod, synthInterpolationLabel, synthInterpolationIndex);
                        EditorGUILayout.Space();
                    }
                    else
                        EditorGUILayout.LabelField(
                            "Warning: with non core mode, all voices will be played in separate Audio Source. " +
                            "SoundFont synth is not fully implemented. This mode will be removed with a future version.", MPTKGui.myStyle.LabelAlert);

                    EditorGUI.indentLevel--;
                }

                instance.MPTK_LogEvents = EditorGUILayout.Toggle(new GUIContent("Log MIDI Events Played", "Log information about each MIDI events played.\nIt's recommended to enable \"Monospace font\" in the setting of the console (three vertical dot in the panel)."), instance.MPTK_LogEvents);
                instance.MPTK_LogWave = EditorGUILayout.Toggle(new GUIContent("Log Samples Used", "Log information about sample played by a NoteOn event."), instance.MPTK_LogWave);

                //instance.MPTK_PlayOnlyFirstWave = EditorGUILayout.Toggle(new GUIContent("Play Only First Wave", "Some Instrument in Preset are using more of one wave at the same time. If checked, play only the first wave, useful on weak device, but sound experience is less good."), instance.MPTK_PlayOnlyFirstWave);
                //instance.MPTK_WeakDevice = EditorGUILayout.Toggle(new GUIContent("Weak Device", "Playing Midi files with WeakDevice activated could cause some bad interpretation of Midi Event, consequently bad sound."), instance.MPTK_WeakDevice);
                instance.MPTK_EnablePanChange = EditorGUILayout.Toggle(new GUIContent("Pan Change", "Enable MIDI event pan change when playing. Uncheck if you want to manage Pan in your application."), instance.MPTK_EnablePanChange);

                instance.MPTK_ApplyRealTimeModulator = EditorGUILayout.Toggle(new GUIContent("Apply Modulator", "Real-Time change Modulator from Midi and ADSR enveloppe Modulator parameters from SoundFont could have an impact on CPU. Initial value of Modulator set at Note On are keep. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyRealTimeModulator);
                instance.MPTK_ApplyModLfo = EditorGUILayout.Toggle(new GUIContent("Apply Mod LFO", "LFO modulation are defined in SoudFont. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyModLfo);
                instance.MPTK_ApplyVibLfo = EditorGUILayout.Toggle(new GUIContent("Apply Vib LFO", "LFO vibrato are defined in SoudFont. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyVibLfo);

                // Moved from Midi Parameters (V2.85)
                instance.MPTK_ReleaseSameNote = EditorGUILayout.Toggle(new GUIContent("Release Same Note", "Enable release note if the same note is hit twice on the same channel."), instance.MPTK_ReleaseSameNote);
                instance.MPTK_ReleaseTimeMod = EditorGUILayout.Slider(new GUIContent("Release Time Modifier", "Multiplier to increase or decrease the default release time defined in the SoundFont for each instrument.Warning: high value could lowering the performance."), instance.MPTK_ReleaseTimeMod, 0.1f, 10f);
                instance.MPTK_KillByExclusiveClass = EditorGUILayout.Toggle(new GUIContent("Kill By Exclusive Class", "Find the exclusive class of the voice. If set, kill all voices that match the exclusive class and are younger than the first voice process created by this noteon event."), instance.MPTK_KillByExclusiveClass);
                instance.MPTK_LeanSynthStarting = EditorGUILayout.Slider(new GUIContent("Lean Synth Starting", "Sets the speed of the increase of the volume of the audio source when synth is starting. Set to 1 for an immediate full volume at start."), instance.MPTK_LeanSynthStarting, 0.001f, 1f);
                instance.MPTK_KeepPlayingNonLooped = EditorGUILayout.Toggle(new GUIContent("Keep Playing Non Looped", "When the option is on, non looped samples (drum samples for the most part) are play through to the end."), instance.MPTK_KeepPlayingNonLooped);

#if MPTK_PRO
                //Debug.LogFormat("MPTK_EffectUnity {0}", (instance.MPTK_EffectUnity == null) ? "null" : "ok");
                if (instance.MPTK_EffectUnity == null)
                {
                    //Debug.Log("MPTK_EffectUnity is null, create it");
                    instance.MPTK_EffectUnity = ScriptableObject.CreateInstance<MPTKEffectUnity>();
                    instance.MPTK_EffectUnity.DefaultAll();
                    instance.MPTK_EffectUnity.Init(instance);
                }

                if (instance.MPTK_EffectSoundFont == null)
                {
                    //Debug.Log("MPTK_EffectSoundFont is null, create it");
                    instance.MPTK_EffectSoundFont = ScriptableObject.CreateInstance<MPTKEffectSoundFont>();
                    instance.MPTK_EffectSoundFont.DefaultAll();
                    instance.MPTK_EffectSoundFont.Init(instance);
                }
#endif

                instance.showSoundFontEffect = MidiCommonEditor.DrawFoldoutAndHelp(instance.showSoundFontEffect, "Show SoundFont Effect Parameters", "https://paxstellar.fr/sound-effects/");
                if (instance.showSoundFontEffect)
#if MPTK_PRO
                    CommonProEditor.EffectSoundFontParameters(instance, MPTKGui.myStyle);
#else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("[Available with MPTK Pro] These effects will be applied independently on each voices. Effects values are defined in the SoundFont, so limited modification can't be applied.", MPTKGui.myStyle.LabelGreen);
                    EditorGUI.indentLevel--;
                }
#endif

                instance.showUnitySynthEffect = MidiCommonEditor.DrawFoldoutAndHelp(instance.showUnitySynthEffect, "Show Unity Effect Parameters", "https://paxstellar.fr/sound-effects/");
                if (instance.showUnitySynthEffect)
#if MPTK_PRO
                    CommonProEditor.EffectUnityParameters(instance, MPTKGui.myStyle);
#else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("[Available with MPTK Pro] These effects will be applied to all voices processed by the current MPTK gameObject. You can add multiple MPTK gameObjects to apply for different effects.", MPTKGui.myStyle.LabelGreen);
                    EditorGUI.indentLevel--;
                }

#endif
                instance.showUnityPerformanceParameter = DrawFoldoutAndHelp(instance.showUnityPerformanceParameter, "Show Performance Parameters", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Performance");
                if (instance.showUnityPerformanceParameter)
                {
                    EditorGUI.indentLevel++;
                    instance.waitThreadMidi = EditorGUILayout.IntSlider(new GUIContent("Thread Midi Delay", "Delay in milliseconds between call to the MIDI sequencer, 0 for now waiting."), instance.waitThreadMidi, 0, 30);
                    instance.MaxDspLoad = EditorGUILayout.IntSlider(new GUIContent("Max Level DSP Load", "When DSP is over the 'Max Level DSP Load' (by default 50%), some actions are taken on current playing voices for better performance"), (int)instance.MaxDspLoad, 0, 100);
                    instance.DevicePerformance = EditorGUILayout.IntSlider(new GUIContent("Device Performance", "Define amount of cleaning of the voice. 1 for weak device and high cleaning. If <=25 some voice could be stopped."), instance.DevicePerformance, 1, 100);
                    instance.MPTK_CutOffVolume = EditorGUILayout.Slider(new GUIContent("Cut Off Volume", "When amplitude of a sample is below this value the playing of sample is stopped.\nCan be increase for better performance (when a lot of samples are played concurrently) but with degraded quality because sample could be stopped too early."), instance.MPTK_CutOffVolume, 0.0001f, 0.5f);
                    EditorGUI.indentLevel--;
                }

                instance.showSynthEvents = MidiCommonEditor.DrawFoldoutAndHelp(instance.showSynthEvents, "Show Synth Unity Events", "https://paxstellar.fr/midi-file-player-detailed-view-2/#Foldout-Synth-Unity-Events");
                if (instance.showSynthEvents)
                {
                    EditorGUI.indentLevel++;
                    if (CustomEventSynthAwake == null)
                        CustomEventSynthAwake = sobject.FindProperty("OnEventSynthAwake");
                    EditorGUILayout.PropertyField(CustomEventSynthAwake);

                    if (CustomEventSynthStarted == null)
                        CustomEventSynthStarted = sobject.FindProperty("OnEventSynthStarted");
                    EditorGUILayout.PropertyField(CustomEventSynthStarted);

                    sobject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        public static void ErrorNoSoundFont()
        {
            GUIStyle labError = new GUIStyle("Label");
            //labError.normal.background = SetColor(new Texture2D(2, 2), new Color(0.9f, 0.9f, 0.9f));
            //labError.normal.textColor = new Color(0.8f, 0.1f, 0.1f);
            labError.alignment = TextAnchor.MiddleLeft;
            labError.wordWrap = true;
            labError.fontSize = 10;
            Texture buttonIconFolder = Resources.Load<Texture2D>("Textures/question-mark");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(MidiPlayerGlobal.ErrorNoSoundFont, labError, GUILayout.Height(30f));
            if (GUILayout.Button(new GUIContent(buttonIconFolder, "Help"), GUILayout.Width(20f), GUILayout.Height(20f)))
                Application.OpenURL("https://paxstellar.fr/setup-mptk-quick-start-v2/");
            EditorGUILayout.EndHorizontal();
            //MidiPlayerGlobal.InitPath();
            //ToolsEditor.LoadMidiSet();
            //ToolsEditor.CheckMidiSet();
            //Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
        }

        public static void ErrorNoMidiFile()
        {
            GUIStyle labError = new GUIStyle("Label");
            labError.normal.background = SetColor(new Texture2D(2, 2), new Color(0.9f, 0.9f, 0.9f));
            labError.normal.textColor = new Color(0.8f, 0.1f, 0.1f);
            labError.alignment = TextAnchor.MiddleLeft;
            labError.wordWrap = true;
            labError.fontSize = 12;
            Texture buttonIconFolder = Resources.Load<Texture2D>("Textures/question-mark");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(MidiPlayerGlobal.ErrorNoMidiFile, labError, GUILayout.Height(40f));
            if (GUILayout.Button(new GUIContent(buttonIconFolder, "Help"), GUILayout.Width(40f), GUILayout.Height(40f)))
                Application.OpenURL("https://paxstellar.fr/setup-mptk-quick-start-v2/");
            EditorGUILayout.EndHorizontal();
            MidiPlayerGlobal.InitPath();
            ToolsEditor.LoadMidiSet();
            ToolsEditor.CheckMidiSet();
            Debug.Log(MidiPlayerGlobal.ErrorNoMidiFile);
        }

        public static Texture2D SetColor(Texture2D tex2, Color32 color)
        {
            var fillColorArray = tex2.GetPixels32();
            for (var i = 0; i < fillColorArray.Length; ++i)
                fillColorArray[i] = color;
            tex2.SetPixels32(fillColorArray);
            tex2.Apply();
            return tex2;
        }
        public static void SetSceneChangedIfNeed(UnityEngine.Object instance, bool changed)
        {
            if (changed)
            {
                EditorUtility.SetDirty(instance);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}
#endif
