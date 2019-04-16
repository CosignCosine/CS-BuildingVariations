using ICities;
using Harmony;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace TestProject
{
    public class TestProjectLoading : LoadingExtensionBase
    {
        public static UIDropDown m_variationDropdown;

        public static List<BuildingVariation> buildingVariations = new List<BuildingVariation>();
        public static byte currentVariation = 0;

        public static InstanceID Instance {
            get {
                return WorldInfoPanel.GetCurrentInstanceID();
            }
        }

        public static LoadMode m_loadMode = 0;

        public static UIPanel m_variationPanel;

        public override void OnLevelLoaded(LoadMode mode)
        {
            m_loadMode = mode;
            GameObject.FindObjectOfType<ToolController>().eventEditPrefabChanged += UpdateHelperPanel;

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
                string identifierKey = data.Info.name.Replace("_Data", "");

                List<BuildingVariation> buildingVariations = TestProjectBuildingData.PotentialVariationsMap[identifierKey];
                TestProjectBuildingData.IngameBuildingVariationMap[building] = (byte)(value + 1);
                Debug.LogWarning(TestProjectBuildingData.IngameBuildingVariationMap[building]);
            };
        }

        public void UpdateHelperPanel(PrefabInfo info) {
            UpdateHelperPanel(info, false);
        }

        public void UpdateHelperPanel(PrefabInfo info, bool useToolController) {
            Debug.LogWarning("UpdateHelperPanel fired");
            if (useToolController) info = GameObject.FindObjectOfType<ToolController>().m_editPrefabInfo;
            if (info == null) return;
            if (m_variationPanel == null && info is BuildingInfo) CreateVariationHelperPanel();

            Debug.Log("Nothing was null.");

            BuildingInfo infoCopy = info as BuildingInfo;

            UIView.Find<UICheckboxDropDown>("SubmeshDropdown").isVisible = (buildingVariations.Count != 0);
            UIView.Find<UIDropDown>("VariationDropdown").isVisible = (buildingVariations.Count != 0);
            UIView.Find<UIButton>("RemVariation").isVisible = (buildingVariations.Count != 0);

            UIView.Find<UIButton>("AddVariation").isVisible = (buildingVariations.Count != 255);

            List<string> submeshes = new List<string>();
            foreach (BuildingInfo.MeshInfo mesh in infoCopy.m_subMeshes)
            {
                if (mesh.m_subInfo == null) continue;
                submeshes.Add(mesh.m_subInfo.name);
            }

            Debug.Log("Submeshes compiled");

            if(submeshes.ToArray() != UIView.Find<UICheckboxDropDown>("SubmeshDropdown").items){
                UIView.Find<UICheckboxDropDown>("SubmeshDropdown").items = submeshes.ToArray();
                for (int i = 0; i < UIView.Find<UICheckboxDropDown>("SubmeshDropdown").items.Length; i++)
                {
                    if (buildingVariations[currentVariation].m_enabledSubMeshes.Contains(UIView.Find<UICheckboxDropDown>("SubmeshDropdown").items[i]))
                    {
                        UIView.Find<UICheckboxDropDown>("SubmeshDropdown").SetChecked(i, true);
                    }
                }
            }


            Debug.Log("Submeshes added");

            List<string> variations = new List<string>();
            foreach(BuildingVariation variation in buildingVariations){
                variations.Add(variation.m_publicName);
            }
            UIView.Find<UIDropDown>("VariationDropdown").items = variations.ToArray();
            UIView.Find<UIDropDown>("VariationDropdown").selectedIndex = currentVariation;
            UIView.Find<UIButton>("VariationListTriggerButton").text = buildingVariations[currentVariation].m_publicName;

            UIView.Find<UITextField>("VariationName").text = buildingVariations[currentVariation].m_publicName;
            // more code
        }
        public bool CreateVariationHelperPanel()
        {
            if (GameObject.FindObjectOfType<ToolController>().m_editPrefabInfo == null || m_variationPanel != null) return false;
            UIPanel variationPanel = UIView.Find("FullScreenContainer").AddUIComponent<UIPanel>();
            variationPanel.width = 400;
            variationPanel.height = 160;
            variationPanel.backgroundSprite = "MenuPanel2";
            variationPanel.name = "VariationPanel";
            variationPanel.absolutePosition = new Vector3(200f, 250f);

            UISlicedSprite variationPanelSub = variationPanel.AddUIComponent<UISlicedSprite>();
            variationPanelSub.width = 400;
            variationPanelSub.height = 40;
            variationPanelSub.name = "VariationPanelSub";
            variationPanelSub.relativePosition = Vector3.zero;

            UILabel vpL = variationPanelSub.AddUIComponent<UILabel>();
            vpL.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
            vpL.height = 23;
            vpL.text = "Building Variations Helper";
            vpL.name = "VariationPanelLabel";
            vpL.font = UIView.Find<UILabel>("Caption").font;
            vpL.textScale = 1.3f;

            UIDragHandle dragHandle = variationPanel.AddUIComponent<UIDragHandle>();
            dragHandle.width = 400;
            dragHandle.height = 40;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = variationPanel;
            dragHandle.BringToFront();

            // Select Variations
            UISlicedSprite variationSelector = variationPanel.AddUIComponent<UISlicedSprite>();
            variationSelector.width = 400;
            variationSelector.height = 30;
            variationSelector.name = "VariationSelectorBase";
            variationSelector.relativePosition = new Vector3(0, 40);

            UILabel variationLabel = variationSelector.AddUIComponent<UILabel>();
            variationLabel.text = "Variation";
            variationLabel.textColor = new Color32(125, 185, 255, 255);
            variationLabel.relativePosition = new Vector3(10, 12);
            variationLabel.name = "VariationSelectorLabel";

            UIDropDown variationDropdown = variationSelector.AddUIComponent<UIDropDown>();
            variationDropdown.relativePosition = new Vector3(246, 12);
            variationDropdown.name = "VariationDropdown";
            variationDropdown.itemHighlight = "ListItemHighlight";
            variationDropdown.itemHover = "ListItemHover";
            variationDropdown.itemHeight = 25;
            variationDropdown.listBackground = "InfoDisplay";
            variationDropdown.listHeight = 400;
            variationDropdown.listWidth = 300;
            variationDropdown.listPosition = UIDropDown.PopupListPosition.Automatic;
            variationDropdown.normalBgSprite = "TextFieldPanel";
            variationDropdown.size = new Vector2(150, 20);
            variationDropdown.horizontalAlignment = UIHorizontalAlignment.Right;
            //variationDropdown.atlas = UIView.Find<UIDropDown>("Value").atlas;
            variationDropdown.eventSelectedIndexChanged += (component, value) =>
            {
                currentVariation = (byte) value;
                UpdateHelperPanel(null, true);
            };

            UIButton dropdownListTriggerButton = variationDropdown.AddUIComponent<UIButton>();
            dropdownListTriggerButton.normalBgSprite = "TextFieldPanel";
            dropdownListTriggerButton.normalFgSprite = dropdownListTriggerButton.hoveredFgSprite = "IconUpArrow";
            dropdownListTriggerButton.clipChildren = true;
            dropdownListTriggerButton.text = "None";
            dropdownListTriggerButton.textColor = dropdownListTriggerButton.hoveredTextColor = dropdownListTriggerButton.focusedTextColor = dropdownListTriggerButton.pressedTextColor = new Color32(0, 0, 0, 255);
            dropdownListTriggerButton.size = variationDropdown.size;
            dropdownListTriggerButton.relativePosition = Vector3.zero;
            dropdownListTriggerButton.name = "VariationListTriggerButton";
            variationDropdown.triggerButton = dropdownListTriggerButton;

            UIButton addVariationButton = variationSelector.AddUIComponent<UIButton>();
            addVariationButton.height = 20;
            addVariationButton.width = 30;
            addVariationButton.textColor = addVariationButton.hoveredTextColor = addVariationButton.focusedTextColor = addVariationButton.pressedTextColor = new Color32(0, 0, 0, 255);
            addVariationButton.text = "+";
            addVariationButton.relativePosition = new Vector3(160, 12);
            addVariationButton.normalBgSprite = "TextFieldPanel";
            addVariationButton.name = "AddVariation";
            addVariationButton.eventClick += (component, eventParam) =>
            {
                buildingVariations.Add(new BuildingVariation());
                currentVariation = (byte) (buildingVariations.Count - 1);
                UpdateHelperPanel(null, true);
                Debug.Log(buildingVariations[currentVariation]);
            };

            UIButton remVariationButton = variationSelector.AddUIComponent<UIButton>();
            remVariationButton.height = 20;
            remVariationButton.width = 30;
            remVariationButton.textColor = remVariationButton.hoveredTextColor = remVariationButton.focusedTextColor = remVariationButton.pressedTextColor = new Color32(0, 0, 0, 255);
            remVariationButton.text = "-";
            remVariationButton.relativePosition = new Vector3(200, 12);
            remVariationButton.normalBgSprite = "TextFieldPanel";
            remVariationButton.name = "RemVariation";
            remVariationButton.eventClick += (component, eventParam) =>
            {
                buildingVariations.RemoveAt(currentVariation);
                currentVariation--;
                if (currentVariation < 0) currentVariation = 0;
                UpdateHelperPanel(null, true);
            };


            UIButton upArrow1 = variationSelector.AddUIComponent<UIButton>();
            upArrow1.normalFgSprite = "IconUpArrow";
            upArrow1.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            upArrow1.relativePosition = (Vector2)variationDropdown.relativePosition + variationDropdown.size - new Vector2(20, 20);
            upArrow1.size = new Vector2(20, 20);
            upArrow1.eventClick += (component, click) =>
            {
                dropdownListTriggerButton.SimulateClick();
            };
            upArrow1.name = "SubmeshListUpArrowFakeButton";
            upArrow1.atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            upArrow1.BringToFront();



            UISlicedSprite variationName = variationPanel.AddUIComponent<UISlicedSprite>();
            variationName.width = 400;
            variationName.height = 25;
            variationName.name = "VariationNameBase";
            variationName.relativePosition = new Vector3(0, 65);

            UILabel variationNameLabel = variationName.AddUIComponent<UILabel>();
            variationNameLabel.text = "Name";
            variationNameLabel.textColor = new Color32(125, 185, 255, 255);
            variationNameLabel.relativePosition = new Vector3(10, 12);
            variationNameLabel.name = "VariationNameLabel";

            UITextField nameTextField = variationName.AddUIComponent<UITextField>();
            nameTextField.size = new Vector2(150, 20);
            nameTextField.normalBgSprite = "TextFieldPanel";
            nameTextField.relativePosition = new Vector3(246, 12);
            nameTextField.cursorWidth = 1;
            nameTextField.cursorBlinkTime = 0.45f;
            nameTextField.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            nameTextField.maxLength = 35;
            nameTextField.textColor = new Color32(12, 21, 22, 255);
            nameTextField.canFocus = true;
            nameTextField.readOnly = false;
            nameTextField.bottomColor = new Color32(255, 255, 255, 255);
            nameTextField.selectionBackgroundColor = new Color32(0, 105, 210, 255);
            nameTextField.builtinKeyNavigation = true;
            nameTextField.selectionSprite = "EmptySprite";
            nameTextField.name = "VariationName";
            nameTextField.eventTextChanged += (component, text) =>
            {
                buildingVariations[currentVariation].m_publicName = text;
                UpdateHelperPanel(null, true);
                Debug.Log(buildingVariations[currentVariation]);
            };

            // submeshes

            UISlicedSprite submeshSelector = variationPanel.AddUIComponent<UISlicedSprite>();
            submeshSelector.width = 400;
            submeshSelector.height = 25;
            submeshSelector.name = "SubmeshSelectorBase";
            submeshSelector.relativePosition = new Vector3(0, 90);

            UILabel includedSubmeshes = submeshSelector.AddUIComponent<UILabel>();
            includedSubmeshes.text = "Included Submeshes";
            includedSubmeshes.textColor = new Color32(125, 185, 255, 255);
            includedSubmeshes.relativePosition = new Vector3(10, 12);
            includedSubmeshes.name = "SubmeshSelectorLabel";

            UICheckboxDropDown submeshList = submeshSelector.AddUIComponent<UICheckboxDropDown>();
            submeshList.relativePosition = new Vector3(246, 12);
            submeshList.name = "SubmeshDropdown";
            submeshList.checkedSprite = "check-checked";
            submeshList.itemHighlight = "ListItemHighlight";
            submeshList.itemHover = "ListItemHover";
            submeshList.itemHeight = 25;
            submeshList.listBackground = "InfoDisplay";
            submeshList.listHeight = 400;
            submeshList.listWidth = 300;
            submeshList.listPosition = UICheckboxDropDown.PopupListPosition.Automatic;
            submeshList.uncheckedSprite = "check-unchecked";
            submeshList.normalBgSprite = "TextFieldPanel";
            submeshList.size = new Vector2(150, 20);
            submeshList.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            submeshList.horizontalAlignment = UIHorizontalAlignment.Right;
            submeshList.eventCheckedChanged += (component, value) =>
            {
                string submesh = submeshList.items[value];

                if (buildingVariations[currentVariation].m_enabledSubMeshes.Contains(submesh)) {
                    buildingVariations[currentVariation].m_enabledSubMeshes.Remove(submesh);
                } else {
                    buildingVariations[currentVariation].m_enabledSubMeshes.Add(submesh);
                }
            };
            submeshList.eventAfterDropdownClose += (component) =>
            {
                UpdateHelperPanel(null, true);
                Debug.Log(buildingVariations[currentVariation]);
            };

            UIScrollbar submeshListScrollbar = submeshList.AddUIComponent<UIScrollbar>();
            submeshListScrollbar.height = 400;
            submeshListScrollbar.incrementAmount = 1;
            submeshListScrollbar.minValue = 0;
            submeshListScrollbar.maxValue = 100;
            submeshListScrollbar.size = new Vector2(12, 400);
            submeshListScrollbar.orientation = UIOrientation.Vertical;
            submeshListScrollbar.scrollSize = submeshListScrollbar.stepSize = 1;

            UIButton submeshListTriggerButton = submeshList.AddUIComponent<UIButton>();
            submeshListTriggerButton.normalBgSprite = "TextFieldPanel";
            submeshListTriggerButton.normalFgSprite = submeshListTriggerButton.hoveredFgSprite = "IconUpArrow";
            submeshListTriggerButton.clipChildren = true;
            submeshListTriggerButton.text = "None";
            submeshListTriggerButton.textColor = submeshListTriggerButton.hoveredTextColor = submeshListTriggerButton.focusedTextColor = submeshListTriggerButton.pressedTextColor = new Color32(0, 0, 0, 255);
            submeshListTriggerButton.size = submeshList.size;
            submeshListTriggerButton.relativePosition = Vector3.zero;
            submeshListTriggerButton.name = "SubmeshListTriggerButton";

            UIButton upArrow = submeshSelector.AddUIComponent<UIButton>();
            upArrow.normalFgSprite = "IconUpArrow";
            upArrow.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            upArrow.relativePosition = (Vector2)submeshList.relativePosition + submeshList.size - new Vector2(20, 20);
            upArrow.size = new Vector2(20, 20);
            upArrow.eventClick += (component, click) =>
            {
                submeshListTriggerButton.SimulateClick();
            };
            upArrow.name = "SubmeshListUpArrowFakeButton";
            upArrow.atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            upArrow.BringToFront();

            submeshList.listScrollbar = submeshListScrollbar;
            submeshList.triggerButton = submeshListTriggerButton;

            UISlicedSprite setDefault = variationPanel.AddUIComponent<UISlicedSprite>();
            setDefault.width = 400;
            setDefault.height = 25;
            setDefault.name = "SetDefaultBase";
            setDefault.relativePosition = new Vector3(0, 115);

            UILabel setDefaultLabel = setDefault.AddUIComponent<UILabel>();
            setDefaultLabel.text = "Is Default?";
            setDefaultLabel.textColor = new Color32(125, 185, 255, 255);
            setDefaultLabel.relativePosition = new Vector3(10, 12);
            setDefaultLabel.name = "SetDefaultLabel";

            UICheckBox setDefaultCheckbox = setDefault.AddUIComponent<UICheckBox>();
            setDefaultCheckbox.relativePosition = new Vector3(375f, 12f);
            setDefaultCheckbox.size = new Vector2(16, 16);
            setDefaultCheckbox.name = "VariationDefault";
            UISprite sprite = setDefaultCheckbox.AddUIComponent<UISprite>();
            sprite.spriteName = "check-unchecked";
            sprite.atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            sprite.size = setDefaultCheckbox.size;
            sprite.relativePosition = Vector3.zero;
            setDefaultCheckbox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)setDefaultCheckbox.checkedBoxObject).spriteName = "check-checked";
            ((UISprite)setDefaultCheckbox.checkedBoxObject).atlas = UIView.Find<UIDropDown>("CellLength").Find<UIButton>("Button").atlas;
            setDefaultCheckbox.checkedBoxObject.size = setDefaultCheckbox.size;
            setDefaultCheckbox.checkedBoxObject.relativePosition = Vector3.zero;
            setDefaultCheckbox.eventCheckChanged += (component, check) => {
                buildingVariations[currentVariation].m_isDefault = !buildingVariations[currentVariation].m_isDefault;
                UpdateHelperPanel(null, true);
            };

            m_variationPanel = variationPanel;
            return true;
        }

        public override void OnLevelUnloading()
        {
            if (m_variationDropdown != null) m_variationDropdown.parent.RemoveUIComponent(m_variationDropdown);
        }
    }
}
