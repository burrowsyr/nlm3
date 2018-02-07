using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using MaxCustomControls;
//  NLM
using NestedLayerManager.MaxInteractivity;
using NestedLayerManager.IO;
using NestedLayerManager.Events;
using NestedLayerManager.Events.CustomArgs;



namespace NestedLayerManager.SubControls
{
    public class NlmCheckBox:CheckBox
    {
        private Color   __shadowBorderColor;
        private Color   __highlightBorderColor;
        public int      TickThickness;
        public Color    TickColor;
        public int      BorderThickness;
        public Color    BorderColor;
        public Color    CheckedBorderColor;
        public Size     CheckBoxSize;
        public Color    CheckedBackColor;
        public Color    UnCheckedBackColor;
        //  Hook for Settings:
        private NlmSettings Settings;
        //  We'll pass Settings to ClickEventArgs.
        public event EventHandler<ClickEventArgs> ClickNlmCheckBox;



        public NlmCheckBox()
        {
        }



        public NlmCheckBox( NlmSettings settings )
        {
            Settings = settings;
        }



        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if( Settings != null )
            {
                ClickNlmCheckBox( this, new ClickEventArgs( Settings ));
            }
        }


        private void NlmCheckBox_Click( Object sender, EventArgs e )
        {
            NlmCheckBox chckBox = sender as NlmCheckBox;
            MaxListener.PrintToListener( String.Format( "{0} {1} {2} {3}", chckBox.Name, chckBox.Parent, chckBox.Checked, e.ToString() ));
        }



        protected override void OnPaint(PaintEventArgs pevent)
        /*  Override the PaintEvent to draw a custom tick for the checkbox. This is based on code downloaded from here:
        http://devblog.antongochev.net/2008/07/08/create-custom-ui-appearance-for-winforms-checkbox-part-2/ */
        {
            base.OnPaint(pevent);
            //  Standard CheckBox padding:
            int offset = 2;
            //  CheckBox to labal:
            int distance = 4;
            //  Tick's height above the border:
            int tickOffset = 6;

            Graphics graphics = pevent.Graphics;
            graphics.Clear( BackColor );
            SizeF stringMeasure = graphics.MeasureString( Text, Font );

            //  AA:
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int leftOffset = offset + Padding.Left;
            int topOffset = (int)(ClientRectangle.Height - stringMeasure.Height) / 2;

            if (topOffset < 0)
            {
                topOffset = offset + Padding.Top;
            }
            else
            {
                topOffset -= 4;
            }

            __shadowBorderColor = ControlPaint.Dark(BackColor, (float)0.05);
            __highlightBorderColor = ControlPaint.Light(BackColor, (float)0.66);
            
            if(Checked)
            {
                //  Draw tick:
                Point[] points = new Point[]
                {
                    new Point( leftOffset, topOffset+tickOffset + TickThickness + (int)(CheckBoxSize.Height*0.33) ),
                    new Point( leftOffset + (int)(CheckBoxSize.Width*0.45), topOffset+(CheckBoxSize.Height-TickThickness) + tickOffset ),
                    new Point( leftOffset + (int)(CheckBoxSize.Width*0.75), topOffset+((int)(CheckBoxSize.Height*0.5)+TickThickness )),
                    new Point( leftOffset + CheckBoxSize.Width+1, topOffset+TickThickness ),
                };
                graphics.DrawLines( new Pen(TickColor, TickThickness), points );
            }
            else
            {
                //  Fill CheckBox rectangle:
                graphics.FillRectangle(new SolidBrush(UnCheckedBackColor), leftOffset, topOffset + tickOffset, CheckBoxSize.Width, CheckBoxSize.Height);
                //  Draw CheckBox rectangle:
                ControlPaint.DrawBorder(graphics,
                                        new Rectangle(leftOffset, topOffset + tickOffset, CheckBoxSize.Width, CheckBoxSize.Height),
                                        __shadowBorderColor,
                                        BorderThickness,
                                        ButtonBorderStyle.Solid,
                                        __shadowBorderColor,
                                        BorderThickness,
                                        ButtonBorderStyle.Solid,
                                        __highlightBorderColor,
                                        BorderThickness,
                                        ButtonBorderStyle.Solid,
                                        __highlightBorderColor,
                                        BorderThickness,
                                        ButtonBorderStyle.Solid);
            }
            graphics.DrawString( Text, Font, new SolidBrush(ForeColor), new Point( leftOffset+CheckBoxSize.Width+distance, topOffset+tickOffset ));
        }
    }
}
