using System;
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



        #region Holiday
        IEnumerable<Holiday> GetAllHolidays();
        IEnumerable<Holiday> GetAllHolidays(int year);
        Holiday GetHoliday(int holidayID);
        Holiday GetHoliday(DateTime holidayDate);
        Holiday CreateHoliday(Holiday holiday);
        bool UpdateHoliday(Holiday holiday);
        bool DeleteHoliday(DateTime date);
        IEnumerable<Holiday> GetAllHolidaysbyMonthAndYear(int month, int year);
        #endregion Holiday

    }
}
