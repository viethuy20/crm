using System.Web.Mvc;
using CPO.Domain.Abstract;
using CPO.Domain.Entities;

namespace CPO.Web.Infrastructure.Notification
{
    public static class ConfirmDocumentNotificator
    {
        private static INotificationService<ConfirmDocument> NotificationService
        {
            get { return DependencyResolver.Current.GetService<INotificationService<ConfirmDocument>>(); }
        }

        private static IIndentRepository IndentRepository
        {
            get { return DependencyResolver.Current.GetService<IIndentRepository>(); }
        }

        public static void Notify(int confirmDocId)
        {
            ConfirmDocument confirmDoc = IndentRepository.GetConfirmDocument(confirmDocId);
            NotificationService.NotifyAll(confirmDoc);
        }
    }
}
