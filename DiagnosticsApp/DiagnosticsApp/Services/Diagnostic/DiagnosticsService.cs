using DiagnosticsApp.DatabaseModels;
using DiagnosticsApp.Models;
using DiagnosticsApp.Services.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DiagnosticsApp.Services.Diagnostic
{
    public class DiagnosticsService : IDiagnosticsService
    {
        private DiagnosticsDBContext diagnosticsDBContext;
        private IBlobService blobService;

        public DiagnosticsService(DiagnosticsDBContext diagnosticsDbContext, IBlobService blobService)
        {
            this.diagnosticsDBContext = diagnosticsDbContext;
            this.blobService = blobService;
        }

        public void AddDiagnostics(DiagnosticsModel diagnosticsModel)
        {
            long? examinationId = null;
            if ((bool)diagnosticsModel.HasExamination)
            {
                if (diagnosticsModel.Breath == null || diagnosticsModel.Complaint == null || diagnosticsModel.Height == null
                    || diagnosticsModel.Pressure == null || diagnosticsModel.Temperature == null || diagnosticsModel.Weight == null)
                {
                    throw new Exception("Не указаны все параметры об осмотре. Заполните эти данные или укажите, что осмотр не проводился");
                }

                var examination = new Examination()
                {
                    Breath = diagnosticsModel.Breath,
                    Complaint = diagnosticsModel.Complaint,
                    Height = (int)diagnosticsModel.Height,
                    Other = diagnosticsModel.Other,
                    Pressure = diagnosticsModel.Pressure,
                    Temperature = (double)diagnosticsModel.Temperature,
                    Weight = (int)diagnosticsModel.Weight
                };

                diagnosticsDBContext.Examination.Add(examination);
                diagnosticsDBContext.SaveChanges();
                examinationId = examination.ExaminationId;
            }

            var diagnostics = new Diagnostics()
            {
                ClientId = (long)diagnosticsModel.ClientId,
                DoctorId = (long)diagnosticsModel.DoctorId,
                StartTime = DateTime.UtcNow,
                ExaminationId = examinationId
            };

            diagnosticsDBContext.Diagnostics.Add(diagnostics);
            diagnosticsDBContext.SaveChanges();

            //записать пути в BLOB в базу данных
            int count = 0;
            foreach (var file in diagnosticsModel.OriginalImagesFiles)
            {
                string originalImagesRef = blobService.UploadBlob("" + count, file);
                count++;
                var image = new Image()
                {
                    DiagnosticsId = diagnostics.DiagnosticsId,
                    RefNotParsed = originalImagesRef
                };
                diagnosticsDBContext.Add(image);
            }

            diagnosticsDBContext.SaveChanges();

            if(diagnosticsModel.SearchNow != null)
            {
                if((bool)diagnosticsModel.SearchNow)
                {
                    var model = new DiagnosticsModel()
                    {
                        DiagnosticsId = diagnostics.DiagnosticsId
                    };
                    FindCalcinatesRegions(model);
                }
            }
            
        }

        public void FindCalcinatesRegions(DiagnosticsModel diagnosticsModel)
        {

            //get blob path
            if(diagnosticsModel.DiagnosticsId == null)
            {
                throw new Exception("Укажите id диагностики");
            }

            var images = diagnosticsDBContext.Image.Where(o => o.DiagnosticsId == diagnosticsModel.DiagnosticsId);
            if (images == null)
            {
                throw new Exception("Снимков для диагностики с заданным id не найдено");
            }

            //download files from blob and save to folder
            foreach(var image in images)
            {
                string imagePath = blobService.DownloadBlob(image.RefNotParsed);
            }
            
            var origPath = blobService.GetTempFolderOrigin();
            var parsedPath = blobService.GetTempFolderParsed();

            //find
            FindCalcinatesRegions(origPath, parsedPath);

            //save blobs parsed
            string[] files = Directory.GetFiles(parsedPath, "*.bmp");
            var parsedFilesPaths = new List<string>();
            foreach(var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                byte[] data = new byte[fileInfo.Length];
                using (var binaryReader = new BinaryReader(fileInfo.OpenRead()))
                {
                    data = binaryReader.ReadBytes((int)fileInfo.Length);
                }
                string parsedBlobPath = blobService.UploadBlob(fileInfo.Name, data);
                parsedFilesPaths.Add(parsedBlobPath);
            }

            //set blobPath for parsed images
            var ind = 0;
            foreach (var image in images)
            {
                image.RefParsed = parsedFilesPaths[ind];
                ind += 1;
            }
            diagnosticsDBContext.SaveChanges();

            //delete from temp folder
            DirectoryInfo dir = new DirectoryInfo(origPath);
            var dirFiles = dir.GetFiles();
            foreach (FileInfo fi in dirFiles)
            {
                fi.Delete();
            }
            dir = new DirectoryInfo(parsedPath);
            dirFiles = dir.GetFiles();
            foreach (FileInfo fi in dirFiles)
            {
                fi.Delete();
            }

            //TODO: Сформировать отчет

            return;
        }

        private void FindCalcinatesRegions(string origPath, string parsedPath)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var server = "127.0.0.1";
            var port = 8080;
            socket.Connect(server, port);

            byte[] dataOrigPath = Encoding.UTF8.GetBytes(origPath);
            byte[] dataParsedPath = Encoding.UTF8.GetBytes(parsedPath);

            byte[] message = new byte[1024];
            for (int i = 0; i < 512; i++)
            {
                if (i < dataOrigPath.Length)
                {
                    message[i] = dataOrigPath[i];
                }
                if (i < dataParsedPath.Length)
                {
                    message[i + 512] = dataParsedPath[i];
                }
            }

            socket.Send(message);
            byte[] dataAns = new byte[1024];
            var answerLen = socket.Receive(dataAns);
            string answer = Encoding.UTF8.GetString(dataAns);

            socket.Close();
            return;
        }
    }
}
