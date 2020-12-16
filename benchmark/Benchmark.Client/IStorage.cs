using Benchmark.Client.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Client
{
    public interface IStorage
    {
        Task Upload(string path, string prefix, string name, string content, CancellationToken ct);
        Task Download(string path, string prefix, string downloadPath, CancellationToken ct);
    }

    public static class StorageFactory
    {
        public static IStorage Create(ILogger logger)
        {
            // todo: Google... etc...?
            if (AmazonUtils.IsAmazonEc2())
            {
                return new AmazonS3Storage(logger);
            }
            else
            {
                // fall back
                return new LocalStorage(logger);
            }
        }
    }


    public class LocalStorage : IStorage
    {
        private readonly ILogger _logger;
        public LocalStorage(ILogger logger)
        {
            _logger = logger;
        }
        public async Task Download(string path, string prefix, string downloadPath, CancellationToken ct = default)
        {
            var dir = $"{path}/{prefix}";
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            foreach (var item in Directory.EnumerateFiles(dir))
            {
                var dest = Path.Combine(downloadPath, Path.GetFileName(item));
                File.Copy(item, dest, true);
            }
        }

        /// <summary>
        /// Save content to desired local directory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Upload(string path, string prefix, string name, string content, CancellationToken ct = default)
        {
            var dir = Path.Combine(path, prefix);
            Directory.CreateDirectory(dir);

            _logger.LogInformation($"uploading content to local storage {dir}");
            await File.WriteAllTextAsync(Path.Combine(dir, name), content, ct);
        }
    }

    public class AmazonS3Storage : IStorage
    {
        private readonly ILogger _logger;
        private readonly Amazon.S3.AmazonS3Client _client;

        public AmazonS3Storage(ILogger logger)
        {
            _logger = logger;
            _client = new Amazon.S3.AmazonS3Client(Amazon.Util.EC2InstanceMetadata.Region);
        }

        public async Task Download(string path, string prefix, string downloadPath, CancellationToken ct = default)
        {
            // todo: continuation
            var objects = await _client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
            {
                BucketName = path,
                Prefix = prefix,
            }, ct);

            if (objects.KeyCount == 0)
                return;

            var tasks = objects.S3Objects.Select(async x =>
            {
                var res = await _client.GetObjectAsync(new Amazon.S3.Model.GetObjectRequest
                {
                    BucketName = x.BucketName,
                    Key = x.Key,
                }, ct);
                var downloadFullPath = Path.Combine(downloadPath, Path.GetFileName(res.Key));
                await res.WriteResponseStreamToFileAsync(downloadFullPath, false, ct);
            });
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Upload content to S3 bucket
        /// </summary>
        /// <param name="path">BucketName</param>
        /// <param name="prefix">Bucket Prefix</param>
        /// <param name="name">Key Name</param>
        /// <param name="content">File Content</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Upload(string path, string prefix, string name, string content, CancellationToken ct = default)
        {
            var key = $"{prefix.TrimEnd('/')}/{name}";

            _logger.LogInformation($"uploading content to S3. bucket {path}, key {key}");
            await _client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = path,
                ContentBody = content,
                Key = key,
            }, ct);
        }
    }
}
