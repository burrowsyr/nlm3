using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
//  Import Max
using Autodesk.Max;
//  Assembly imports
using NestedLayerManager.Maps;
using NestedLayerManager.SubControls;
using NestedLayerManager.MaxInteractivity;
using NestedLayerManager.MaxInteractivity.MaxAnims;
using NestedLayerManager.IO.Data;
using NestedLayerManager.Nodes;
using NestedLayerManager.Nodes.Base;
using NestedLayerManager.NodeControl;



namespace NestedLayerManager.IO
{
    public class ReadWriteNLM2Data
    {
        public IAnimatable rootNode;
        public IGlobal Global = GlobalInterface.Instance;
        public IInterface16 Interface = GlobalInterface.Instance.COREInterface16;

        //  Currently we're using the MaxScript function from NLM2 to create the NLM2 CustAttrib since I 
        //can't work out how to accomplish it in C#!
        private string nlm2_rootProp_ms;

        //  NLM2 Lists named after the relevant IParamDef.IntName in the 'nestedLayerManager' ParamBlock:
        public List<string>     folderDirs      = new List<string>();
        public List<IPoint3>    foldercolours   = new List<IPoint3>();
        public List<string>     layerDirs       = new List<string>();
        public List<Autodesk.Max.Wrappers.ILayer>   layerRefs  = new List<Autodesk.Max.Wrappers.ILayer>();
        public List<string>     layerNames      = new List<string>();
        public float dataVersion = 2.00F;

        //  Initialise NLM3 lists
        public List<FolderData> folderDataNodes = new List<FolderData>();

        //  Shove the paramter values and types on here for verbose / debug output.
        public string NLM_Params = "";
        private string logging = "";

        //  Set the logging level here, we're using a delegate to point to MaxLogger.log to make 
        // life a bit easier:
        private static MaxLogger logger = new MaxLogger(LOGGING.INFO);
        private delegate void logPntr(string in_string, LOGGING lvl = LOGGING.INFO);
        logPntr log = new logPntr(logger.log);

        //  We need this to create FolderData
        private NlmTreeListView ListView;

        //  Have we got any NLM2 properties?
        private Boolean? nlm2 = null;

        private enum data{ read, write };



        public ReadWriteNLM2Data()
        {
            //  I tried setting this using Properties.Resources but this threw errors in 3dsMax: "Can't find 'NestedLayerManager.resource'"
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("NestedLayerManager.Resources.MaxScript.NLM2RootProps.ms"))
            {
                using (var reader = new StreamReader(stream))
                {
                    nlm2_rootProp_ms = reader.ReadToEnd();
                }
            }
        }



        public void getRootNode()
        {
            rootNode = Interface.RootNode as IAnimatable;
        }



        public bool getNLMProp()
        {
            if( nlm2 != null )
            //  If we've already checked then we know whether we have NLM2 properties.
            {
                return (Boolean)nlm2;
            }
            getRootNode();
            //Global.ExecuteMAXScriptScript("clearlistener()",true, null);
            log("getNLMProp()...");
            IICustAttribContainer ICustAttrCont = rootNode.CustAttribContainer;
            if (ICustAttrCont == null) 
            {
                log("\n#\tError: No Attribute Container on RootNode.\n", LOGGING.ERROR);
                nlm2 = false;
                return (Boolean)nlm2;
            }
            logging = "{0,-23}{1}\n";
            NLM_Params +=( string.Format(  logging, "IICustAttribContainer:" ,ICustAttrCont.ToString() ));
            
            //  Look for the 'nestedLayerManager' and return false if we can't find it.
            ICustAttrib ICustAttr = getCustAttribByName( ICustAttrCont, "nestedLayerManager" );
            if (ICustAttr == null) 
            {
                log("\n#\tError: No 'nestedLayerManager' Custom Attribute found.\n", LOGGING.ERROR);
                nlm2 = false;
                return (Boolean)nlm2;
            }
            NLM_Params += ( string.Format( logging, "ICustAttrib name: ",ICustAttr.Name ));

            readWriteData( ICustAttr, data.read);
            nlm2 = true;
            return (Boolean)nlm2;
        }



