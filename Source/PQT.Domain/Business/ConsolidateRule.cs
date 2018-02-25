using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPO.Domain.Entities;
using CPO.Domain.Enum;

namespace CPO.Domain.Business
{
    public class ConsolidateRule : RuleBase<Consolidate>
    {
        public ConsolidateRule(Consolidate consolidate)
            : base(consolidate)
        {
        }

        public override RuleResult<Consolidate> Check(params object[] predicate)
        {
            if (predicate.Length == 0)
                throw new ArgumentNullException("predicate");

            if (predicate[0] is ConsolidateStatus)
                return CanChangeStatus(Target.StatusRecord.Status, predicate[0] as ConsolidateStatus);

            return Success();
        }

        private RuleResult<Consolidate> CanChangeStatus(ConsolidateStatus from, ConsolidateStatus to)
        {
            if (Target == null)
                return false;

            // If an indent is cancelled, the only action allowed is update
            if (from == ConsolidateStatus.Cancelled && to == ConsolidateStatus.Initial)
                return Failure("You can't edit this consolidate, it was cancelled.");
            if (from == ConsolidateStatus.Cancelled && to == ConsolidateStatus.Confirm)
                return Failure("You can't confirm this consolidate, it was cancelled.");
            if (from == ConsolidateStatus.Cancelled && to == ConsolidateStatus.Denied)
                return Failure("You can't deny this consolidate, it was cancelled.");
            if (from == ConsolidateStatus.Cancelled && to == ConsolidateStatus.Cancelled)
                return Failure("You can't cancel this consolidate, it was cancelled.");

            // If an indent is confirmed, prevent deny
            if (from == ConsolidateStatus.Confirm && to == ConsolidateStatus.Initial)
                return Failure("You can't edit this consolidate, it was confirmed.");
            // If an indent is confirmed, prevent deny
            if (from == ConsolidateStatus.Confirm && to == ConsolidateStatus.Denied)
                return Failure("You can't deny this consolidate, it was confirmed.");
            if (from == ConsolidateStatus.Confirm && to == ConsolidateStatus.Confirm)
                return Failure("You can't confirm this consolidate, it was confirmed.");
            //if (from == ConsolidateStatus.Confirm && to == ConsolidateStatus.Cancelled)
            //    return Failure("You can't delete this consolidate, it was confirmed.");

            //If consolidate is denied, prevent deny
            if (from == ConsolidateStatus.Denied && to == ConsolidateStatus.Initial)
                return Failure("You can't update this consolidate, it has already denied.");
            if (from == ConsolidateStatus.Denied && to == ConsolidateStatus.Denied)
                return Failure("You can't deny this consolidate, it has already denied.");
            if (from == ConsolidateStatus.Denied && to == ConsolidateStatus.Confirm)
                return Failure("You can't deny this consolidate, it has already denied.");
            // If and indent is denied, prevent confirm
            //if (from == IndentStatus.Denied && to == IndentStatus.ManufactureConfirmed)
            //    return Failure("You can't approve this consolidate, it was denied.");

            return Success();
        }
    }
}
