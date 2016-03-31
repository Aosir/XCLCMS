﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XCLCMS.FileManager.Controllers
{
    public class UploadController : Controller
    {
        /// <summary>
        /// 上传
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        public JsonResult UploadSubmit(FormCollection fm)
        {
            XCLNetTools.Message.MessageModel msgModel = new XCLNetTools.Message.MessageModel();

            #region 基本信息

            HttpPostedFileBase file = Request.Files["FileInfo"];
            DateTime dtNow = DateTime.Now;
            string name = string.Format("{0:yyyyMMdd}_{1}", dtNow, System.Guid.NewGuid().ToString("N"));
            string ext = XCLNetTools.FileHandler.ComFile.GetExtName(file.FileName);
            string newFileName = string.Format("{0}.{1}", name, ext);
            //附件主目录
            string directoryPath = string.Format("{0}/{1}/{2}/", XCLCMS.Lib.SysWebSetting.Setting.SettingModel.FileManager_UploadPath.TrimEnd('/'), dtNow.Year, dtNow.Month);
            //附件临时目录
            string directoryTempPath = string.Format("{0}/{1}/{2}/", XCLCMS.Lib.SysWebSetting.Setting.SettingModel.FileManager_UploadPathTemp.TrimEnd('/'), dtNow.Year, dtNow.Month);

            //保存后的相对路径
            string relativePath = string.Empty;
            //原图保存后的物理地址
            string savedServerPath = string.Empty;
            //主文件id
            long mainFileID = 0;

            //生成目录
            XCLNetTools.FileHandler.FileDirectory.MakeDirectory(Server.MapPath(directoryPath));
            XCLNetTools.FileHandler.FileDirectory.MakeDirectory(Server.MapPath(directoryTempPath));

            #endregion 基本信息

            #region 文件参数

            string fileSetting = fm["FileSetting"] ?? "";
            XCLCMS.FileManager.Models.Uploader.FileSetting settingModel = null;
            if (!string.IsNullOrEmpty(fileSetting))
            {
                settingModel = XCLNetTools.Serialize.JSON.DeSerialize<XCLCMS.FileManager.Models.Uploader.FileSetting>(fileSetting);
            }

            if (null == settingModel)
            {
                msgModel.IsSuccess = false;
                msgModel.Message = "参数设置无效！";
                return Json(msgModel);
            }

            if (null != settingModel.ThumbImgSettings && settingModel.ThumbImgSettings.Count > 0)
            {
                settingModel.ThumbImgSettings = settingModel.ThumbImgSettings.Where(k => k.Width > 0 && k.Height > 0).Distinct().ToList();
            }

            #endregion 文件参数

            #region 裁剪主图或直接保存文件

            if (settingModel.IsNeedCrop)
            {
                //如果需要裁剪，则原图先保存到临时文件夹中
                savedServerPath = Server.MapPath(string.Format("{0}/{1}", directoryTempPath.TrimEnd('/'), newFileName));
                file.SaveAs(savedServerPath);

                using (var img = XCLNetTools.FileHandler.ImgLib.Crop(savedServerPath, settingModel.ImgCropWidth, settingModel.ImgCropHeight, settingModel.ImgX1, settingModel.ImgY1))
                {
                    if (null != img)
                    {
                        relativePath = string.Format("{0}/{1}_{2}_{3}.{4}", directoryPath.TrimEnd('/'), name, settingModel.ImgCropWidth, settingModel.ImgCropHeight, ext);
                        savedServerPath = Server.MapPath(relativePath);
                        img.Save(savedServerPath);

                        mainFileID = this.SaveFileInfoToDB(0, settingModel, relativePath, settingModel.ImgCropWidth, settingModel.ImgCropHeight);
                    }
                }
            }
            else
            {
                //如果不裁剪，则直接保存原图
                relativePath = string.Format("{0}/{1}", directoryPath.TrimEnd('/'), newFileName);
                savedServerPath = Server.MapPath(relativePath);
                file.SaveAs(savedServerPath);

                mainFileID = this.SaveFileInfoToDB(0, settingModel, relativePath, settingModel.ImgWidth, settingModel.ImgHeight);
            }

            if (mainFileID == 0)
            {
                msgModel.IsSuccess = false;
                msgModel.Message = "主文件保存失败，请重新上传！";
                return Json(msgModel);
            }

            #endregion 裁剪主图或直接保存文件

            #region 如果是图片，则根据主图的参数设置再生成缩略图

            if (null != settingModel.ThumbImgSettings && settingModel.ThumbImgSettings.Count > 0)
            {
                foreach (var thumb in settingModel.ThumbImgSettings)
                {
                    using (var img = XCLNetTools.FileHandler.ImgLib.Crop(savedServerPath, thumb.Width, thumb.Height, 0, 0))
                    {
                        if (null != img)
                        {
                            relativePath = string.Format("{0}/{1}_{2}_{3}.{4}", directoryPath.TrimEnd('/'), name, thumb.Width, thumb.Height, ext);
                            string thumbPath = Server.MapPath(relativePath);
                            img.Save(thumbPath);

                            this.SaveFileInfoToDB(mainFileID, settingModel, relativePath, thumb.Width, thumb.Height);
                        }
                    }
                }
            }

            #endregion 如果是图片，则根据主图的参数设置再生成缩略图

            msgModel.IsSuccess = true;
            msgModel.Message = "上传成功！";

            return Json(msgModel);
        }

        /// <summary>
        /// 保存文件信息到数据库
        /// </summary>
        /// <param name="parentId">父文件ID(如果保存缩略图的话，此id就是主图的id)</param>
        /// <param name="settingModel">设置信息</param>
        /// <param name="relativePath">相对路径</param>
        /// <returns>记录主键ID</returns>
        private long SaveFileInfoToDB(long parentId, XCLCMS.FileManager.Models.Uploader.FileSetting settingModel, string relativePath, int width, int height)
        {
            System.IO.FileInfo info = new System.IO.FileInfo(Server.MapPath(relativePath));
            DateTime dtNow = DateTime.Now;
            XCLCMS.Data.BLL.Attachment bll = new Data.BLL.Attachment();
            XCLCMS.Data.Model.Attachment model = new Data.Model.Attachment();
            model.AttachmentID = XCLCMS.Data.BLL.Common.Common.GenerateID(Data.CommonHelper.EnumType.IDTypeEnum.ATT);
            model.CreaterID = 0;
            model.CreaterName = "";
            model.CreateTime = dtNow;
            model.Description = settingModel.Description;
            model.DownLoadCount = settingModel.DownloadCount;
            model.Ext = (info.Extension ?? "").Trim('.');
            model.FileSize = (decimal)(info.Length / 8.0 / 1024);
            model.FormatType = "";
            model.ImgHeight = height;
            model.ImgWidth = width;
            model.ParentID = parentId;
            model.RecordState = XCLCMS.Data.CommonHelper.EnumType.RecordStateEnum.N.ToString();
            model.Title = settingModel.Title;
            model.UpdaterID = 0;
            model.UpdaterName = "";
            model.UpdateTime = dtNow;
            model.URL = relativePath;
            model.ViewCount = settingModel.ViewCount;
            model.ViewType = settingModel.ViewType;
            return bll.Add(model) ? model.AttachmentID : 0;
        }
    }
}