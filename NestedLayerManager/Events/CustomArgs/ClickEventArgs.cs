using System;
using NestedLayerManager.SubControls;
using NestedLayerManager.IO;

namespace NestedLayerManager.Events.CustomArgs
{
    public class ClickEventArgs : EventArgs
    {
        public ClickEventArgs(NlmTreeListView listView)
        {
            ListView = listView;
        }
        public NlmTreeListView ListView { get; set; }


        public ClickEventArgs(NlmSettings settings)
        {
            Settings = settings;
        }
        public NlmSettings Settings { get; set; }
    }
}
