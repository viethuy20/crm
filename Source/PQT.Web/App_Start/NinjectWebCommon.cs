using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using CsQuery.Engine.PseudoClassSelectors;
using FluentValidation;
using FluentValidation.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PQT.Web.Validation;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Mvc.FluentValidation;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Helpers.Encryption;
using PQT.Web;
using PQT.Web.Models;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Models;
using WebActivator;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace PQT.Web
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        ///     Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        ///     Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        ///     Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = DependencyContainer.Current ?? new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            var ninjectValidatorFactory = new NinjectValidatorFactory(kernel);
            ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(ninjectValidatorFactory));
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            RegisterServices(kernel);
            return kernel;
        }


        /// <summary>
        ///     Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // Database context
            kernel.Bind<DbContext>().To<PQTDb>().InRequestScope();
            // Logging
            kernel.Bind<IAuditTracker>().To<EFAuditTracker>()
                // initialize DbContext in another block so it will not persist temporary changes in business flow
                  .WithConstructorArgument("db", c => c.Kernel.BeginBlock().Get<DbContext>());

            // Repositories
            kernel.Bind<IAuthenticationService, IMembershipService>().To<InMemLoginTracker>();
            kernel.Bind<IAuthorizationService, IRoleService>().To<EFRoleBasedAuthorizer>();
            kernel.Bind<ILoginTracker>().To<InMemLoginTracker>()
                  .WithConstructorArgument("concurrentMax", Convert.ToInt32(ConfigurationManager.AppSettings["ConcurrentLoginMax"]));
            //_kernel.Bind<ILoginTracker>().To<InMemLoginTracker>();
            kernel.Bind<EFMenuRepository>().ToSelf();
            kernel.Bind<IMenuRepository>().To<MemoryMenuRepository>().InSingletonScope();
            kernel.Bind<EFCompanyRepository>().ToSelf();
            kernel.Bind<ICompanyRepository>().To<MemoryCompanyRepository>().InSingletonScope();
            kernel.Bind<EFEventService>().ToSelf();
            kernel.Bind<IEventService>().To<MemoryEventRepository>().InSingletonScope();
            kernel.Bind<MemoryNotifyRepository>().ToSelf();
            kernel.Bind<IUserNotificationService>().To<MemoryNotifyRepository>().InSingletonScope();
            kernel.Bind<IUnitRepository>().To<EFUnitRepository>();
            kernel.Bind<ISettingRepository>().To<EFSettingRepository>();
            kernel.Bind<ILeadService>().To<EFLeadService>();
            kernel.Bind<ISalesGroupService>().To<EFSalesGroupService>();
            kernel.Bind<ITrainerService>().To<EFTrainerService>();
            kernel.Bind<IBookingService>().To<EfBookingService>();
            kernel.Bind<IUploadTemplateService>().To<EFUploadTemplateService>();
            kernel.Bind<IInvoiceService>().To<EFInvoiceService>();
            kernel.Bind<IRecruitmentService>().To<EFRecruitmentService>();

            // Notification services
            kernel.Bind<INotificationService<Lead>>().To<LeadNotificationService>();
            kernel.Bind<INotificationService<Booking>>().To<BookingNotificationService>();

            kernel.Bind<IStringEncryptor>().To<SimplerAES>();

            kernel.Bind<IValidator<User>>().To<UserValidator>();
            kernel.Bind<IValidator<Menu>>().To<MenuValidator>();
            kernel.Bind<IValidator<Company>>().To<CompanyValidator>();
            kernel.Bind<IValidator<Country>>().To<CountryValidator>();
            kernel.Bind<IValidator<Event>>().To<EventValidator>();
            kernel.Bind<IValidator<Trainer>>().To<TrainerValidator>();
            kernel.Bind<IValidator<Lead>>().To<LeadValidator>();
            kernel.Bind<IValidator<PhoneCall>>().To<PhoneCallValidator>();
            kernel.Bind<IValidator<Booking>>().To<BookingValidator>();
            kernel.Bind<IValidator<OfficeLocation>>().To<OfficeLocationValidator>();
            kernel.Bind<IValidator<UploadTemplate>>().To<UploadTemplateValidator>();
            kernel.Bind<IValidator<Domain.Entities.Delegate>>().To<DelegateValidator>();
            kernel.Bind<IValidator<Candidate>>().To<CandidateValidator>();
            // cache configurations.
            kernel.Bind<ICacheStorageLocation>().To<RequestCacheSolution>().Named("InRequest");
            kernel.Bind<ICacheStorageLocation>().To<SessionCacheSolution>().Named("InSession");

            //

            DependencyResolver.SetResolver(new NinjectResolver(kernel));
        }
    }
}
