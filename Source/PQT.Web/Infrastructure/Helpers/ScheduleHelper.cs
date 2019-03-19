

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using PQT.Domain;
using PQT.Web.Infrastructure.Utility;
using NS.Mail;
using Quartz;
using Quartz.Impl;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Helpers;
using PQT.Web.Controllers;

namespace PQT.Web.Infrastructure.Helpers
{
    public class ScheduleHelper
    {
        public static void CheckLeadExpireDaily(string jobname, string triggerName)
        {
            try
            {
                //Create the scheduler factory
                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                //Ask the scheduler factory for a scheduler
                IScheduler scheduler = schedulerFactory.GetScheduler();
                //Start the scheduler so that it can start executing jobs
                scheduler.Start();
                // Create a job of Type WriteToConsoleJob
                IJobDetail job = JobBuilder.Create(typeof(CheckLeadExpireDaily)).WithIdentity(jobname).Build();
                //Schedule this job to execute every second, a maximum of 10 times
                ITrigger trigger =
                    TriggerBuilder.Create().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 5)).StartNow().
                        WithIdentity(triggerName).Build();
                scheduler.ScheduleJob(job, trigger);
            }
            catch
            {
            }
        }

    }
    public class CheckLeadExpireDaily : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            LeadHelper.MakeExpiredLead();
        }
    }
}