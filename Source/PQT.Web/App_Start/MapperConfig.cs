using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PQT.Domain.Models;
using NS;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;
using PQT.Web.Models;

// ReSharper disable once CheckNamespace

namespace PQT.Web
{
    public static class MapperConfig
    {
        public static void RegisterMappers()
        {
            Mapper.CreateMap<Enumeration, int>().ConvertUsing<EnumerationTypeConverter>();
            //Mapper.CreateMap<Enumeration, string>().ConvertUsing<EnumerationStringTypeConverter>();
            Mapper.AddProfile<ModelMappingProfile>();
            Mapper.CreateMap<Company, CompanyJson>();
            Mapper.CreateMap<CompanyJson, Company>();
            Mapper.CreateMap<CompanyResource, CompanyResourceJson>();
            Mapper.CreateMap<CompanyResourceJson, CompanyResource>();
            Mapper.CreateMap<Country, CountryJson>();
            Mapper.CreateMap<CountryJson, Country>();
            Mapper.CreateMap<CompanyResource, Lead>()
                .ForMember(d => d.ID, d => d.MapFrom(s => 0))
                  .ForMember(d => d.JobTitle, d => d.MapFrom(s => s.Role));
            Mapper.CreateMap<Lead, CompanyResource>()
                .ForMember(d => d.ID, d => d.MapFrom(s => 0))
                .ForMember(d => d.Role, d => d.MapFrom(s => s.JobTitle));
        }
    }
}
