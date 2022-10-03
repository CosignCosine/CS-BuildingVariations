using ICities;
using Harmony;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Linq;

namespace BuildingVariations
{
    public class BuildingVariationsLoading : LoadingExtensionBase
    {
        public static UIDropDown m_variationDropdown;
        public static UIPanel m_variationPanel;

        public UICheckboxDropDown AE_submeshDropdown;
        public UIDropDown AE_variationDropdown;
        public UITextField AE_nameField;
        public UIButton AE_addVariationButton;
        public UIButton AE_remVariationButton;
        public UICheckBox AE_setDefaultCheckbox;
        public UIPanel AE_variationPanel;

        public static List<BuildingVariation> buildingVariations = new List<BuildingVariation>();
        public int currentVariation = 0;

        public bool m_isMinimum;
        public bool m_isMaximum;

        public static InstanceID Instance {
            get {
                return WorldInfoPanel.GetCurrentInstanceID();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            GameObject.FindObjectOfType<ToolController>().eventEditPrefabChanged += (info) => {
                if (!(info is BuildingInfo)) return;
                BuildingInfo quickInfo = info as BuildingInfo;
                string[] keys = BVBuildingData.PotentialVariationsMap.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++){
                    string[] testStrings = keys[i].Split('.');
                    if (testStrings.Length < 2) Debug.LogWarning("[BuildingVariations] Malformed object name, aborting (at " + keys[i] + ")");
                    if(keys[i].Split('.')[1] == quickInfo.name){
                        buildingVariations = BVBuildingData.PotentialVariationsMap[keys[i]];
                    }
                }
                UpdateHelperPanel(info);
            };

            if (UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name) == null) return;
            m_variationDropdown = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name).component.AddUIComponent<UIDropDown>();
            m_variationDropdown.name = "VariationDropdownZoned";
            m_variationDropdown.normalBgSprite = "OptionsDropbox";
            m_variationDropdown.hoveredBgSprite = "OptionsDropboxHovered";
            m_variationDropdown.focusedBgSprite = "OptionsDropboxFocused";
            m_variationDropdown.listBackground = "OptionsDropboxListbox";
            m_variationDropdown.listHeight = 200;
            m_variationDropdown.itemHeight = 24;
            m_variationDropdown.itemHover = "ListItemHover";
            m_variationDropdown.itemHighlight = "ListItemHighlight";
            m_variationDropdown.itemPadding = new RectOffset(14, 14, 0, 0);
            m_variationDropdown.isVisible = false;
            m_variationDropdown.clipChildren = true;
            m_variationDropdown.height = 25f;
            m_variationDropdown.width = 200f;
            m_variationDropdown.relativePosition = new Vector3(260f, m_variationDropdown.parent.height - 50f);
            m_variationDropdown.textScale = 0.8f;
            m_variationDropdown.listPosition = UIDropDown.PopupListPosition.Automatic;
            m_variationDropdown.listPadding = new RectOffset(4, 4, 4, 4);
            m_variationDropdown.textFieldPadding = new RectOffset(8, 0, 8, 0);
            m_variationDropdown.triggerButton = m_variationDropdown;
            m_variationDropdown.tooltip = "Select Building Variations";
            m_variationDropdown.listScrollbar = new UIScrollbar();

