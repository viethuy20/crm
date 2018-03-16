using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFTrainerService : Repository, ITrainerService
    {
        public EFTrainerService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<Trainer> GetAllTrainers()
        {
            return GetAll<Trainer>().AsEnumerable();
        }

        public Trainer GetTrainer(int id)
        {
            return Get<Trainer>(id);
        }

        public Trainer CreateTrainer(Trainer info)
        {
            return Create(info);
        }

        public bool UpdateTrainerIncludeUpdateCollection(Trainer info)
        {
            return TransactionWrapper.Do(() =>
            {
                var itemExist = Get<Trainer>(info.ID);
                Update(info);
                if (info.TrainerBanks != null && info.TrainerBanks.Any())
                {
                    foreach (var photo in itemExist.TrainerBanks.Where(m => !info.TrainerBanks.Select(n => n.ID).Contains(m.ID)).ToList())
                    {
                        itemExist.TrainerBanks.Remove(photo);
                        Delete(photo);
                    }
                    UpdateCollection(info, m => m.ID == info.ID, m => m.TrainerBanks, m => m.ID);
                }
                else if (itemExist.TrainerBanks != null)
                    foreach (var photo in itemExist.TrainerBanks.ToList())
                    {
                        itemExist.TrainerBanks.Remove(photo);
                        Delete(photo);
                    }
                Update(itemExist);
                return true;
            });
        }

        public bool UpdateTrainer(Trainer info)
        {
            return Update(info);
        }

        public bool DeleteTrainer(int id)
        {
            return Delete<Trainer>(id);
        }
    }
}
