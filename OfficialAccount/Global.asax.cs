using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace OfficialAccount
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            /* CO2NET ȫ��ע�Ὺʼ
            * ���鰴������˳�����ע��
            */

            //����ȫ�� Debug ״̬
            var isGLobalDebug = true;
            var senparcSetting = SenparcSetting.BuildFromWebConfig(isGLobalDebug);

            //CO2NET ȫ��ע�ᣬ���룡��
            IRegisterService register = RegisterService.Start(senparcSetting)
                                          .UseSenparcGlobal(false, null);

            /* ΢�����ÿ�ʼ
             * ���鰴������˳�����ע��
             */

            //����΢�� Debug ״̬
            var isWeixinDebug = true;
            var senparcWeixinSetting = SenparcWeixinSetting.BuildFromWebConfig(isWeixinDebug);

            //΢��ȫ��ע�ᣬ���룡��
            register.UseSenparcWeixin(senparcWeixinSetting, senparcSetting);

        }
    }
}