            m_variationDropdown.eventSelectedIndexChanged += (component, value) => {
                ushort building = Instance.Building;
                Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
                BVBuildingData.IngameBuildingVariationMap[building] = (byte)(value + 1);
            };
        }

        public string[] GetVariationNamesFrom(List<BuildingVariation> variations){
            List<string> r = new List<string>();
            foreach (BuildingVariation variation in variations)
            {
                r.Add(variation.m_publicName);
            }
            return r.ToArray();
        }

        // quick overload
        public void UpdateHelperPanel(PrefabInfo info) {
            UpdateHelperPanel(info, false);
        }

        public void UpdateHelperPanel(PrefabInfo info, bool useToolController) {
            Debug.Log("[Building Variations] UpdateHelperPanel fired");
            if (useToolController) info = GameObject.FindObjectOfType<ToolController>().m_editPrefabInfo;
            if (info == null) return;

            BuildingInfo infoCopy = info as BuildingInfo;

            int defaultIndex = 0;
            for (int i = 0; i < buildingVariations.Count; i++)
            {
                if (buildingVariations[i].m_isDefault)
                {
                    defaultIndex = i;
                    break;
                }
            }

            BuildingInfo.MeshInfo[] subMeshes = infoCopy.m_subMeshes;
            for (int i = 0; i < subMeshes.Length; i++)
            {
                if (!buildingVariations[defaultIndex].m_enabledSubMeshes.Contains(subMeshes[i].m_subInfo.name))
                {
                    subMeshes[i].m_flagsForbidden |= Building.Flags.Created;
                }else {
                    subMeshes[i].m_flagsForbidden &= ~Building.Flags.Created;
                }
            }

            if (m_variationPanel == null && info is BuildingInfo) CreateVariationHelperPanel();



            m_isMinimum = buildingVariations.Count == 0;
            m_isMaximum = buildingVariations.Count == 255;


            AE_submeshDropdown.isVisible = !m_isMinimum;
            AE_variationDropdown.isVisible = !m_isMinimum;
            AE_remVariationButton.isVisible = !m_isMinimum;
            AE_addVariationButton.isVisible = !m_isMaximum;
            AE_setDefaultCheckbox.isVisible = !m_isMinimum;
            AE_nameField.isVisible = !m_isMinimum;

            if (m_isMinimum) return;

            List<string> submeshNames = new List<string>();
            int c = 0;
            foreach (BuildingInfo.MeshInfo mesh in infoCopy.m_subMeshes)
            {
                if (mesh.m_subInfo == null) continue;
                submeshNames.Add(mesh.m_subInfo.name);
                c++;
            }

            string[] submeshes = submeshNames.ToArray();

            bool fireVariations = submeshes.Length != 0;

            //AE_variationPanel.isVisible = fireVariations;

            if(submeshes != AE_submeshDropdown.items){
                AE_submeshDropdown.items = submeshes;
                for (int i = 0; i < AE_submeshDropdown.items.Length; i++)
                {
                    AE_submeshDropdown.SetChecked(i, buildingVariations[currentVariation].m_enabledSubMeshes.Contains(AE_submeshDropdown.items[i]));
                }
            }
            (AE_submeshDropdown.triggerButton as UIButton).text = buildingVariations[currentVariation].m_enabledSubMeshes.Join();


            AE_variationDropdown.items = GetVariationNamesFrom(buildingVariations);


            (AE_variationDropdown.triggerButton as UIButton).text = buildingVariations[currentVariation].m_publicName;

            AE_nameField.text = buildingVariations[currentVariation].m_publicName;
            AE_setDefaultCheckbox.isChecked = buildingVariations[currentVariation].m_isDefault;
        }
        public bool CreateVariationHelperPanel()
        {
            if (GameObject.FindObjectOfType<ToolController>().m_editPrefabInfo == null || m_variationPanel != null) return false;
            AE_variationPanel = UIView.Find("FullScreenContainer").AddUIComponent<UIPanel>();
            AE_variationPanel.width = 400;
            AE_variationPanel.height = 160;
            AE_variationPanel.backgroundSprite = "MenuPanel2";
            AE_variationPanel.name = "VariationPanel";
            AE_variationPanel.absolutePosition = new Vector3(200f, 250f);

            UISlicedSprite variationPanelSub = AE_variationPanel.AddUIComponent<UISlicedSprite>();
            variationPanelSub.width = 400;
            variationPanelSub.height = 40;
            variationPanelSub.name = "VariationPanelSub";
            variationPanelSub.relativePosition = Vector3.zero;

            UILabel vpL = variationPanelSub.AddUIComponent<UILabel>();
            vpL.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
            vpL.height = 23;
            vpL.text = "Building Variations Helper";
            vpL.name = "VariationPanelLabel";
            //vpL.font = UIView.Find<UILabel>("Caption").font;
            vpL.textScale = 1.3f;

            UIDragHandle dragHandle = AE_variationPanel.AddUIComponent<UIDragHandle>();
            dragHandle.width = 400;
            dragHandle.height = 40;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = AE_variationPanel;
            dragHandle.BringToFront();

            // Select Variations
            UISlicedSprite variationSelector = AE_variationPanel.AddUIComponent<UISlicedSprite>();
            variationSelector.width = 400;
            variationSelector.height = 30;
            variationSelector.name = "VariationSelectorBase";
            variationSelector.relativePosition = new Vector3(0, 40);

            UILabel variationLabel = variationSelector.AddUIComponent<UILabel>();
            variationLabel.text = "Variation";
            variationLabel.textColor = new Color32(125, 185, 255, 255);
            variationLabel.relativePosition = new Vector3(10, 12);
            variationLabel.name = "VariationSelectorLabel";

            AE_variationDropdown = variationSelector.AddUIComponent<UIDropDown>();
            AE_variationDropdown.relativePosition = new Vector3(246, 12);
            AE_variationDropdown.name = "VariationDropdown";
            AE_variationDropdown.itemHighlight = "ListItemHighlight";
            AE_variationDropdown.itemHover = "ListItemHover";
            AE_variationDropdown.itemHeight = 25;
            AE_variationDropdown.listBackground = "InfoDisplay";
            AE_variationDropdown.listHeight = 400;
            AE_variationDropdown.listWidth = 300;
            AE_variationDropdown.listPosition = UIDropDown.PopupListPosition.Automatic;
            AE_variationDropdown.normalBgSprite = "TextFieldPanel";
            AE_variationDropdown.size = new Vector2(150, 20);
            AE_variationDropdown.atlas = UIView.GetAView().defaultAtlas;
            AE_variationDropdown.horizontalAlignment = UIHorizontalAlignment.Right;
            AE_variationDropdown.eventSelectedIndexChanged += (component, value) =>
            {
                if(AE_variationDropdown.selectedIndex != currentVariation){
                    currentVariation = value;
                    UpdateHelperPanel(null, true);
                }
            };

            UIButton dropdownListTriggerButton = AE_variationDropdown.AddUIComponent<UIButton>();
            dropdownListTriggerButton.normalBgSprite = "TextFieldPanel";
            dropdownListTriggerButton.normalFgSprite = dropdownListTriggerButton.hoveredFgSprite = "IconUpArrow";
            dropdownListTriggerButton.clipChildren = true;
            dropdownListTriggerButton.text = "None";
            dropdownListTriggerButton.textColor = new Color32(0, 0, 0, 255);
            dropdownListTriggerButton.hoveredTextColor = dropdownListTriggerButton.focusedTextColor = dropdownListTriggerButton.pressedTextColor = new Color32(28, 50, 52, 255);
            dropdownListTriggerButton.hoveredColor = new Color32(200, 200, 200, 255);
            dropdownListTriggerButton.size = AE_variationDropdown.size;
            dropdownListTriggerButton.relativePosition = Vector3.zero;
            dropdownListTriggerButton.name = "VariationListTriggerButton";
            AE_variationDropdown.triggerButton = dropdownListTriggerButton;

            AE_addVariationButton = variationSelector.AddUIComponent<UIButton>();
            AE_addVariationButton.height = 20;
            AE_addVariationButton.width = 30;
            AE_addVariationButton.textColor = new Color32(0, 0, 0, 255);
            AE_addVariationButton.hoveredColor = new Color32(200, 200, 200, 255);
            AE_addVariationButton.hoveredTextColor = AE_addVariationButton.focusedTextColor = AE_addVariationButton.pressedTextColor = new Color32(28, 50, 52, 255);
            AE_addVariationButton.text = "+";
            AE_addVariationButton.relativePosition = new Vector3(160, 12);
            AE_addVariationButton.normalBgSprite = "TextFieldPanel";
            AE_addVariationButton.name = "AddVariation";
            AE_addVariationButton.eventClick += (component, eventParam) =>
            {
                buildingVariations.Add(new BuildingVariation());
                currentVariation = (buildingVariations.Count - 1);
                UpdateHelperPanel(null, true);
            };

            AE_remVariationButton = variationSelector.AddUIComponent<UIButton>();
            AE_remVariationButton.height = 20;
            AE_remVariationButton.width = 30;
            AE_remVariationButton.textColor = new Color32(0, 0, 0, 255);
            AE_remVariationButton.hoveredColor = new Color32(200, 200, 200, 255);
            AE_remVariationButton.hoveredTextColor = AE_remVariationButton.focusedTextColor = AE_remVariationButton.pressedTextColor = new Color32(28, 50, 52, 255);
            AE_remVariationButton.text = "-";
            AE_remVariationButton.relativePosition = new Vector3(200, 12);
            AE_remVariationButton.normalBgSprite = "TextFieldPanel";
            AE_remVariationButton.name = "RemVariation";
            AE_remVariationButton.eventClick += (component, eventParam) =>
            {
                buildingVariations.RemoveAt(currentVariation);
                currentVariation--;
                if (currentVariation < 0) currentVariation = 0;
                UpdateHelperPanel(null, true);
            };


            UIButton upArrow1 = variationSelector.AddUIComponent<UIButton>();
            upArrow1.normalFgSprite = "IconUpArrow";
            upArrow1.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            upArrow1.relativePosition = (Vector2)AE_variationDropdown.relativePosition + AE_variationDropdown.size - new Vector2(20, 20);
            upArrow1.size = new Vector2(20, 20);
            upArrow1.eventClick += (component, click) =>
            {
                dropdownListTriggerButton.SimulateClick();
            };
            upArrow1.name = "SubmeshListUpArrowFakeButton";
            upArrow1.atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            upArrow1.BringToFront();



            UISlicedSprite variationName = AE_variationPanel.AddUIComponent<UISlicedSprite>();
            variationName.width = 400;
            variationName.height = 25;
            variationName.name = "VariationNameBase";
            variationName.relativePosition = new Vector3(0, 65);

            UILabel variationNameLabel = variationName.AddUIComponent<UILabel>();
            variationNameLabel.text = "Name";
            variationNameLabel.textColor = new Color32(125, 185, 255, 255);
            variationNameLabel.relativePosition = new Vector3(10, 12);
            variationNameLabel.name = "VariationNameLabel";

            AE_nameField = variationName.AddUIComponent<UITextField>();
            AE_nameField.size = new Vector2(150, 20);
            AE_nameField.normalBgSprite = "TextFieldPanel";
            AE_nameField.relativePosition = new Vector3(246, 12);
            AE_nameField.cursorWidth = 1;
            AE_nameField.cursorBlinkTime = 0.45f;
            AE_nameField.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            AE_nameField.maxLength = 35;
            AE_nameField.textColor = new Color32(12, 21, 22, 255);
            AE_nameField.canFocus = true;
            AE_nameField.readOnly = false;
            AE_nameField.bottomColor = new Color32(255, 255, 255, 255);
            AE_nameField.selectionBackgroundColor = new Color32(0, 105, 210, 255);
            AE_nameField.builtinKeyNavigation = true;
            AE_nameField.selectionSprite = "EmptySprite";
            AE_nameField.name = "VariationName";
            AE_nameField.eventMouseEnter += (component, ms) =>
            {
                AE_nameField.color = new Color32(200, 200, 200, 255);
            };
            AE_nameField.eventMouseLeave += (component, ms) =>
            {
                AE_nameField.color = new Color32(255, 255, 255, 255);
            };

            AE_nameField.eventTextChanged += (component, text) =>
            {
                buildingVariations[currentVariation].m_publicName = text;
                UpdateHelperPanel(null, true);
            };

            // submeshes

            UISlicedSprite submeshSelector = AE_variationPanel.AddUIComponent<UISlicedSprite>();
            submeshSelector.width = 400;
            submeshSelector.height = 25;
            submeshSelector.name = "SubmeshSelectorBase";
            submeshSelector.relativePosition = new Vector3(0, 90);

            UILabel includedSubmeshes = submeshSelector.AddUIComponent<UILabel>();
            includedSubmeshes.text = "Included Submeshes";
            includedSubmeshes.textColor = new Color32(125, 185, 255, 255);
            includedSubmeshes.relativePosition = new Vector3(10, 12);
            includedSubmeshes.name = "SubmeshSelectorLabel";

            AE_submeshDropdown = submeshSelector.AddUIComponent<UICheckboxDropDown>();
            AE_submeshDropdown.relativePosition = new Vector3(246, 12);
            AE_submeshDropdown.name = "SubmeshDropdown";
            AE_submeshDropdown.checkedSprite = "check-checked";
            AE_submeshDropdown.itemHighlight = "ListItemHighlight";
            AE_submeshDropdown.itemHover = "ListItemHover";
            AE_submeshDropdown.itemHeight = 25;
            AE_submeshDropdown.listBackground = "InfoDisplay";
            AE_submeshDropdown.listHeight = 400;
            AE_submeshDropdown.listWidth = 300;
            AE_submeshDropdown.listPosition = UICheckboxDropDown.PopupListPosition.Automatic;
            AE_submeshDropdown.uncheckedSprite = "check-unchecked";
            AE_submeshDropdown.normalBgSprite = "TextFieldPanel";
            AE_submeshDropdown.size = new Vector2(150, 20);
            AE_submeshDropdown.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            AE_submeshDropdown.horizontalAlignment = UIHorizontalAlignment.Right;
            AE_submeshDropdown.atlas = UIView.GetAView().defaultAtlas;
            AE_submeshDropdown.eventAfterDropdownClose += (component) =>
            {
                if (AE_submeshDropdown.items.Length == 0) return;

                for (int i = 0; i < AE_submeshDropdown.items.Length; i++){
                    string submesh = AE_submeshDropdown.items[i];
                    if(AE_submeshDropdown.GetChecked(i)){
                        if(!buildingVariations[currentVariation].m_enabledSubMeshes.Contains(submesh)) buildingVariations[currentVariation].m_enabledSubMeshes.Add(submesh);
                    }else{
                        buildingVariations[currentVariation].m_enabledSubMeshes.Remove(submesh);
                    }
                }
                UpdateHelperPanel(null, true);

            };

            /*
            UIScrollbar submeshListScrollbar = AE_submeshDropdown.AddUIComponent<UIScrollbar>();
            submeshListScrollbar.height = 400;
            submeshListScrollbar.incrementAmount = 1;
            submeshListScrollbar.minValue = 0;
            submeshListScrollbar.maxValue = 100;
            submeshListScrollbar.size = new Vector2(12, 400);
            submeshListScrollbar.orientation = UIOrientation.Vertical;
            submeshListScrollbar.scrollSize = submeshListScrollbar.stepSize = 1;*/

            UIButton submeshListTriggerButton = AE_submeshDropdown.AddUIComponent<UIButton>();
            submeshListTriggerButton.normalBgSprite = "TextFieldPanel";
            submeshListTriggerButton.normalFgSprite = submeshListTriggerButton.hoveredFgSprite = "IconUpArrow";
            submeshListTriggerButton.clipChildren = true;
            submeshListTriggerButton.text = "None";
            submeshListTriggerButton.textColor = submeshListTriggerButton.hoveredTextColor = submeshListTriggerButton.focusedTextColor = submeshListTriggerButton.pressedTextColor = new Color32(0, 0, 0, 255);
            submeshListTriggerButton.size = AE_submeshDropdown.size;
            submeshListTriggerButton.relativePosition = Vector3.zero;
            submeshListTriggerButton.name = "SubmeshListTriggerButton";
            submeshListTriggerButton.hoveredColor = new Color32(200, 200, 200, 255);

            UIButton upArrow = submeshSelector.AddUIComponent<UIButton>();
            upArrow.normalFgSprite = "IconUpArrow";
            upArrow.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            upArrow.relativePosition = (Vector2)AE_submeshDropdown.relativePosition + AE_submeshDropdown.size - new Vector2(20, 20);
            upArrow.size = new Vector2(20, 20);
            upArrow.eventClick += (component, click) =>
            {
                submeshListTriggerButton.SimulateClick();
            };
            upArrow.name = "SubmeshListUpArrowFakeButton";
            upArrow.atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            upArrow.BringToFront();

            AE_submeshDropdown.listScrollbar = UIView.Find("Scrollbar") as UIScrollbar;
            AE_submeshDropdown.triggerButton = submeshListTriggerButton;

            UISlicedSprite setDefault = AE_variationPanel.AddUIComponent<UISlicedSprite>();
            setDefault.width = 400;
            setDefault.height = 25;
            setDefault.name = "SetDefaultBase";
            setDefault.relativePosition = new Vector3(0, 115);

            UILabel setDefaultLabel = setDefault.AddUIComponent<UILabel>();
            setDefaultLabel.text = "Is Default?";
            setDefaultLabel.textColor = new Color32(125, 185, 255, 255);
            setDefaultLabel.relativePosition = new Vector3(10, 12);
            setDefaultLabel.name = "SetDefaultLabel";

            AE_setDefaultCheckbox = setDefault.AddUIComponent<UICheckBox>();
            AE_setDefaultCheckbox.relativePosition = new Vector3(375f, 12f);
            AE_setDefaultCheckbox.size = new Vector2(16, 16);
            AE_setDefaultCheckbox.name = "VariationDefault";
            UISprite sprite = AE_setDefaultCheckbox.AddUIComponent<UISprite>();
            sprite.spriteName = "check-unchecked";
            sprite.atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            sprite.size = AE_setDefaultCheckbox.size;
            sprite.relativePosition = Vector3.zero;
            AE_setDefaultCheckbox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)AE_setDefaultCheckbox.checkedBoxObject).spriteName = "check-checked";
            ((UISprite)AE_setDefaultCheckbox.checkedBoxObject).atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            AE_setDefaultCheckbox.checkedBoxObject.size = AE_setDefaultCheckbox.size;
            AE_setDefaultCheckbox.checkedBoxObject.relativePosition = Vector3.zero;
            AE_setDefaultCheckbox.eventCheckChanged += (component, check) => {
                buildingVariations[currentVariation].m_isDefault = check;
                UpdateHelperPanel(null, true);
            };

            m_variationPanel = AE_variationPanel;

            Debug.Log("[Building Variations] Created asset editor variation panel.");
            return true;
        }

        public override void OnLevelUnloading()
        {
            // Asset editor UI unloading
            if (m_variationDropdown != null) m_variationDropdown.parent.RemoveUIComponent(m_variationDropdown);
            AE_nameField?.parent.RemoveUIComponent(AE_nameField);
            AE_submeshDropdown?.parent.RemoveUIComponent(AE_submeshDropdown);
            AE_variationDropdown?.parent.RemoveUIComponent(AE_variationDropdown);

            // Ingame potential variations unloader
            BVBuildingData.PotentialVariationsMap.Clear();
        }
    }
}
