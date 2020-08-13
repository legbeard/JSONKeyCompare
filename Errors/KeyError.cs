namespace JSONCompareKeys
{
    public abstract class KeyError
    {
        public string Key { get; set; }
        public string FileName { get; set; }
        public ErrorType ErrorType;
        protected KeyError(string key, string fileName)
        {
            Key = key;
            FileName = fileName;
        }
    }
}