using Benchmark.Client.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Client.Storage
{
    public interface IStorage
    {
        /// <summary>
        /// Get Reports
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string[]> Get(string path, string prefix, CancellationToken ct);
        /// <summary>
        /// List report names
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string[]> List(string path, string prefix, CancellationToken ct);
        /// <summary>
        /// Save report
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="overwrite"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task Save(string path, string prefix, string name, string content, bool overwrite = false, CancellationToken ct = default);
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

        /// <summary>
        /// Get reports
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string[]> Get(string path, string prefix, CancellationToken ct = default)
        {
            var dir = $"{path}/{prefix.TrimEnd('/')}";
            _logger.LogInformation($"Get content from local storage {dir}");

            if (!Directory.Exists(dir))
                throw new FileNotFoundException($"Directory not found. {dir}");

            var contents = new List<string>();
            foreach (var item in Directory.EnumerateFiles(dir))
            {
                var content = File.ReadAllText(item);
                contents.Add(content);
            }
            return contents.ToArray();
        }

        /// <summary>
        /// List report names
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string[]> List(string path, string prefix, CancellationToken ct)
        {
            var dir = $"{path}/{prefix.TrimEnd('/')}";
            _logger.LogInformation($"listing content from local storage {dir}");

            return Directory.EnumerateFiles(dir).ToArray();
        }

        /// <summary>
        /// Save report to desired local directory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="overwrite"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Save(string path, string prefix, string name, string content, bool overwrite = false, CancellationToken ct = default)
        {
            var dir = $"{path}/{prefix.TrimEnd('/')}";
            _logger.LogInformation($"Save content to local storage {dir}");

            Directory.CreateDirectory(dir);
            var basePath = Path.Combine(dir, name);
            var savePath = overwrite ? basePath : GetSafeSavePath(basePath, 1);
            await File.WriteAllTextAsync(savePath, content, ct);
        }

        /// <summary>
        /// Get safe filename to save. If same name found, increment index and renew filename.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="index"></param>
        /// <param name="suffixNamePattern"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        private string GetSafeSavePath(string basePath, int index, string suffixNamePattern = "{0:000}", string savePath = "")
        {
            if (index == 999)
                return savePath;
            if (string.IsNullOrEmpty(savePath))
                savePath = basePath;
            if (!File.Exists(savePath))
                return savePath;

            var fileName = Path.GetFileNameWithoutExtension(basePath) + "_" + string.Format(suffixNamePattern, index) + Path.GetExtension(basePath);
            savePath = Path.Combine(Path.GetDirectoryName(basePath), fileName);
            return GetSafeSavePath(basePath, index + 1, suffixNamePattern, savePath);
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

        /// <summary>
        /// Get report from S3 Bucket
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string[]> Get(string path, string prefix, CancellationToken ct = default)
        {
            _logger.LogInformation($"downloading content from S3. bucket {path}, prefix {prefix}");

            // todo: continuation
            var objects = await _client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
            {
                BucketName = path,
                Prefix = prefix,
            }, ct);

            if (objects.KeyCount == 0)
                return Array.Empty<string>();

            var contents = new ConcurrentBag<string>();
            var tasks = objects.S3Objects.Select(async x =>
            {
                using var res = await _client.GetObjectAsync(new Amazon.S3.Model.GetObjectRequest
                {
                    BucketName = x.BucketName,
                    Key = x.Key,
                }, ct);
                using var stream = res.ResponseStream;
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                contents.Add(content);
            });
            await Task.WhenAll(tasks);
            return contents.ToArray();
        }

        /// <summary>
        /// List report of S3 Bucket
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string[]> List(string path, string prefix, CancellationToken ct)
        {
            _logger.LogInformation($"listing content from S3. bucket {path}, prefix {prefix}");
            var objects = await _client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
            {
                BucketName = path,
                Prefix = prefix,
            }, ct);
            return objects.S3Objects.Select(x => x.Key).ToArray();
        }

        /// <summary>
        /// Save report to S3 Bucket
        /// </summary>
        /// <param name="path">BucketName</param>
        /// <param name="prefix">Bucket Prefix</param>
        /// <param name="name">Key Name</param>
        /// <param name="content">File Content</param>
        /// <param name="overwrite"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Save(string path, string prefix, string name, string content, bool overwrite = false, CancellationToken ct = default)
        {
            var key = $"{prefix.TrimEnd('/')}/{name}";

            _logger.LogInformation($"uploading content to S3. bucket {path}, prefix {prefix}, key {key}");
            await _client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = path,
                ContentBody = content,
                Key = key,
            }, ct);
        }
    }
}
