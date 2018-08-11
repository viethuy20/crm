﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Entities
{
    public class Event : Entity
    {
        public Event()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
            var rand = new Random((int)DateTime.Now.Ticks).Next(0x1000000);
            EventSessions = new HashSet<EventSession>();
            SalesGroups = new HashSet<SalesGroup>();
            ManagerUsers = new HashSet<User>();
            EventCompanies = new HashSet<EventCompany>();
            //Trainers = new HashSet<Trainer>();
            BackgroundColor = string.Format("#{0:X6}", rand);
        }
        public Event(Event e)
        {
            ID = e.ID;
            EventCode = e.EventCode;
            EventName = e.EventName;
            BackgroundColor = e.BackgroundColor;
            HotelVenue = e.HotelVenue;
            SalesRules = e.SalesRules;
            PrimaryJobtitleKeywords = e.PrimaryJobtitleKeywords;
            SecondaryJobtitleKeywords = e.SecondaryJobtitleKeywords;
            Location = e.Location;
            Remark = e.Remark;
            Brochure = e.Brochure;
            FinanceEmail = e.FinanceEmail;
            OperationEmail = e.OperationEmail;
            OperationOnsiteEmail = e.OperationOnsiteEmail;
            ProductionEmail = e.ProductionEmail;
            SalesEmail = e.SalesEmail;
            RegContract = e.RegContract;
            EventStatus = e.EventStatus;
            DateOfOpen = e.DateOfOpen;
            DateOfConfirmation = e.DateOfConfirmation;
            ClosingDate = e.ClosingDate;
            StartDate = e.StartDate;
            EndDate = e.EndDate;
            IsEventEnd = e.IsEventEnd;
            UserID = e.UserID;
            EventSessions = e.EventSessions.Select(m => new EventSession(m)).ToList();
            SalesGroups = e.SalesGroups.Select(m => new SalesGroup(m)).ToList();
            ManagerUsers = e.ManagerUsers;
            EventCompanies = e.EventCompanies.Select(m => new EventCompany(m)).ToList();
            CreatedTime = e.CreatedTime;
            UpdatedTime = e.UpdatedTime;
        }
        public string EventCode { get; set; }
        public string EventName { get; set; }
        public string BackgroundColor { get; set; }
        public string HotelVenue { get; set; }//venue of event
        public string SalesRules { get; set; }//Sales Rules
        public string PrimaryJobtitleKeywords { get; set; }
        public string SecondaryJobtitleKeywords { get; set; }
        public string Location { get; set; }
        public string Remark { get; set; }
        public string Brochure { get; set; }
        public string FinanceEmail { get; set; }
        public string OperationEmail { get; set; }//Operation Office
        public string OperationOnsiteEmail { get; set; }
        public string ProductionEmail { get; set; }
        public string SalesEmail { get; set; }
        public string RegContract { get; set; }
        public EventStatus EventStatus { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfOpen { get; set; }//Lack Date of Open (Cross Sell)
        [DataType(DataType.Date)]
        public DateTime? DateOfConfirmation { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ClosingDate { get; set; }//Date of Closing Sales
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }//Sales start - Event First Day
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }//Sales end - Event Last Day
        public bool IsEventEnd { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<EventSession> EventSessions { get; set; }
        //for assign sales group
        public virtual ICollection<SalesGroup> SalesGroups { get; set; }
        //for assign salesman
        public virtual ICollection<User> ManagerUsers { get; set; }
        public virtual ICollection<EventCompany> EventCompanies { get; set; }
        //public virtual ICollection<Trainer> Trainers { get; set; }

        public string DateOfConfirmationStr
        {
            get
            {
                if (DateOfConfirmation != null)
                {
                    return Convert.ToDateTime(DateOfConfirmation).ToString("dd/MM/yyyy");
                }
                return "";
            }
        }
        public string ClosingDateStr
        {
            get
            {
                if (ClosingDate != null)
                {
                    return Convert.ToDateTime(ClosingDate).ToString("dd/MM/yyyy");
                }
                return "";
            }
        }

        public string EventStatusDisplay
        {
            get
            {
                if (EventStatus != null)
                {
                    return EventStatus.DisplayName;
                }
                return "";
            }
        }

    }
    public class EventStatus : Enumeration
    {
        public static readonly EventStatus Initial = New<EventStatus>("", "Initial");
        public static readonly EventStatus Live = New<EventStatus>(1, "Live");
        public static readonly EventStatus Confirmed = New<EventStatus>(2, "Confirmed");
        public static readonly EventStatus Completed = New<EventStatus>(3, "Completed");
        public static readonly EventStatus Cancelled = New<EventStatus>(4, "Cancelled");
    }
    public class MonthStatus : Enumeration
    {
        public static readonly MonthStatus NotSet = New<MonthStatus>(0, "Not Set");
        public static readonly MonthStatus January = New<MonthStatus>(1, "January");
        public static readonly MonthStatus February = New<MonthStatus>(2, "February");
        public static readonly MonthStatus March = New<MonthStatus>(3, "March");
        public static readonly MonthStatus April = New<MonthStatus>(4, "April");
        public static readonly MonthStatus May = New<MonthStatus>(5, "May");
        public static readonly MonthStatus June = New<MonthStatus>(6, "June");
        public static readonly MonthStatus July = New<MonthStatus>(7, "July");
        public static readonly MonthStatus August = New<MonthStatus>(8, "August");
        public static readonly MonthStatus September = New<MonthStatus>(9, "September");
        public static readonly MonthStatus October = New<MonthStatus>(10, "October");
        public static readonly MonthStatus November = New<MonthStatus>(11, "November");
        public static readonly MonthStatus December = New<MonthStatus>(12, "December");
    }
}
