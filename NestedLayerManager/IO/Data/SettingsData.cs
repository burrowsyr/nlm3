using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NestedLayerManager.IO.Data
{
    public class Nlm2Data : ConfigurationSection
    {
        [ConfigurationProperty("readNLM2", DefaultValue = true)]
        public Boolean readNLM2
        {
            get { return (Boolean)this["readNLM2"]; }
            set { this["readNLM2"] = value; }
        }


        [ConfigurationProperty("writeNLM2", DefaultValue = true)]
        public Boolean writeNLM2
        {
            get { return (Boolean)this["writeNLM2"]; }
            set { this["writeNLM2"] = value; }
        }
    }




    public class IOOptions : ConfigurationSection
    {
        [ConfigurationProperty("mergeOnFile", DefaultValue = false)]
        public Boolean mergeOnFile
        {
            get { return (Boolean)this["mergeOnFile"]; }
            set { this["mergeOnFile"] = value; }
        }


        [ConfigurationProperty("mergeOnXref", DefaultValue = false)]
        public Boolean mergeOnXref
        {
            get { return (Boolean)this["mergeOnXref"]; }
            set { this["mergeOnXref"] = value; }
        }
    }




    public class SelectionOptions : ConfigurationSection
    {
        [ConfigurationProperty("selectObjectsUI", DefaultValue = false)]
        public Boolean selectObjectsUI
        {
            get { return (Boolean)this["selectObjectsUI"]; }
            set { this["selectObjectsUI"] = value; }
        }


        [ConfigurationProperty("selectObjectsScene", DefaultValue = false)]
        public Boolean selectObjectsScene
        {
            get { return (Boolean)this["selectObjectsScene"]; }
            set { this["selectObjectsScene"] = value; }
        }


        [ConfigurationProperty("selectLayersUI", DefaultValue = false)]
        public Boolean selectLayersUI
        {
            get { return (Boolean)this["selectLayersUI"]; }
            set { this["selectLayersUI"] = value; }
        }
    }
}
