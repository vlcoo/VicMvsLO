using System.Collections.Generic;
using UnityEngine.Events;

namespace MidiPlayerTK
{

    [System.Serializable]
    public class EventMidiClass : UnityEvent<MPTKEvent>
    {
    }

    [System.Serializable]
    public class EventNotesMidiClass : UnityEvent<List<MPTKEvent>>
    {
    }

    [System.Serializable]
    public class EventSynthClass : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class EventStartMidiClass : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class EventEndMidiClass : UnityEvent<string, EventEndMidiEnum>
    {
    }

    [System.Serializable]
    static public class ToolsUnityEvent
    {

        static public bool HasPersistantEvent(this EventMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasPersistantEvent(this UnityEvent evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }
        static public bool HasPersistantEvent(this EventNotesMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasPersistantEvent(this EventStartMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasPersistantEvent(this EventEndMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasPersistantEvent(this EventSynthClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

    }
}
