namespace TestTaskTraineeCamp.Server
{
    public class AzureBlobSettingsOption
    {
        public const string ConfigKey = "AzBlobSettings";

        public string ConnectionName { get; set; } = default!;

        public string ConnectionString { get; set; } = default!;

        public string ContainerName {  get; set; } = default!;
    }
}