        private void readWriteData(ICustAttrib ICustAttr,  data data )
        {
            //  For printing parameter values, put it here since there's no need to keep declaring it:
            string param_val = "\t\t\t{0}\n";
            logging = "\t{0,-10}[{1}]: {2}  Version: {3:0.00}\n";
            int param_blocks_count = ICustAttr.NumParamBlocks;
            for (int par_blck_idx = 0; par_blck_idx < param_blocks_count; par_blck_idx++)
            {
                IIParamBlock2 ParamBlock = ICustAttr.GetParamBlock(par_blck_idx);
                //  Keep this line for reference:
                IInterval ivalid = Global.Interval.Create();
                IPB2Value ver = ParamBlock.GetPB2Value(4, 0);
                NLM_Params += (string.Format(logging, "ParamBlock", par_blck_idx, ParamBlock.LocalName.ToString(), ver.F));

                logging = "\t\t{0}{1,-10} Tab-Size:{2}\n";
                //  We're inside the IIParamBlock2 and stepping through the parameters:
                int param_def_count = ParamBlock.NumParams;
                for (int par_def_idx = 0; par_def_idx < param_def_count; par_def_idx++)
                {
                    short param_id = ParamBlock.IndextoID(par_def_idx);
                    IParamDef ParamDef = ParamBlock.GetParamDef(param_id);
                    ParamType2 PT = ParamDef.Type;
                    int tab_count = ParamBlock.Count(param_id);

                    string param_name = ParamDef.IntName;
                    Char pad = '.';
                    NLM_Params += (string.Format(logging, param_name.PadRight(14, pad), PT, tab_count));

                    //  ======== We're reading NLM2 data ========
                    if (data==data.read)
                    {
                        for (int tab_idx = 0; tab_idx < tab_count; tab_idx++)
                        {
                            IPB2Value ParamVal = ParamBlock.GetPB2Value(param_id, tab_idx);
                            switch (PT)
                            {
                                case ParamType2.StringTab:
                                    //  Folders are String data.
                                    NLM_Params += (string.Format(param_val, ParamVal.S));
                                    //  Get the relevant list using the parameter name and append the value.
                                    List<string> t = (List<string>)this.GetType().GetField(param_name).GetValue(this);
                                    t.Add(ParamVal.S);
                                    break;
                                case ParamType2.RgbaTab:
                                    //  Get the folder colour stored as RGBA in an IPoint3.
                                    MColour folderColour = new MColour(ParamVal.P);
                                    string rgb = string.Format("({0,3},{1,3},{2,3})", folderColour.R, folderColour.G, folderColour.B);
                                    NLM_Params += (string.Format(param_val, rgb));
                                    foldercolours.Add(ParamVal.P);
                                    break;
                                case ParamType2.ReftargTab:
                                    //  If the Parameter is a Reference Target then it's pointing at a layer. We need to
                                    //cast the IReferenceTarget to an ILayer wrapper so we can get the layer methods.
                                    //We also add the Layer name to a list to help with buildLayerData().
                                    Autodesk.Max.Wrappers.ILayer RefTar = (Autodesk.Max.Wrappers.ILayer)ParamVal.R;
                                    NLM_Params += (string.Format(param_val, RefTar.Name));
                                    layerRefs.Add(RefTar);
                                    layerNames.Add(RefTar.Name);
                                    break;
                                case ParamType2.Float:
                                    NLM_Params += (ParamVal.F.ToString());
                                    break;
                                default:
                                    log("default");
                                    break;
                            }
                        } 
                    }

                    //  ======== We're writing NLM2 data ========
                    if (data == data.write)
                    {
                        ParamBlock.Delete( param_id, 0, tab_count );
                        switch (PT)
                        {
                            case ParamType2.StringTab:
                                List<string> s = (List<string>)this.GetType().GetField(param_name).GetValue(this);
                                ParamBlock.SetCount(param_id, s.Count);
                                for (int i = 0; i < s.Count; i++)
                                {
                                    NLM_Params += ( string.Format( param_val, s[i] ));
                                    ParamBlock.SetValue(param_id, 0, s[i], i);
                                }
                                break;
                            case ParamType2.RgbaTab:
                                List<IPoint3> c = (List<IPoint3>)this.GetType().GetField(param_name).GetValue(this);
                                ParamBlock.SetCount(param_id, c.Count);
                                for (int i = 0; i < c.Count; i++)
                                {
                                    string rgb = string.Format("({0,3},{1,3},{2,3})", (byte)(c[i].X*255), (byte)(c[i].Y*255), (byte)(c[i].Z*255) );
                                    NLM_Params += (string.Format(param_val, rgb));
                                    ParamBlock.SetValue(param_id, 0, c[i], i);
                                    
                                }
                                break;
                            case ParamType2.ReftargTab:
                                List<Autodesk.Max.Wrappers.ILayer> l = (List<Autodesk.Max.Wrappers.ILayer>)this.GetType().GetField(param_name).GetValue(this);
                                ParamBlock.SetCount( param_id, l.Count );
                                for (int i = 0; i < l.Count; i++)
                                {
                                    NLM_Params += (string.Format(param_val, l[i].Name));
                                    ParamBlock.SetValue(param_id, 0, l[i], i);
                                }
                                break;
                        }
                        
                    }
                }
                log(NLM_Params, LOGGING.INFO);
            }
        }



