namespace JSONCompareKeys
{
    public class NotInOtherFileKeyError : KeyError
    {
        public string NotInFileName { get; set; }

        public NotInOtherFileKeyError(string key, string fileName, string notInFileName) : base(key, fileName)
        {
            NotInFileName = notInFileName;
            this.ErrorType = ErrorType.NotInOtherFile;
        }
    }
}