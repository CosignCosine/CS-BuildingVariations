using System;
using ColossalFramework.Packaging;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using ColossalFramework.IO;
using UnityEngine;
using System.Collections.Generic;
using ICities;

namespace BuildingVariations
{

    // adapted from Boformer's 4th modding tutorial 
    public class BuildingVariationsDataContainer : IDataContainer {
        public const string DataId = "BuildingVariations";
        public const int DataVersion = 0;

        public void Serialize(DataSerializer s)
        {
            s.WriteByteArray(BVBuildingData.IngameBuildingVariationMap);
        }

        public void Deserialize(DataSerializer s)
        {
            BVBuildingData.IngameBuildingVariationMap = s.ReadByteArray();
        }

        public void AfterDeserialize(DataSerializer s)
        {
            if (!BuildingManager.exists) return;

            Building[] buildingInstances = BuildingManager.instance.m_buildings.m_buffer;

            for (int i = 0; i < BVBuildingData.IngameBuildingVariationMap.Length; i++)
            {
                if (buildingInstances[i].m_flags == Building.Flags.None)
                {
                    BVBuildingData.IngameBuildingVariationMap[i] = 0;
                    continue;
                }

                BuildingInfo info = buildingInstances[i].Info;
                BuildingInfo.MeshInfo[] subMeshes = info.m_subMeshes;
                for (int y = 0; y < subMeshes.Length; y++)
                {
                    subMeshes[y].m_flagsForbidden &= ~Building.Flags.Created;
                }
            }
        }
    }

    public class BuildingVariationsDataManager : SerializableDataExtensionBase
    {
        private BuildingVariationsDataContainer _data;

        public override void OnLoadData()
        {
            byte[] bytes = serializableDataManager.LoadData(BuildingVariationsDataContainer.DataId);
            if (bytes != null)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    _data = DataSerializer.Deserialize<BuildingVariationsDataContainer>(stream, DataSerializer.Mode.Memory);
                }
            }
            else
            {
                _data = new BuildingVariationsDataContainer();
            }
        }

        public override void OnSaveData()
        {
            byte[] bytes;

            using (var stream = new MemoryStream())
            {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, BuildingVariationsDataContainer.DataVersion, _data);
                bytes = stream.ToArray();
            }

            serializableDataManager.SaveData(BuildingVariationsDataContainer.DataId, bytes);
        }
    }

    // If the mod is loaded, remove all flags that ban submeshes from non-default variations from ever being shown
    public class BuildingSubmeshEnabler : BuildingExtensionBase
    {
        public override void OnBuildingCreated(ushort id)
        {

            Building building = BuildingManager.instance.m_buildings.m_buffer[id];
            BuildingInfo info = building.Info;
            BuildingInfo.MeshInfo[] subMeshes = info.m_subMeshes;
            for (int i = 0; i < subMeshes.Length; i++)
            {

                // removes the created flag from the forbidden flags category
                subMeshes[i].m_flagsForbidden &= ~Building.Flags.Created;
            }

            Debug.Log("Submesh default flags should be disabled. If they aren't, this is a bug.");
        }
    }
}