        public List<FolderData> buildFolderRootNodeData(NlmTreeListView listView) 
        {
            ListView = listView;
            //  Initialise the folderDataNodes list
            foreach (string ff in folderDirs) { folderDataNodes.Add(new FolderData(new FolderTreeNode(""), ListView)); }
            //  Iterate folderDirs using indexing, that way we can access foldercolours and folderDataNodes.
            for (int folder_path_idx = 0; folder_path_idx < folderDirs.Count(); folder_path_idx++)
            {
                //  Chop off the trailing '\' otherwise we get an extra element in folders[].
                string folder_path = Regex.Replace( folderDirs[folder_path_idx], @"\\$", @"" );
                string[] folders = folder_path.Split(new char[] { '\\' });
                //  Last item in folders[] is the name of this folder.
                string folder_name = folders[folders.Length-1];
                //  Get the parent folder path by chopping the last item off folders[] and turning this into a string.
                string[] parent_folder = folders.Take( folders.Length-1 ).ToArray();
                string parent_folder_path = String.Format( "{0}\\", String.Join( "\\", parent_folder ));
                //  Use the parent_folder_path to index into folderDataNodes.
                int parent_idx = folderDirs.IndexOf( parent_folder_path );
                //  Use this folder_path to find the list of children.
                //Is this reduntant? Seems like it may be but I'll leave it just in case.
                //I put it in since BaseTreeNode has a Children property but failed to notice
                //that FolderData doesn't use it...
                //string p = String.Format( @"{0}\\([0-9a-zA-Z_]+)\\$", Regex.Replace( folder_path, @"\\",@"\\" ));
                //List<string> children = folderDirs.FindAll( x => Regex.IsMatch( x , p ));

                FolderData FD = folderDataNodes[folder_path_idx];
                FD.Color = new MColour( foldercolours[folder_path_idx] ).Colour;
                FD.Name = folder_name;
                if (parent_idx > -1)
                {
                    FD.ParentID = folderDataNodes[parent_idx].ID;
                }
                /*
                foreach( string child in children )
                {
                    int child_idx = folderDirs.IndexOf( child );
                }
                */
                log( string.Format( "folder_path:   {0}", folder_path ), LOGGING.DEBUG );
                log( string.Format( "parent_folder: {0} {1}", parent_folder_path, FD.ParentID ), LOGGING.DEBUG );
                log( string.Format( "name | ID:     {0,-10} {1}\n", FD.Name, FD.ID ), LOGGING.DEBUG );
            }
            log( "buildFolderRootNodeData Done!!", LOGGING.DEBUG );
            return folderDataNodes;
        }



        public LayerData buildLayerData(IILayer layer)
        {
            //  Which Layer are we working with?
            int layer_idx = layerNames.IndexOf( layer.Name );
            //  Don't fully understand this but seems necessary to build layer data.
            UIntPtr layer_handle = MaxAnimatable.GetHandleByAnim(layer);
            LayerTreeNode LTN = new LayerTreeNode(layer_handle, new HandleMap());
            LTN.Parent = null;
            //  Now we can look for the layers parent folder. We ensure check layer_idx
            //is valid since we might have an empty nestedLayerManager CustomAttrib. 
            if ( layer_idx > -1 )
            {
                string parent_folder_name = layerDirs[layer_idx];
                int parent_folder_idx = folderDirs.IndexOf(parent_folder_name);
                if (parent_folder_idx > -1)
                {
                    FolderData FD = folderDataNodes[parent_folder_idx];
                    FolderTreeNode FTN = new FolderTreeNode(FD);
                    LTN.Parent = FTN;
                } 
            }
            LayerData LD = new LayerData(LTN, ListView, new HandleMap());
            return LD;
        }



        private FolderData folderDataInstance( string folder_name ) 
        {
            return new FolderData( new FolderTreeNode(""), null );
        }



        private ICustAttrib getCustAttribByName( IICustAttribContainer ICustAttrCont, string param_block )
        /*  Simple name matching loop to get an ICustAttr via its name, passed as a string */
        {
            int custom_attr_count = ICustAttrCont.NumCustAttribs;
            for (int cust_attr_idx = 0; cust_attr_idx < custom_attr_count; cust_attr_idx++)
            {
                ICustAttrib ICustAttr = ICustAttrCont.GetCustAttrib(cust_attr_idx);
                if( ICustAttr.Name == param_block )
                {
                    return ICustAttr;
                }
            }
            return null;
        }



