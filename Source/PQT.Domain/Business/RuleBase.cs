using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PQT.Domain.Business
{
    public abstract class RuleBase<T>
    {
        protected RuleBase(T obj)
        {
            Target = obj;
        }

        protected T Target { get; private set; }

        public abstract RuleResult<T> Check(params object[] predicate);

        protected RuleResult<T> Result(bool success, string message = "")
        {
            return new RuleResult<T>(success, message);
        }

        protected RuleResult<T> Success(string message = "")
        {
            return Result(true, message);
        }

        protected RuleResult<T> Failure(string message = "")
        {
            return Result(false, message);
        }
    }

    public class RuleResult<T>
    {
        public RuleResult(bool success, string msg = "")
        {
            Success = success;
            Message = msg;
        }

        public string Message { get; set; }
        public bool Success { get; set; }

        public static implicit operator RuleResult<T>(bool success)
        {
            return new RuleResult<T>(success);
        }

        public static implicit operator bool(RuleResult<T> result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            return result.Success;
        }
    }
}
