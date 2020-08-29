﻿using Jobs;
using NPC;
using Pandaros.API;
using Pandaros.API.Extender;
using Pandaros.Civ.Storage;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Civ.Jobs.Goals
{
    public class StockpikeToCrateGoal : INpcGoal, IOnTimedUpdate
    {
        public static List<Vector3Int> InProgress { get; set; } = new List<Vector3Int>();
        public static Dictionary<Vector3Int, List<StoredItem>> ItemsNeeded { get; set; } = new Dictionary<Vector3Int, List<StoredItem>>();

        public StockpikeToCrateGoal(IJob job, IPandaJobSettings jobSettings)
        {
            Job = job;
            JobSettings = jobSettings;
            PorterJob = job as PorterJob;
        }

        public PorterJob PorterJob { get; set; }
        public IPandaJobSettings JobSettings { get; set; }
        public IJob Job { get; set; }
        public string Name { get; set; } = nameof(StockpikeToCrateGoal);
        public string LocalizationKey { get; set; } = GameSetup.GetNamespace("Goals", nameof(StockpikeToCrateGoal));
        public Vector3Int CurrentCratePosition { get; set; } = Vector3Int.invalidPos;
        public List<Vector3Int> LastCratePosition { get; set; } = new List<Vector3Int>();
        public StorageType WalkingTo { get; set; } = StorageType.Stockpile;

        public int NextUpdateTimeMinMs => 50;

        public int NextUpdateTimeMaxMs => 70;

        public ServerTimeStamp NextUpdateTime { get; set; }
        public bool CrateFull { get; set; }

        public Vector3Int GetPosition()
        {
            if (CurrentCratePosition == Vector3Int.invalidPos)
                CurrentCratePosition = PorterJob.OriginalPosition;

            if (WalkingTo == StorageType.Crate)
                return CurrentCratePosition;
            else
                return StorageFactory.GetStockpilePosition(Job.Owner).Position;
        }

        public void LeavingGoal()
        {
           
        }

        public void PerformGoal(ref NPCBase.NPCState state)
        {
            if (WalkingTo == StorageType.Stockpile)
            {
                if (CrateFull)
                {
                    Job.NPC.Inventory.Dump(Job.Owner.Stockpile);
                    CrateFull = false;
                }

                var locations = Job.NPC.Position.SortClosestPositions(StorageFactory.CrateLocations[Job.Owner].Keys.ToList());
                var nexPos = Vector3Int.invalidPos;

                foreach (var location in locations)
                    if (!LastCratePosition.Contains(location) &&
                        !InProgress.Contains(location) &&
                        !StorageFactory.CrateLocations[Job.Owner][location].IsAlmostFull &&
                        ItemsNeeded.TryGetValue(location, out var itemsNeeded))
                    {
                        nexPos = location;
                        InProgress.Add(location);
                        WalkingTo = StorageType.Crate;

                        var addToInv = StorageFactory.TryTakeItems(Job.Owner, itemsNeeded);
                        var leftovers = new List<StoredItem>();

                        foreach (var item in addToInv)
                            if (Job.NPC.Inventory.Full)
                                leftovers.Add(item);
                            else
                                Job.NPC.Inventory.Add(item);

                        StorageFactory.StoreItems(Job.Owner, leftovers);
                        state.SetCooldown(5);
                        state.SetIndicator(new Shared.IndicatorState(5, ColonyBuiltIn.ItemTypes.CRATE.Id));

                        break;
                    }

                CurrentCratePosition = nexPos;

                if (nexPos == Vector3Int.invalidPos)
                    JobSettings.SetGoal(Job, new CrateToStockpikeGoal(Job, JobSettings));
            }
            else
            {
                if (StorageFactory.CrateLocations[Job.Owner].TryGetValue(CurrentCratePosition, out var crateInventory))
                {
                    var leftovers = crateInventory.TryAdd(Job.NPC.Inventory.Inventory.Select(ii => new StoredItem(ii, int.MaxValue, StorageType.Crate)).ToArray());

                    if (leftovers.Count > 0)
                    {
                        Job.NPC.Inventory.Inventory.Clear();

                        foreach (var item in leftovers)
                            Job.NPC.Inventory.Add(item);

                        CrateFull = true;
                    }
                }
                else
                {
                    CrateFull = true;
                }
            }

        }

        public void OnTimedUpdate()
        {
            ItemsNeeded.Clear();

            foreach (var crafter in CraftingGoal.CurrentlyCrafing)
            {
                var jobLoc = crafter.Job.GetJobLocation();
                ItemsNeeded[jobLoc] = new List<StoredItem>();

                if (!crafter.CraftingJobInstance.IsCrafting)
                {
                    ItemsNeeded[jobLoc].AddRange(crafter.CraftingJobInstance.SelectedRecipe.Requirements);
                }
                else
                {
                    if (crafter.NextRecipe.MatchType == Recipes.Recipe.RecipeMatchType.FoundMissingRequirements ||
                        crafter.NextRecipe.MatchType == Recipes.Recipe.RecipeMatchType.FoundCraftable)
                        ItemsNeeded[jobLoc].AddRange(crafter.NextRecipe.FoundRecipe.Requirements);
                }
            }

        }
    }
}