using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BinaryToEBS
{
    class Program
    {
        protected static string DBConnection()
        {
            //string conn = @"data source=10.10.2.6;initial catalog=Isbcorporate;user id=isbdbuser;password=JK%rt#G@N2;multipleactiveresultsets=True;application name=EntityFramework providerName=System.Data.SqlClient";
            string conn = @"data source=DESKTOP-9LOEOMP\SQLEXPRESS;initial catalog=isbcorporate_test;Integrated Security=SSPI;multipleactiveresultsets=True;application name=EntityFramework";
            return conn;
        }
        static void Main(string[] args)
        {
            UploadBinaryToISB();
            /*if (args != null && args.Length > 0)
            {
                try
                {

                    for (int c1 = 0; c1 < args.Length; c1++)
                    {
                        Console.Write(args[c1] + Environment.NewLine);
                        UploadBinaryToISB(args[c1]);
                    }
                }
                catch (Exception ex1)
                {
                    Console.Write("Exception: " + ex1);
                }
            }*/
            Console.Read();
        }
        protected static void UploadBinaryToISB()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBConnection()))
                {
                    string sql = "SELECT TOP(10) ui.id, ui.file1Binary, ui.file2Binary, pd.PaperPortFolder FROM AIDUploadImage_new ui WITH (NOLOCK) INNER JOIN AIDOrders o ON o.AIDGUID = ui.AIDGUID INNER JOIN WSPackage wp ON wp.PackageGUID = o.AIDGUID INNER JOIN PackageDetails pd ON pd.PackageID = wp.WebPackageID WHERE pd.ProductID = 1603 AND ui.uploadedToServer = 0 ORDER BY ui.id ASC";
                    //string sql = "SELECT TOP(1) ui.id, ui.file1Binary, ui.file2Binary FROM AIDUploadImage_new ui WITH (NOLOCK) WHERE ui.id=1002 ORDER BY ui.id DESC";
                    //string sql = "SELECT * from aidorders where id = 2447";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        DataSet dataSet = new DataSet();
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        SqlDataAdapter sqlData = new SqlDataAdapter(cmd);
                        sqlData.Fill(dataSet);
                        conn.Close();
                        if(dataSet.Tables[0].Rows.Count > 0)
                        {
                            for (var c2 = 0; c2 < dataSet.Tables[0].Rows.Count; c2++)
                            {
                                string path = dataSet.Tables[0].Rows[c2]["PaperPortFolder"].ToString();
                                if (path.IndexOf(@"Ebs\") > -1)
                                {
                                    string path2 = path.Substring(0, path.LastIndexOf(@"\"));
                                    path = @"\\10.10.2.5\d$\isb\" + path2;
                                    //path = @"E:\prashant\project-works\ISBC\Dev\tmpupload\" + path2;

                                    byte[] File1Binary = (byte[])dataSet.Tables[0].Rows[c2]["file1Binary"];
                                    string File1base64String = Convert.ToBase64String(File1Binary, 0, File1Binary.Length);
                                    byte[] File1BinaryBytes = Convert.FromBase64String(File1base64String);

                                    byte[] File2Binary = (byte[])dataSet.Tables[0].Rows[c2]["file2Binary"];
                                    string File2base64String = Convert.ToBase64String(File2Binary, 0, File2Binary.Length);
                                    byte[] File2BinaryBytes = Convert.FromBase64String(File2base64String);

                                    if (MoveToFolder(path, File1BinaryBytes, File2BinaryBytes) != false)
                                    {
                                        using (SqlConnection conn1 = new SqlConnection(DBConnection()))
                                        {
                                            using (SqlCommand cmd1 = new SqlCommand("UPDATE AIDUploadImage_new SET uploadedToServer=1 WHERE id = @uiID", conn1))
                                            {
                                                cmd1.CommandType = CommandType.Text;
                                                cmd1.Parameters.AddWithValue("@uiID", dataSet.Tables[0].Rows[c2]["id"]);
                                                conn1.Open();
                                                cmd1.ExecuteNonQuery();
                                                conn1.Close();
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex1)
            {

            }
            return;
        }
        public static bool MoveToFolder(string path, byte[] file1Binary, byte[] file2Binary)
        {
            try
            {
                /*Random rnd = new Random();
                path = @"E:\prashant\project-works\ISBC\Dev\tmpupload\Ebs\2020\11\603. Premium National Criminal Record Check\Day8-14\M\MIDEIDTest Ron";*/
                using (var ms = new MemoryStream(file1Binary))
                {
                    using (var fs = new FileStream(path + @"\ID1.jpg", FileMode.Create))
                    {
                        ms.WriteTo(fs);
                    }
                }
                using (var ms = new MemoryStream(file2Binary))
                {
                    using (var fs = new FileStream(path + @"\ID2.jpg", FileMode.Create))
                    {
                        ms.WriteTo(fs);
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.Write("Exception:" + ex);
            }
            return false;
        }

    }
}
