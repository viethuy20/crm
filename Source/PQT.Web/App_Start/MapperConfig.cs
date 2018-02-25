﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PQT.Domain.Models;
using NS;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

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
        }
    }
}
