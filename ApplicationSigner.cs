using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace OpenKNX.Toolbox.Sign
{
    class ApplicationProgramHasher
    {
        System.Version lVersion = new System.Version("0.0.0");

        public ApplicationProgramHasher(
                    FileInfo applProgFile,
                    IDictionary<string, string> mapBaggageIdToFileIntegrity,
                    string basePath,
                    int nsVersion,
                    bool patchIds = true)
        {
            Assembly asm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.XmlSigning.dll"));
            
            lVersion = asm.GetName().Version ?? new System.Version("0.0.0");
            if(lVersion >= new System.Version("6.2.0")) { //ab ETS6.2
                Assembly objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Common.dll"));
                Type? schemaVersion = objm.GetType("Knx.Ets.Common.Schema.KnxXmlSchemaVersion");
                if(schemaVersion == null)
                    throw new Exception($"Could not create schemaVersion (ETS {lVersion})");
                object knxSchemaVersion = Enum.ToObject(schemaVersion, nsVersion);
                _type = asm.GetType("Knx.Ets.XmlSigning.Signer.ApplicationProgramHasher");
                if(_type == null)
                    throw new Exception($"Could not create ApplicationProgramHasher (ETS {lVersion})");
                _instance = Activator.CreateInstance(_type, applProgFile, mapBaggageIdToFileIntegrity, patchIds, knxSchemaVersion);
            } else if(lVersion >= new System.Version("6.0.0")) { //ab ETS6.0/6.1
                Assembly objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Xml.ObjectModel.dll"));
                Type? schemaVersion = objm.GetType("Knx.Ets.Xml.ObjectModel.KnxXmlSchemaVersion");
                if(schemaVersion == null)
                    throw new Exception($"Could not create schemaVersion (ETS {lVersion})");
                object knxSchemaVersion = Enum.ToObject(schemaVersion, nsVersion);
                if (lVersion < new System.Version("6.1.0"))
                    _type = asm.GetType("Knx.Ets.XmlSigning.ApplicationProgramHasher");
                else
                    _type = asm.GetType("Knx.Ets.XmlSigning.Signer.ApplicationProgramHasher");
                if(_type == null)
                    throw new Exception($"Could not create ApplicationProgramHasher (ETS {lVersion})");
                _instance = Activator.CreateInstance(_type, applProgFile, mapBaggageIdToFileIntegrity, patchIds, knxSchemaVersion);
            } else { //für ETS5 und früher
                _type = asm.GetType("Knx.Ets.XmlSigning.ApplicationProgramHasher");
                if(_type == null)
                    throw new Exception($"Could not create ApplicationProgramHasher (ETS {lVersion})");
                _instance = Activator.CreateInstance(_type, applProgFile, mapBaggageIdToFileIntegrity, patchIds);
            }
        }

        public void HashFile()
        {
            if(_instance == null || _type == null)
                throw new Exception($"Could not hash file. _instance or _type is null (ETS {lVersion})");
            MethodInfo? hashfile = _type.GetMethod("HashFile", BindingFlags.Instance | BindingFlags.Public);
            if(hashfile == null)
                throw new Exception($"Could not hash file. hashfile method not found (ETS {lVersion})");
            hashfile.Invoke(_instance, null);
        }

        public string OldApplProgId
        {
            get
            {
                if(_instance == null || _type == null)
                    throw new Exception($"Could not get OldApplProgId. _instance or _type is null (ETS {lVersion})");
                PropertyInfo? propertyInfo = _type.GetProperty("OldApplProgId", BindingFlags.Public | BindingFlags.Instance);
                if(propertyInfo == null)
                    throw new Exception($"Could not get OldApplProgId. property is null (ETS {lVersion})");
                object? value = propertyInfo.GetValue(_instance);
                if(value == null)
                    throw new Exception($"Could not get OldApplProgId. value is null (ETS {lVersion})");
                return value.ToString() ?? "";
            }
        }

        public string NewApplProgId
        {
            get
            {
                if(_instance == null || _type == null)
                    throw new Exception($"Could not get NewApplProgId. _instance or _type is null (ETS {lVersion})");
                object? value = _type.GetProperty("NewApplProgId", BindingFlags.Public | BindingFlags.Instance)?.GetValue(_instance);
                if(value == null)
                    throw new Exception($"Could not get NewApplProgId. value is null (ETS {lVersion})");
                return value.ToString() ?? "";
            }
        }

        public string GeneratedHashString
        {
            get
            {
                if(_instance == null || _type == null)
                    throw new Exception($"Could not get GeneratedHashString. _instance or _type is null (ETS {lVersion})");
                object? value = _type.GetProperty("GeneratedHashString", BindingFlags.Public | BindingFlags.Instance)?.GetValue(_instance);
                if(value == null)
                    throw new Exception($"Could not get GeneratedHashString. value is null (ETS {lVersion})");
                return value.ToString() ?? "";
            }
        }

        private readonly object? _instance;
        private readonly Type? _type;
    }
}