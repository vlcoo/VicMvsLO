using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Scripting;

namespace MidiPlayerTK
{
    /// <summary>
    /// This class is useful when a list of paired value string+id is needed.\n
    /// 
    /// This is also the entry point to display a popup for selecting a value by user: midi, preset, bank, drum, generator, ...
    /// </summary>
    public class MPTKListItem
    {
        /// <summary>@brief
        /// Index associated to the label (not to mix up with Position in list): 
        /// @li Patch number if patch list, 
        /// @li Bank number if bank list, 
        /// @li Midi Index for selecting a Midi from the MidiDB.
        /// @li Generator Id for selecting a generator to apply.
        /// </summary>
        public int Index;

        /// <summary>@brief
        /// Label associated to the index.
        /// @li Patch Label if patch list (Piano, Violin, ...), 
        /// @li Midi File Name for selecting a Midi from the MidiDB.
        /// @li Generator Name for selecting a generator to apply.
        /// </summary>
        public string Label;

        /// <summary>@brief
        /// Position in a list (not to mix up with Index which is a value associated to the Label)
        /// </summary>
        public int Position;
        
        [Preserve]
        public MPTKListItem() { }
    }
}
