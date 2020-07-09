using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using WindowsServiceMentorshipTest.Classes;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using Newtonsoft.Json;

namespace WindowsServiceMentorshipTest
{
    public partial class Service1 : ServiceBase
    {

        Timer timer = new Timer(); // name space(using System.Timers;)  

        public Service1()
        {

            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            //Read from db
            string selectQuery = @"SELECT * FROM OverPayment";
            using (System.Data.SQLite.SQLiteConnection conn = new System.Data.SQLite.SQLiteConnection("data source=C:/Users/Isaiah Yu/SQLite Databases/Overpayment DB.db"))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(conn))
                {
                    conn.Open();                             // Open the connection to the database

                    com.CommandText = selectQuery;     // Set CommandText to our query that will select all rows from the table
                    com.ExecuteNonQuery();                  // Execute the query

                    using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                    {
                        //string postJSON;
                        //OverPaymentDetails opd = new OverPaymentDetails();
                        var overPaymentDetail = new OverPaymentDetail();
                        while (reader.Read())
                        {
                            overPaymentDetail.OverPaymentID = Convert.ToInt32(reader["OverPaymentID"]);
                            overPaymentDetail.MemberID = Convert.ToInt32(reader["MemberID"]);
                            overPaymentDetail.OverPaymentAmt = Convert.ToInt32(reader["OverPaymentAmt"]);
                            overPaymentDetail.ClaimNumber = reader["ClaimNumber"].ToString();
                            overPaymentDetail.BalanceAmt = Convert.ToInt32(reader["BalanceAmt"]);
                            overPaymentDetail.SysSrcSyncDate = reader["SysSrcSyncDate"].ToString();
                            overPaymentDetail.LastUpdated = reader["LastUpdated"].ToString();
                            //postJSON = JsonSerializer.Serialize(dataClass001);
                            //Log the data before it is sent
                            System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_PrePost.txt", overPaymentDetail.OverPaymentID.ToString());
                            System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_PrePost.txt", overPaymentDetail.MemberID.ToString());
                            System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_PrePost.txt", overPaymentDetail.OverPaymentAmt.ToString());
                            System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_PrePost.txt", overPaymentDetail.ClaimNumber.ToString());
                            System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_PrePost.txt", overPaymentDetail.BalanceAmt.ToString());
                            System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_PrePost.txt", overPaymentDetail.SysSrcSyncDate.ToString());
                            System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_PrePost.txt", overPaymentDetail.LastUpdated.ToString());



                            //Send email
                            string memberSelectQuery = @"SELECT FirstName, Email FROM Member WHERE MemberID = " + overPaymentDetail.MemberID;
                            using (System.Data.SQLite.SQLiteCommand comEmail = new System.Data.SQLite.SQLiteCommand(conn))
                            {
                                comEmail.CommandText = memberSelectQuery;
                                comEmail.ExecuteNonQuery();
                                using (System.Data.SQLite.SQLiteDataReader readerEmail = comEmail.ExecuteReader())
                                {
                                    while (readerEmail.Read())
                                    {
                                        Email("Hi " + readerEmail["FirstName"].ToString(),readerEmail["Email"].ToString());
                                    }
                                }
                            }


                            //Post to web api
                            using (var client = new HttpClient())
                            {
                                //client.BaseAddress = new Uri("http://localhost:44312/api/");
                                client.BaseAddress = new Uri("http://overpayment_webapi.local/api/");


                                //HTTP POST
                                //Calling POST method in web api values controller
                                //var postTask = client.PostAsJsonAsync<DataClass001>("values", dataClass001);
                                var format = "yyyy-MM-dd HH:mm:ss:fff";
                                var stringDate = DateTime.Now.ToString(format);
                                overPaymentDetail.CreateDate = stringDate;
                                /*var postTask = client.PostAsJsonAsync<string>("values", @"{"+
                                    "\"overpayment_id\":"+ "\"" + overPaymentDetail.OverPaymentID+ "\"" + "," +
                                    "\"member_id\":" + "\"" + overPaymentDetail.MemberID + "\"" + "," +
                                    "\"overpayment_amt\":" + "\"" + overPaymentDetail.OverPaymentAmt + "\"" + "," +
                                    "\"claim_number\":" + "\"" + overPaymentDetail.ClaimNumber + "\"" + "," +
                                    "\"balance_amt\":" + "\"" + overPaymentDetail.BalanceAmt + "\"" + "," +
                                    "\"create_date\":" + "\"" + stringDate + "\"" + "," +
                                    "\"sys_src_sync_date\":" + "\"" + overPaymentDetail.SysSrcSyncDate + "\"" + "," +
                                    "\"last_updated\":" + "\"" + overPaymentDetail.LastUpdated + "\"" +
                                    "}");*/
                                var postTask = client.PostAsJsonAsync<string>("values", JsonConvert.SerializeObject(overPaymentDetail));
                                postTask.Wait();

                                var result = postTask.Result;
                                if (result.IsSuccessStatusCode)
                                {
                                    //return RedirectToAction("Index");
                                    System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_Post.txt", "Success" + result.StatusCode);
                                }
                                else
                                {
                                    System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result_Post.txt", "Fail" + result.StatusCode);
                                }

                            }
                        }
                        using (var client = new HttpClient())
                        {
                            //client.BaseAddress = new Uri("http://localhost:44312/api/");
                            client.BaseAddress = new Uri("http://overpayment_webapi.local/api/");
                            //HTTP GET
                            var responseTask = client.GetAsync("values");
                            responseTask.Wait();

                            var result = responseTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                System.IO.File.WriteAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result.txt", "Success");
                                var readTask = result.Content.ReadAsAsync<IList<string>>();
                                readTask.Wait();

                                IList<string> results = readTask.Result;

                                foreach (var s in results)
                                {
                                    //Console.WriteLine(d);
                                    System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\ResultBody.txt", s);
                                }
                                System.IO.File.AppendAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\ResultBody.txt", Environment.NewLine);
                            }
                            else
                            {
                                System.IO.File.WriteAllText(@"C:\Users\Isaiah Yu\Documents\Mentorship Result Files\Result.txt", "FAIL");
                            }
                        }
                        conn.Close();        // Close the connection to the database
                    }
                }

                WriteToFile("Service is started at " + DateTime.Now);

                timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);

                timer.Interval = 5000; //number in milisecinds  

                timer.Enabled = true;
            }

        }

        protected override void OnStop()
        {

            WriteToFile("Service is stopped at " + DateTime.Now);

        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {

            WriteToFile("Service is recall at " + DateTime.Now);

        }

        public void WriteToFile(string Message)
        {

            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";

            if (!Directory.Exists(path))
            {

                Directory.CreateDirectory(path);

            }

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            if (!File.Exists(filepath))
            {

                // Create a file to write to.   

                using (StreamWriter sw = File.CreateText(filepath))
                {

                    sw.WriteLine(Message);

                }

            }
            else
            {

                using (StreamWriter sw = File.AppendText(filepath))
                {

                    sw.WriteLine(Message);

                }

            }

        }
        private static void Email(string htmlString, string toEmailAddress)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("splashy22@gmail.com");
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Test";
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = htmlString;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com"; //for gmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("splashy22@gmail.com", "doodoodoo");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception) { }
        }

    }
}
