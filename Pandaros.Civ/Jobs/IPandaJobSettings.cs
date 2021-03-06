﻿using Jobs;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Civ.Jobs
{
    public interface IPandaJobSettings
    {
        Dictionary<IJob, Vector3Int> OriginalPosition { get; set; }

        Dictionary<IJob, INpcGoal> CurrentGoal { get; set; }

        event EventHandler<(INpcGoal, INpcGoal)> GoalChanged;

        void SetGoal(IJob job, INpcGoal npcGoal, ref NPC.NPCBase.NPCState state);
    }
}
