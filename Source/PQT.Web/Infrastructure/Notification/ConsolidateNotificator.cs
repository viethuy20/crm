using System.Threading;
using System.Web.Mvc;
using CPO.Domain.Abstract;
using CPO.Domain.Entities;

namespace CPO.Web.Infrastructure.Notification
{
    public static class ConsolidateNotificator
    {
        private static INotificationService<Consolidate> NotificationService
        {
            get { return DependencyResolver.Current.GetService<INotificationService<Consolidate>>(); }
        }

        private static IIndentRepository IndentRepository
        {
            get { return DependencyResolver.Current.GetService<IIndentRepository>(); }
        }

        public static void Notify(int consolidateId)
        {
            new Thread(() =>
            {
                var consolidate = IndentRepository.GetConsolidate(consolidateId);
                NotificationService.NotifyAll(consolidate);
            }).Start();
        }

        //public static void Notify(Consolidate consolidate)
        //{
        //    new Thread(() =>
        //    {
        //        NotificationService.NotifyAll(consolidate);
        //    }).Start();
        //}
    }
}
