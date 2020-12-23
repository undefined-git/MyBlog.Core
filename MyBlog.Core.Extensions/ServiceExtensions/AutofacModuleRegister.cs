using Autofac;
using Autofac.Extras.DynamicProxy;
using log4net;
using MyBlog.Core.IRepository.Base;
using MyBlog.Core.Repository.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Extensions.ServiceExtensions
{
    public class AutofacModuleRegister : Autofac.Module
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AutofacModuleRegister));
        protected override void Load(ContainerBuilder builder)
        {
            var basePath = AppContext.BaseDirectory;

            #region 带接口层的服务注入
            var servicesDllFile = Path.Combine(basePath, "MyBlog.Core.Services.dll");
            var repositoryDllFile = Path.Combine(basePath, "MyBlog.Core.Repository.dll");

            if (!(File.Exists(servicesDllFile) && File.Exists(repositoryDllFile)))
            {
                var msg = "Repository.dll和service.dll 丢失，因为项目解耦了，所以需要先F6编译，再F5运行，请检查 bin 文件夹，并拷贝。";
                log.Error(msg);
                throw new Exception(msg);
            }

            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();//注册仓储

            // 获取 Service.dll 程序集服务，并注册
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices)
                      .AsImplementedInterfaces()
                      .InstancePerDependency()
                      .EnableInterfaceInterceptors();//引用Autofac.Extras.DynamicProxy;

            // 获取 Repository.dll 程序集服务，并注册
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository)
                   .AsImplementedInterfaces()
                   .InstancePerDependency();
            #endregion
        }
    }
}
