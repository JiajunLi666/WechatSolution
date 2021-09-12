
/*----------------------------------------------------------------
    
    文件名：WexinController.cs
    文件功能描述：基于Senparc的微信开发测试文件
    
    
    创建标识：Senparc - 20210903
    
----------------------------------------------------------------*/


using Senparc.CO2NET;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OfficialAccount.Controllers
{
    using Senparc.Weixin;
    using Senparc.Weixin.MP.Entities.Request;
    using Senparc.Weixin.MP.MvcExtension;
    using OfficialAccount.MessageHandlers.CustomMessageHandler;
    using Senparc.Weixin.MP.Containers;
    using Senparc.Weixin.MP.AdvancedAPIs.User;
    using Senparc.Weixin.MP.AdvancedAPIs;
    using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
    using System.Text;
    using System.Web.Services;

    public class WeixinController : Controller
    {
        public static readonly string Token = Config.SenparcWeixinSetting.Token;//与微信公众账号后台的Token设置保持一致，区分大小写。
        public static readonly string EncodingAESKey = Config.SenparcWeixinSetting.EncodingAESKey;//与微信公众账号后台的EncodingAESKey设置保持一致，区分大小写。
        public static readonly string AppId = Config.SenparcWeixinSetting.WeixinAppId;//与微信公众账号后台的AppId设置保持一致，区分大小写。
        public static readonly string AppSecret = Config.SenparcWeixinSetting.WeixinAppSecret;//与微信公众账号后台的AppSecret设置保持一致，区分大小写。
        private BatchGetUserInfoData userList;

        /// 微信后台验证地址（使用Get），微信后台的“接口配置信息”的Url

        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                return Content("failed:" + postModel.Signature + "," + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, Token) + "。" +
                    "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }
        }


        /// 用户发送消息后，微信平台自动Post一个请求到这里，并等待响应XML。

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(PostModel postModel)
        {
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                return Content("参数错误！");
            }

            postModel.Token = Token;//根据自己后台的设置保持一致
            postModel.EncodingAESKey = EncodingAESKey;//根据自己后台的设置保持一致
            postModel.AppId = AppId;//根据自己后台的设置保持一致

            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var messageHandler = new CustomMessageHandler(Request.InputStream, postModel);//接收消息

            messageHandler.Execute();//执行微信处理过程

            return new FixWeixinBugWeixinResult(messageHandler);//返回结果

        }

        // GET: Weixin
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取所有用户详细资料
        /// </summary>

        public ActionResult GetUser()
        {
            List<UserInfoJson> userInfoList = new List<UserInfoJson>();
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            var followers = UserApi.Get(accessToken, null);
            for (int i = 0; i < followers.count; i++)
            {
                var userInfoResult = UserApi.Info(accessToken, followers.data.openid[i]);
                userInfoList.Add(userInfoResult);
            }
            var groups = GroupsApi.Get(accessToken);
            var groupsSelect = new List<SelectListItem>();
            foreach(var group in groups.groups)
            {
                groupsSelect.Add(new SelectListItem() { Value=group.id.ToString(),Text=group.name});
            }
            ViewBag.groupSelect = groupsSelect;
            ViewBag.userInfoList = userInfoList;
            return View();

        }

        /// <summary>
        /// 分组管理
        /// </summary>
        /// <returns></returns>
        public ActionResult GroupManage()
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            var groups = GroupsApi.Get(accessToken);
            
            ViewBag.groups = groups.groups;
            return View();
        }

        /// <summary>
        /// 更新分组名称
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateGroupName()
        {
            int groupId = int.Parse(Request.Form["groupId"]);
            string groupName = Request.Form["groupName"];
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            GroupsApi.Update(accessToken,groupId,groupName);
            return RedirectToAction("GroupManage");
        }
        
        /// <summary>
        /// 创建分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateGroup(string groupName)
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            GroupsApi.Create(accessToken,groupName);
            return RedirectToAction("GroupManage");
        }


        /// <summary>
        /// 标签管理
        /// </summary>
        /// <returns></returns>
        public ActionResult TagManage()
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            var result = UserTagApi.Get(accessToken);
            ViewBag.tags = result.tags;
            return View();
        }

        /// <summary>
        /// 新建标签
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateTag(string tagName)
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            var result = UserTagApi.Create(accessToken, tagName);
            return RedirectToAction("TagManage");
        }


        /// <summary>
        /// 修改用户分组
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateGroupId()
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            int newGroupId = int.Parse(Request.Form["newGroupId"]);
            string openid = Request.Form["openid"];
            var result = GroupsApi.MemberUpdate(accessToken, openid, newGroupId);
            return this.Json(new { errorcode = result.errcode, msg = result.ToString() });
        }

        public ActionResult AjaxTest()
        {
            return View();
        }

        /// <summary>
        ///  前端手动推送模板消息
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public JsonResult SendTemplateMessage()
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            var templateId = "U8IGrydPVjzTum1vIOj2_mWzgnE9QM6AhfxROSJl6jM";
            var data = new
            {
                first = new TemplateDataItem("这是一条测试的模板消息"),
                keyword1 = new TemplateDataItem("关键字1"),
                keyword2 = new TemplateDataItem("关键字2"),
                keyword3 = new TemplateDataItem("关键字3"),
                keyword4 = new TemplateDataItem("关键字4"),
                keyword5 = new TemplateDataItem("关键字5"),
                remark = new TemplateDataItem("测试成功！")
            };
            ///字符串的方式进行参数传递的获取方式
            string openid = Request.Form["openid"];
            var sendResult = TemplateApi.SendTemplateMessage(accessToken, openid, templateId, "http://www.baidu.com", data);
            //return this.Json(new { success = "true", msg = sendResult.ToString() });
            return this.Json(new { errorcode = sendResult.errcode , msg = sendResult.ToString() });
        }

    }
}