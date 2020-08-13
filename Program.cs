using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace JSONCompareKeys
{
    public class Program
    {
        static int Main(string[] args)
        {
            string[] mockArgs = { "en.json", "da.json", "template.json"};

            var filePaths = mockArgs;

            var dictionary = ReadFilesToJsonDictionary(filePaths);

            var keyErrors = CompareDocuments(dictionary);

            if (keyErrors.Count > 0)
            {
                FormatAndPrintErrors(keyErrors);
                return -1;
            }

            Console.WriteLine("No differences found in keys!");
            return 0;
        }

        private static Dictionary<string, JsonDocument> ReadFilesToJsonDictionary(string[] filePaths)
        {
            var dictionary = new Dictionary<string, JsonDocument>();

            foreach (var filePath in filePaths)
            {
                using var sr = new StreamReader(filePath, Encoding.UTF8);
                var deserialized = JsonDocument.Parse(sr.BaseStream);

                dictionary.Add(filePath, deserialized);
            }

            return dictionary;
        }

        private static List<KeyError> CompareDocuments(Dictionary<string, JsonDocument> dictionary)
        {
            var keyErrors = new List<KeyError>();

            foreach (var (fileName, document) in dictionary)
            {
                keyErrors.AddRange(IsNamingInconsistent(fileName, document.RootElement));

                var otherEntries = dictionary.Where((entry) => entry.Key != fileName);
                foreach (var entry in otherEntries)
                {
                    keyErrors.AddRange(FindAndCompareKeys(document.RootElement, fileName, entry));
                }
            }

            return keyErrors;
        }

        private static void FormatAndPrintErrors(List<KeyError> errors)
        {
            PrintErrors(FormatErrors(errors));
        }

        private static IEnumerable<IGrouping<string, ErrorCollection>> FormatErrors(List<KeyError> errors)
        {
            var errorCollectionList = new List<ErrorCollection>();

            for (var i = errors.Count - 1; i >= 0; i--)
            {
                var current = errors.ElementAt(i);

                var errorCollection = new ErrorCollection(current.Key, current.FileName, current.ErrorType);
                errorCollection.Errors.Add(current);

                for (var j = i - 1; j >= 0; j--)
                {
                    var next = errors.ElementAt(j);
                    if (new KeyErrorEqualityComparer().Equals(next, current))
                    {
                        errorCollection.Errors.Add(next);
                        errors.RemoveAt(j);
                        i--;
                    }
                }

                errorCollectionList.Add(errorCollection);
            }

            return errorCollectionList
                    .OrderBy(ec => ec.Key)
                        .ThenBy(ec => ec.ErrorType)
                    .GroupBy(ec => ec.FileName);
        }

        private static void PrintErrors(IEnumerable<IGrouping<string, ErrorCollection>> errorGroups)
        {
            foreach (var group in errorGroups)
            {
                Console.WriteLine($"{group.Key}:\n");
                foreach (var collection in group.ToList())
                {
                    switch (collection.ErrorType)
                    {
                        case ErrorType.InconsistentNaming:
                        {
                            foreach (var namingError in collection.Errors.Cast<InconsistentNamingKeyError>())
                            {
                                Console.WriteLine($"\t{namingError.Key} has inconsistent naming - Inconsistent naming of sub-key:\n\t\t{namingError.InconsistentName}\n");
                            }
                        } break;

                        case ErrorType.NotInOtherFile:
                        {
                            var notInFileNames = collection.Errors.Cast<NotInOtherFileKeyError>()
                                .Select(e => e.NotInFileName).ToList();
                            Console.WriteLine($"\t{collection.Key} does not exist in files:\n\t\t{string.Join(", ", notInFileNames)}\n");
                        } break;
                    }
                }
            }
        }

        private static List<KeyError> FindAndCompareKeys(JsonElement rootElement, string fileName, KeyValuePair<string, JsonDocument> tupleToCompare, string composedKey = "")
        {
            var errors = new List<KeyError>();
            using var enumerator = rootElement.EnumerateObject();

            foreach (var i in enumerator)
            {
                var key = !string.IsNullOrEmpty(composedKey) ? composedKey + "." + i.Name : i.Name;
                if (i.Value.ValueKind == JsonValueKind.Object)
                {
                    if(IsKeyInElement(key.Split('.'), tupleToCompare.Value.RootElement)){
                        errors.AddRange(FindAndCompareKeys(i.Value, fileName, tupleToCompare, key));
                    } else
                    {
                        errors.Add(new NotInOtherFileKeyError(key, fileName, tupleToCompare.Key));
                    }
                }
                else
                {
                    if(!IsKeyInElement(key.Split('.'), tupleToCompare.Value.RootElement))
                    {
                        errors.Add(new NotInOtherFileKeyError(key, fileName, tupleToCompare.Key));
                    }
                }
            }

            return errors;
        }

        private static bool IsKeyInElement(string[] keyTokens, JsonElement element)
        {
            using var enumerator = element.EnumerateObject();
            
                if (keyTokens.Length == 1)
                {
                    return enumerator.Any(e => e.Name == keyTokens[0]);
                }

                foreach (var e in enumerator)
                {
                    if (e.Name == keyTokens[0])
                    {
                        return IsKeyInElement(keyTokens.Where((value, index) => index != 0).ToArray(), e.Value);
                    }
                }

                return false;
        }

        private static List<KeyError> IsNamingInconsistent(string fileName, JsonElement element, string composedKey = "")
        {
            using var enumerator = element.EnumerateObject();
            var errors = new List<KeyError>();
            foreach (var i in enumerator)
            {
                var key = !string.IsNullOrEmpty(composedKey) ? composedKey + "." + i.Name : i.Name;

                if (i.Name.Contains('.'))
                {
                    errors.Add(new InconsistentNamingKeyError(key, fileName, i.Name));
                }

                if (i.Value.ValueKind == JsonValueKind.Object)
                {
                    errors.AddRange(IsNamingInconsistent(fileName, i.Value, key));
                }
            }

            return errors;
        }
    }
}
