﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using XCLCMS.Data.WebAPIEntity;
using XCLCMS.Data.WebAPIEntity.RequestEntity;
using XCLNetTools.Generic;

namespace XCLCMS.WebAPI.Controllers
{
    /// <summary>
    /// 系统配置 管理
    /// </summary>
    public class SysWebSettingController : BaseAPIController
    {
        private XCLCMS.Data.BLL.View.v_SysWebSetting vSysWebSettingBLL = new Data.BLL.View.v_SysWebSetting();
        private XCLCMS.Data.BLL.SysWebSetting sysWebSettingBLL = new Data.BLL.SysWebSetting();
        private XCLCMS.Data.BLL.MerchantApp merchantAppBLL = new Data.BLL.MerchantApp();

        /// <summary>
        /// 查询系统配置信息实体
        /// </summary>
        [HttpGet]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysWebSettingView)]
        public APIResponseEntity<XCLCMS.Data.Model.SysWebSetting> Detail([FromUri] string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<long>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<XCLCMS.Data.Model.SysWebSetting>();
            response.Body = sysWebSettingBLL.GetModel(request.Body);
            response.IsSuccess = true;

            //限制商户
            if (base.IsOnlyCurrentMerchant && null != response.Body && response.Body.FK_MerchantID != base.CurrentUserModel.FK_MerchantID)
            {
                response.Body = null;
                response.IsSuccess = false;
            }

            return response;
        }

        /// <summary>
        /// 查询系统配置分页列表
        /// </summary>
        [HttpGet]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysWebSettingView)]
        public APIResponseEntity<XCLCMS.Data.WebAPIEntity.ResponseEntity.PageListResponseEntity<XCLCMS.Data.Model.View.v_SysWebSetting>> PageList([FromUri] string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<PageListConditionEntity>>(System.Web.HttpUtility.UrlDecode(json));
            var pager = request.Body.PagerInfoSimple.ToPagerInfo();
            var response = new APIResponseEntity<XCLCMS.Data.WebAPIEntity.ResponseEntity.PageListResponseEntity<XCLCMS.Data.Model.View.v_SysWebSetting>>();
            response.Body = new Data.WebAPIEntity.ResponseEntity.PageListResponseEntity<Data.Model.View.v_SysWebSetting>();

            //限制商户
            if (base.IsOnlyCurrentMerchant)
            {
                request.Body.Where = XCLNetTools.DataBase.SQLLibrary.JoinWithAnd(new List<string>() {
                    request.Body.Where,
                    string.Format("FK_MerchantID={0}",base.CurrentUserModel.FK_MerchantID)
                });
            }

            response.Body.ResultList = vSysWebSettingBLL.GetPageList(pager, request.Body.Where, "", "[SysWebSettingID]", "[KeyName] asc");
            response.Body.PagerInfo = pager;
            response.IsSuccess = true;
            return response;
        }

        /// <summary>
        /// 判断系统配置名是否存在
        /// </summary>
        [HttpGet]
        public APIResponseEntity<bool> IsExistKeyName([FromUri] string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<XCLCMS.Data.WebAPIEntity.RequestEntity.SysWebSetting.IsExistKeyNameEntity>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<bool>();
            response.IsSuccess = true;
            response.Message = "该配置名可以使用！";

            request.Body.KeyName = (request.Body.KeyName ?? "").Trim();

            if (request.Body.SysWebSettingID > 0)
            {
                var model = this.sysWebSettingBLL.GetModel(request.Body.SysWebSettingID);
                if (null != model)
                {
                    if (string.Equals(request.Body.KeyName, model.KeyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return response;
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.Body.KeyName))
            {
                bool isExist = this.sysWebSettingBLL.IsExistKeyName(request.Body.KeyName);
                if (isExist)
                {
                    response.IsSuccess = false;
                    response.Message = "该配置名已被占用！";
                }
            }

            return response;
        }

        /// <summary>
        /// 新增系统配置信息
        /// </summary>
        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysWebSettingAdd)]
        public APIResponseEntity<bool> Add(JObject obj)
        {
            var request = obj.ToObject<APIRequestEntity<XCLCMS.Data.Model.SysWebSetting>>();
            var response = new APIResponseEntity<bool>();

            #region 数据校验

            request.Body.KeyName = (request.Body.KeyName ?? "").Trim();
            request.Body.FK_MerchantID = base.CurrentUserModel.FK_MerchantID;

            if (string.IsNullOrWhiteSpace(request.Body.KeyName))
            {
                response.IsSuccess = false;
                response.Message = "请提供配置名！";
                return response;
            }

            if (this.sysWebSettingBLL.IsExistKeyName(request.Body.KeyName))
            {
                response.IsSuccess = false;
                response.Message = string.Format("配置名【{0}】已存在！", request.Body.KeyName);
                return response;
            }

            //限制商户
            if (base.IsOnlyCurrentMerchant && request.Body.FK_MerchantID != base.CurrentUserModel.FK_MerchantID)
            {
                response.IsSuccess = false;
                response.Message = "只能在自己所属的商户下面添加配置信息！";
                return response;
            }

            //应用号与商户一致
            if (!this.merchantAppBLL.IsTheSameMerchantInfoID(request.Body.FK_MerchantID, request.Body.FK_MerchantAppID))
            {
                response.IsSuccess = false;
                response.Message = "商户号与应用号不匹配，请核对后再试！";
                return response;
            }

            #endregion 数据校验

            response.IsSuccess = this.sysWebSettingBLL.Add(request.Body);
            if (response.Body)
            {
                response.Message = "系统配置信息添加成功！";
            }
            else
            {
                response.Message = "系统配置信息添加失败！";
            }
            return response;
        }

        /// <summary>
        /// 修改系统配置信息
        /// </summary>
        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysWebSettingEdit)]
        public APIResponseEntity<bool> Update(JObject obj)
        {
            var request = obj.ToObject<APIRequestEntity<XCLCMS.Data.Model.SysWebSetting>>();
            var response = new APIResponseEntity<bool>();

            #region 数据校验

            var model = this.sysWebSettingBLL.GetModel(request.Body.SysWebSettingID);
            if (null == model)
            {
                response.IsSuccess = false;
                response.Message = "请指定有效的商户信息！";
                return response;
            }

            if (!string.Equals(model.KeyName, request.Body.KeyName))
            {
                if (this.sysWebSettingBLL.IsExistKeyName(request.Body.KeyName))
                {
                    response.IsSuccess = false;
                    response.Message = string.Format("配置名【{0}】已存在！", request.Body.KeyName);
                    return response;
                }
            }

            //限制商户
            if (base.IsOnlyCurrentMerchant && request.Body.FK_MerchantID != base.CurrentUserModel.FK_MerchantID)
            {
                response.IsSuccess = false;
                response.Message = "只能在自己所属的商户下面修改配置信息！";
                return response;
            }

            //应用号与商户一致
            if (!this.merchantAppBLL.IsTheSameMerchantInfoID(request.Body.FK_MerchantID, request.Body.FK_MerchantAppID))
            {
                response.IsSuccess = false;
                response.Message = "商户号与应用号不匹配，请核对后再试！";
                return response;
            }

            #endregion 数据校验

            model.FK_MerchantAppID = request.Body.FK_MerchantAppID;
            model.FK_MerchantID = request.Body.FK_MerchantID;
            model.KeyName = request.Body.KeyName;
            model.KeyValue = request.Body.KeyValue;
            model.RecordState = request.Body.RecordState;
            model.Remark = request.Body.Remark;
            model.UpdaterID = base.CurrentUserModel.UserInfoID;
            model.UpdaterName = base.CurrentUserModel.UserName;
            model.UpdateTime = DateTime.Now;

            response.IsSuccess = this.sysWebSettingBLL.Update(model);
            if (response.IsSuccess)
            {
                response.Message = "系统配置信息修改成功！";
            }
            else
            {
                response.Message = "系统配置信息修改失败！";
            }
            return response;
        }

        /// <summary>
        /// 删除系统配置信息
        /// </summary>
        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysWebSettingDel)]
        public APIResponseEntity<bool> Delete(JObject obj)
        {
            var request = obj.ToObject<APIRequestEntity<List<long>>>();
            var response = new APIResponseEntity<bool>();

            if (request.Body.IsNotNullOrEmpty())
            {
                request.Body = request.Body.Where(k => k > 0).Distinct().ToList();
            }

            if (request.Body.IsNullOrEmpty())
            {
                response.IsSuccess = false;
                response.Message = "请指定要删除的系统配置ID！";
                return response;
            }

            //限制商户
            if (base.IsOnlyCurrentMerchant)
            {
                if (request.Body.Exists(id =>
                {
                    var settingModel = this.sysWebSettingBLL.GetModel(id);
                    return null != settingModel && settingModel.FK_MerchantID != base.CurrentUserModel.FK_MerchantID;
                }))
                {
                    response.IsSuccess = false;
                    response.Message = "只能删除属于自己的商户节点！";
                    return response;
                }
            }

            foreach (var k in request.Body)
            {
                var model = this.sysWebSettingBLL.GetModel(k);
                if (null == model)
                {
                    continue;
                }

                model.UpdaterID = base.CurrentUserModel.UserInfoID;
                model.UpdaterName = base.CurrentUserModel.UserName;
                model.UpdateTime = DateTime.Now;
                model.RecordState = XCLCMS.Data.CommonHelper.EnumType.RecordStateEnum.R.ToString();
                if (!this.sysWebSettingBLL.Update(model))
                {
                    response.IsSuccess = false;
                    response.Message = "系统配置删除失败！";
                    return response;
                }
            }

            response.IsSuccess = true;
            response.Message = "已成功删除系统配置！";
            response.IsRefresh = true;

            return response;
        }
    }
}