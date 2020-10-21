using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Racoonogram.Models;

namespace Racoonogram.Services
{
    public class PlanService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public PlanService() { }

        #region Plan

        public double GetPlanPrice(string planId)
        {
            return db.Plans.Where(p => p.Id == planId).Select(p => p.PlanPrice).FirstOrDefault();
        }

        public int GetPlanImages(string planId)
        {
            return db.Plans.Where(p => p.Id == planId).Select(p => p.ImgCount).FirstOrDefault();
        }

        public void HidePlan(int id)
        {
            var plan = db.PlanBuyings.Where(pb => pb.Id == id).Select(pb => pb).FirstOrDefault();
            plan.isHide = 1;
            db.SaveChanges();
        }

        #endregion

        #region PlanBuying

        public PlanBuying GetPlanBuying(string userId)
        {
            return db.PlanBuyings.Where(pb => pb.Id_plan.Contains("s") && pb.Id_user == userId).Select(p => p).FirstOrDefault();
        }

        public PlanBuying GetPlanBuying(string userId, double price)
        {
            return db.PlanBuyings.Where(p => p.Id_user == userId && (p.ImageBalance > 0 || p.MoneyBalance > price)).Select(p => p).FirstOrDefault();
        }

        public int GetPlanRest(string id, double price)
        {
            //нужно добавить миграцию с таблицей planBuyings
            return db.PlanBuyings.Where(p => p.Id_user == id && (p.ImageBalance > 0 || p.MoneyBalance > price)).Select(p => p.Id).Count();
        }

        public void PlanBuyingAdd(PlanBuying buying)
        {
            db.PlanBuyings.Add(buying);
            db.SaveChanges();
        }

        public void PlanBuyingDelete(PlanBuying buying)
        {
            db.PlanBuyings.Remove(buying);
            db.SaveChanges();
        }

        #endregion
    }
}