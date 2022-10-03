using Harmony;
using ColossalFramework.UI;
using System.Reflection;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/** UI Patches
 * Make growable infopanel display a dropdown
 * Make ploppable infopanel always display a dropdown
 */
namespace BuildingVariations
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    public class BuildingVariationsPatchPloppableDropdownVisibility
    {
        private static void Postfix(CityServiceWorldInfoPanel __instance){
            InstanceID? instanceID = __instance.GetType().GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as InstanceID?;
            if (instanceID == null) return;
            ushort building = ((InstanceID)instanceID).Building;
            Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building];
            string identifierKey = data.Info.name;

            if (!BVBuildingData.PotentialVariationsMap.ContainsKey(identifierKey)) return;
            List<BuildingVariation> buildingVariations = new List<BuildingVariation>();
            buildingVariations = BVBuildingData.PotentialVariationsMap[identifierKey];


            if (buildingVariations.Count == 0) return;

            UIPanel variationPanel = __instance.GetType().GetField("m_VariationPanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as UIPanel;
            variationPanel.isVisible = true;
            __instance.component.height += 80f;

            UIDropDown variationDropdown = __instance.GetType().GetField("m_VariationDropdown", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as UIDropDown;
            variationDropdown.zOrder = 3;


            List<string> items = new List<string>();
            for (int i = 0; i < buildingVariations.Count; i++){
                items.Add(buildingVariations[i].m_publicName);

            }
            variationDropdown.items = items.ToArray();

            variationDropdown.selectedIndex = BVBuildingData.IngameBuildingVariationMap[building] - 1;
            if (variationDropdown.selectedIndex == -1){
                int q = buildingVariations.FindIndex(r => r.m_isDefault);
                variationDropdown.selectedIndex = q == -1 ? 0 : q;
            }
        }
    }

    // Cause the BuildingAI to only render meshes that are enabled as a variation or is enabled by default
    [HarmonyPatch(typeof(BuildingAI), "RenderMeshes")]
    public static class BuildingVariationsPatchSubmeshRendering
    {
        public static bool Prefix(BuildingAI __instance, RenderManager.CameraInfo cameraInfo, ushort buildingID, ref Building data, int layerMask, ref RenderManager.Instance instance)
        {
            __instance.m_info.m_rendered = true;
            if (__instance.m_info.m_mesh != null)
            {
                BuildingAI.RenderMesh(cameraInfo, buildingID, ref data, __instance.m_info, ref instance);
            }
            if (__instance.m_info.m_subMeshes != null)
            {
                for (int i = 0; i < __instance.m_info.m_subMeshes.Length; i++)
                {
                    BuildingInfo.MeshInfo meshInfo = __instance.m_info.m_subMeshes[i];

                    // The magic is here:
                    if (((meshInfo.m_flagsRequired | meshInfo.m_flagsForbidden) & data.m_flags) == meshInfo.m_flagsRequired && BVBuildingData.IsSubmeshEnabled(buildingID, meshInfo))
                    {
                        BuildingInfoSub buildingInfoSub = meshInfo.m_subInfo as BuildingInfoSub;
                        buildingInfoSub.m_rendered = true;
                        if (buildingInfoSub.m_subMeshes != null && buildingInfoSub.m_subMeshes.Length != 0)
                        {
                            for (int j = 0; j < buildingInfoSub.m_subMeshes.Length; j++)
                            {
                                BuildingInfo.MeshInfo meshInfo2 = buildingInfoSub.m_subMeshes[j];
                                if (((meshInfo2.m_flagsRequired | meshInfo2.m_flagsForbidden) & data.m_flags) == meshInfo2.m_flagsRequired)
                                {
                                    BuildingInfoSub buildingInfoSub2 = meshInfo2.m_subInfo as BuildingInfoSub;
                                    buildingInfoSub2.m_rendered = true;
                                    BuildingAI.RenderMesh(cameraInfo, __instance.m_info, buildingInfoSub2, meshInfo.m_matrix, ref instance);
                                }
                            }
                        }
                        else
                        {
                            BuildingAI.RenderMesh(cameraInfo, __instance.m_info, buildingInfoSub, meshInfo.m_matrix, ref instance);
                        }
                    }
                }
            }
            return false;
        }
    }

    // Cause the growable info panel to show a dropdown to set variations.
    [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "OnSetTarget")]
    public static class BuildingVariationsPatchGrowableInfopanel
    {
        public static void Postfix()
        {
            ushort building = BuildingVariationsLoading.Instance.Building;
            Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            string identifierKey = data.Info.name;

            if (BVBuildingData.PotentialVariationsMap.ContainsKey(identifierKey))
            {
                List<BuildingVariation> buildingVariations = BVBuildingData.PotentialVariationsMap[identifierKey];
                BuildingVariationsLoading.m_variationDropdown.isVisible = true;

                List<string> items = new List<string>();
                for (int i = 0; i < buildingVariations.Count; i++)
                {
                    items.Add(buildingVariations[i].m_publicName);

                }
                BuildingVariationsLoading.m_variationDropdown.items = items.ToArray();
                if (BVBuildingData.IngameBuildingVariationMap[building] != 0)
                {
                    BuildingVariationsLoading.m_variationDropdown.selectedIndex = BVBuildingData.IngameBuildingVariationMap[building] - 1;
                }
                else
                {
                    BuildingVariationsLoading.m_variationDropdown.selectedIndex = buildingVariations.FindIndex(r => r.m_isDefault);
                }
            }
            else
            {
                BuildingVariationsLoading.m_variationDropdown.isVisible = false;
            }
        }
    }

    // Cause the ploppable info panel to show a dropdown to set variations, on all assets, and not just Industries DLC fields.
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnVariationDropdownChanged")]
    public class BuildingVariationsPatchPloppableInfopanel
    {
        private static bool Prefix(CityServiceWorldInfoPanel __instance, UIComponent component, int value)
        {
            IndustryBuildingAI nullTest = __instance.GetType().GetField("m_IndustryBuildingAI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as IndustryBuildingAI;
            if (nullTest == null)
            {
                // @TODO What is this used for or necessary for?
                //UIDropDown variationDropdown = __instance.GetType().GetField("m_VariationDropdown", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as UIDropDown;

                InstanceID? instanceID = __instance.GetType().GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as InstanceID?;
                if (instanceID == null) return false;
                ushort building = ((InstanceID)instanceID).Building;
                Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building];
                string identifierKey = data.Info.name;

                List<BuildingVariation> buildingVariations = BVBuildingData.PotentialVariationsMap[identifierKey];
                BVBuildingData.IngameBuildingVariationMap[building] = (byte)(value + 1);
                return false;
            }
            return true;
        }
    }
}
