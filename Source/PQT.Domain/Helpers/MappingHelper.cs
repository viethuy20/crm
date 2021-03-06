using AutoMapper;
using NS;

namespace PQT.Domain.Helpers
{
    public class EnumerationTypeConverter : ITypeConverter<Enumeration, int>
    {
        #region ITypeConverter<Enumeration,int> Members

        public int Convert(ResolutionContext context)
        {
            return int.Parse(((Enumeration) context.SourceValue).Value);
        }

        #endregion
    }
    //public class EnumerationStringTypeConverter : ITypeConverter<Enumeration, string>
    //{
    //    #region ITypeConverter<Enumeration,int> Members

    //    public string Convert(ResolutionContext context)
    //    {
    //        return ((Enumeration) context.SourceValue).Value;
    //    }

    //    #endregion
    //}
}
