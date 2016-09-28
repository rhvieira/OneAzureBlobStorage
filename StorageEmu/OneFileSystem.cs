using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneAzureStorageFS
{
    public class OneFileSystem
    {
        private string _basedir;


        public OneFileSystem(string basedir)
        {
            _basedir = basedir;
        }

        private string RemoveData(string data)
        {
            if (data.StartsWith("/"))
                return data.Substring(1, data.Length - 1).Replace("/",@"\");
            else
                return data.Replace("/", @"\"); ;
        }

        public void DeleteContainer(string name)
        {
            if (Directory.Exists(_basedir + RemoveData(name)))
            Directory.Delete(_basedir + RemoveData(name), true);
        }
        public void DeleteFile(string filename)
        {
            File.Delete(_basedir + RemoveData(filename));
        }
        public bool ExistContainer(string name)
        {

            return System.IO.Directory.Exists(_basedir + RemoveData(name));
        }
        public void CreateContainer(string name)
        {
            System.IO.Directory.CreateDirectory(_basedir + RemoveData(name));
        }
        public bool FileExists(string filename)
        {
            return System.IO.File.Exists(_basedir + RemoveData(filename));
        }
        public void CreateFile(string filename, string tempfile)
        {
            var fullfilename = _basedir + RemoveData(filename);
            var shortname = Path.GetFileName(fullfilename);
            var dir = fullfilename.Replace(shortname,"");

            if (Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(fullfilename))
                System.IO.File.Copy(tempfile, _basedir + RemoveData(filename));

            File.Delete(tempfile);
        }
        public void UnionFiles(BlockList bl)
        {
            var tempfile = Path.GetTempFileName();
            int bytesRead = 0;
            byte[] buffer = new byte[1024];
            FileStream output = null;
            var filename = "";
            var originalname ="";

            foreach (var blockid in bl.Latest)
            {
                var file = Directory.GetFiles(_basedir, "*" + blockid + "*", SearchOption.AllDirectories);
                if (file.Length == 1)
                {
                    var filenametemp = Path.GetFileName(file[0]);
                    var ftp = filenametemp.ToString().Split(new string[] { "_#_" }, StringSplitOptions.RemoveEmptyEntries);
                    var fulldirname = file[0].Replace(filenametemp, "");



                    if (filename == "")
                    {
                        originalname = fulldirname + ftp[0];
                        filename =  originalname+ ".temp";
                        output = new FileStream(filename, FileMode.OpenOrCreate);
                    }


                    FileStream inputTempFile = new FileStream(file[0], FileMode.OpenOrCreate, FileAccess.Read);
                    while ((bytesRead = inputTempFile.Read(buffer, 0, 1024)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                        output.Flush();
                    }


                    inputTempFile.Close();
                    File.Delete(file[0]);
                }
            }
            output.Close();
            System.IO.File.Copy(filename, originalname);
            File.Delete(filename);
        }
        public FileStream ReadFile(string filename)
        {
            return new FileStream(_basedir + RemoveData(filename), FileMode.Open);
        }
        public string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    //return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }
        public EnumerationResults ListContainer(string container)
        {
            var data = Directory.GetFiles(_basedir + RemoveData(container));
            EnumerationResults bls = new EnumerationResults();
            bls.Blobs = new Blobs();
            bls.Blobs.Blob = new List<Blob>();
            foreach (var file in data)
            {
                if(!file.Contains("_#_"))
                bls.Blobs.Blob.Add(ReturnFileInfo(file));
            }

            var datadir = Directory.GetDirectories(_basedir + RemoveData(container));
            bls.Containers = new Containers();
            bls.Containers.Container = new List<Container>();
            foreach (var file in datadir)
            {
                DirectoryInfo f = new DirectoryInfo(file);

                EntityTagHeaderValue eTag = new EntityTagHeaderValue("\"" + Guid.NewGuid().ToString() + "\"");

                bls.Containers.Container.Add(new Container() { Name = Path.GetFileNameWithoutExtension(file), Properties = new PropertiesContainer() { LastModified = f.LastWriteTimeUtc, Etag = eTag.Tag, LeaseStatus = "unlocked", LeaseState = "available" } });
            }

            return bls;
        }
        public Blob ReturnFileInfo(string file)
        {
            if (file.StartsWith("/"))
            {
                file = _basedir + RemoveData(file);
            }
            FileInfo f = new FileInfo(file);
            EntityTagHeaderValue eTag = new EntityTagHeaderValue("\"" + Guid.NewGuid().ToString() + "\"");
            return new Blob() { Name = Path.GetFileName(file), Properties = new PropertiesBlob() { BlobType = "BlockBlob", LastModified = f.LastWriteTimeUtc, Etag = eTag.Tag, ContentLength = f.Length, ContentType = "application/octet-stream", ContentMD5 = checkMD5(file), LeaseStatus = "unlocked", LeaseState = "available" } };

        }
    }
}
