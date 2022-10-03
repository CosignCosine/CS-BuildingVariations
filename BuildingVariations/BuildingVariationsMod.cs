using System;
using ICities;
using Harmony;
using System.Reflection;

namespace BuildingVariations
{
    public class BuildingVariationsMod : IUserMod
    {
        private readonly string harmonyId = "elektrix.buildingvariations";
        private HarmonyInstance harmony;

        public string Name => "Building Variations 2.0";
        public string Description => "Submeshes can now be configured as a variation on the original building. Updated for the Campus release, fixing MANY bugs.";

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
