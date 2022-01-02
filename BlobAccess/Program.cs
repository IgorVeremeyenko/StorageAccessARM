using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;

namespace BlobAccess
{
    class Program
    {
        public static void Main(string[] args)
        {
            
            BlobServiceClient serviceClient = new BlobServiceClient(@"DefaultEndpointsProtocol=https;AccountName=igor07;AccountKey=4uHx0M0H2a5/vrC4/jOXm3tlE0xZaURSuZD9NaKZf7dA3+Zhi050AM2Ydewo+tQV5u/QYjh4rmP+1RD9jJ28BA==;BlobEndpoint=https://igor07.blob.core.windows.net/;QueueEndpoint=https://igor07.queue.core.windows.net/;TableEndpoint=https://igor07.table.core.windows.net/;FileEndpoint=https://igor07.file.core.windows.net/;");
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient("container");
            int count = 0;
            foreach (var blob in containerClient.GetBlobs())
            {
                var name = blob.Name;
                int index = name.IndexOf("/") + 1;
                string piece = name.Substring(0, index);
                if(piece == "")
                {
                    Run(blob, name, false, containerClient, piece, index);
                }
                else
                {
                    Run(blob, name, true, containerClient, piece, index);
                }
                count++;
            }
            Console.WriteLine("Скачиваются файлов: {0}", count);
            count = 0;
            Console.ReadKey();
        }

        private static string StringEdit(string name, int index, string piece)
        {
            int len = name.Length;
            string piece2 = name.Substring(index, len - index);
            string name2 = piece + piece2;
            return name2;
        }

        private static async void Run(BlobItem blob, string name, bool createFolder, BlobContainerClient containerClient, string piece, int index)
        {
            
            if (createFolder)
            {
                var reference = containerClient.GetBlobClient(name);

                DirectoryInfo directoryInfo = new DirectoryInfo(piece);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                string name2 = StringEdit(name, index, piece);
                
                await reference.DownloadToAsync(name);
                Console.WriteLine("Скачано {0}", name2);
            }
            else
            {
                var reference = containerClient.GetBlobClient(name);
               
                await reference.DownloadToAsync(blob.Name);
                Console.WriteLine("Скачано {0}", blob.Name);
            }
            
        }

    }
}
