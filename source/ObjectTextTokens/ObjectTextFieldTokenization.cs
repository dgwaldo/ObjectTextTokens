using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ObjectTextTokens {

    /// <summary>
    /// Replaces object field values marked for tokenization with text representation of other fields.
    /// </summary>
    public class ObjectTextFieldTokenization {

        //Pattern to find @object.field@
        const string regexPattern = @"@\w*@|@\w*\.\w\S*@";

        public static bool ThrowOnUnfoundToken { get; set; } = false;

        public static void Tokenize(object objectToReplaceTextIn) {
            Tokenize(objectToReplaceTextIn, objectToReplaceTextIn);
        }

        /// <summary>
        /// Recursively replaces tokenized text (@object.field@) in the object.
        /// </summary>
        /// <param name="inputObj"></param>
        /// <param name="lookupObj"></param>
        /// <returns></returns>
        public static object Tokenize(Object inputObj, Object lookupObj) {

            if (inputObj == null) {
                return null;
            }

            Type objType = inputObj.GetType();
            PropertyInfo[] properties = objType.GetProperties();

            var unmatchedTokens = new List<string>();
            foreach (PropertyInfo property in properties) {
                var propertyValue = property.GetValue(inputObj);

                if (propertyValue != null && property.CanWrite) {
                    bool replaced = ReplaceTypeOfDictionary(inputObj, lookupObj, property, propertyValue);
                    replaced = replaced || ReplaceTypeOfIEnumerableKeyVal(inputObj, lookupObj, property, propertyValue);
                    replaced = replaced || ReplaceTypeOfArrayString(inputObj, lookupObj, property, propertyValue);
                    replaced = replaced || ReplaceTypeOfIEnumerableString(inputObj, lookupObj, property, propertyValue);
                    replaced = replaced || ReplaceTypeOfIEnumerableObjects(lookupObj, propertyValue);
                    replaced = replaced || RecurseObjectProperty(inputObj, lookupObj, property);

                    if (!replaced) {
                        replaced = AssignFieldTokenValue(inputObj, lookupObj, property);
                    }

                    // Check to see if the token has been replaced.
                    propertyValue = property.GetValue(inputObj);
                    var matchedTokens = propertyValue != null ? Regex.Matches(propertyValue.ToString(), regexPattern, RegexOptions.IgnoreCase) : null;
                    unmatchedTokens.AddRange(matchedTokens.OfType<Match>().Select(x => x.Value));
                }

                if (unmatchedTokens.Any()) { // Rerun tokenization to try and match tokens that may have a dependency on other tokens being filled first.
                    Tokenize(inputObj, lookupObj);
                }

            }

            return inputObj;
        }

        private static bool ReplaceTypeOfDictionary(object inputObj, object lookupObj, PropertyInfo property, object propertyValue) {
            if (propertyValue is IDictionary<string, string> dictionary) {
                var outDict = new Dictionary<string, string>();
                foreach (var keyVal in dictionary) {
                    var val = keyVal.Value;
                    outDict.Add(keyVal.Key, GetFieldTokenValue(val, lookupObj) as string);
                }
                property.SetValue(inputObj, outDict);
                return true;
            }
            return false;
        }

        private static bool ReplaceTypeOfIEnumerableKeyVal(object inputObj, object lookupObj, PropertyInfo property, object propertyValue) {
            if (propertyValue is IEnumerable<KeyValuePair<string, string>> keyPairList) {
                var outlist = new List<KeyValuePair<string, string>>();
                foreach (var keyVal in keyPairList) {
                    outlist.Add(new KeyValuePair<string, string>(keyVal.Key, GetFieldTokenValue(keyVal.Value, lookupObj) as string));
                }
                property.SetValue(inputObj, outlist);
                return true;
            }
            return false;
        }

        private static bool ReplaceTypeOfIEnumerableString(object inputObj, object lookupObj, PropertyInfo property, object propertyValue) {
            if (propertyValue is IEnumerable<string> list) {
                var collection = new List<string>();
                foreach (var textItem in list) {
                    collection.Add(GetFieldTokenValue(textItem as string, lookupObj) as string);
                }
                property.SetValue(inputObj, collection);
                return true;
            }
            return false;
        }

        private static bool ReplaceTypeOfArrayString(object inputObj, object lookupObj, PropertyInfo property, object propertyValue) {
            if (propertyValue is string[] list) {
                var collection = new List<string>();
                foreach (var textItem in list) {
                    collection.Add(GetFieldTokenValue(textItem as string, lookupObj) as string);
                }
                property.SetValue(inputObj, collection.ToArray());
                return true;
            }
            return false;
        }

        private static bool ReplaceTypeOfIEnumerableObjects(object lookupObj, object propertyValue) {
            if (propertyValue is IList elems) {
                foreach (var item in elems) {
                    Tokenize(item, lookupObj);
                }
                return true;
            }
            return false;
        }

        private static bool RecurseObjectProperty(object inputObj, object lookupObj, PropertyInfo property) {
            if (property.PropertyType.IsClass && !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string)) {
                Tokenize(property.GetValue(inputObj), lookupObj);
                return true;
            }
            return false;
        }

        private static bool AssignFieldTokenValue(object inputObj, object lookupObj, PropertyInfo property) {
            var fieldContents = property.GetValue(inputObj);
            var tokenFieldContents = GetFieldTokenValue(fieldContents, lookupObj);
            if (tokenFieldContents != null) {
                property.SetValue(inputObj, tokenFieldContents);
                return true;
            }
            return false;
        }

        private static object GetFieldTokenValue(object fieldContents, object lookupObj) {
            var matchedTokens = fieldContents != null ? Regex.Matches(fieldContents.ToString(), regexPattern, RegexOptions.IgnoreCase) : null;
            if (matchedTokens != null && matchedTokens.OfType<Match>().Any()) {
                matchedTokens.OfType<Match>().ToList().ForEach(t => {
                    var fieldPath = t.Value.Replace("@", String.Empty);
                    var content = GetPropertyValue(lookupObj, fieldPath, t.Value)?.ToString();
                    if (content == null && ThrowOnUnfoundToken) {
                        throw new Exception($"{t.Value} token path not found in the object, check the token");
                    }
                    fieldContents = content != null ? fieldContents.ToString().Replace(t.Value, content) : fieldContents.ToString().Replace(t.Value, "");
                });
            }
            return fieldContents;
        }

        public static object GetPropertyValue(object src, string propName, string tokenName) {
            if (src == null) {
                return null;
            }
            if (propName.Contains(".")) {//complex type nested
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0], tokenName), temp[1], tokenName);
            } else {
                var prop = src.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                var propertyValue = prop?.GetValue(src);
                if (propertyValue != null && propertyValue is string[] stringArray) {
                    return string.Join(", ", stringArray).ToString();
                }
                if (propertyValue != null && propertyValue is IEnumerable<string> enumerableString) {
                    return string.Join(", ", enumerableString).ToString();
                }
                return propertyValue;
            }
        }

    }

}
