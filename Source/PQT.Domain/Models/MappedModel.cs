using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using NS;

namespace PQT.Domain.Models
{

    public class ModelMappingProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            Mapper.Configuration.RecognizePrefixes("Model");

            //Mapper.CreateMap<ShippingItem, ShippingItemHistory>();
        }
    }
}
