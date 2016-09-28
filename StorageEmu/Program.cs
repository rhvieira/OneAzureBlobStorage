using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneAzureStorageFS
{
    class Program
    {
        static void Main(string[] args)
        {

            var logon = Convert.ToBoolean(ConfigurationManager.AppSettings["Log"]);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");

            OneFileSystem ofs = new OneFileSystem(ConfigurationManager.AppSettings["Storage_file"]);
            
            HttpListener hpl = new HttpListener();
            hpl.Prefixes.Add(ConfigurationManager.AppSettings["Prefixes"]);
            hpl.Start();


            while (true)
            {
                var requestcontext = hpl.GetContext();

                Task t = new Task(new Action<object>((object d) =>
                {
                    HttpListenerContext c = d as HttpListenerContext;
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");


                    var dd = c.Request.HttpMethod;

                    c.Response.Headers.Add("Server", "ONETECH/Windows-Azure-Blob/1.0 Microsoft-HTTPAPI/2.0");
                    c.Response.Headers.Add("x-ms-request-id", "d5a8bd1b-0001-0007-0d98-baa08f000000");
                    c.Response.Headers.Add("x-ms-version", "2009-09-19");
                    c.Response.Headers.Add("Date", DateTimeOffset.Now.ToString("R"));

                    if (dd == "HEAD")
                    {

                        if (c.Request.QueryString["restype"] == "container")
                        {

                            if (!ofs.ExistContainer(c.Request.Url.AbsolutePath))
                            {
                                c.Response.StatusCode = 404;
                                c.Response.StatusDescription = "The specified container does not exist.";
                            }
                            else
                            {
                                c.Response.StatusCode = 200;
                                c.Response.StatusDescription = "OK";
                            }
                        }
                        else
                        {
                            if (!ofs.FileExists(c.Request.Url.AbsolutePath))
                            {
                                c.Response.StatusCode = 404;
                                c.Response.StatusDescription = "The specified container does not exist.";
                            }
                            else
                            {
                                var data = ofs.ReturnFileInfo(c.Request.Url.AbsolutePath);

                                c.Response.Headers.Add("Content-MD5",   Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( data.Properties.ContentMD5)));
                                c.Response.Headers.Add("Last-Modified", data.Properties.LastModified.ToString("R"));
                                c.Response.Headers.Add("Accept-Ranges", "bytes");
                                c.Response.Headers.Add("ETag", data.Properties.Etag);
                                c.Response.Headers.Add("x-ms-write-protection", "false");
                                c.Response.Headers.Add("x-ms-lease-status", "unlocked");
                                c.Response.Headers.Add("x-ms-lease-state", "available");
                                c.Response.Headers.Add("x-ms-blob-type", "BlockBlob");
                                c.Response.ContentLength64 = data.Properties.ContentLength;
                                c.Response.ContentType = "application/octet-stream";
                                c.Response.StatusCode = 200;
                                c.Response.StatusDescription = "OK";
                            }
                        }
                    }
                    else if (dd == "DELETE")
                    {
                        if (c.Request.QueryString["restype"] == "container")
                        {
                            ofs.DeleteContainer(c.Request.Url.AbsolutePath);
                        }
                        else
                        {
                            ofs.DeleteFile(c.Request.Url.AbsolutePath);
                           
                        }
                        c.Response.StatusCode = 202;
                        c.Response.StatusDescription = "Accepted";
                    }
                    else if (dd == "PUT")
                    {
                        if (c.Request.QueryString["restype"] == "container"
                            && c.Request.QueryString["comp"] == "acl")
                        {
                            c.Response.StatusCode = 200;
                            c.Response.StatusDescription = "OK";
                        }
                        else if (c.Request.QueryString["restype"] == "service"
                       && c.Request.QueryString["comp"] == "properties")
                        {
                            c.Response.StatusCode = 202;
                            c.Response.StatusDescription = "Accepted";
                        }
                        else if (c.Request.QueryString["restype"] == "container")
                        {

                            ofs.CreateContainer(c.Request.Url.AbsolutePath);

                            c.Response.StatusCode = 201;
                            c.Response.StatusDescription = "Created";
                            c.Response.SendChunked = true;
                            var buffer = GetContent("0");
                            c.Response.ContentLength64 = buffer.Length;
                            c.Response.OutputStream.Write(buffer, 0, buffer.Length);

                        }
                        else if (c.Request.QueryString["comp"] == "block")
                        {
                            var blockid = c.Request.QueryString["blockid"];
                            var tempfile = Path.GetTempFileName();
                            using (FileStream fs = new FileStream(tempfile, FileMode.OpenOrCreate))
                            {
                                byte[] bufferdata = new byte[8192];


                                int bytesRead;
                                while ((bytesRead = c.Request.InputStream.Read(bufferdata, 0, bufferdata.Length)) > 0)
                                {
                                    fs.Write(bufferdata, 0, bytesRead);
                                }
                            }

                            ofs.CreateFile(c.Request.Url.AbsolutePath + "_#_" + blockid, tempfile);
                            c.Response.StatusCode = 201;
                            c.Response.StatusDescription = "Created";

                        }
                        else if (c.Request.QueryString["comp"] == "blocklist")
                        {
                            BlockList lst = Utils.Deserialize<BlockList>(c.Request.InputStream);
                            ofs.UnionFiles(lst);
                            c.Response.StatusCode = 201;
                            c.Response.StatusDescription = "Created";

                        }
                        else if (c.Request.QueryString["comp"] == "properties")
                        {
                            c.Response.StatusCode = 200;
                            c.Response.StatusDescription = "OK";
                        }
                        else
                        {
                            var tempfile = Path.GetTempFileName();
                            using (FileStream fs = new FileStream(tempfile, FileMode.OpenOrCreate))
                            {
                                byte[] bufferdata = new byte[8192];


                                int bytesRead;
                                while ((bytesRead = c.Request.InputStream.Read(bufferdata, 0, bufferdata.Length)) > 0)
                                {
                                    fs.Write(bufferdata, 0, bytesRead);
                                }
                            }

                            ofs.CreateFile(c.Request.Url.AbsolutePath, tempfile);
                            c.Response.StatusCode = 201;
                            c.Response.StatusDescription = "Created";
                        }
                    }
                    else if (dd == "GET")
                    {

                        if (c.Request.QueryString["comp"] == "list")
                        {

                            var er = ofs.ListContainer(c.Request.Url.AbsolutePath);
                            er.ContainerName = c.Request.Url.AbsolutePath;
                            er.ServiceEndpoint = c.Request.Url.AbsoluteUri;
                            var xml = Utils.Serialize<EnumerationResults>(er);
                            var buffer = GetContent(xml);
                            c.Response.ContentType = "application/xml";
                            c.Response.StatusCode = 200;
                            c.Response.StatusDescription = "OK";

                            c.Response.OutputStream.Write(buffer, 0, buffer.Length);

                        }
                        else if (c.Request.QueryString["comp"] =="properties"
                            && c.Request.QueryString["restype"] =="service")
                        {
                            var defaultresp = @"<?xml version=""1.0"" encoding=""utf-8""?><StorageServiceProperties><Logging><Version>1.0</Version><Read>false</Read><Write>false</Write><Delete>false</Delete><RetentionPolicy><Enabled>false</Enabled></RetentionPolicy></Logging><HourMetrics><Version>1.0</Version><Enabled>true</Enabled><IncludeAPIs>true</IncludeAPIs><RetentionPolicy><Enabled>true</Enabled><Days>7</Days></RetentionPolicy></HourMetrics><MinuteMetrics><Version>1.0</Version><Enabled>false</Enabled><RetentionPolicy><Enabled>false</Enabled></RetentionPolicy></MinuteMetrics><Cors /></StorageServiceProperties>";
                            var buffer = GetContent(defaultresp);
                            c.Response.ContentType = "application/xml";
                            c.Response.StatusCode = 200;
                            c.Response.StatusDescription = "OK";

                            c.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                        else if (c.Request.QueryString["comp"] == "acl"
                            && c.Request.QueryString["restype"] == "container")
                        {
                            var defaultresp = @"﻿<?xml version=""1.0"" encoding=""utf-8""?><SignedIdentifiers />";
                            var buffer = GetContent(defaultresp);
                            c.Response.ContentType = "application/xml";
                            c.Response.StatusCode = 200;
                            c.Response.StatusDescription = "OK";

                            c.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                        else if (ofs.FileExists(c.Request.Url.AbsolutePath))
                        {

                            using (var fs = ofs.ReadFile(c.Request.Url.AbsolutePath))
                            {
                                byte[] buffer = new byte[4096];
                                int count = 0;

                                while ((count = fs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    c.Response.OutputStream.Write(buffer, 0, count);
                                    c.Response.OutputStream.Flush();
                                }
                                c.Response.ContentType = "application/" + Path.GetExtension(c.Request.Url.AbsolutePath).Replace(".", "");
                            }
                            c.Response.StatusCode = 200;
                            c.Response.StatusDescription = "OK";
                        }
                        else
                        {
                            c.Response.StatusCode = 404;
                            c.Response.StatusDescription = "The specified container does not exist.";
                        }


                    }

                    Console.WriteLine(dd);
                    Console.WriteLine(c.Request.RawUrl);

                    c.Response.Close();

                }), requestcontext);

                t.Start();
            }



        }


        static byte[] GetContent(string message)
        {
            return System.Text.Encoding.UTF8.GetBytes(message);
        }


    }
}
