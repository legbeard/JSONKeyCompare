using System.Collections.Generic;

namespace JSONCompareKeys
{
    public class ErrorCollection
    {
        public ErrorCollection(string key, string fileName, ErrorType errorType, List<KeyError> errors = null)
        {
            Key = key;
            FileName = fileName;
            ErrorType = errorType;
            Errors = errors ?? new List<KeyError>();
        }

        public string Key { get; set; }
        public string FileName { get; set; }
        public List<KeyError> Errors { get; set; }

        public ErrorType ErrorType { get; set; }
    }

    public class KeyErrorEqualityComparer : IEqualityComparer<KeyError>
    {
        public bool Equals(KeyError x, KeyError y)
        {
            return x.Key.Equals(y.Key) && x.FileName.Equals(y.FileName) && x.ErrorType.Equals(y.ErrorType);
        }

        public int GetHashCode(KeyError obj)
        {
            return (obj.Key.GetHashCode() + obj.FileName.GetHashCode() + obj.ErrorType.GetHashCode()).GetHashCode();
        }
    }
}