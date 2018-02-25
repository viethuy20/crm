using FluentValidation;
using Ninject.Modules;

namespace PQT.Web.Infrastructure
{
    public class NinjectValidator : NinjectModule
    {
        public override void Load()
        {
            AssemblyScanner.FindValidatorsInAssemblyContaining<IValidator>()
                           .ForEach(match => Bind(match.InterfaceType).To(match.ValidatorType));
        }
    }
}