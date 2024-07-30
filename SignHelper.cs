using System.Xml;
using System.Xml.Linq;

namespace OpenKNX.Toolbox.Sign
{
    public class SignHelper
    {
        private enum DocumentCategory
        {
            None,
            Catalog,
            Hardware,
            Application
        }

        
        private static int ExportKnxprod(string iPathETS, string iWorkingDir, string iKnxprodFileName, string lTempXmlFileName, string iBaggageName, string iXsdFileName, bool iIsDebug, bool iAutoXsd)
        {
            if(Directory.Exists(outpuFolder))
                Directory.Delete(outpuFolder, true);
            Directory.CreateDirectory(outpuFolder);

            SplitXml(inputFile, outputFolder);

        }

        private static int SplitXml(string lTempXmlFileName, string outputFolder)
        {
            //if (ValidateXsd(iWorkingDir, lTempXmlFileName, lTempXmlFileName, iXsdFileName, iAutoXsd)) return 1;

            Console.WriteLine("Generating knxprod file...");

            XDocument xdoc = null;
            string xmlContent = File.ReadAllText(lTempXmlFileName);
            xdoc = XDocument.Parse(xmlContent, LoadOptions.SetLineInfo);

            XNode lXmlModel = xdoc.FirstNode;
            if (lXmlModel.NodeType == XmlNodeType.ProcessingInstruction)
                lXmlModel.Remove();

            string ns = xdoc.Root.Name.NamespaceName;
            XElement xmanu = xdoc.Root.Element(XName.Get("ManufacturerData", ns)).Element(XName.Get("Manufacturer", ns));

            string manuId = xmanu.Attribute("RefId").Value;
            Directory.CreateDirectory(Path.Combine(outputFolder, manuId));
            XElement xcata = xmanu.Element(XName.Get("Catalog", ns));
            XElement xhard = xmanu.Element(XName.Get("Hardware", ns));
            XElement xappl = xmanu.Element(XName.Get("ApplicationPrograms", ns));
            XElement xbagg = xmanu.Element(XName.Get("Baggages", ns));

            List<XElement> xcataL = new List<XElement>();
            List<XElement> xhardL = new List<XElement>();
            List<XElement> xapplL = new List<XElement>();
            List<XElement> xbaggL = new List<XElement>();
            XElement xlangs = xmanu.Element(XName.Get("Languages", ns));

            if (xlangs != null)
            {
                xlangs.Remove();
                foreach (XElement xTrans in xlangs.Descendants(XName.Get("TranslationUnit", ns)).ToList())
                {
                    DocumentCategory lCategory = GetDocumentCategory(xTrans);
                    switch (lCategory)
                    {
                        case DocumentCategory.Catalog:
                            AddTranslationUnit(xTrans, xcataL, ns);
                            break;
                        case DocumentCategory.Hardware:
                            AddTranslationUnit(xTrans, xhardL, ns);
                            break;
                        case DocumentCategory.Application:
                            AddTranslationUnit(xTrans, xapplL, ns);
                            break;
                        default:
                            throw new Exception("Unknown Translation Type: " + lCategory.ToString());
                    }

                }
            }
            xhard.Remove();
            if (xbagg != null) xbagg.Remove();

            //Save Catalog
            xappl.Remove();
            if (xcataL.Count > 0)
            {
                xlangs.Elements().Remove();
                foreach (XElement xlang in xcataL)
                    xlangs.Add(xlang);
                xmanu.Add(xlangs);
            }
            xdoc.Save(Path.Combine(outputFolder, manuId, "Catalog.xml"));
            if (xcataL.Count > 0) xlangs.Remove();
            xcata.Remove();

            // Save Hardware
            xmanu.Add(xhard);
            if (xhardL.Count > 0)
            {
                xlangs.Elements().Remove();
                foreach (XElement xlang in xhardL)
                    xlangs.Add(xlang);
                xmanu.Add(xlangs);
            }
            xdoc.Save(Path.Combine(outputFolder, manuId, "Hardware.xml"));
            if (xhardL.Count > 0) xlangs.Remove();
            xhard.Remove();

            if (xbagg != null)
            {
                // Save Baggages
                xmanu.Add(xbagg);
                if (xbaggL.Count > 0)
                {
                    xlangs.Elements().Remove();
                    foreach (XElement xlang in xbaggL)
                        xlangs.Add(xlang);
                    xmanu.Add(xlangs);
                }
                xdoc.Save(Path.Combine(outputFolder, manuId, "Baggages.xml"));
                if (xbaggL.Count > 0) xlangs.Remove();
                xbagg.Remove();
            }

            xmanu.Add(xappl);
            if (xapplL.Count > 0)
            {
                xlangs.Elements().Remove();
                foreach (XElement xlang in xapplL)
                    xlangs.Add(xlang);
                xmanu.Add(xlangs);
            }
            string appId = xappl.Elements(XName.Get("ApplicationProgram", ns)).First().Attribute("Id").Value;
            xdoc.Save(Path.Combine(outputFolder, manuId, $"{appId}.xml"));
            if (xapplL.Count > 0) xlangs.Remove();

            string lSourceBaggageName = Path.Combine(iWorkingDir, iBaggageName);
            var lSourceBaggageDir = new DirectoryInfo(lSourceBaggageName);
            Directory.CreateDirectory(Path.Combine(outputFolder, manuId, "Baggages"));
            if (lSourceBaggageDir.Exists)
                lSourceBaggageDir.DeepCopy(Path.Combine(outputFolder, manuId, "Baggages"));

            return 0;
        }

        private static DocumentCategory GetDocumentCategory(XElement iTranslationUnit)
        {
            DocumentCategory lCategory = DocumentCategory.None;
            string lId = iTranslationUnit.Attribute("RefId").Value;

            lId = lId.Substring(6);
            if (lId.StartsWith("_A-"))
                lCategory = DocumentCategory.Application;
            else if (lId.StartsWith("_CS-"))
                lCategory = DocumentCategory.Catalog;
            else if (lId.StartsWith("_H-") && lId.Contains("_CI-"))
                lCategory = DocumentCategory.Catalog;
            else if (lId.StartsWith("_H-") && lId.Contains("_P-"))
                lCategory = DocumentCategory.Hardware;
            else if (lId.StartsWith("_H-"))
                lCategory = DocumentCategory.Hardware;

            return lCategory;
        }
        
        private static void AddTranslationUnit(XElement iTranslationUnit, List<XElement> iLanguageList, string iNamespaceName)
        {
            // we assume, that here are adding just few TranslationUnits
            // get parent element (Language)
            XElement lSourceLanguage = iTranslationUnit.Parent;
            string lSourceLanguageId = lSourceLanguage.Attribute("Identifier").Value;
            XElement lTargetLanguage = iLanguageList.Elements("Child").FirstOrDefault(child => child.Attribute("Name").Value == lSourceLanguageId);
            if (lTargetLanguage == null)
            {
                // we create language element
                lTargetLanguage = new XElement(XName.Get("Language", iNamespaceName), new XAttribute("Identifier", lSourceLanguageId));
                iLanguageList.Add(lTargetLanguage);
            }
            iTranslationUnit.Remove();
            lTargetLanguage.Add(iTranslationUnit);
        }
    }
}