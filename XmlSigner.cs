using System;
using System.Reflection;
using System.IO;

namespace OpenKNX.Toolbox.Sign
{
    class XmlSigning
    {
        public static void SignDirectory(
            string path,
            string basePath,
            bool useCasingOfBaggagesXml = false,
            string[]? excludeFileEndings = null)
        {
            Assembly asm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.XmlSigning.dll"));

            Type? ds = asm.GetType("Knx.Ets.XmlSigning.XmlSigning");
            if(ds == null)
                throw new Exception("Could not create XmlSigning");

            MethodInfo? signdirectory = ds.GetMethod("SignDirectory", BindingFlags.Static | BindingFlags.NonPublic);
            if(signdirectory == null)
                throw new Exception("Could not sign directory. signdirectory method not found");
            string[] temp = [];
            if(excludeFileEndings != null)
                temp = excludeFileEndings;
            signdirectory.Invoke(null, new object[] { path, useCasingOfBaggagesXml, temp });
        }
    }
}