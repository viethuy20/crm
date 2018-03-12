using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ITrainerService
    {

        IEnumerable<Trainer> GetAllTrainers();
        Trainer GetTrainer(int id);
        Trainer CreateTrainer(Trainer info);
        bool UpdateTrainer(Trainer info);
        bool DeleteTrainer(int id);
    }
}
