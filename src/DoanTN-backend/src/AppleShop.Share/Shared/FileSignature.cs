namespace AppleShop.Share.Shared
{
    public class FileSignature
    {
        public string Extension { get; }
        public byte?[] Signature { get; }

        public FileSignature(string extension, byte?[] signature)
        {
            Extension = extension;
            Signature = signature;
        }
    }
}