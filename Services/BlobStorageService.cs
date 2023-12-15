//MIT License
//
//Copyright (c) 2023 Microsoft - Jose Batista-Neto
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//

using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace SK_Connector.Services;

internal static class BlobStorageService
{
    internal static IServiceCollection AddBlobStorageService(this IServiceCollection services)
    {
        // Register AzureStorageHelper for the scope 
        services.AddSingleton< BlobStorage>(sp =>
        {
            return new BlobStorage();
        });

        return services;
    }
}


// BlobStorage Class - This class is used to read and write to Azure Blob Storage
public class BlobStorage
{
    private class Connection
    {
        public string? ConnectionString;
        public string? ContainerName;
        public BlobContainerClient? BlobContainerClient = null;

    }

    // This is a list of connections to the blob storage
    private readonly List<Connection> _connections = new();
    private readonly object _lock = new();

    // Connect to the blob storage
    public BlobContainerClient Connect(string? connectionString, string? containerName)
    {
        if(string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
            throw new Exception("BlobStorageHelper is not initialized");

        lock (_lock)
        {
            var connection = _connections.Where(x => x.ConnectionString == connectionString && x.ContainerName == containerName).FirstOrDefault();
            if (connection != null)
                if (connection.BlobContainerClient != null)
                    return connection.BlobContainerClient;


            // Provide the client configuration options for connecting to Azure Blob Storage
            BlobClientOptions blobServiceClientOptions = new()
            {
                Retry = {
                            Delay = TimeSpan.FromSeconds(2),
                            MaxRetries = 5,
                            Mode = RetryMode.Exponential,
                            MaxDelay = TimeSpan.FromSeconds(10),
                            NetworkTimeout = TimeSpan.FromSeconds(100)
                        }
            };

            var blobServiceClient = new BlobServiceClient(connectionString, blobServiceClientOptions);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            _connections.Add(new Connection { ConnectionString = connectionString, ContainerName = containerName, BlobContainerClient = blobContainerClient });
            return blobContainerClient;
        }
    }


    // Write a string to a file or storage (Append)
    // this is slow and should only be used for log files
    public void AppendString(BlobContainerClient blobContainerClient, string fullFileName, string contents)
    {
        // Check if we have any contents
        if (string.IsNullOrEmpty(contents))
            return;

        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        // Append data to a blob
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(contents);
        using MemoryStream stream = new(byteArray);

        // Get the append blob
        var appendBlobClient = blobContainerClient.GetAppendBlobClient(fullFileName);

        appendBlobClient.CreateIfNotExists();

        int maxBlockSize = appendBlobClient.AppendBlobMaxAppendBlockBytes;

        if (stream.Length <= maxBlockSize)
        {
            appendBlobClient.AppendBlock(stream);
        }
        else
        {
            long bytesLeft = stream.Length;

            while (bytesLeft > 0)
            {
                int blockSize = (int)Math.Min(bytesLeft, maxBlockSize);
                byte[] buffer = new byte[blockSize];
                stream.Read(buffer, 0, blockSize);
                appendBlobClient.AppendBlock(new MemoryStream(buffer));
                bytesLeft -= blockSize;
            }
        }
    }

    // Write a string to a file or storage (overwrite)
    public void WriteString(BlobContainerClient blobContainerClient, string fullFileName, string contents)
    {
        // Check if we have any contents
        if (string.IsNullOrEmpty(contents))
            return;

        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        // Write data to a blob
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(contents);
        using MemoryStream stream = new(byteArray);

        var blobClient = blobContainerClient.GetBlobClient(fullFileName);

        blobClient.Upload(stream);

    }

    // Read a string from a file or storage
    public string? ReadString(BlobContainerClient blobContainerClient, string fullFileName)
    {
        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        // No need to clear the path string
        string fn = fullFileName;

        // Read data from a blob
        var readBlobClient = blobContainerClient.GetBlobClient(fn);
        if (readBlobClient.Exists())
        {
            BlobDownloadInfo blobDownloadInfo = readBlobClient.Download();

            using (StreamReader reader = new(blobDownloadInfo.Content))
                return reader.ReadToEnd();
        }

        return null;
    }


    // Write a stream to a file or storage
    public void WriteStream(BlobContainerClient blobContainerClient, string fullFileName, string contentType, Stream stream)
    {
        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        // Write data to a blob
        var blobClient = blobContainerClient.GetBlobClient(fullFileName);

        // Rewind the memory stream back to the beginning
        stream.Seek(0, SeekOrigin.Begin);

        // Upload data
        blobClient.Upload(stream);

        // add context-type
        blobClient.SetHttpHeaders(new BlobHttpHeaders { ContentType = contentType });

        // Close the memory stream
        stream.Close();
    }

    // get a stream from file or storage
    public Stream? GetStream(BlobContainerClient blobContainerClient, string fullFileName)
    {
        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        // Read data from a blob
        var readBlobClient = blobContainerClient.GetBlobClient(fullFileName);
        if (readBlobClient.Exists())
            return readBlobClient.OpenRead();

        return null;
    }

    // Delete a file from storage
    public void DeleteFile(BlobContainerClient blobContainerClient, string fileName)
    {
        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        var blobClient = blobContainerClient.GetBlobClient(fileName);

        blobClient.DeleteIfExists();
    }

    // Check if the file exists
    public bool FileExists(BlobContainerClient blobContainerClient, string fullFileName)
    {
        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        if (blobContainerClient.Exists())
            if (blobContainerClient.GetBlobClient(fullFileName).Exists())
                return true;

        return false;
    }

    // Check if the container exists
    public bool ContainerExists(BlobContainerClient blobContainerClient, string containerName)
    {
        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        return blobContainerClient.Exists();
    }

    // Check if the container is empty
    public bool IsContainerEmpty(BlobContainerClient blobContainerClient, string containerName)
    {
        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        // Get the list of blobs in the container
        Azure.Pageable<BlobItem> blobs = blobContainerClient.GetBlobs();

        // Check if the list is empty
        if (blobs.Count() == 0)
            return true;

        return false;
    }

    // Get the list of files in a prefix
    public List<string[]> GetItemList(BlobContainerClient blobContainerClient, string prefix)
    {
        List<string[]> ret= new();

        if (blobContainerClient == null)
            throw new Exception("BlobStorageHelper is not initialized");

        // Get the blobs
        Azure.Pageable<BlobItem> blobItems = blobContainerClient.GetBlobs(BlobTraits.None, BlobStates.None, prefix);

        // Loop through the blobs
        foreach (BlobItem blobItem in blobItems)
            ret.Add(blobItem.Name.Split('/').ToArray());

        return ret;
    }

}