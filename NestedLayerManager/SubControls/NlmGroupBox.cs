using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using MaxCustomControls;



namespace NestedLayerManager.SubControls
{
    public class NlmGroupBox: GroupBox
    {
        private Color borderColour;


        public Color BorderColour
        {
            get { return this.borderColour; }
            set { this.borderColour = value; }
        }


        public NlmGroupBox()
        {
            this.borderColour = Color.Black;
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            Size tSize = TextRenderer.MeasureText(this.Text, this.Font);

            Rectangle borderRect = e.ClipRectangle;
            borderRect.Y += tSize.Height / 2;
            borderRect.Height -= tSize.Height / 2;
            ControlPaint.DrawBorder(e.Graphics, borderRect, this.borderColour, ButtonBorderStyle.Solid);

            Rectangle textRect = e.ClipRectangle;
            textRect.X = e.ClipRectangle.Width - tSize.Width - 4;
            textRect.Width = tSize.Width;
            textRect.Height = tSize.Height;
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), textRect);
            e.Graphics.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), textRect.X, 0);
        }
    }
}
