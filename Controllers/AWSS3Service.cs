using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Catalog.API;
public class AWSS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _cloudFrontUrl;

    public AWSS3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3:BucketName"];
        _cloudFrontUrl = configuration["CloudFrontDomain"];
    }

    public AWSS3Service()
    {

    }

    public async Task<string> UploadFileToS3Async(string filePath, string key)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(fileStream, _bucketName, key);

        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
    }

    public string GetCloudFrontUrl(string objectKey)
    {
        string imageUrl = _cloudFrontUrl + objectKey;
        return imageUrl;
    }
}
