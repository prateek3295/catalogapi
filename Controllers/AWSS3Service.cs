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

    public AWSS3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3:BucketName"];
    }

    public async Task<string> UploadFileToS3Async(string filePath, string key)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(fileStream, _bucketName, key);

        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
    }

    public string GeneratePresignedUrl(string objectKey)
    {
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(1) // Set the expiration time for the URL
        };

        var url = _s3Client.GetPreSignedURL(request);

        return url;
    }
}
