﻿using Android.Net;
using FluentFTP;
using System;
using Android.Content;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FreediverApp.WifiCommunication
{
    class FtpConnector
    {
        private Context context;
        private string address_v4;
        private string username;
        private string password;
        private FtpProfile serverProfile;
        private FtpClient client;
        private string localDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        public FtpConnector(Context context)
        {
            this.context = context;
            client = new FtpClient();
        }

        public FtpConnector(Context context, string address_v4, string username, string password) 
        {
            this.context = context;
            this.address_v4 = address_v4;
            this.username = username;
            this.password = password;
            client = new FtpClient(address_v4, username, password);
            serverProfile = client.AutoConnect();
        }

        public async Task<List<FtpResult>> downloadDirectory(string localDirectory, string remoteDirectory) 
        {
            List<FtpResult> result;
            serverProfile = await client.AutoConnectAsync();
            result = await client.DownloadDirectoryAsync(localDirectory, remoteDirectory);
            return result;
        }

        public async Task<bool> downloadFile(string localDirectory, string remoteDirectory, string filename) 
        {
            serverProfile = await client.AutoConnectAsync();
            FtpStatus status = await client.DownloadFileAsync(localDirectory, remoteDirectory + filename);
            handleError(status);
            return status == FtpStatus.Success;
        }

        public async void disconnect() 
        {
            await client.DisconnectAsync();
        }

        public bool isConnected() 
        {
            try
            {
                ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                return connectivityManager.ActiveNetworkInfo.IsConnectedOrConnecting;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return false;
            }     
        }

        private void handleError(FtpStatus errorcode) 
        {
            switch (errorcode) 
            {
                case FtpStatus.Success: 
                {
                    Console.WriteLine("Ftp Operation successful!");
                    break;
                }
                case FtpStatus.Failed: 
                case FtpStatus.Skipped: 
                {
                    Console.WriteLine("Error while processing requested Ftp Operation!");
                    break;
                }
                default: { break; }
            }
        }

        public async Task<bool> synchronizeData()
        {
            FtpClient client = new FtpClient("192.168.4.1", "diver", "diverpass");
            client.AutoConnect();

            bool success = false;

            if (File.Exists(localDirectory + "/sessions.log"))
            {
                File.Delete(localDirectory + "/sessions.log");
            }

            success = await downloadFile(localDirectory, "/", "sessions.log");

            List<string> sessions = new List<string>();
            List<string> results = new List<string>();
            using (StreamReader sr = new StreamReader(File.Open(localDirectory + "/sessions.log", FileMode.Open)))
            {
                while (!sr.EndOfStream)
                {
                    sessions.Add(sr.ReadLine());
                }
            }
            sessions.ForEach(async (string session) =>
            {
                List<FtpResult> result;
                result = await downloadDirectory(localDirectory, "/logFiles/" + session);
                success = result.Count > 0;
                using (StreamReader sr = new StreamReader(File.Open(localDirectory + session, FileMode.Open)))
                {
                    while (!sr.EndOfStream)
                    {
                        results.Add(sr.ReadLine());
                    }
                }
            });
            return success;
        }

        //public async Task downloadFile_v2(string url, string username, string password, string filename)
        //{
        //    //using (WebClient client = new WebClient())
        //    //{
        //    //    client.DownloadFile(url, filename);
        //    //}

        //    try
        //    {
        //        string fileUrl = String.Format("{0}/{1}", url, filename);
        //        FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(fileUrl);
        //        req.Proxy = null;
        //        req.Method = WebRequestMethods.Ftp.DownloadFile;
        //        req.Credentials = new NetworkCredential(username, password);
        //        req.UseBinary = true;
        //        req.UsePassive = true;
        //        WebResponse res = await req.GetResponseAsync();
        //        FtpWebResponse webRes = (FtpWebResponse) res;
        //        using(StreamReader reader = new StreamReader(res.GetResponseStream()))
        //        {
        //            string dataString = reader.ReadToEnd();
        //            Console.WriteLine(dataString);
        //            //return dataString;
        //        }
        //    } 
        //    catch(Exception exp)
        //    {
        //        Console.WriteLine("------ ERROR ------ ERROR ------ ERROR ------");
        //        Console.WriteLine(exp);
        //        Console.WriteLine("------ ERROR ------ ERROR ------ ERROR ------");
        //        //return "ERROR";
        //    }
        //}

        //public async void downloadFile_v3()
        //{
        //    try
        //    {
        //        ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
        //        if (connectivityManager.ActiveNetworkInfo.IsConnectedOrConnecting)
        //        {
        //            Console.WriteLine("------------- IS CONNECTED ---------------");
        //        }

        //        FtpClient client = new FtpClient("ftp://192.168.4.1");
        //        client.Credentials = new NetworkCredential("diver", "diverpass");
        //        await client.ConnectAsync();
        //        client.Rename("/divelog25.txt", "/divelog30.txt");
        //        client.Disconnect();
        //    } 
        //    catch(Exception e)
        //    {
        //        Console.WriteLine("------------------ ERROR START --------------------");
        //        Console.WriteLine(e);
        //        Console.WriteLine("------------------ ERROR END --------------------");
        //    }
        //}

        //public void downloadFile()
        //{
        //    //FtpClient ftpClient = new FtpClient(url);

        //    //ftpClient.Credentials = new NetworkCredential(username, password);

        //    //ftpClient.Connect();

        //    //FtpStatus status = ftpClient.DownloadFile("/" + filename, "/" + filename);

        //    //Console.WriteLine("FTP STATUS: " + status);

        //    Uri uri = new Uri("ftp://192.168.4.1/divelog25.txt");
        //    FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);

        //    ftpRequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
        //    ftpRequest.Timeout = System.Threading.Timeout.Infinite;
        //    ftpRequest.KeepAlive = false;
        //    ftpRequest.Credentials = new NetworkCredential("diver", "diverpass");
        //    ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

        //    try
        //    {
        //        using (FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse()) //Stream streamWriter = ftpRequest.GetResponse().GetResponseStream()
        //        {
        //            Stream stream = ftpResponse.GetResponseStream();
        //            StreamReader streamReader = new StreamReader(stream);
        //            var output = streamReader.ReadToEnd();
        //            Console.Write("Stream output: ------>");
        //            Console.WriteLine(output);
        //        }
        //    }
        //    catch (WebException webexp)
        //    {
        //        Console.WriteLine("------------------------------------------------------");
        //        Console.WriteLine(webexp);
        //    }

        //}
    }
}
