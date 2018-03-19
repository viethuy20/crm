using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class TrainerModel
    {
        public Trainer Trainer { get; set; }
        public HttpPostedFileBase Picture { get; set; }
        public bool Create()
        {
            return TransactionWrapper.Do(() =>
            {
                var repo = DependencyHelper.GetService<ITrainerService>();
                if (Picture != null)
                {
                    string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, Picture);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Trainer.Picture = uploadPicture;
                    }
                }
                if (repo.CreateTrainer(Trainer) != null)
                {
                    return true;
                }
                return false;
            });
        }
        public bool Update()
        {
            return TransactionWrapper.Do(() =>
            {
                var repo = DependencyHelper.GetService<ITrainerService>();
                if (Picture != null)
                {
                    string uploadPicture = FileUpload.Upload(FileUploadType.Trainer, Picture);
                    if (!string.IsNullOrEmpty(uploadPicture))
                    {
                        Trainer.Picture = uploadPicture;
                    }
                }
                if (repo.UpdateTrainerIncludeUpdateCollection(Trainer))
                {
                    return true;
                }
                return false;
            });
        }
    }

}