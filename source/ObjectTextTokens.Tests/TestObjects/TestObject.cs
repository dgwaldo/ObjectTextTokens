using System.Collections.Generic;

namespace ObjectTextTokens.Tests {
    public class TestObject {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public int Prop4 { get; set; }
        public ChildTestObject Child { get; set; }

        public IEnumerable<ChildTestObject> ObjectCollection { get; set; }
        public IEnumerable<string> Collection { get; set; }
        public string[] ArrayStrings { get; set; }

        public List<KeyValuePair<string, string>> KeyValPair { get; set; }
        public Dictionary<string, string> Dictionary { get; set; }

    }

}

