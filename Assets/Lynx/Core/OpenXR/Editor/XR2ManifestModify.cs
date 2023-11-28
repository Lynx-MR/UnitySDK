/**
 * @file XR2ManifestModify.cs
 *
 * @author Geoffrey Marhuenda (improvement)
 *
 * @brief Add lynx menu in Unity Editor to manage OpenXR packages installation and configuration.
 */
#if LYNX_OPENXR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor.Build.Reporting;

using UnityEditor.XR.OpenXR.Features;
#endif

namespace Lynx.OpenXR
{
#if LYNX_OPENXR
    internal class XR2ManifestModify : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;
        public override Type featureType => typeof(LynxR1Feature);

        private string m_ManifestFilePath;

        #region OVERRIDES
        protected override void OnPreprocessBuildExt(BuildReport report) { }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            AndroidManifest androidManifest = new AndroidManifest(GetManifestPath(path));
            androidManifest.AddOpenXRMetaData();
            androidManifest.Save();
        }

        protected override void OnPostprocessBuildExt(BuildReport report) { }
        #endregion


        private string GetManifestPath(string basePath)
        {
            if (!string.IsNullOrEmpty(m_ManifestFilePath)) return m_ManifestFilePath;

            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            m_ManifestFilePath = pathBuilder.ToString();

            return m_ManifestFilePath;
        }

        private class AndroidXmlDocument : XmlDocument
        {
            private readonly string m_Path;
            protected readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

            public AndroidXmlDocument(string path)
            {
                m_Path = path;
                using (var reader = new XmlTextReader(m_Path))
                {
                    reader.Read();
                    base.Load(reader);
                }

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(NameTable);
                namespaceManager.AddNamespace("android", AndroidXmlNamespace);
            }

            public string Save()
            {
                return SaveAs(m_Path);
            }

            public string SaveAs(string path)
            {
                using (XmlTextWriter writer = new XmlTextWriter(path, new UTF8Encoding(false)))
                {
                    writer.Formatting = Formatting.Indented;
                    Save(writer);
                }
                return path;
            }
        }

        private class AndroidManifest : AndroidXmlDocument
        {
            private readonly XmlElement m_ManifestElement;

            public AndroidManifest(string path) : base(path)
            {
                m_ManifestElement = SelectSingleNode("/manifest") as XmlElement;
            }

            private void AddUsesPermission(string permission)
            {
                XmlNodeList xmlNodeList = m_ManifestElement.SelectNodes("uses-permission");

                // Create node only if the tag does not exist
                if (!(from XmlNode node in xmlNodeList from XmlAttribute attrib in node.Attributes select attrib).Any(attrib => attrib.LocalName == "name" && attrib.Value == permission))
                {
                    XmlElement permissionElement = CreateElement("uses-permission");
                    permissionElement.SetAttribute("name", AndroidXmlNamespace, permission);
                    m_ManifestElement.AppendChild(permissionElement);
                }
            }

            private void AddUsesFeature(string feature, bool required)
            {
                XmlNodeList xmlNodeList = m_ManifestElement.SelectNodes("uses-feature");

                // Create node only if the tag does not exist
                if (!(from XmlNode node in xmlNodeList from XmlAttribute attrib in node.Attributes select attrib).Any(attrib => attrib.LocalName == "name" && attrib.Value == feature))
                {
                    XmlElement usesFeatureElement = CreateElement("uses-feature");
                    usesFeatureElement.SetAttribute("name", AndroidXmlNamespace, feature);
                    usesFeatureElement.SetAttribute("required", AndroidXmlNamespace, required.ToString().ToLower());
                    m_ManifestElement.AppendChild(usesFeatureElement);
                }
            }

            private void AddIntentAction(string actionName)
            {
                // Manage manifest > queries
                XmlElement queriesNode = SelectSingleNode("/manifest/queries") as XmlElement;
                if (queriesNode == null)
                {
                    queriesNode = CreateElement("queries");
                    m_ManifestElement.AppendChild(queriesNode);
                }

                // Manage manifest > queries > intent 
                XmlElement queriesIntentNode = SelectSingleNode("/manifest/queries/intent") as XmlElement;
                if (queriesIntentNode == null)
                {
                    queriesIntentNode = CreateElement("intent");
                    queriesNode.AppendChild(queriesIntentNode);
                }

                // Manage manifest > queries > intent > action
                XmlNodeList xmlNodeList = queriesIntentNode.SelectNodes("action");
                if (!(from XmlNode node in xmlNodeList from XmlAttribute attrib in node.Attributes select attrib).Any(attrib => attrib.LocalName == "name" && attrib.Value == actionName))
                {
                    XmlElement actionElement = CreateElement("action");
                    actionElement.SetAttribute("name", AndroidXmlNamespace, actionName);
                    queriesIntentNode.AppendChild(actionElement);
                }
            }

            private void AddAndroidPackage(string packageName)
            {
                // Manage manifest > queries
                XmlElement queriesNode = SelectSingleNode("/manifest/queries") as XmlElement;
                if (queriesNode == null)
                {
                    queriesNode = CreateElement("queries");
                    m_ManifestElement.AppendChild(queriesNode);
                }

                // Manage manifest > queries > intent > action
                XmlNodeList packagesList = queriesNode.SelectNodes("package");
                if (!(from XmlNode node in queriesNode from XmlAttribute attrib in node.Attributes select attrib).Any(attrib => attrib.LocalName == "name" && attrib.Value == packageName))
                {
                    XmlElement actionElement = CreateElement("package");
                    actionElement.SetAttribute("name", AndroidXmlNamespace, packageName);
                    queriesNode.AppendChild(actionElement);
                }
            }

            internal void AddOpenXRMetaData()
            {
                AddUsesFeature("android.hardware.vr.headtracking", true);
                AddUsesPermission("org.khronos.openxr.permission.OPENXR");

                AddIntentAction("org.khronos.openxr.OpenXRRuntimeService");
                AddAndroidPackage("com.ultraleap.tracking.service");
                AddAndroidPackage("com.ultraleap.openxr.api_layer");

                AddUsesPermission("android.permission.READ_EXTERNAL_STORAGE");
                AddUsesPermission("android.permission.WRITE_EXTERNAL_STORAGE");
                AddUsesPermission("android.permission.MANAGE_EXTERNAL_STORAGE");
            }
        }
    }
#else
    internal class XR2ManifestModify { }
#endif
}