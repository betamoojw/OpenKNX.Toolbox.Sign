using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace OpenKNX.Toolbox.Sign
{
    class CatalogIdPatcher
    {
        System.Version lVersion  = new System.Version("0.0.0");
        public CatalogIdPatcher(
            FileInfo catalogFile,
            IDictionary<string, string> hardware2ProgramIdMapping,
            string basePath,
            int nsVersion)
        {
            Assembly asm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.XmlSigning.dll"));
            
            lVersion = asm.GetName().Version ?? new System.Version("0.0.0");
            if(lVersion >= new System.Version("6.2.0")) { //ab ETS6.2
                Assembly objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Common.dll"));
                Type? schemaVersion = objm.GetType("Knx.Ets.Common.Schema.KnxXmlSchemaVersion");
                if(schemaVersion == null)
                    throw new Exception($"Could not create schemaVersion (ETS {lVersion})");
                object knxSchemaVersion = Enum.ToObject(schemaVersion, nsVersion);
                _type = asm.GetType("Knx.Ets.XmlSigning.Signer.CatalogIdPatcher");
                if(_type == null)
                    throw new Exception($"Could not create CatalogIdPatcher (ETS {lVersion})");
                _instance = Activator.CreateInstance(_type, catalogFile, hardware2ProgramIdMapping, knxSchemaVersion);
            } else if(lVersion >= new System.Version("6.0.0")) { //ab ETS6.0/6.1
                Assembly objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Xml.ObjectModel.dll"));
                Type? schemaVersion = objm.GetType("Knx.Ets.Xml.ObjectModel.KnxXmlSchemaVersion");
                if(schemaVersion == null)
                    throw new Exception($"Could not create schemaVersion (ETS {lVersion})");
                object knxSchemaVersion = Enum.ToObject(schemaVersion, nsVersion);
                if (lVersion < new System.Version("6.1.0"))
                    _type = asm.GetType("Knx.Ets.XmlSigning.CatalogIdPatcher");
                else
                    _type = asm.GetType("Knx.Ets.XmlSigning.Signer.CatalogIdPatcher");
                if(_type == null)
                    throw new Exception($"Could not create CatalogIdPatcher (ETS {lVersion})");
                _instance = Activator.CreateInstance(_type, catalogFile, hardware2ProgramIdMapping, knxSchemaVersion);
            } else {
                _type = asm.GetType("Knx.Ets.XmlSigning.CatalogIdPatcher");
                if(_type == null)
                    throw new Exception($"Could not create CatalogIdPatcher (ETS {lVersion})");
                _instance = Activator.CreateInstance(_type, catalogFile, hardware2ProgramIdMapping);
            }
        }

        public void Patch()
        {
            if(_instance == null || _type == null)
                throw new Exception($"Could not patch file. _instance or _type is null (ETS {lVersion})");
            MethodInfo? patch = _type.GetMethod("Patch", BindingFlags.Instance | BindingFlags.Public);
            if(patch == null)
                throw new Exception($"Could not patch file. Patch method not found (ETS {lVersion})");
            patch.Invoke(_instance, null);
        }

        private readonly object? _instance;
        private readonly Type? _type;
    }
}