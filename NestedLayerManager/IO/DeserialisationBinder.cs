using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using NestedLayerManager.MaxInteractivity;

namespace NestedLayerManager.IO
{
    class DeserialisationBinder:SerializationBinder
    /*  Got this here: http://spazzarama.com/2009/06/25/binary-deserialize-unable-to-find-assembly/ 
    The hope is that this class will solve the 'Unable to load type' deserialisation error. */
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
#if false
            MaxListener.PrintToListener(assemblyName + " | " + typeName); 
#endif
            Type typeToDeserialise = null;
            string currentAssembly = Assembly.GetExecutingAssembly().FullName;

            //  We are always using this assembly: 
            //no we're not! we're using the assembly passed to this method so we can resolve it.
            //assemblyName = currentAssembly;

            //  Example 'Type not found' error:
            //  System.Collections.Generic.List`1[[NestedLayerManager.IO.Data.FolderData, NestedLayerManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
            //  Get the type using the typeName and assemblyName:
            string type_name_arg = string.Format( "{0}, {1}", typeName, assemblyName );
#if false
		    MaxListener.PrintToListener( "GetType( " + type_name_arg + " )" );  
#endif
            typeToDeserialise = Type.GetType( type_name_arg );

            return typeToDeserialise;
        }
    }
}
