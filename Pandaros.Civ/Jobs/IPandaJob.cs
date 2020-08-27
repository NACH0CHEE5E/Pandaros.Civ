﻿using Jobs;
using NPC;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLoaderInterfaces;
using Pandaros.API.Extender;
using BlockEntities;

namespace Pandaros.Civ.Jobs
{
    public interface IPandaJob : IJob, IBlockEntityKeepLoaded
    {
        /// <summary>
        ///     Old goal, new goal
        /// </summary>
        event EventHandler<(INpcGoal, INpcGoal)> GoalChanged;
        INpcGoal CurrentGoal { get; }
        string LocalizationKey { get; set; }
        string JobBlock { get; set; }
        Vector3Int Position { get; set; }
        void SetGoal(INpcGoal npcGoal);
        void JobRemoved();
    }
}
