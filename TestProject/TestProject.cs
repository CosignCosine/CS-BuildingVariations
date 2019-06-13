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

        public string Name => "Building Variations";
        public string Description => "Submeshes can now be configured as a variation on the original building.";

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
