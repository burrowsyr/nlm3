using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//  NLM modules:
using NestedLayerManager.MaxInteractivity;
using NestedLayerManager.IO.Data;



namespace NestedLayerManager.IO
{
    public class NlmSettings
    /*  I've put NLM settings in the class fields, default settings come from our ConfigurationSection
    classes declared in NestedLayerManager.IO.Data.SettingsData. We're using ConfigurationManager which 
    seems to handle the FileExists checking for the NLM3.UserSettings.config file: if it isn't present 
    then the defaults are used.
        Since we only instance this class once and attach it to NlmTreeListView, then these settings will
    be alive for the life time of the UI and save when it closes. At least that's the idea...*/
    {
        //  NLM Settings.
        //These are written to on instantiation by loadUserSettings() and by the click events in the NLMSettings code.
        //They are read from (obviously!) by appropriate methods in the application and by saveUserSettings() when the
        //assembly closes...
        public Boolean readNLM2;
        public Boolean writeNLM2;
        public Boolean mergeOnFile;
        public Boolean mergeOnXref;
        public Boolean selectObjectsUI;
        public Boolean selectObjectsScene;
        public Boolean selectLayersUI;
        //  UserSettings.config path stuff:
        private string user_dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private string settings_dir = "AppData\\Local\\Autodesk\\3dsMax\\2016 - 64bit\\ENU";
        private string settings_ini = "NLM3.UserSettings.config";
        private string user_settings_path;
        //  Initialise our ConfigManager pointers:
        ExeConfigurationFileMap configMap;
        Configuration Config;
        //  ConfigSections:
        Nlm2Data Nlm2Data = new Nlm2Data();
        IOOptions IOOptions = new IOOptions();
        SelectionOptions SelectionOptions = new SelectionOptions();
        List<ConfigurationSection> configSections;



        public NlmSettings()
        {
            user_settings_path = Path.Combine(user_dir, settings_dir, settings_ini);
            configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = user_settings_path;
            Config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            //  Having field instances of our config classes we can put them in a list we can iterate 
            //whenever we need to later:
            configSections = new List<ConfigurationSection>() { Nlm2Data, IOOptions, SelectionOptions };
            loadUserSettings();
        }



        private void loadUserSettings()
        {
            for (int i = 0; i < configSections.Count; i++)
            {
                ConfigurationSection CS = Config.GetSection(configSections[i].GetType().Name);
                if (CS != null) { configSections[i] = CS; }
                //  Iterate the properties in each ConfigurationSection and grab the matching NlmSettings field.
                foreach (var prop in configSections[i].GetType().GetProperties())
                {
                    //  If we've got a valid field then copy the values from the ConfigurationSection.Property. 
                    var fieldProp = this.GetType().GetField(prop.Name);
                    if (fieldProp != null)
                    {
                        //Console.WriteLine(prop + "=" + prop.GetValue(configSections[i]));
                        fieldProp.SetValue(this, prop.GetValue(configSections[i]));
                    }
                }
            }
        }



        public void saveUserSettings()
        {
#if DEBUG
            MaxListener.PrintToListener("Saving userSettings..."); 
#endif
            foreach (ConfigurationSection configSection in configSections)
            {
                foreach (var prop in configSection.GetType().GetProperties())
                {
                    var fieldProp = this.GetType().GetField(prop.Name);
                    if (fieldProp != null)
                    {
                        //Console.WriteLine(fieldProp + " = " + fieldProp.GetValue(this));
                        prop.SetValue(configSection, fieldProp.GetValue(this));
                    }
                }
                configSection.SectionInformation.ForceSave = true;
                if (Config.Sections[configSection.GetType().Name] == null)
                {
                    Config.Sections.Add(configSection.GetType().Name, configSection);
                }
            }
            Config.Save(ConfigurationSaveMode.Full);
        }
    }

}
