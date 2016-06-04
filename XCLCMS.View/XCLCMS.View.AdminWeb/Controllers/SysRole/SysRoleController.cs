﻿using System;
using System.Linq;
using System.Web.Mvc;
using XCLNetTools.Generic;

namespace XCLCMS.View.AdminWeb.Controllers.SysRole
{
    /// <summary>
    /// 角色controller
    /// </summary>
    public class SysRoleController : BaseController
    {
        public ActionResult Index()
        {
            return View("~/Views/SysRole/SysRoleList.cshtml");
        }

        /// <summary>
        /// 添加与编辑页面首页
        /// </summary>
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_SysRoleAdd)]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_SysRoleEdit)]
        public ActionResult Add()
        {
            long sysRoleID = XCLNetTools.StringHander.FormHelper.GetLong("SysRoleID");

            XCLCMS.Data.BLL.SysRole bll = new Data.BLL.SysRole();
            XCLCMS.Data.BLL.SysFunction functionBLL = new Data.BLL.SysFunction();
            XCLCMS.View.AdminWeb.Models.SysRole.SysRoleAddVM viewModel = new XCLCMS.View.AdminWeb.Models.SysRole.SysRoleAddVM();

            switch (base.CurrentHandleType)
            {
                case XCLCMS.Lib.Common.Comm.HandleType.ADD:
                    viewModel.SysRole = new Data.Model.SysRole();
                    viewModel.ParentID = sysRoleID;
                    viewModel.SysRoleID = -1;
                    viewModel.FormAction = Url.Action("AddSubmit", "SysRole");
                    break;

                case XCLCMS.Lib.Common.Comm.HandleType.UPDATE:

                    var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<long>(base.UserToken);
                    request.Body = sysRoleID;
                    var response = XCLCMS.Lib.WebAPI.SysRoleAPI.Detail(request);

                    viewModel.SysRole = response.Body;
                    viewModel.ParentID = response.Body.ParentID;
                    viewModel.SysRoleID = response.Body.SysRoleID;
                    var roleHadFunctions = functionBLL.GetListByRoleID(sysRoleID);
                    if (roleHadFunctions.IsNotNullOrEmpty())
                    {
                        viewModel.RoleFunctionIDList = roleHadFunctions.Select(m => m.SysFunctionID).ToList();
                    }
                    viewModel.FormAction = Url.Action("UpdateSubmit", "SysRole");
                    break;
            }

            viewModel.PathList = bll.GetLayerListBySysRoleID(sysRoleID);

            return View("~/Views/SysRole/SysRoleAdd.cshtml", viewModel);
        }

        /// <summary>
        /// 将表单值转为viewModel
        /// </summary>
        private XCLCMS.View.AdminWeb.Models.SysRole.SysRoleAddVM GetViewModel(FormCollection fm)
        {
            XCLCMS.View.AdminWeb.Models.SysRole.SysRoleAddVM viewModel = new XCLCMS.View.AdminWeb.Models.SysRole.SysRoleAddVM();
            viewModel.SysRole = new Data.Model.SysRole();
            viewModel.SysRoleID = XCLNetTools.Common.DataTypeConvert.ToLong(fm["SysRoleID"]);
            viewModel.ParentID = XCLNetTools.Common.DataTypeConvert.ToLong(fm["ParentID"]);
            viewModel.SysRole.Code = (fm["txtCode"] ?? "").Trim();
            viewModel.SysRole.RoleName = (fm["txtRoleName"] ?? "").Trim();
            viewModel.SysRole.Remark = (fm["txtRemark"] ?? "").Trim();
            viewModel.SysRole.Weight = XCLNetTools.Common.DataTypeConvert.ToIntNull(fm["txtWeight"]);
            viewModel.SysRole.FK_MerchantID = base.CurrentUserModel.FK_MerchantID;
            viewModel.RoleFunctionIDList = XCLNetTools.StringHander.FormHelper.GetLongList("txtRoleFunction");
            return viewModel;
        }

        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_SysRoleAdd)]
        public override ActionResult AddSubmit(FormCollection fm)
        {
            base.AddSubmit(fm);
            XCLCMS.View.AdminWeb.Models.SysRole.SysRoleAddVM viewModel = this.GetViewModel(fm);

            XCLCMS.Data.BLL.SysRole bll = new Data.BLL.SysRole();
            XCLCMS.Data.Model.SysRole model = null;
            model = new Data.Model.SysRole();
            model.CreaterID = base.CurrentUserModel.UserInfoID;
            model.CreaterName = base.CurrentUserModel.UserName;
            model.CreateTime = DateTime.Now;
            model.UpdaterID = base.CurrentUserModel.UserInfoID;
            model.UpdaterName = base.CurrentUserModel.UserName;
            model.UpdateTime = DateTime.Now;
            model.ParentID = viewModel.ParentID;
            model.RoleName = viewModel.SysRole.RoleName;
            model.Remark = viewModel.SysRole.Remark;
            model.RecordState = XCLCMS.Data.CommonHelper.EnumType.RecordStateEnum.N.ToString();
            model.SysRoleID = XCLCMS.Data.BLL.Common.Common.GenerateID(Data.CommonHelper.EnumType.IDTypeEnum.RLE);
            model.Code = viewModel.SysRole.Code;
            model.Weight = viewModel.SysRole.Weight;
            model.FK_MerchantID = viewModel.SysRole.FK_MerchantID;

            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<XCLCMS.Data.WebAPIEntity.RequestEntity.SysRole.AddOrUpdateEntity>(base.UserToken);
            request.Body = new Data.WebAPIEntity.RequestEntity.SysRole.AddOrUpdateEntity();
            request.Body.SysRole = model;
            request.Body.FunctionIdList = viewModel.RoleFunctionIDList;
            var response = XCLCMS.Lib.WebAPI.SysRoleAPI.Add(request);

            return Json(response);
        }

        [HttpPost]
        [XCLCMS.Lib.Filters.FunctionFilter(Function = XCLCMS.Lib.Permission.Function.FunctionEnum.SysFun_SysRoleEdit)]
        public override ActionResult UpdateSubmit(FormCollection fm)
        {
            base.UpdateSubmit(fm);
            XCLCMS.View.AdminWeb.Models.SysRole.SysRoleAddVM viewModel = this.GetViewModel(fm);
            XCLCMS.Data.BLL.SysRole bll = new Data.BLL.SysRole();
            XCLCMS.Data.Model.SysRole model = null;
            model = bll.GetModel(viewModel.SysRoleID);
            model.RoleName = viewModel.SysRole.RoleName;
            model.UpdaterID = base.CurrentUserModel.UserInfoID;
            model.UpdaterName = base.CurrentUserModel.UserName;
            model.UpdateTime = DateTime.Now;
            model.Remark = viewModel.SysRole.Remark;
            model.Code = viewModel.SysRole.Code;
            model.Weight = viewModel.SysRole.Weight;
            model.FK_MerchantID = viewModel.SysRole.FK_MerchantID;

            var request = XCLCMS.Lib.WebAPI.Library.CreateRequest<XCLCMS.Data.WebAPIEntity.RequestEntity.SysRole.AddOrUpdateEntity>(base.UserToken);
            request.Body = new Data.WebAPIEntity.RequestEntity.SysRole.AddOrUpdateEntity();
            request.Body.SysRole = model;
            request.Body.FunctionIdList = viewModel.RoleFunctionIDList;
            var response = XCLCMS.Lib.WebAPI.SysRoleAPI.Update(request);

            return Json(response);
        }
    }
}