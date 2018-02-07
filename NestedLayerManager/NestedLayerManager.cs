/*
 *  Nested Layer Manager 
 *  Copyright (c) 2014 Tim Hawker
 *  
 *  Updated by Simon Clarke 2016
 *  
 * v3.0.0 beta 6.0
 * 2016/07/27   SimonClarke -   Moved ReadNLM2Data to ReadWriteNLM2Data. Can now read and write NLM2 data.
 *                          -   Added NestedLayerManager.IO.Data.SettingsData to record user settings.
 *                          -   Added NestedLayerManager.IO.NlmSettings to load and save the above.
 * 2016/06/28   SimonClarke -   Fixed 'Unable to load type' deserialisation error by adding IO.Data.DeserialisationBinder class.
 *                          -   Updated MaxIO to use above.
 *                          -   Added AssemblyResolve ResolveEventHandler() to NestedLayerManager to fix "can't find assembly errors".
 * 2016/06/24   SimonClarke -   Added ReadNLM2Data class
 *                          -   Added MaxLogger class
 * 
 * v3.0.0 Beta 5.0
 * 2016/04/23   Tim Hawker  -   Committed GitHub
 */

using System;
using System.Reflection;
using System.Windows.Forms;
using NestedLayerManager.SubControls;
using NestedLayerManager.IO;
using NestedLayerManager.MaxInteractivity;

// The following variables are defined for easy searching:
// OLVBUGFIX    -   Any bug fixes to the OLV.
// DEBUG        -   Operations for debug mode only.
// Max2013      -   Max 2013 specific code.
// Max2014      -   Max 2014 specific code.
// Max2015      -   Max 2015 specific code.
// TODO:        -   Things to do / look into.

namespace NestedLayerManager
{

    public class NestedLayerManager : TableLayoutPanel
    {
        NlmSearchBar SearchBar;
        NlmTreeListView ListView;

        NlmButtonPanelLeft ButtonPanelLeft;
        NlmButtonPanelRight ButtonPanelRight;
        NlmButtonPanelSide ButtonPanelSide;

        public NlmSettings Settings;


#region AssemblyResolver
        static NestedLayerManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(assemblyResolver);
        }



        static Assembly assemblyResolver(object sender, ResolveEventArgs args)
        /*  This event is primarily here for DEBUG and development purposes. It allows you to load the assembly into 3dsMax memory using: 
        (dotNetClass "System.Reflection.Assembly").Load((dotNetClass "System.IO.File").ReadAllBytes([path to assembly.dll])) 
        To do this ObjectListView.dll has 'Properties'=>'Build Action' set to 'Embedded Resource', which means Max can't find it (because 
        NestedLayerManager is in memory) so this event will fire and the switch statement will return ObjectListView.
            This approach also seems to mean that this assembly can't find itself in NestedLayerManager.MaxInteractivity.MaxIO.GetAppData(),
        which also fires the event, so we return this assembly in the switch as well.
            EDIT:
            Seems we have to use this even when not in DEBUG / development since Max can't find this assembly when NLM3 is reopend. Is it
        possible I've missed something elsewhere that would make this unnecessary?
        */
        {
            string assembly_name = new AssemblyName(args.Name).Name;
            char[] sep = new char[] { '.' };
            string[] an = assembly_name.Split(sep);
            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            switch (an[0])
            {
                case "ObjectListView":
                    MaxListener.PrintToListener("case ObjectListView");
                    string resourceName = "NestedLayerManager.ObjectListView.dll";
                    System.IO.Stream resourceStream = thisAssembly.GetManifestResourceStream(resourceName);

                    byte[] buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, buffer.Length);
                    Assembly referencedAssembly = Assembly.Load(buffer);
                    resourceStream.Dispose();
                    return referencedAssembly;

                case "NestedLayerManager":
                    MaxListener.PrintToListener("case NestedLayerManager: " + args);
                    return thisAssembly;

                default:
                    return null;
            }
        }  
#endregion



        public NestedLayerManager()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Settings = new NlmSettings();

            ListView = new NlmTreeListView( Settings );
            SearchBar = new NlmSearchBar(ListView);

            ButtonPanelLeft = new NlmButtonPanelLeft(ListView);
            ButtonPanelRight = new NlmButtonPanelRight(ListView);
            ButtonPanelSide = new NlmButtonPanelSide(ListView);

            MaxLook.ApplyLook(this);

            ColumnCount = 3;
            RowCount = 3;
            Padding = new Padding(3);
            Dock = DockStyle.Fill;

            Controls.Add(ButtonPanelLeft, 1, 0);
            SetColumnSpan(ButtonPanelLeft, 1);

            Controls.Add(ButtonPanelRight, 2, 0);

            Controls.Add(SearchBar, 1, 1);
            SetColumnSpan(SearchBar, 2);

            Controls.Add(ButtonPanelSide, 0, 2);

            Controls.Add(ListView, 1, 2);
            SetColumnSpan(ListView, 2);

            RowStyles.Add(new RowStyle(SizeType.Absolute, ButtonPanelLeft.Controls[0].Height + 2));
            RowStyles.Add(new RowStyle(SizeType.Absolute, SearchBar.Height + 2));
            RowStyles.Add(new RowStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ButtonPanelSide.Controls[0].Width + 2));
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ButtonPanelLeft.Controls.Count * ButtonPanelLeft.Controls[0].Width));
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ButtonPanelRight.Controls.Count * ButtonPanelRight.Controls[0].Width));

            ListView.NodeControl.Create.BuildSceneTree();

            stopwatch.Stop();
            string listenerMessage = "Loaded in " + stopwatch.ElapsedMilliseconds + " milliseconds.";
            MaxListener.PrintToListener(listenerMessage);
        }



        protected override void Dispose(bool disposing)
        {
            MaxListener.PrintToListener("==== Disposing NestedLayerManager ====");
            Settings.saveUserSettings();
            MaxUI.RefreshButtonStates();
            base.Dispose(disposing);
        }
    }
}
