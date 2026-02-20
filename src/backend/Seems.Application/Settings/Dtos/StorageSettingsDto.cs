namespace Seems.Application.Settings.Dtos;

public class StorageSettingsDto
{
    public string Provider { get; set; } = "local";
    public LocalStorageConfig Local { get; set; } = new();
    public S3StorageConfig S3 { get; set; } = new();
}

public class LocalStorageConfig
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
}

public class S3StorageConfig
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
