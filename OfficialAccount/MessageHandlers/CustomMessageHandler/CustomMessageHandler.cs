using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using OfficialAccount.MessageHandlers.CustomMessageHandler;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;

namespace OfficialAccount.MessageHandlers.CustomMessageHandler
{
    public class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        public CustomMessageHandler(Stream inputStream, PostModel postModel)
            : base(inputStream, postModel)
        {

        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>(); //ResponseMessageText也可以是News等其他类型
            responseMessage.Content = "这条消息来自DefaultResponseMessage。";
            return responseMessage;
        }

        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您的OpenID是：" + requestMessage.FromUserName      //这里的requestMessage.FromUserName也可以直接写成base.WeixinOpenId
                                    + "。\r\n您发送了文字信息：" + requestMessage.Content;  //\r\n用于换行，requestMessage.Content即用户发过来的文字内容
            return responseMessage;
        }

        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "欢迎使用 Senparc.Weixin SDK",
                Description = "这是一条默认消息",
                PicUrl = "https://sdk.weixin.senparc.com/images/v2/logo.png",
                Url = "https://weixin.senparc.com"
            });
            return responseMessage;
        }

    }
}