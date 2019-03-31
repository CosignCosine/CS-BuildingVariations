using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestProject
{
    public class BuildingVariation {
        
        public string m_publicName;

        public List<string> m_enabledSubMeshes;
        public List<string> m_enabledSubBuildings;
        public List<string> m_enabledOtherBuildings;

        public bool IsVariationEnabled(ref Building data){
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
            List<BuildingInfo.MeshInfo> makeVisibleOnly = data.Info.m_subMeshes.Where(r => m_enabledSubMeshes.Contains(r.m_subInfo.name)).ToList();
            List<BuildingInfo.MeshInfo> makeHidden = data.Info.m_subMeshes.Where(r => !m_enabledSubMeshes.Contains(r.m_subInfo.name)).ToList();

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
        }
    }

    public static class TestProjectBuildingData
    {
        public static Dictionary<string, List<BuildingVariation>> NameToVariationDataMap = new Dictionary<string, List<BuildingVariation>>();
        public static bool deserialized = false;

        public static void Deserialize(){
            if (!deserialized)
            {
                deserialized = true;
                BuildingVariation testVariation = new BuildingVariation();
                testVariation.m_publicName = "Orange Cube";
                testVariation.m_enabledSubMeshes = new List<string>();
                testVariation.m_enabledSubMeshes.Add("cube1");

                BuildingVariation testVariation2 = new BuildingVariation();
                testVariation2.m_publicName = "Green and Yellow Cubes";
                testVariation2.m_enabledSubMeshes = new List<string>();
                testVariation2.m_enabledSubMeshes.Add("cube2");
                testVariation2.m_enabledSubMeshes.Add("cube3");

                List<BuildingVariation> r = new List<BuildingVariation>();
                r.Add(testVariation);
                r.Add(testVariation2);
                NameToVariationDataMap.Add("ELEKTRIX TEST ASSET", r);
            }
        }
    }
}
