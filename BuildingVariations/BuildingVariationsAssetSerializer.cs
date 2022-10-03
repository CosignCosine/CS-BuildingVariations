using System;
using System.Collections.Generic;
using ICities;
using ColossalFramework.IO;
using System.Linq;
using UnityEngine;
using System.Text;

namespace BuildingVariations
{
    public class BVCustomAssetDataSerializer : AssetDataExtensionBase
    {
        private string kKey = "BuildingVariations";

        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {
            base.OnAssetLoaded(name, asset, userData);

            // If the data is not building variation data then it should be skipped.
            if(userData.ContainsKey(kKey)){
                byte[] bytes = userData[kKey];
                string s1 = Encoding.ASCII.GetString(bytes);

                Debug.Log("[Building Variations] Asset configuration: {" + s1 + "} loaded.");
                List<string> s = s1.Split('|').ToList();

                List<BuildingVariation> buildingVariations = new List<BuildingVariation>();
                for (int i = 0; i < s.Count; i++){
                    buildingVariations.Add(BuildingVariation.FromSerializedString(s[i]));
                }

                if (!(asset is BuildingInfo))
                {
                    Debug.LogError("[Building Variations] Asset isn't BuildingInfo, aborting");
                    return;
                }

                BVBuildingData.PotentialVariationsMap.Add((asset as BuildingInfo).name, buildingVariations);
            }
        }

        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData)
        {
            base.OnAssetSaved(name, asset, out userData);

            // Create user data because it is required to compile the mod.
            userData = new Dictionary<string, byte[]>();

            if (asset is BuildingInfo)
            {

                // Is there a default variation? No? Set the first variation to be the default variation.
                bool noDefault = false;
                int defaultIndex = 0;
                for (int i = 0; i < BuildingVariationsLoading.buildingVariations.Count; i++)
                {
                    if (BuildingVariationsLoading.buildingVariations[i].m_isDefault)
                    {
                        noDefault = true;
                        defaultIndex = i;
                        break;
                    }
                }
                if (!noDefault && BuildingVariationsLoading.buildingVariations.Count > 0)
                {
                    BuildingVariationsLoading.buildingVariations[0].m_isDefault = true;
                    // no need to set default variation, index is already 0.
                }

                BuildingInfo info = asset as BuildingInfo;
                BuildingInfo.MeshInfo[] subMeshes = info.m_subMeshes;
                for (int i = 0; i < subMeshes.Length; i++)
                {
                    if (!BuildingVariationsLoading.buildingVariations[defaultIndex].m_enabledSubMeshes.Contains(subMeshes[i].m_subInfo.name))
                    {
                        subMeshes[i].m_flagsForbidden |= Building.Flags.Created;
                    }
                }
                (asset as BuildingInfo).m_subMeshes = subMeshes;
            }
            else return; // If we're saving a prop, etc

            

            // If the user did not create any building variation data, it does not need to be loaded.
            if (BuildingVariationsLoading.buildingVariations == null || BuildingVariationsLoading.buildingVariations.Count == 0) return;

            List<string> s = new List<string>();
            for (int i = 0; i < BuildingVariationsLoading.buildingVariations.Count; i++){
                s.Add(BuildingVariationsLoading.buildingVariations[i].ToSerializableString());
            }
            string s1 = String.Join("|", s.ToArray());
            Debug.Log("[Building Variations] Asset configuration: {" + s1 + "} saved.");
            byte[] bytes = Encoding.ASCII.GetBytes(s1);
            userData.Add(kKey, bytes);
        }
    }
}
