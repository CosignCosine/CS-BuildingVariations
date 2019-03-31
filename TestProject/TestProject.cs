using System;
using ICities;
using Harmony;
using System.Reflection;

namespace TestProject
{
    public class TestProject : IUserMod
    {
        private readonly string harmonyId = "elektrix.buildingvariations";
        private HarmonyInstance harmony;

        public string Name => "! Elektrix's Test Mod";
        public string Description => "Where elektrix tests his electric things. [in a purely platonic manner]";

        public void OnEnabled(){
            harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnDisabled(){
            harmony.UnpatchAll(harmonyId);
            harmony = null;
        }
    }
}
