using System;
using ColossalFramework.Packaging;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using ColossalFramework.IO;
using UnityEngine;
using System.Collections.Generic;
using ICities;

namespace TestProject
{
    public class TestProjectXMLDeserializer : LoadingExtensionBase
    {
        public static bool alreadyDeserializedXML = false;

        public static void DeserializeXML(){
            if (alreadyDeserializedXML) return;
            List<string> XMLPaths = new List<string>();
                
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (prefab == null) continue;

                Package.Asset asset = PackageManager.FindAssetByName(prefab.name);
                if (asset == null || asset.package == null) continue;
                
                string crpPath = asset.package.packagePath;
                string crpName = asset.package.packageName;
                if (crpPath == null || crpName == null) continue;

                string xmlPath = Path.Combine(Path.GetDirectoryName(crpPath), crpName + ".xml");
                Debug.Log("Checking: " + xmlPath);
                if (XMLPaths.Contains(xmlPath)) continue;
                XMLPaths.Add(xmlPath);
                XmlDocument xml = new XmlDocument();
                try{
                    xml.Load(xmlPath);
                }catch(XmlException e){
                    Debug.LogError("There seems to be an XML error with asset " + prefab.name + ". Please give this error to the creator of that asset: " + e.Message);
                    continue;
                }
                if (xml == null) continue;

                List<BuildingVariation> buildingVariations = new List<BuildingVariation>();
                string overrideName = prefab.name.Replace("_Data", "");
                Debug.Log(overrideName);
                foreach (XmlNode e in xml.DocumentElement.ChildNodes)
                {
                    if (e.Name == "Variation")
                    {
                        BuildingVariation v = new BuildingVariation();
                        v.m_publicName = e.Attributes["name"]?.InnerText;
                        v.m_isDefault = (e.Attributes["default"] != null);
                        foreach (XmlNode sm in e.ChildNodes)
                        {
                            if (v.m_enabledSubMeshes == null) v.m_enabledSubMeshes = new List<string>();
                            if(sm.Name == "Submesh") v.m_enabledSubMeshes.Add(sm.InnerText);
                        }
                        buildingVariations.Add(v);
                    }
                    else if (e.Name == "OverrideName") overrideName = e.InnerText;
                }

                TestProjectBuildingData.PotentialVariationsMap.Add(overrideName, buildingVariations);
            }
            alreadyDeserializedXML = true;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(mode == LoadMode.NewGame || mode == LoadMode.LoadGame){
                DeserializeXML();
            }
        }

        public override void OnLevelUnloading()
        {
            TestProjectBuildingData.PotentialVariationsMap.Clear();
            alreadyDeserializedXML = false;
        }
    }

    // adapted from Boformer's 4th modding tutorial 
    public class TestProjectBuildingDataContainer : IDataContainer {
        public const string DataId = "BuildingVariations";
        public const int DataVersion = 0;

        public void Serialize(DataSerializer s)
        {
            s.WriteByteArray(TestProjectBuildingData.IngameBuildingVariationMap);
        }

        public void Deserialize(DataSerializer s)
        {
            TestProjectBuildingData.IngameBuildingVariationMap = s.ReadByteArray();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            if (!BuildingManager.exists) return;

            Building[] buildingInstances = BuildingManager.instance.m_buildings.m_buffer;

            for (int i = 0; i < TestProjectBuildingData.IngameBuildingVariationMap.Length; i++)
            {
                if (buildingInstances[i].m_flags == Building.Flags.None)
                {
                    TestProjectBuildingData.IngameBuildingVariationMap[i] = 0;
                }
            }
        }
    }

    public class MakeHistoricalDataManager : SerializableDataExtensionBase
    {
        private TestProjectBuildingDataContainer _data;

        public override void OnLoadData()
        {
            byte[] bytes = serializableDataManager.LoadData(TestProjectBuildingDataContainer.DataId);
            if (bytes != null)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    _data = DataSerializer.Deserialize<TestProjectBuildingDataContainer>(stream, DataSerializer.Mode.Memory);
                }
            }
            else
            {
                _data = new TestProjectBuildingDataContainer();
            }
        }

        public override void OnSaveData()
        {
            byte[] bytes;

            using (var stream = new MemoryStream())
            {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, TestProjectBuildingDataContainer.DataVersion, _data);
                bytes = stream.ToArray();
            }

            serializableDataManager.SaveData(TestProjectBuildingDataContainer.DataId, bytes);
        }
    }
}
