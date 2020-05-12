using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using DocumentManager;
using DocumentManager.Models;
using GdPicture14.WEB;

namespace DocumentManager
{
    public class GalleryDemo
    {
        /// <summary>
        /// Entry point for the custom action handler
        /// </summary>
        public static void HandleLoadDocumentAction(CustomActionEventArgs e)
        {
            string docRef = (string)e.args;
            List<FileDetailsModel> DetList = new List<FileDetailsModel>();
            string constr = ConfigurationManager.ConnectionStrings["dbcon"].ToString();
            SqlConnection con = new SqlConnection(constr);         
            con.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Id", docRef);         
            DetList = SqlMapper.Query<FileDetailsModel>(con, "GetFileDetails", parameters, commandType: CommandType.StoredProcedure).ToList();
            con.Close();           
            e.docuVieware.LoadFromStream(new System.IO.MemoryStream(DetList[0].FileContent), true, DetList[0].FileName);
        }

    }
}