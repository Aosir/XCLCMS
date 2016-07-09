﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using XCLCMS.Data.WebAPIEntity;
using XCLNetTools.Generic;

namespace XCLCMS.WebAPI.Controllers
{
    /// <summary>
    /// 功能模块 管理
    /// </summary>
    public class SysFunctionController : BaseAPIController
    {
        private XCLCMS.Data.BLL.Merchant merchantBLL = new Data.BLL.Merchant();
        private XCLCMS.Data.BLL.SysFunction sysFunctionBLL = new Data.BLL.SysFunction();
        private XCLCMS.Data.BLL.View.v_SysFunction vSysFunctionBLL = new Data.BLL.View.v_SysFunction();

        /// <summary>
        /// 查询功能信息实体
        /// </summary>
        [HttpGet]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_SysRoleView)]
        public APIResponseEntity<XCLCMS.Data.Model.SysFunction> Detail([FromUri] string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<long>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<XCLCMS.Data.Model.SysFunction>();
            response.Body = this.sysFunctionBLL.GetModel(request.Body);
            response.IsSuccess = true;
            return response;
        }

        /// <summary>
        /// 判断功能标识是否已经存在
        /// </summary>
        [HttpGet]
        public APIResponseEntity<bool> IsExistCode([FromUri] string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<XCLCMS.Data.WebAPIEntity.RequestEntity.SysFunction.IsExistCodeEntity>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<bool>();
            response.IsSuccess = true;
            response.Message = "该标识可以使用！";

            XCLCMS.Data.Model.SysFunction model = null;
            if (request.Body.SysFunctionID > 0)
            {
                model = sysFunctionBLL.GetModel(request.Body.SysFunctionID);
                if (null != model)
                {
                    if (string.Equals(request.Body.Code, model.Code, StringComparison.OrdinalIgnoreCase))
                    {
                        return response;
                    }
                }
            }
            if (!string.IsNullOrEmpty(request.Body.Code))
            {
                bool isExist = sysFunctionBLL.IsExistCode(request.Body.Code);
                if (isExist)
                {
                    response.IsSuccess = false;
                    response.Message = "该标识名已存在！";
                }
            }
            return response;
        }

        /// <summary>
        /// 判断功能名，在同一级别中是否存在
        /// </summary>
        [HttpGet]
        public APIResponseEntity<bool> IsExistFunctionNameInSameLevel([FromUri] string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<XCLCMS.Data.WebAPIEntity.RequestEntity.SysFunction.IsExistFunctionNameInSameLevelEntity>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<bool>();
            response.IsSuccess = true;
            response.Message = "该功能名可以使用！";

            XCLCMS.Data.Model.SysFunction model = null;

            if (request.Body.SysFunctionID > 0)
            {
                model = sysFunctionBLL.GetModel(request.Body.SysFunctionID);
                if (null != model)
                {
                    if (string.Equals(request.Body.FunctionName, model.FunctionName, StringComparison.OrdinalIgnoreCase))
                    {
                        return response;
                    }
                }
            }

            List<XCLCMS.Data.Model.SysFunction> lst = sysFunctionBLL.GetChildListByID(request.Body.ParentID);
            if (lst.IsNotNullOrEmpty())
            {
                if (lst.Exists(k => string.Equals(k.FunctionName, request.Body.FunctionName, StringComparison.OrdinalIgnoreCase)))
                {
                    response.IsSuccess = false;
                    response.Message = "该功能名在当前层级中已存在！";
                }
            }

            return response;
        }

        /// <summary>
        /// 查询所有功能列表
        /// </summary>
        [HttpGet]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysFunctionView)]
        public APIResponseEntity<List<XCLCMS.Data.Model.View.v_SysFunction>> GetList(string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<long>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<List<XCLCMS.Data.Model.View.v_SysFunction>>();
            response.Body = this.vSysFunctionBLL.GetList(request.Body);
            response.IsSuccess = true;
            return response;
        }

        /// <summary>
        /// 获取easyui tree格式的所有功能json
        /// </summary>
        [HttpGet]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysFunctionView)]
        public APIResponseEntity<List<XCLNetTools.Entity.EasyUI.TreeItem>> GetAllJsonForEasyUITree([FromUri] string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<XCLCMS.Data.WebAPIEntity.RequestEntity.SysFunction.GetAllJsonForEasyUITreeEntity>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<List<XCLNetTools.Entity.EasyUI.TreeItem>>();
            response.IsSuccess = true;

            List<XCLCMS.Data.Model.View.v_SysFunction> allData = null;
            List<XCLNetTools.Entity.EasyUI.TreeItem> tree = new List<XCLNetTools.Entity.EasyUI.TreeItem>();

            var merchantModel = this.merchantBLL.GetModel(request.Body.MerchantID);
            if (null == merchantModel)
            {
                response.IsSuccess = false;
                response.Message = "您指定的商户号无效！";
                return response;
            }

            //根据情况，是否只显示普通商户的功能权限以供选择
            if (merchantModel.MerchantSystemType == XCLCMS.Data.CommonHelper.EnumType.MerchantSystemTypeEnum.NOR.ToString())
            {
                allData = XCLCMS.Lib.Permission.PerHelper.GetNormalMerchantFunctionTreeList();
            }
            else
            {
                //所有权限功能
                allData = this.vSysFunctionBLL.GetModelList("");
            }

            if (allData.IsNotNullOrEmpty())
            {
                var root = allData.Where(k => k.ParentID == 0).FirstOrDefault();//根节点
                if (null != root)
                {
                    tree.Add(new XCLNetTools.Entity.EasyUI.TreeItem()
                    {
                        ID = root.SysFunctionID.ToString(),
                        State = root.IsLeaf == 1 ? "open" : "closed",
                        Text = root.FunctionName
                    });

                    Action<XCLNetTools.Entity.EasyUI.TreeItem> getChildAction = null;
                    getChildAction = new Action<XCLNetTools.Entity.EasyUI.TreeItem>((parentModel) =>
                    {
                        var childs = allData.Where(k => k.ParentID == Convert.ToInt64(parentModel.ID)).ToList();
                        if (childs.IsNotNullOrEmpty())
                        {
                            parentModel.Children = new List<XCLNetTools.Entity.EasyUI.TreeItem>();
                            childs.ForEach(m =>
                            {
                                var treeItem = new XCLNetTools.Entity.EasyUI.TreeItem()
                                {
                                    ID = m.SysFunctionID.ToString(),
                                    State = m.IsLeaf == 1 ? "open" : "closed",
                                    Text = m.FunctionName
                                };
                                getChildAction(treeItem);
                                parentModel.Children.Add(treeItem);
                            });
                        }
                    });

                    //从根节点开始
                    getChildAction(tree[0]);
                }
            }
            response.Body = tree;
            return response;
        }

        /// <summary>
        /// 获取当前SysFunctionID所属的层级list
        /// 如:根目录/子目录/文件
        /// </summary>
        [HttpGet]
        public APIResponseEntity<List<XCLCMS.Data.Model.Custom.SysFunctionSimple>> GetLayerListBySysFunctionId(string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<XCLCMS.Data.WebAPIEntity.RequestEntity.SysFunction.GetLayerListBySysFunctionIdEntity>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<List<XCLCMS.Data.Model.Custom.SysFunctionSimple>>();
            response.Body = this.sysFunctionBLL.GetLayerListBySysFunctionId(request.Body.SysFunctionId);
            response.IsSuccess = true;
            return response;
        }

        /// <summary>
        /// 获取指定角色的所有功能
        /// </summary>
        [HttpGet]
        public APIResponseEntity<List<XCLCMS.Data.Model.SysFunction>> GetListByRoleID(string json)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIRequestEntity<long>>(System.Web.HttpUtility.UrlDecode(json));
            var response = new APIResponseEntity<List<XCLCMS.Data.Model.SysFunction>>();
            response.Body = this.sysFunctionBLL.GetListByRoleID(request.Body);
            response.IsSuccess = true;
            return response;
        }

        /// <summary>
        /// 添加功能
        /// </summary>
        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysFunctionAdd)]
        public APIResponseEntity<bool> Add([FromBody] APIRequestEntity<XCLCMS.Data.Model.SysFunction> request)
        {
            var response = new APIResponseEntity<bool>();

            #region 数据校验

            request.Body.FunctionName = (request.Body.FunctionName ?? "").Trim();
            request.Body.Code = (request.Body.Code ?? "").Trim();

            //字典名必填
            if (string.IsNullOrEmpty(request.Body.FunctionName))
            {
                response.IsSuccess = false;
                response.Message = "请提供功能名！";
                return response;
            }

            //若有code，则判断是否唯一
            if (!string.IsNullOrEmpty(request.Body.Code))
            {
                if (this.sysFunctionBLL.IsExistCode(request.Body.Code))
                {
                    response.IsSuccess = false;
                    response.Message = string.Format("功能唯一标识【{0}】已存在！", request.Body.Code);
                    return response;
                }
            }

            #endregion 数据校验

            if (this.sysFunctionBLL.Add(request.Body))
            {
                response.IsSuccess = true;
                response.Message = "添加成功！";
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "添加失败！";
            }

            return response;
        }

        /// <summary>
        /// 修改功能
        /// </summary>
        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysFunctionEdit)]
        public APIResponseEntity<bool> Update([FromBody] APIRequestEntity<XCLCMS.Data.Model.SysFunction> request)
        {
            var response = new APIResponseEntity<bool>();

            #region 数据校验

            var model = this.sysFunctionBLL.GetModel(request.Body.SysFunctionID);
            if (null == model)
            {
                response.IsSuccess = false;
                response.Message = "请指定有效的功能信息！";
                return response;
            }

            request.Body.FunctionName = (request.Body.FunctionName ?? "").Trim();
            request.Body.Code = (request.Body.Code ?? "").Trim();

            //功能名必填
            if (string.IsNullOrEmpty(request.Body.FunctionName))
            {
                response.IsSuccess = false;
                response.Message = "请提供功能名！";
                return response;
            }

            //若有code，则判断是否唯一
            if (!string.IsNullOrEmpty(request.Body.Code))
            {
                if (!string.Equals(model.Code, request.Body.Code) && this.sysFunctionBLL.IsExistCode(request.Body.Code))
                {
                    response.IsSuccess = false;
                    response.Message = string.Format("功能唯一标识【{0}】已存在！", request.Body.Code);
                    return response;
                }
            }

            #endregion 数据校验

            model.Code = request.Body.Code;
            model.Remark = request.Body.Remark;
            model.UpdaterID = base.CurrentUserModel.UserInfoID;
            model.UpdaterName = base.CurrentUserModel.UserName;
            model.UpdateTime = DateTime.Now;
            model.FunctionName = request.Body.FunctionName;

            if (this.sysFunctionBLL.Update(model))
            {
                response.IsSuccess = true;
                response.Message = "修改成功！";
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "修改失败！";
            }

            return response;
        }

        /// <summary>
        /// 删除功能
        /// </summary>
        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysFunctionDel)]
        public APIResponseEntity<bool> Delete([FromBody] APIRequestEntity<List<long>> request)
        {
            var response = new APIResponseEntity<bool>();

            if (null == request.Body || request.Body.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "请指定要删除的功能ID！";
                return response;
            }

            request.Body = request.Body.Distinct().ToList();

            int successCount = 0;

            request.Body.ForEach(id =>
            {
                var sysDicModel = this.sysFunctionBLL.GetModel(id);
                if (null != sysDicModel)
                {
                    sysDicModel.UpdaterID = base.CurrentUserModel.UserInfoID;
                    sysDicModel.UpdaterName = base.CurrentUserModel.UserName;
                    sysDicModel.UpdateTime = DateTime.Now;
                    sysDicModel.RecordState = XCLCMS.Data.CommonHelper.EnumType.RecordStateEnum.D.ToString();
                    if (this.sysFunctionBLL.Update(sysDicModel))
                    {
                        successCount++;
                    }
                }
            });

            response.IsSuccess = true;
            response.Message = string.Format("已成功删除【{0}】条记录！", successCount);

            return response;
        }

        /// <summary>
        /// 删除指定功能的所有节点
        /// </summary>
        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_Set_SysFunctionDel)]
        public APIResponseEntity<bool> DelChild([FromBody] APIRequestEntity<long> request)
        {
            var response = new APIResponseEntity<bool>();

            if (request.Body <= 0)
            {
                response.IsSuccess = false;
                response.Message = "请指定要删除所有子节点的功能ID！";
                return response;
            }

            response.IsSuccess = this.sysFunctionBLL.DelChild(new Data.Model.SysFunction()
            {
                SysFunctionID = request.Body,
                UpdaterID = base.CurrentUserModel.UserInfoID,
                UpdaterName = base.CurrentUserModel.UserName,
                UpdateTime = DateTime.Now
            });

            if (response.IsSuccess)
            {
                response.Message = "成功删除所有子节点！";
            }
            else
            {
                response.Message = "删除所有子节点失败！";
            }

            return response;
        }
    }
}