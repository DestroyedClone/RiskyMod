﻿using System.Collections.Generic;
using R2API;

namespace RiskyMod.Enemies.Spawnpools
{
    public class SkyMeadow
    {

        public static bool enabled = true;
        public SkyMeadow()
        {
            if (!enabled) return;

            DirectorAPI.MonsterActions += delegate (List<DirectorAPI.DirectorCardHolder> list, DirectorAPI.StageInfo stage)
            {
                if (stage.stage == DirectorAPI.Stage.SkyMeadow)
                {
                    List<DirectorAPI.DirectorCardHolder> toRemove = new List<DirectorAPI.DirectorCardHolder>();
                    foreach (DirectorAPI.DirectorCardHolder dch in list)
                    {
                        if (dch.Card.spawnCard == SpawnCards.Mushrum || dch.Card.spawnCard == SpawnCards.Bronzong)
                        {
                            toRemove.Add(dch);
                        }
                    }

                    foreach (DirectorAPI.DirectorCardHolder dch in toRemove)
                    {
                        list.Remove(dch);
                    }

                    list.Add(DirectorCards.GolemBasic);
                    list.Add(DirectorCards.Imp);
                    list.Add(DirectorCards.LunarGolemSkyMeadow);
                }
            };
        }
    }
}
