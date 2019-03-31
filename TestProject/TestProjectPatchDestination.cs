using Harmony;
using ColossalFramework.UI;
using System.Reflection;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace TestProject
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    public class TestProjectPatchPanelVisibility
    {
        private static void Postfix(CityServiceWorldInfoPanel __instance){
            TestProjectBuildingData.Deserialize();

            InstanceID? instanceID = __instance.GetType().GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as InstanceID?;
            if (instanceID == null) return;
            ushort building = ((InstanceID)instanceID).Building;
            Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building];
            string identifierKey = Regex.Replace(data.Info.name, @".+?\.", "").Replace("_Data", "");
            List<BuildingVariation> buildingVariations = new List<BuildingVariation>();
            try
            {
                buildingVariations = TestProjectBuildingData.NameToVariationDataMap[identifierKey];
            }catch(KeyNotFoundException e){
                Debug.Log("[Building Variations] Selected building has no variations {" + e.Message + "}");
                return;
            }

            if (buildingVariations == null || (buildingVariations != null && buildingVariations.Count == 0)) return;

            UIPanel variationPanel = __instance.GetType().GetField("m_VariationPanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as UIPanel;
            variationPanel.isVisible = true;
            __instance.component.height += 80f;

            UIDropDown variationDropdown = __instance.GetType().GetField("m_VariationDropdown", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as UIDropDown;
            variationDropdown.zOrder = 3;


            List<string> items = new List<string>();
            for (int i = 0; i < buildingVariations.Count; i++){
                items.Add(buildingVariations[i].m_publicName);
                if(buildingVariations[i].IsVariationEnabled(ref data)){
                    variationDropdown.selectedIndex = i;
                }
            }
            variationDropdown.items = items.ToArray();

        }
    }

    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnVariationDropdownChanged")]
    public class TestProjectPatchPanelVariations
    {
        private static bool Prefix(CityServiceWorldInfoPanel __instance, UIComponent component, int value){
            IndustryBuildingAI nullTest = __instance.GetType().GetField("m_IndustryBuildingAI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as IndustryBuildingAI;
            if(nullTest == null){
                UIDropDown variationDropdown = __instance.GetType().GetField("m_VariationDropdown", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as UIDropDown;
                
                InstanceID? instanceID = __instance.GetType().GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as InstanceID?;
                if (instanceID == null) return false;
                ushort building = ((InstanceID)instanceID).Building;
                Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building];
                string identifierKey = Regex.Replace(data.Info.name, @".+?\.", "").Replace("_Data", "");

                List<BuildingVariation> buildingVariations = TestProjectBuildingData.NameToVariationDataMap[identifierKey];
                buildingVariations[value].ApplyVariation(ref data);
                return false;
            }
            return true;
        }
    }
}
