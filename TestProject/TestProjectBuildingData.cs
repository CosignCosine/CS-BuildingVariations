using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Elektrix.Utilities.Extensions;
using System.Reflection;
using ICities;

namespace TestProject
{
    public class BuildingVariation {
        
        public string m_publicName;
        public bool m_isDefault;

        public List<string> m_enabledSubMeshes;

        public BuildingVariation(){
            m_publicName = "Public Name";
            m_isDefault = false;
            m_enabledSubMeshes = new List<string>();
        }

        public override string ToString()
        {
            return m_publicName + (m_isDefault ? "[default] " : " ") + m_enabledSubMeshes.Count + " enabled submeshes";
        }

        public string ToSerializableString(){
            string submeshNames = "";
            for (int i = 0; i < m_enabledSubMeshes.Count; i++){
                submeshNames += m_enabledSubMeshes[i] + ((i == m_enabledSubMeshes.Count - 1) ? "," : "");
            }
            return m_publicName + ":" + m_isDefault + ":" + submeshNames;
        }

        public static BuildingVariation FromSerializedString(string s){
            BuildingVariation b = new BuildingVariation();
            List<string> strings = s.Split(':').ToList();
            b.m_publicName = strings[0];
            Debug.LogError("[Building Variations] " + strings[1]);
            b.m_isDefault = strings[1].ToLower() == "true";
            b.m_enabledSubMeshes = strings[2].Split(',').ToList();
            return b;
        }

        /*public bool IsVariationEnabled(ref Building data){
            List<BuildingInfo.MeshInfo> makeVisibleOnly = data.Info.m_subMeshes.Where(r => m_enabledSubMeshes.Contains(r.m_subInfo.name)).ToList();
            List<BuildingInfo.MeshInfo> makeHidden = data.Info.m_subMeshes.Where(r => !m_enabledSubMeshes.Contains(r.m_subInfo.name)).ToList();

            for (int i = 0; i < makeHidden.Count; i++){
                BuildingInfo.MeshInfo info = makeHidden[i];

                if((info.m_flagsForbidden & Building.Flags.Created) == Building.Flags.None){
                    return false;
                }
            }

            for (int i = 0; i < makeVisibleOnly.Count; i++)
            {
                BuildingInfo.MeshInfo info = makeVisibleOnly[i];

                if ((info.m_flagsForbidden & Building.Flags.Created) != Building.Flags.None)
                {
                    return false;
                }
            }
            return true;
        }

        public void ApplyVariation(ref Building data){
            BuildingInfo newInfo = data.Info.Copy();

            newInfo.name += "_variation";

            List<BuildingInfo.MeshInfo> makeVisibleOnly = newInfo.m_subMeshes.Where(r => m_enabledSubMeshes.Contains(r.m_subInfo.name)).ToList();
            List<BuildingInfo.MeshInfo> makeHidden = newInfo.m_subMeshes.Where(r => !m_enabledSubMeshes.Contains(r.m_subInfo.name)).ToList();

            for (int i = 0; i < makeHidden.Count; i++){
                BuildingInfo.MeshInfo info = makeHidden[i];
                if (info != null && info.m_subInfo != null && (info.m_flagsForbidden & Building.Flags.Created) == Building.Flags.None){
                    info.m_flagsForbidden |= Building.Flags.Created;
                } 
            }

            for (int i = 0; i < makeVisibleOnly.Count; i++)
            {
                BuildingInfo.MeshInfo info = makeVisibleOnly[i];
                if (info != null && info.m_subInfo != null && (info.m_flagsForbidden & Building.Flags.Created) != Building.Flags.None){
                    info.m_flagsForbidden &= ~Building.Flags.Created;
                } 
            }


        }*/
    }

    public static class TestProjectBuildingData
    {
        public static Dictionary<string, List<BuildingVariation>> PotentialVariationsMap = new Dictionary<string, List<BuildingVariation>>();
        public static byte[] IngameBuildingVariationMap = new byte[BuildingManager.MAX_BUILDING_COUNT];

        public static bool IsSubmeshEnabled(ushort building, BuildingInfo.MeshInfo submesh){
            string key = BuildingManager.instance.m_buildings.m_buffer[building].Info.name;
            if (!PotentialVariationsMap.ContainsKey(key)) return true;
            if(IngameBuildingVariationMap[building] == 0){
                BuildingVariation[] defaultVariations = PotentialVariationsMap[key].Where(i => i.m_isDefault).ToArray();
                if (defaultVariations == null || defaultVariations.Length == 0) return false;
                BuildingVariation defaultVariation = defaultVariations[0];
                if (defaultVariation == null) return false;
                if (defaultVariation.m_enabledSubMeshes == null || defaultVariation.m_enabledSubMeshes.Count == 0) return false;
                return defaultVariation.m_enabledSubMeshes.Contains(submesh.m_subInfo.name);
            }

            if (PotentialVariationsMap[key][IngameBuildingVariationMap[building] - 1] == null) return false;
            if (PotentialVariationsMap[key][IngameBuildingVariationMap[building] - 1].m_enabledSubMeshes == null || PotentialVariationsMap[key][IngameBuildingVariationMap[building] - 1].m_enabledSubMeshes.Count == 0) return false;
            return PotentialVariationsMap[key][IngameBuildingVariationMap[building] - 1].m_enabledSubMeshes.Contains(submesh.m_subInfo.name);
        }
    }

    public class TestProjectBuildingMonitor : BuildingExtensionBase
    {
        public override void OnBuildingReleased(ushort id)
        {
            // reset to default
            TestProjectBuildingData.IngameBuildingVariationMap[id] = 0;
        }
    }
}
