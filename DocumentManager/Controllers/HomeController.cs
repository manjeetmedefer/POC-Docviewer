using Dapper;
using DocumentManager.Models;
using GdPicture14;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DocumentManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
              

        #region Upload Download file  
        public ActionResult FileUpload()
        {
            return View();
        }

        public ActionResult GalleryLocal()
        {
            string LogFileName = System.Web.HttpContext.Current.Session["LogFileName"].ToString();
            string log = string.Empty;

            log += ("Before Calling Gallery view - " + DateTime.Now + System.Environment.NewLine);
            System.IO.File.AppendAllText(@MvcApplication.GetDocumentsDirectory() + "\\Logs\\" + LogFileName + "", log);

            // GalleryModel galleryModel = new GalleryModel { fileDetails = GetFileList(0), fileModel = GetFileList(id) };
            return View("GalleryLocal");
        }

        public ActionResult Gallery(int id)
        {

            GalleryModel galleryModel = new GalleryModel { fileDetails = GetFileList(0) , fileModel = GetFileList(id) };
            return View("Gallery", galleryModel);
        }

        public ActionResult GalleryGetDocumentThumbnail(string doc)
        {
            const int THUMBNAIL_WIDTH = 120;
            const int THUMBNAIL_HEIGHT = 100;
            Color thumbnailBackgroundColor = Color.Transparent;

            if (doc != null)
            {
                string docPath = MvcApplication.GetDocumentsDirectory() + "\\" + HttpUtility.UrlDecode(doc);

                string thumbPath = string.Empty;

                //thumbPath = MvcApplication.GetCacheDirectory() + "\\" + "DOCX.thumb";

                GdPicture14.DocumentFormat documentFormat = new GdPicture14.DocumentFormat();

                switch (Path.GetExtension(doc).ToUpper())
                {
                    case ".PDF":
                        thumbPath = MvcApplication.GetCacheDirectory() + "\\" + "PDF.thumb";
                         break;

                    case ".JPEG":
                        thumbPath = MvcApplication.GetCacheDirectory() + "\\" + "JPEG.thumb";                        
                        break;

                    case ".DOCX":
                        thumbPath = MvcApplication.GetCacheDirectory() + "\\" + "DOCX.thumb";
                        break;

                    case ".XLSX":
                        thumbPath = MvcApplication.GetCacheDirectory() + "\\" + "XLSX.thumb";
                        break;

                    default:
                        thumbPath = MvcApplication.GetCacheDirectory() + "\\" + "PDF.thumb";
                        break;
                }


                if (System.IO.File.Exists(thumbPath))
                {
                    byte[] content = null;
                    // getting thumbnail from cache
                    try
                    {
                        using (FileStream fileStream = new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            content = new byte[fileStream.Length];
                            fileStream.Read(content, 0, (int)fileStream.Length); // safe cast as thumbnail size will never exceed int.maxValue
                        }
                    }
                    catch // can fail during first concurrent accesses.
                    {
                        goto gen_thumb;
                    }
                    return File(content, "image/png");
                }

            gen_thumb:
                // GdPicture14.DocumentFormat documentFormat = GdPicture14.DocumentFormat.DocumentFormatUNKNOWN;

               // GdPicture14.DocumentFormat documentFormat = GdPicture14.DocumentFormat.DocumentFormatDOCX;// h.GetExtension(doc);
                int thumbnailId = 0;
                int pageCount = 0;
                GdPictureStatus status = GdPictureDocumentUtilities.GetDocumentPreview(docPath, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT, thumbnailBackgroundColor.ToArgb(), ref documentFormat, ref thumbnailId, ref pageCount);
                if (status == GdPictureStatus.OK)
                {
                    using (GdPictureImaging gdpictureImaging = new GdPictureImaging())
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            try
                            {
                                if (gdpictureImaging.SaveAsStream(thumbnailId, memoryStream, GdPicture14.DocumentFormat.DocumentFormatTXT, 6) == GdPictureStatus.OK)
                                {
                                    byte[] content = memoryStream.ToArray();
                                    // let's cache result
                                    using (FileStream fileStream = new FileStream(thumbPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                                    {
                                        fileStream.Write(content, 0, content.Length);
                                    }
                                }
                            }
                            finally
                            {
                                GdPictureDocumentUtilities.DisposeImage(thumbnailId);
                            }
                        }
                    }
                }
            }
            return null;
        }
               

        [HttpPost]
        public ActionResult FileUpload(HttpPostedFileBase files)
        {                      
            if(files != null)
                {

                string LogFileName = System.Web.HttpContext.Current.Session["LogFileName"].ToString();
                string log = string.Empty;

                log += ("Before Calling Gallery view for File upload - " + DateTime.Now + System.Environment.NewLine);
                


                Stream str = files.InputStream;
                BinaryReader Br = new BinaryReader(str);
                Byte[] FileDet = Br.ReadBytes((Int32)str.Length);

                FileDetailsModel Fd = new Models.FileDetailsModel();
                Fd.FileName = files.FileName;
                Fd.FileContent = FileDet;

                log += ("Before Save file - " + DateTime.Now + System.Environment.NewLine);

                SaveFileDetails(Fd);

                log += ("After Save file - " + DateTime.Now + System.Environment.NewLine);

                ViewBag.Message = "File Uploaded succesfully .... ";


                System.IO.File.AppendAllText(@MvcApplication.GetDocumentsDirectory() + "\\Logs\\" + LogFileName + "", log);

            }
                List<FileDetailsModel> DetList = GetFileList(0);
                return PartialView("FileDetails", DetList);
        }

        [HttpGet]
        public ActionResult ViewDocumentAction(int id)
        {
            System.Diagnostics.Debug.WriteLine("Call start - " + DateTime.Now);
            List<FileDetailsModel> ObjFiles = GetFileList(id);
            var FileById = ObjFiles.FirstOrDefault();
            //var FileById = ObjFiles.Where(o => o.Id == id).FirstOrDefault();

            System.Diagnostics.Debug.WriteLine("Before View called - " + DateTime.Now);
            // return File(FileById.FileContent, "application/pdf", FileById.FileName);
            return View("ViewDocument", FileById);
          
        }

        //FileDetailsModel

        //[HttpGet]
        //public FileResult DownLoadFile(int id)
        //{

        //    List<FileDetailsModel> ObjFiles = GetFileList(id);

        //    var FileById = (from FC in ObjFiles
        //                    where FC.Id.Equals(id)
        //                    select new { FC.FileName, FC.FileContent }).ToList().FirstOrDefault();

        //    return File(FileById.FileContent, "application/pdf", FileById.FileName);

        //}
        #endregion

        #region View Uploaded files  
        [HttpGet]
        public PartialViewResult FileDetails()
        {
            List<FileDetailsModel> DetList = GetFileList(0);

            return PartialView("FileDetails", DetList);


        }
        private List<FileDetailsModel> GetFileList(int id)
        {          
            List<FileDetailsModel> DetList = new List<FileDetailsModel>();

            string LogFileName = System.Web.HttpContext.Current.Session["LogFileName"].ToString();
            string log = string.Empty;
            
            log += ("Before Connection Open - " + DateTime.Now + System.Environment.NewLine);

            DbConnection();
            con.Open();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            //System.Diagnostics.Debug.WriteLine("Before Data read from db - " + DateTime.Now);
            log += ("Before Data read from db - " + DateTime.Now + System.Environment.NewLine);

            if (id != 0)
                DetList = SqlMapper.Query<FileDetailsModel>(con, "GetFileDetails",parameters , commandType: CommandType.StoredProcedure).ToList();
            else
                DetList = SqlMapper.Query<FileDetailsModel>(con, "GetFileDetails", commandType: CommandType.StoredProcedure).ToList();

           // System.Diagnostics.Debug.WriteLine("after read from db- " + DateTime.Now);
            log += ("After read from db- " + DateTime.Now + System.Environment.NewLine);

            con.Close();
            return DetList;
        }

        #endregion

        #region Database related operations  
        private void SaveFileDetails(FileDetailsModel objDet)
        {

            DynamicParameters Parm = new DynamicParameters();
            Parm.Add("@FileName", objDet.FileName);
            Parm.Add("@FileContent", objDet.FileContent);
            DbConnection();
            con.Open();
            con.Execute("AddFileDetails", Parm, commandType: System.Data.CommandType.StoredProcedure);
            con.Close();


        }
        #endregion

        #region Database connection  

        private SqlConnection con;
        private string constr;
        private void DbConnection()
        {
            constr = ConfigurationManager.ConnectionStrings["dbcon"].ToString();
            con = new SqlConnection(constr);

        }
        #endregion


    }
}