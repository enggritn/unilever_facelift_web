using Facelift_App;
using Facelift_App.Repositories;
using Facelift_App.Services;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Ninject.Web.WebApi;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace Facelift_App
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            return kernel;
        }
        private static void RegisterServices(IKernel kernel)
        {
            //kernel.Bind<IRepo>().ToMethod(ctx => new Repo("Ninject Rocks!"));
            kernel.Bind<IUsers>().To<UserRepository>();
            kernel.Bind<IRoles>().To<RoleRepository>();
            kernel.Bind<IMenus>().To<MenuRepository>();
            kernel.Bind<IWarehouses>().To<WarehouseRepository>();
            kernel.Bind<IEquipment>().To<EquipmentRepository>();
            kernel.Bind<ITransporters>().To<TransporterRepository>();
            kernel.Bind<ITransporterDrivers>().To<TransporterDriverRepository>();
            kernel.Bind<ITransporterTrucks>().To<TransporterTruckRepository>();
            kernel.Bind<IPalletTypes>().To<PalletTypeRepository>();
            kernel.Bind<IPalletProducers>().To<PalletProducerRepository>();
            kernel.Bind<IRegistrations>().To<RegistrationRepository>();
            kernel.Bind<IPallets>().To <PalletRepository>();
            kernel.Bind<IShipments>().To<ShipmentRepository>();
            kernel.Bind<IDashboards>().To<DashboardRepository>();
            kernel.Bind<IAccidents>().To<AccidentRepository>();
            kernel.Bind<ICycleCounts>().To<CycleCountRepository>();
            kernel.Bind<IBillings>().To<BillingRepository>();
            kernel.Bind<IBillingHistories>().To<BillingHistoryRepository>();
            kernel.Bind<ICompanies>().To<CompanyRepository>();
            kernel.Bind<IWarehouseCategories>().To<WarehouseCategoryRepository>();
        }
    }
}