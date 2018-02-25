using CPO.Domain.Enum;

namespace CPO.Domain.Entities
{
    public class ConsolidateStatusRecord : StatusRecord<ConsolidateStatus>
    {
        public ConsolidateStatusRecord()
        {
        }

        public ConsolidateStatusRecord(int consolidateID)
            : base(consolidateID)
        {
        }

        public ConsolidateStatusRecord(int consolidateId, ConsolidateStatus status, string message = "")
            : base(consolidateId, status, message)
        {
        }
        public ConsolidateStatusRecord(int entryID, ConsolidateStatus status, int userId, string message = "")
            : base(entryID, status, userId, message)
        {
        }
    }
}
