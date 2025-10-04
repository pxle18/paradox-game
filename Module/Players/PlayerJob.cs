﻿using System;
using GTANetworkAPI;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Jobs;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerJob
    {
        public static Random rnd = new Random();
        
        public static Job GetJob(this DbPlayer dbPlayer)
        {
            return JobsModule.Instance.GetJob(dbPlayer.job[0]);
        }

        public static void SetPlayerCurrentJobSkill(this DbPlayer dbPlayer)
        {
            if (dbPlayer.job[0] == 0) return;
            if (dbPlayer.job_skills[0] != "")
            {
                string[] Items = dbPlayer.job_skills[0].Split(',');
                foreach (string item in Items)
                {
                    string[] parts = item.Split(':');
                    if (parts[0] == Convert.ToString(dbPlayer.job[0]))
                    {
                        dbPlayer.JobSkill[0] = Convert.ToInt32(parts[1]);
                        return;
                    }
                }
            }
        }
        
        //Todo: to DbPlayer attribute
        public static void SetJobStatus(this Player player, int status)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.SetData("jobstate", status);
        }
        
        public static void JobSkillsIncrease(this DbPlayer dbPlayer, int multiplier = 1, int pureinc = 0)
        {
            if (dbPlayer.job[0] <= 0) return;
            var rand = rnd.Next(10, 30);

            rand = rand * multiplier;
            if (pureinc > 0)
            {
                rand = pureinc;
            }

            if (dbPlayer.JobSkill[0] >= 5000) return;
            if (dbPlayer.JobSkill[0] + rand < 5000)
            {
                if (dbPlayer.uni_workaholic[0] > 0)
                {
                    var multiplierx = 100 + (dbPlayer.uni_workaholic[0] * 2);
                    rand = Convert.ToInt32((rand * (multiplierx)) / 100);
                }

                dbPlayer.JobSkill[0] += rand;
                var xJob = JobsModule.Instance.GetJob(dbPlayer.job[0]);
                dbPlayer.SendNewNotification(
                    "Skill erhoeht! Beruf: " + xJob.Name);
            }
            else
            {
                dbPlayer.JobSkill[0] = 5000;
                var xJob = JobsModule.Instance.GetJob(dbPlayer.job[0]);
                dbPlayer.SendNewNotification(
                    "Skill erhoeht! Beruf: " + xJob.Name);
            }
        }
    }
}