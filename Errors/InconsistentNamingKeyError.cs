namespace JSONCompareKeys
{
    public class InconsistentNamingKeyError : KeyError
    {
        public string InconsistentName { get; set; }

        public InconsistentNamingKeyError(string key, string fileName, string inconsistentName) : base(key, fileName)
        {
            InconsistentName = inconsistentName;
            this.ErrorType = ErrorType.InconsistentNaming;
        }
    }
}