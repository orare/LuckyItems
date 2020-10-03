using System;
using BepInEx;
using R2API;
using RoR2;

namespace OrareItems
{
    [BepInPlugin("mod.orare.orareitems", "A small collection of custom items.", modVersion)]
    [BepInDependency(nameof(R2API.R2API.PluginGUID))]

    public class OrareItems : BaseUnityPlugin
    {
        const string modVersion = "0.0.1";
        public void Awake()
        {

        }
    }
}
