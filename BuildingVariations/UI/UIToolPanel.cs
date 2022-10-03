using System;
using ColossalFramework.UI;
using UnityEngine;

namespace TestProject.UI
{
    public class UIToolPanel : UIPanel
    {
        string title;
        public string Title
        {
            get
            {
                return title;
            }
            set {
                title = value;
                name = title.Replace(" ", "");
            }

        }

        public UIToolPanel()
        {
            backgroundSprite = "MenuPanel2";

            UILabel pTitle = AddUIComponent<UILabel>();
            pTitle.text = Title;
            pTitle.relativePosition = new Vector2(5f, 5f);

            UIDragHandle pHandle = AddUIComponent<UIDragHandle>();
            pHandle.target = this;
            pHandle.relativePosition = Vector3.zero;
            pHandle.width = width - 10f;
            pHandle.height = 15f;
        }
    }
}
