﻿using R2API;
using System.Collections.Generic;

namespace RiskyMod.Enemies.Spawnpools
{
    public class DistantRoost
    {
        public static bool enabled = true;
        public DistantRoost()
        {
            if (!enabled) return;

            DirectorAPI.Helpers.RemoveExistingMonsterFromStage(DirectorAPI.Helpers.MonsterNames.Jellyfish, DirectorAPI.Stage.DistantRoost);
            DirectorAPI.Helpers.RemoveExistingMonsterFromStage(DirectorAPI.Helpers.MonsterNames.LesserWisp, DirectorAPI.Stage.DistantRoost);

            DirectorAPI.Helpers.AddNewMonsterToStage(DirectorCards.Jellyfish, false, DirectorAPI.Stage.DistantRoost);

            //TODO: FIGURE OUT HOW TO ONLY RUN THIS WHEN DLC IS ACTIVE
            DirectorAPI.Helpers.AddNewMonsterToStage(DirectorCards.AlphaConstructLoop, false, DirectorAPI.Stage.DistantRoost);

            //Jellyfish are unique to Distant Roost, Wisps are unique to Titanic Plains.
            /*DirectorAPI.MonsterActions += delegate (List<DirectorAPI.DirectorCardHolder> list, DirectorAPI.StageInfo stage)
            {
                if (stage.stage == DirectorAPI.Stage.DistantRoost)
                {
                    List<DirectorAPI.DirectorCardHolder> toRemove = new List<DirectorAPI.DirectorCardHolder>();
                    foreach (DirectorAPI.DirectorCardHolder dch in list)
                    {
                        if (dch.Card.spawnCard == SpawnCards.Jellyfish || dch.Card.spawnCard == SpawnCards.Wisp)
                        {
                            toRemove.Add(dch);
                        }
                    }

                    foreach (DirectorAPI.DirectorCardHolder dch in toRemove)
                    {
                        list.Remove(dch);
                    }

                    //Remove loop jellyfish, let them spawn on stage 1
                    list.Add(DirectorCards.Jellyfish);
                }
            };*/
        }
    }
}
