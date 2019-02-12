﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IUnitRepository
    {

        #region Country
        IEnumerable<Country> GetAllCountries();
        Country GetCountry(int countryID);
        Country GetCountry(string code);
        Country CreateCountry(Country info);
        bool UpdateCountry(Country info);
        bool DeleteCountry(int countryID);
        #endregion Country

        #region Counter
        int GetCounter(string name);
        Counter SetCounter(string name, int value);
        IEnumerable<Counter> GetAllCounter();
        Counter GetCounter(int id);
        bool UpdateCounter(Counter counter);
        decimal GetDepositNextCounter(string name, int start);

        #endregion Counter

        #region Email Template setting

        EmailSetting GetEmailSetting(string type, string nameTemplate);
        bool CreateEmailSetting(EmailSetting email);
        bool DeleteEmailAllInTemplate(string type, string nameTemplate);
        IEnumerable<object> GetSelectListItemOfEmailTemplate(string type, string nameTemplate, EmailType emailType);

        #endregion

        #region OfficeLocation
        IEnumerable<OfficeLocation> GetAllOfficeLocations();
        OfficeLocation GetOfficeLocation(int id);
        OfficeLocation GetOfficeLocation(string name);
        OfficeLocation CreateOfficeLocation(OfficeLocation info);
        bool UpdateOfficeLocation(OfficeLocation holiday);
        bool DeleteOfficeLocation(int id);
        #endregion OfficeLocation

        #region RecruitmentPosition
        IEnumerable<RecruitmentPosition> GetAllRecruitmentPositions();
        RecruitmentPosition GetRecruitmentPosition(int id);
        RecruitmentPosition CreateRecruitmentPosition(RecruitmentPosition info);
        bool UpdateRecruitmentPosition(RecruitmentPosition holiday);
        bool DeleteRecruitmentPosition(int id);
        #endregion RecruitmentPosition



        #region VenueInfo
        VenueInfo GetVenueInfo(int id);
        bool UpdateVenueInfo(VenueInfo holiday);
        #endregion VenueInfo

        #region AccomodationInfo
        AccomodationInfo GetAccomodationInfo(int id);
        bool UpdateAccomodationInfo(AccomodationInfo holiday);
        #endregion AccomodationInfo

        #region Bank Account
        IEnumerable<BankAccount> GetAllBankAccounts();
        BankAccount GetBankAccount(int id);
        BankAccount GetBankAccount(string accountNo);
        BankAccount CreateBankAccount(BankAccount info);
        bool UpdateBankAccount(BankAccount info);
        bool DeleteBankAccount(int id);
        #endregion Bank Account

        #region Event Category
        IEnumerable<EventCategory> GetEventCategories();
        EventCategory GetEventCategory(int id);
        EventCategory GetEventCategory(string code);
        EventCategory CreateEventCategory(EventCategory info);
        bool UpdateEventCategory(EventCategory info);
        bool DeleteEventCategory(int id);
        #endregion Event Category

    }
}
