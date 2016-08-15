﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using XCLCMS.Data.WebAPIEntity;

namespace XCLCMS.Lib.WebAPI
{
    /// <summary>
    /// WEB API 公共库
    /// </summary>
    public class Library
    {
        #region 基础

        /// <summary>
        /// 请求
        /// </summary>
        /// <typeparam name="TRequest">请求类型</typeparam>
        /// <typeparam name="TResponse">返回类型</typeparam>
        /// <param name="request">请求类对象</param>
        /// <param name="path">请求路径</param>
        /// <param name="isGet">是否为get请求，默认为get</param>
        /// <returns>请求结果</returns>
        public static APIResponseEntity<TResponse> Request<TRequest, TResponse>(APIRequestEntity<TRequest> request, string path, bool isGet = true) where TRequest : new() where TResponse : new()
        {
            APIResponseEntity<TResponse> response = new APIResponseEntity<TResponse>();
            try
            {
                string requestURL = (XCLCMS.Lib.SysWebSetting.Setting.SettingModel.Common_WebAPIServiceURL.Trim().Trim('/') + '/' + path.Trim().Trim('/')).Trim('?');
                var httpClient = new HttpClient();
                var httpRequest = new HttpRequestMessage();
                string requestJson = JsonConvert.SerializeObject(request);
                if (isGet)
                {
                    httpRequest.RequestUri = new Uri(requestURL + (requestURL.IndexOf('?') >= 0 ? "&" : "?") + XCLNetTools.Serialize.Lib.ConvertJsonToUrlParameters(requestJson));
                    httpRequest.Method = HttpMethod.Get;
                }
                else
                {
                    httpRequest.RequestUri = new Uri(requestURL);
                    httpRequest.Method = HttpMethod.Post;
                    httpRequest.Content = new StringContent(requestJson);
                    httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
                httpRequest.Headers.Add(XCLCMS.Lib.Common.Comm.WebAPIUserTokenHeaderName, request.UserToken);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var res = httpClient.SendAsync(httpRequest).Result.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(res))
                {
                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<APIResponseEntity<TResponse>>(res);
                }
            }
            catch (Exception ex)
            {
                XCLNetLogger.Log.WriteLog(ex);
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 创建request对象
        /// </summary>
        /// <returns>request对象</returns>
        public static APIRequestEntity<TRequest> CreateRequest<TRequest>(string userToken) where TRequest : new()
        {
            APIRequestEntity<TRequest> request = new APIRequestEntity<TRequest>();
            request.ClientIP = XCLNetTools.Common.IPHelper.GetClientIP();
            request.UserToken = userToken;
            request.Url = System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
            return request;
        }

        #endregion 基础

        #region CommonAPI相关

        /// <summary>
        /// 生成ID
        /// </summary>
        public static long CommonAPI_GenerateID(string userToken, XCLCMS.Data.WebAPIEntity.RequestEntity.Common.GenerateIDEntity entity)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<XCLCMS.Data.WebAPIEntity.RequestEntity.Common.GenerateIDEntity>(userToken);
            request.Body = entity;
            var response = XCLCMS.Lib.WebAPI.CommonAPI.GenerateID(request);
            if (null == response)
            {
                return 0;
            }
            return response.Body;
        }

        #endregion CommonAPI相关

        #region MerchantAPI相关

        /// <summary>
        /// 获取商户类型
        /// </summary>
        public static Dictionary<string, long> MerchantAPI_GetMerchantTypeDic(string userToken)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<object>(userToken);
            request.Body = new object();
            var response = XCLCMS.Lib.WebAPI.MerchantAPI.GetMerchantTypeDic(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        #endregion MerchantAPI相关

        #region SysDicAPI相关

        /// <summary>
        /// 获取证件类型
        /// </summary>
        public static Dictionary<string, long> SysDicAPI_GetPassTypeDic(string userToken)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<object>(userToken);
            request.Body = new object();
            var response = XCLCMS.Lib.WebAPI.SysDicAPI.GetPassTypeDic(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        /// <summary>
        /// 获取当前sysDicID所属的层级list
        /// 如:根目录/子目录/文件
        /// </summary>
        public static List<XCLCMS.Data.Model.Custom.SysDicSimple> SysDicAPI_GetLayerListBySysDicID(string userToken, XCLCMS.Data.WebAPIEntity.RequestEntity.SysDic.GetLayerListBySysDicIDEntity entity)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<XCLCMS.Data.WebAPIEntity.RequestEntity.SysDic.GetLayerListBySysDicIDEntity>(userToken);
            request.Body = entity;
            var response = XCLCMS.Lib.WebAPI.SysDicAPI.GetLayerListBySysDicID(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        /// <summary>
        /// 获取XCLCMS管理后台系统的菜单
        /// </summary>
        public static List<XCLCMS.Data.Model.View.v_SysDic> SysDicAPI_GetSystemMenuModelList(string userToken)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<object>(userToken);
            request.Body = new object();
            var response = XCLCMS.Lib.WebAPI.SysDicAPI.GetSystemMenuModelList(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        /// <summary>
        /// 根据SysDicID查询其子项
        /// </summary>
        public static List<XCLCMS.Data.Model.SysDic> SysDicAPI_GetChildListByID(string userToken, long sysDicID)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<long>(userToken);
            request.Body = sysDicID;
            var response = XCLCMS.Lib.WebAPI.SysDicAPI.GetChildListByID(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        /// <summary>
        /// 递归获取指定SysDicID下的所有列表（不包含该SysDicID的记录）
        /// </summary>
        public static List<XCLCMS.Data.Model.View.v_SysDic> SysDicAPI_GetAllUnderListByID(string userToken, long sysDicID)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<long>(userToken);
            request.Body = sysDicID;
            var response = XCLCMS.Lib.WebAPI.SysDicAPI.GetAllUnderListByID(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        #endregion SysDicAPI相关

        #region SysFunctionAPI相关

        /// <summary>
        /// 获取当前SysFunctionID所属的层级list
        /// 如:根目录/子目录/文件
        /// </summary>
        public static List<XCLCMS.Data.Model.Custom.SysFunctionSimple> SysFunctionAPI_GetLayerListBySysFunctionId(string userToken, XCLCMS.Data.WebAPIEntity.RequestEntity.SysFunction.GetLayerListBySysFunctionIdEntity entity)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<XCLCMS.Data.WebAPIEntity.RequestEntity.SysFunction.GetLayerListBySysFunctionIdEntity>(userToken);
            request.Body = entity;
            var response = XCLCMS.Lib.WebAPI.SysFunctionAPI.GetLayerListBySysFunctionId(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        /// <summary>
        /// 获取指定角色的所有功能
        /// </summary>
        public static List<XCLCMS.Data.Model.SysFunction> SysFunctionAPI_GetListByRoleID(string userToken, long sysRoleID)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<long>(userToken);
            request.Body = sysRoleID;
            var response = XCLCMS.Lib.WebAPI.SysFunctionAPI.GetListByRoleID(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        #endregion SysFunctionAPI相关

        #region SysRoleAPI 相关

        /// <summary>
        /// 获取当前SysRoleID所属的层级list
        /// 如:根目录/子目录/文件
        /// </summary>
        public static List<XCLCMS.Data.Model.Custom.SysRoleSimple> SysRoleAPI_GetLayerListBySysRoleID(string userToken, XCLCMS.Data.WebAPIEntity.RequestEntity.SysRole.GetLayerListBySysRoleIDEntity entity)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<XCLCMS.Data.WebAPIEntity.RequestEntity.SysRole.GetLayerListBySysRoleIDEntity>(userToken);
            request.Body = entity;
            var response = XCLCMS.Lib.WebAPI.SysRoleAPI.GetLayerListBySysRoleID(request);
            if (null == response)
            {
                return null;
            }
            return response.Body;
        }

        /// <summary>
        /// 添加角色
        /// </summary>
        public static bool SysRoleAPI_Add(string userToken, XCLCMS.Data.WebAPIEntity.RequestEntity.SysRole.AddOrUpdateEntity entity)
        {
            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<XCLCMS.Data.WebAPIEntity.RequestEntity.SysRole.AddOrUpdateEntity>(userToken);
            request.Body = entity;
            var response = XCLCMS.Lib.WebAPI.SysRoleAPI.Add(request);
            if (null == response)
            {
                return false;
            }
            return response.Body;
        }

        #endregion SysRoleAPI 相关
    }
}