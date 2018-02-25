using System.Collections.Generic;
using System.Linq;
using CPO.Domain.Entities;
using CPO.Domain.Enum;
using MIT.Web.Hubs;
using NS.Mail;

namespace CPO.Web.Infrastructure.Notification
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConsolidateNotificationService : AbstractNotificationService<Consolidate>
    {
        #region Overrides

        public override void NotifyAll(Consolidate consolidate)
        {
            NotificationHub.Notify(consolidate);

            SendNotifyEmail(consolidate);

            SendEmailToManufacture(consolidate);
        }

        public override void NotifyUser(User user, Consolidate consolidate = null)
        {
        }

        public override void NotifyRole(Role role, Consolidate consolidate = null)
        {
        }

        #endregion

        // 1: general manager approve consolidate
        // 2: consolidate change status to WaitForManufactureConfirm
        // 3: send email to manufacture
        private void SendEmailToManufacture(Consolidate consolidate)
        {
            //if (!Settings.Indent.SendMail()) return;
            //if (consolidate.StatusRecord.Status != ConsolidateStatus.WaitForManufacturerConfirm) return;

            //IEnumerable<CarSupplier> suppliers = consolidate.Indents.Select(i => i.Supplier).DistinctBy(s => s.ID);
            //foreach (CarSupplier sup in suppliers.Where(s => !string.IsNullOrWhiteSpace(s.Email)))
            //{
            //    CarSupplier carSupplier = sup;
            //    IEnumerable<Indent> indentsBySupplier = consolidate.Indents.Where(i => i.SupplierID == carSupplier.ID);
            //    string body = GenerateIndentList(indentsBySupplier);

            //    SendEmail(sup.Email, "New indent package", body);
            //}
        }

        private void SendNotifyEmail(Consolidate consolidate)
        {
            if (consolidate.StatusRecord.Status == ConsolidateStatus.Initial)
                SendEmail_ConsolidateUpdated(consolidate);
            else if (consolidate.StatusRecord.Status == ConsolidateStatus.Denied)
                SendEmail_ConsolidateDenied(consolidate);
            else if (consolidate.StatusRecord.Status == ConsolidateStatus.Confirm)
                SendEmail_Consolidate_ReadyForLC(consolidate);
            else if (consolidate.StatusRecord.Status == ConsolidateStatus.Cancelled)
                SendEmail_ConsolidateCancelled(consolidate);
        }

        // indent user / manager / general manager -> all
        private void SendEmail_ConsolidateCancelled(Consolidate consolidate)
        {
            string subject = "Consolidate cancelled (id :" + consolidate.ID + ")";
            SendEmail(subject, "Consolidate", "Cancelled", consolidate);
        }

        // created
        // updated
        // manager -> general manager
        private void SendEmail_ConsolidateUpdated(Consolidate consolidate)
        {
            List<ConsolidateStatusRecord> statusRecords = IndentRepository.GetConsolidateStatusRecords(consolidate.ID).ToList();

            string subject = statusRecords.Count() == 1
                                 ? "New consolidate (id :" + consolidate.ID + ")"
                                 : "Consolidate updated (id: " + consolidate.ID + ")";
            var viewBag = new { StatusRecords = statusRecords };
            SendEmail(subject, "Consolidate", "Initial", consolidate, viewBag);
        }

        // denied
        // general manager -> manager
        private void SendEmail_ConsolidateDenied(Consolidate consolidate)
        {
            const string subject = "Consolidate denied";
            SendEmail(subject, "Consolidate", "Denied", consolidate);
        }

        // approve
        // general manager -> manager

        // indent user / manager -> general manager
        private void SendEmail_Consolidate_ReadyForLC(Consolidate consolidate)
        {
            const string subject = "Consolidate confirmed";
            SendEmail(subject, "Consolidate", "Confirmed", consolidate);
        }
    }
}
