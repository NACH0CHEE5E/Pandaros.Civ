﻿using Pandaros.API;
using Pandaros.API.Models;
using Pandaros.Civ.Storage;
using Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Civ.IronAge.Items
{

    public class StorageChest : CSType, ICrate
    {
        public static string Name { get; private set; } = GameSetup.GetNamespace("Storage", "StorageChest");
        public int MaxCrateStackSize { get; set; } = 300;
        public int MaxNumberOfStacks { get; set; } = 15;

        public StorageChest()
        {
            name = Name;
            sideall = "crate";
            categories = new List<string>()
            {
                "storage"
            };
            isSolid = true;
            icon = GameSetup.Textures.GetPath(TextureType.icon, "crate.png");
            //mesh = GameSetup.MESH_PATH + "crate.obj";
        }
    }

    public class BasicCrateRecipe : ICSPlayerRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 10),
            new RecipeItem(ColonyBuiltIn.ItemTypes.IRONINGOT.Id, 3)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(StorageChest.Name)
        };

        public bool isOptional => false;

        public string name => StorageChest.Name;
    }
}
