using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using MaxCustomControls;
// NLM:
using NestedLayerManager.IO;
using NestedLayerManager.IO.Data;
using NestedLayerManager.MaxInteractivity.MaxCUI;
using NestedLayerManager.MaxInteractivity;
using NestedLayerManager.Events;
using NestedLayerManager.Events.CustomArgs;



namespace NestedLayerManager.SubControls
{
    public class NlmSettingsWindow : MaxForm
    {
        private Size WINDOW_SIZE= new Size( 310, 337 );
        //  Text to put on the check box and the index of the group it belongs to.
        private List<UIElem> checkBox_names = new List<UIElem>(){ 
                                                                new UIElem("readNLM2","Read NLM2 Data", 0),
                                                                new UIElem("writeNLM2","Write NLM2 Data", 0),
                                                                new UIElem("mergeOnFile","Merge on File > Merge", 1),
                                                                new UIElem("mergeOnXref","Merge on Xref Scene Import",1),
                                                                new UIElem("selectObjectsUI","Select objects in UI after selecting in scene",2),
                                                                new UIElem("selectObjectsScene","Select scene objects after selecting in UI",2),
                                                                new UIElem("selectLayersUI","Select layers in UI after objects in scene",2)
                                                                };

        private List<string> group_names = new List<string>(){  "NLM II Data",
                                                                "IO Options",
                                                                "Selection Options",
                                                                "Remove NLM Data"
                                                                };
        //  Button text, group index and position.
        private List<UIElem> button_names = new List<UIElem>(){ new UIElem("Delete NLM 3",3, new Point( 8, 17 )),
                                                                new UIElem("Delete NLM 2",3, new Point( 157, 17))
                                                                };

        private List<CheckBox> checkBoxs = new List<CheckBox>();
        private List<Button> buttons = new List<Button>();
        private List<GroupBox> groups = new List<GroupBox>();

