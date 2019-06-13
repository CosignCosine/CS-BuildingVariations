using System;
using System.Collections.Generic;
using ICities;
using ColossalFramework.IO;
using System.Linq;
using UnityEngine;
using System.Text;

namespace TestProject
{
    public class TestProjectCustomAssetData : IAssetDataExtension
    {
        private string kKey = "BuildingVariations";

        public void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {
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
                    Debug.LogError("Asset isn't BuildingInfo, aborting");
                    return;
                }

                TestProjectBuildingData.PotentialVariationsMap.Add((asset as BuildingInfo).name, buildingVariations);
            }
        }

        public void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData)
        {
            userData = new Dictionary<string, byte[]>();

            if (TestProjectLoading.buildingVariations == null || TestProjectLoading.buildingVariations.Count == 0) return;
            List<string> s = new List<string>();
            for (int i = 0; i < TestProjectLoading.buildingVariations.Count; i++){
                s.Add(TestProjectLoading.buildingVariations[i].ToSerializableString());
            }
            string s1 = String.Join("|", s.ToArray());
            Debug.Log("[Building Variations] Asset configuration: {" + s1 + "} saved.");
            byte[] bytes = Encoding.ASCII.GetBytes(s1);
            userData.Add(kKey, bytes);
        }

        public void OnCreated(IAssetData assetData)
        {
            Debug.Log("[Building Variations] Asset initiated");
        }

        public void OnReleased()
        {
            Debug.LogError("[Building Variations] Unused");
        }
    }
}
