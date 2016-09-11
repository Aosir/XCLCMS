﻿using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using XCLCMS.Data.Model.Custom;

namespace XCLCMS.Data.DAL
{
    /// <summary>
    /// 数据访问类:Tags
    /// </summary>
    public partial class Tags : XCLCMS.Data.DAL.Common.BaseDAL
    {
        public Tags()
        { }

        #region Method

        /// <summary>
        ///  增加一条数据
        /// </summary>
        public bool Add(XCLCMS.Data.Model.Tags model)
        {
            Database db = base.CreateDatabase();
            DbCommand dbCommand = db.GetStoredProcCommand("sp_Tags_ADD");
            db.AddInParameter(dbCommand, "TagsID", DbType.Int64, model.TagsID);
            db.AddInParameter(dbCommand, "TagName", DbType.String, model.TagName);
            db.AddInParameter(dbCommand, "Description", DbType.String, model.Description);
            db.AddInParameter(dbCommand, "FK_MerchantID", DbType.Int64, model.FK_MerchantID);
            db.AddInParameter(dbCommand, "FK_MerchantAppID", DbType.Int64, model.FK_MerchantAppID);
            db.AddInParameter(dbCommand, "RecordState", DbType.AnsiString, model.RecordState);
            db.AddInParameter(dbCommand, "CreateTime", DbType.DateTime, model.CreateTime);
            db.AddInParameter(dbCommand, "CreaterID", DbType.Int64, model.CreaterID);
            db.AddInParameter(dbCommand, "CreaterName", DbType.String, model.CreaterName);
            db.AddInParameter(dbCommand, "UpdateTime", DbType.DateTime, model.UpdateTime);
            db.AddInParameter(dbCommand, "UpdaterID", DbType.Int64, model.UpdaterID);
            db.AddInParameter(dbCommand, "UpdaterName", DbType.String, model.UpdaterName);

            db.AddOutParameter(dbCommand, "ResultCode", DbType.Int32, 4);
            db.AddOutParameter(dbCommand, "ResultMessage", DbType.String, 1000);
            db.ExecuteNonQuery(dbCommand);

            var result = XCLCMS.Data.DAL.Common.Common.GetProcedureResult(dbCommand.Parameters);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                throw new Exception(result.ResultMessage);
            }
        }

        /// <summary>
        ///  更新一条数据
        /// </summary>
        public bool Update(XCLCMS.Data.Model.Tags model)
        {
            Database db = base.CreateDatabase();
            DbCommand dbCommand = db.GetStoredProcCommand("sp_Tags_Update");
            db.AddInParameter(dbCommand, "TagsID", DbType.Int64, model.TagsID);
            db.AddInParameter(dbCommand, "TagName", DbType.String, model.TagName);
            db.AddInParameter(dbCommand, "Description", DbType.String, model.Description);
            db.AddInParameter(dbCommand, "FK_MerchantID", DbType.Int64, model.FK_MerchantID);
            db.AddInParameter(dbCommand, "FK_MerchantAppID", DbType.Int64, model.FK_MerchantAppID);
            db.AddInParameter(dbCommand, "RecordState", DbType.AnsiString, model.RecordState);
            db.AddInParameter(dbCommand, "CreateTime", DbType.DateTime, model.CreateTime);
            db.AddInParameter(dbCommand, "CreaterID", DbType.Int64, model.CreaterID);
            db.AddInParameter(dbCommand, "CreaterName", DbType.String, model.CreaterName);
            db.AddInParameter(dbCommand, "UpdateTime", DbType.DateTime, model.UpdateTime);
            db.AddInParameter(dbCommand, "UpdaterID", DbType.Int64, model.UpdaterID);
            db.AddInParameter(dbCommand, "UpdaterName", DbType.String, model.UpdaterName);

            db.AddOutParameter(dbCommand, "ResultCode", DbType.Int32, 4);
            db.AddOutParameter(dbCommand, "ResultMessage", DbType.String, 1000);
            db.ExecuteNonQuery(dbCommand);

            var result = XCLCMS.Data.DAL.Common.Common.GetProcedureResult(dbCommand.Parameters);
            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                throw new Exception(result.ResultMessage);
            }
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public XCLCMS.Data.Model.Tags GetModel(long TagsID)
        {
            Database db = base.CreateDatabase();
            DbCommand dbCommand = db.GetSqlStringCommand("select * from Tags  WITH(NOLOCK)  where TagsID=@TagsID");
            db.AddInParameter(dbCommand, "TagsID", DbType.Int64, TagsID);
            DataSet ds = db.ExecuteDataSet(dbCommand);
            var lst = XCLNetTools.Generic.ListHelper.DataTableToList<XCLCMS.Data.Model.Tags>(ds.Tables[0]);
            return null != lst && lst.Count > 0 ? lst[0] : null;
        }

        #endregion Method

        #region Extends

        /// <summary>
        /// 判断指定标签是否存在
        /// </summary>
        public bool IsExist(Tags_NameCondition condition)
        {
            return null != this.GetModel(condition);
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public XCLCMS.Data.Model.Tags GetModel(Tags_NameCondition condition)
        {
            Database db = base.CreateDatabase();
            DbCommand dbCommand = db.GetSqlStringCommand("select top 1 * from tags with(nolock) where TagName=@TagName and FK_MerchantID=@FK_MerchantID and FK_MerchantAppID=@FK_MerchantAppID");
            db.AddInParameter(dbCommand, "TagName", DbType.String, condition.TagName);
            db.AddInParameter(dbCommand, "FK_MerchantID", DbType.Int64, condition.FK_MerchantID);
            db.AddInParameter(dbCommand, "FK_MerchantAppID", DbType.Int64, condition.FK_MerchantAppID);
            DataSet ds = db.ExecuteDataSet(dbCommand);
            var lst = XCLNetTools.Generic.ListHelper.DataTableToList<XCLCMS.Data.Model.Tags>(ds.Tables[0]);
            return null != lst && lst.Count > 0 ? lst[0] : null;
        }

        /// <summary>
        /// 批量添加tags，并返回添加成功的tagid列表
        /// </summary>
        public XCLNetTools.Entity.MethodResult<Tags_AddMethodResult> Add(List<XCLCMS.Data.Model.Tags> lst)
        {
            var result = new XCLNetTools.Entity.MethodResult<Tags_AddMethodResult>();
            result.Result = new Tags_AddMethodResult()
            {
                AddedTagIdList = new List<long>(),
                ExistTagIdList = new List<long>()
            };
            if (null == lst || lst.Count == 0)
            {
                result.IsSuccess = true;
                return result;
            }

            lst = lst.Distinct().Where(k => !string.IsNullOrWhiteSpace(k.TagName)).ToList();

            foreach (var m in lst)
            {
                //判断标签是否存在
                var model = this.GetModel((new Tags_NameCondition()
                {
                    FK_MerchantAppID = m.FK_MerchantAppID,
                    FK_MerchantID = m.FK_MerchantID,
                    TagName = m.TagName
                }));
                if (null != model)
                {
                    result.Result.ExistTagIdList.Add(model.TagsID);
                    continue;
                }

                //添加标签
                if (this.Add(m))
                {
                    result.Result.AddedTagIdList.Add(m.TagsID);
                }
            }

            result.IsSuccess = true;

            return result;
        }

        #endregion Extends
    }
}