        private Font item_font  = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        private Font group_font = new Font("Tahoma", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        //  Get to the NLM3.UserSettings.config through this:
        private NlmSettings Settings;


        

        public NlmSettingsWindow( NestedLayerManager parent )
        {
            Settings = parent.Settings;
            //  Think this gets the BackColor from the styling Max applies?
            this.BackColor = MaxForm.ActiveForm.BackColor;
            //  Create our groups and store them the groups list.
            foreach (string n in group_names){ 
                GroupBox GB = addGroupBox(n);
                GB.SuspendLayout();
                groups.Add(GB);
            }
            this.SuspendLayout();
            //  Initialise GroupBox layout settings.
            int grp_idx = -1; int chckBoxPosY=0; int grpHeight=0;
            //  Add CheckBoxs to groups using settings in List<checkBox_names>.
            foreach (UIElem elem in checkBox_names)
            {   
                //  Use the Lis<checkBox_names> to create our NlmCheckBoxes, add their
                //text and grab the appropriate NlmGroupBox.
                CheckBox CB = addCheckBox(elem);
                GroupBox GB = groups[elem.group];
                checkBoxs.Add( CB );
                //  Keep a tally of which group we're in to set the
                //NlmCheckBox position and NlmGroupBox height.
                if (grp_idx == elem.group)
                {
                    chckBoxPosY += 21;
                }
                else 
                {
                    grpHeight = 20;
                    chckBoxPosY = 17;
                    grp_idx = elem.group;
                }
                //  Now we know if we're in 'this' group or the 'next' one we
                //can set the NlmCheckBox postion, NlmGroupBox height and add
                //the NlmCheckBox to its' group.
                CB.Location = new Point(11, chckBoxPosY);
                grpHeight += 22;
                GB.Controls.Add(CB);
                GB.Size = new Size(302, grpHeight);
            }
            //  Set group positions by totalling GroupBox heights.
            int groupPosY = 1;
            for (int group_idx = 1; group_idx < groups.Count(); group_idx++)
            {
                groupPosY += groups[group_idx - 1].Size.Height + 2;
                groups[group_idx].Location = new Point(4, groupPosY);
            }
            
            foreach (UIElem elem in button_names)
            {
                Button BT = addButton(elem.text);
                GroupBox GB = groups[elem.group];
                BT.Location = elem.location;
                GB.Controls.Add(BT);
                buttons.Add( BT );
                GB.Location = new Point( 4,282 );
            }
            
            foreach (GroupBox g in groups)
            {
                this.Controls.Add(g);
                g.ResumeLayout();
                g.PerformLayout();
            }

            this.ResumeLayout();
            
            // Set window properties.
            this.Text = MaxCUIProperties.WindowTitle + " | WIP Settings";
            this.ClientSize = WINDOW_SIZE;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.Manual;
            Point parentLocation = parent.PointToScreen(Point.Empty);
            this.Left = parentLocation.X + (parent.ClientSize.Width / 2) - (this.ClientSize.Width / 2);
            this.Top = parentLocation.Y + (parent.ClientSize.Height / 2) - (this.ClientSize.Height / 2);
        }



        private CheckBox addCheckBox( UIElem elem ){
            NlmCheckBox chckBx = new NlmCheckBox(Settings);
            chckBx.Name = elem.name;
            chckBx.TabIndex = 0;
            chckBx.Text = elem.text;
            //  Styling:
            chckBx.TickColor = Color.White;
            chckBx.TickThickness = 2;
            chckBx.CheckBoxSize = new Size( 12, 14 );
            chckBx.BorderThickness = 2;
            chckBx.BackColor = MaxForm.ActiveForm.BackColor;
            Color chckBxBackColor = Color.FromArgb( 88,88,88 );
            chckBx.UnCheckedBackColor = chckBxBackColor;
            chckBx.CheckedBackColor = chckBxBackColor;
            chckBx.Font = item_font;
            chckBx.TextAlign = ContentAlignment.MiddleCenter;
            chckBx.UseVisualStyleBackColor = true;
            chckBx.AutoSize = true;
            chckBx.CheckAlign = ContentAlignment.MiddleRight;
            //  Position & Size:
            chckBx.Location = new Point(11,17);
            chckBx.Size = new Size( 255, 18 );
            //  Read the Settings field for this CheckBox:
            var prop = Settings.GetType().GetField(elem.name);
            chckBx.Checked = (Boolean)prop.GetValue(Settings);
            //  Connect the CheckBox. ClickEvents.onChangeSetting() will copy
            //Checked value to appropriate Settings field.
            chckBx.ClickNlmCheckBox += new EventHandler<ClickEventArgs>(ClickEvents.onChangeSetting);
            return chckBx;
        }



        private Button addButton( string text ){
            Button bttn = new Button();
            bttn.TabIndex = 0;
            bttn.Text = text;
            bttn.Name = "";
            //  Styling:
            bttn.Font = item_font;
            bttn.FlatStyle = FlatStyle.Popup;
            bttn.FlatAppearance.BorderColor = Color.Black;
            bttn.FlatAppearance.BorderSize = 2;
            bttn.UseVisualStyleBackColor = true;
            //  Position & Size:
            bttn.Location = new Point(8,17);
            bttn.Size = new Size( 136, 26 );
            return bttn;
        }



        private GroupBox addGroupBox( string text ){
            GroupBox grpBx = new NlmGroupBox();
            grpBx.TabStop = false;
            grpBx.TabIndex = 0;
            grpBx.Text = text;
            grpBx.Name = "";
            //  Styling:
            grpBx.Font = group_font;
            grpBx.FlatStyle = FlatStyle.Flat;
            //  Position & Size:
            grpBx.Location = new Point(4,1);
            grpBx.Size = new Size(302, 50);
            //  Layout:
            grpBx.Margin = new Padding(10,3,3,3);
            grpBx.Padding = new Padding(8,3,3,3);
            grpBx.RightToLeft = RightToLeft.Yes;
            return grpBx;
        }



        private void setSettings(){ 
        }
    }



    public struct UIElem
    {
        public string name { get; set; }
        public string text { get; set; }
        public int group { get; set; }
        public Point location { get; set; }
        public UIElem(string n, string t,int g) : this()
        {
            name = n;
            text = t;
            group = g;
        }
        public UIElem(string t, int g, Point p) : this()
        {
            text = t;
            group = g;
            location = p;
        }
    }
}
