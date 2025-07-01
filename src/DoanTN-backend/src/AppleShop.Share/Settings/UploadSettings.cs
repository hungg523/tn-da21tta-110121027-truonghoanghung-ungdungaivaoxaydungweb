namespace AppleShop.Share.Settings
{
    public class UploadSettings
    {
        public BasePaths BasePaths { get; set; }
    }

    public class BasePaths
    {
        public string Images { get; set; }
        public string Videos { get; set; }
    }
}