        public Boolean writeNLMProp(NlmTreeListView listView, NodeController nodeControl)
        {
            NLM_Params = "";
            log( "writeNLMProp()..." );
            getRootNode();
            IICustAttribContainer ICustAttrCont = rootNode.CustAttribContainer;
            if (ICustAttrCont == null) {
                rootNode.AllocCustAttribContainer();
                ICustAttrCont = rootNode.CustAttribContainer;
            }
            logging = "{0,-23}{1}\n";
            NLM_Params += (string.Format(logging, "IICustAttribContainer:", ICustAttrCont.ToString()));

            ICustAttrib ICustAttr = getCustAttribByName(ICustAttrCont, "nestedLayerManager");
            if (ICustAttr == null) {
                log(nlm2_rootProp_ms);
                //  MaxScript custAttributes.add returns true so we have to look for nestedLayerManager again:
                Global.ExecuteMAXScriptScript(nlm2_rootProp_ms, false, null);
                ICustAttr = getCustAttribByName(ICustAttrCont, "nestedLayerManager");
            }
            NLM_Params += (string.Format(logging, "ICustAttrib name: ", ICustAttr.Name));

            // Create folder and layer treeNode arrays.
            IEnumerable<FolderTreeNode> folderNodes = nodeControl.Query.FolderNodes;
            IEnumerable<LayerTreeNode> layerNodes = nodeControl.Query.LayerNodes;

            //  Clear the folderDirs and foldercolors Lists, iterate folderNodes, concatenate their names into
            //strings and add to folderDirs. Get each folder colour and add to foldercolours:
            folderDirs.Clear();
            foldercolours.Clear();
            foreach (FolderTreeNode FN in folderNodes) {
                //  NLM2 Folder name ends with \
                string ss = FN.Name + "\\";
                //  Step up through the folder hierarchy and build the string path for the folder name: 
                FolderTreeNode np = (FolderTreeNode)FN.Parent;
                while (np != null)
                {
                    ss = np.Name + "\\" + ss;
                    np = (FolderTreeNode)np.Parent;
                }
                folderDirs.Add( ss );
                MColour folderColour = new MColour(FN.Color.Value);
                IPoint3 col = folderColour.ColourVector;
                foldercolours.Add( col );
            }

            //  Clear the layer lists. Iterate layerNodes getting the appropriate layer from its' handle and add
            //it to the layerRefs list. Get the layer's parent ID and index into folderNodes to find the string
            //path for the parent folder, then add this to layerDirs List.
            layerRefs.Clear();
            layerDirs.Clear();
            foreach (LayerTreeNode LTN in layerNodes) {
                Autodesk.Max.Wrappers.ILayer layer = (Autodesk.Max.Wrappers.ILayer)MaxAnimatable.GetAnimByHandle(LTN.Handle);
                layerRefs.Add( layer );
                FolderTreeNode lp = (FolderTreeNode)LTN.Parent;
                int parent_folder_idx = folderNodes.ToList().IndexOf(lp);
                string folder_path = (parent_folder_idx > -1 )? folderDirs[ parent_folder_idx ]:"";
                layerDirs.Add(folder_path);
            }
            readWriteData( ICustAttr, data.write );
            log( "readWriteData() Done" );
            return true;
        }
    }



    public class MColour
    {
        public int R = 0;
        public int G = 0;
        public int B = 0;
        public float r = 0;
        public float g = 0;
        public float b = 0;
        public Color Colour;
        public IPoint3 ColourVector;
        public MColour( IPoint3 colourVector )
        /*  Instance the class with IPoint3 when reading NLM2 data.*/
        {
            //  Used for printing out the custom properties at the moment.
            R = (int)(colourVector.X * 255);
            G = (int)(colourVector.Y * 255);
            B = (int)(colourVector.Z * 255);
            //  NLM3 is using System.Drawing.Color for folder colours.
            Colour = System.Drawing.Color.FromArgb( this.R, this.G, this.B );
        }
        public MColour( Color colour)
        /*  Instance the class with System.Drawing.Color when writing NLM2 data.*/
        {
            R = colour.R;
            G = colour.G;
            B = colour.B;
            r = (R/255F);
            g = (G/255F);
            b = (B/255F);
            //  NLM2 uses IPoint3 to store colour values.
            ColourVector = GlobalInterface.Instance.Point3.Create( new float[]{r,g,b} );
            
            
        }
    }



    public static class RW
    {
        public const int read   = 0;
        public const int write  = 1;
    }
}
