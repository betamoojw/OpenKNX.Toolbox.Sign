using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace OpenKNX.Toolbox.Sign
{
    class HardwareSigner
    {
        public HardwareSigner(
                FileInfo hardwareFile,
                IDictionary<string, string> applProgIdMappings,
                IDictionary<string, string> applProgHashes,
                string basePath,
                int nsVersion,
                bool patchIds)
        {
            Assembly asm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.XmlSigning.dll"));
            Assembly objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Xml.ObjectModel.dll"));

            Type? RegistrationKeyEnum = objm.GetType("Knx.Ets.Xml.ObjectModel.RegistrationKey");
            if(RegistrationKeyEnum == null)
                throw new Exception("Could not create RegistrationKeyEnum");
            object registrationKey = Enum.Parse(RegistrationKeyEnum, "knxconv");

            System.Version lVersion = asm.GetName().Version ?? new System.Version("0.0.0");
            if(lVersion >= new System.Version("6.2.0")) { //ab ETS6.2
                objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Common.dll"));
                Type? schemaVersion = objm.GetType("Knx.Ets.Common.Schema.KnxXmlSchemaVersion");
                if(schemaVersion == null)
                    throw new Exception("Could not create schemaVersion");
                object knxSchemaVersion = Enum.ToObject(schemaVersion, nsVersion);
                _type = asm.GetType("Knx.Ets.XmlSigning.Signer.HardwareSigner");
                if(_type == null)
                    throw new Exception("Could not create HardwareSigner");
                _instance = Activator.CreateInstance(_type, hardwareFile, applProgIdMappings, applProgHashes, patchIds, registrationKey, knxSchemaVersion);
            } else if(lVersion >= new System.Version("6.0.0")) { //ab ETS6.0/6.1
                Type? schemaVersion = objm.GetType("Knx.Ets.Common.Schema.KnxXmlSchemaVersion");
                if(schemaVersion == null)
                    throw new Exception("Could not create schemaVersion");
                object knxSchemaVersion = Enum.ToObject(schemaVersion, nsVersion);
                if (lVersion < new System.Version("6.1.0"))
                    _type = asm.GetType("Knx.Ets.XmlSigning.HardwareSigner");
                else
                    _type = asm.GetType("Knx.Ets.XmlSigning.Signer.HardwareSigner");
                if(_type == null)
                    throw new Exception("Could not create HardwareSigner");
                _instance = Activator.CreateInstance(_type, hardwareFile, applProgIdMappings, applProgHashes, patchIds, registrationKey, knxSchemaVersion);
            } else {
                _type = asm.GetType("Knx.Ets.XmlSigning.HardwareSigner");
                if(_type == null)
                    throw new Exception("Could not create HardwareSigner");
                _instance = Activator.CreateInstance(_type, hardwareFile, applProgIdMappings, applProgHashes, patchIds, registrationKey);
            }
        }

        public void SignFile()
        {
            if(_instance == null || _type == null)
                throw new Exception("Could not sign file. _instance or _type is null");
            MethodInfo? signfile = _type.GetMethod("SignFile", BindingFlags.Instance | BindingFlags.Public);
            if(signfile == null)
                throw new Exception("Could not sign file. Method is null");
            signfile.Invoke(_instance, null);
        }

        private readonly object? _instance;
        private readonly Type? _type;

        public IDictionary<string, string> OldNewIdMappings
        {
            get
            {
                if(_instance == null || _type == null)
                    throw new Exception("Could not get OldNewIdMappings. _instance or _type is null");
                PropertyInfo? prop = _type.GetProperty("OldNewIdMappings", BindingFlags.Public | BindingFlags.Instance);
                if(prop == null)
                    throw new Exception("Could not get OldNewIdMappings. Property is null");
                object? value = prop.GetValue(_instance);
                if(value == null)
                    throw new Exception("Could not get OldNewIdMappings. Value is null");
                return (IDictionary<string, string>)value;
            }
        }
    }
}