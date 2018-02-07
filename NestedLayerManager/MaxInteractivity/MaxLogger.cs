using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//  Import Max
using Autodesk.Max;

//  Global enum for logging level
public enum LOGGING : int { OFF, DEBUG, INFO, WARNING, ERROR, CRITICAL };

namespace NestedLayerManager.MaxInteractivity
{
    public class MaxLogger
    {
        public IGlobal Global = GlobalInterface.Instance;
        private int level = 0;
        private enum col { black, blue, red }
        private int[] text_colour = new int[]{  (int)col.black,
                                                (int)col.blue,
                                                (int)col.black,
                                                (int)col.red,
                                                (int)col.red,
                                                (int)col.red};


        public MaxLogger(LOGGING lvl = LOGGING.DEBUG)
        /*  Set the logging level when you instance this class. Any message
        that has a level >= to the init. level will print to the Script
        Listener; any that is < will not.*/
        {
            level = (int)lvl;
        }


        public void log(string in_string, LOGGING lvl = LOGGING.DEBUG)
        /*  ToDo: Add formatting depending on logging level? */
        {
            int ll = (int)lvl;
            if ( (ll*level)>0 && ll >= level)
            {
                int style = Global.TheListener._Style;
                Global.TheListener.SetStyle(text_colour[(int)lvl]);
                Global.TheListener.EditStream.Printf(in_string + "\n");
                Global.TheListener.SetStyle(style);
                
            }
        }
    }
}
