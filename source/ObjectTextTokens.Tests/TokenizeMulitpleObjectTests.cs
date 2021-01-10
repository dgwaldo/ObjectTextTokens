using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ObjectTextTokens.Tests {

    [TestClass]
    public class TokenizeMulitpleObjectTests {

        [TestMethod]
        public void Should_replace_token_content_on_property() {
            //Arrange
            var testObject = new TestObject { Prop2 = "@Prop1@" };
            var lookupObj = new SecondTestObject { Prop1 = "Test" };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject, lookupObj);
            //Assert
            Assert.AreEqual("Test", testObject.Prop2);
        }

        [TestMethod]
        public void Should_replace_multiple_tokens_on_property() {
            //Arrange
            var testObject = new TestObject {
                Prop3 = "@Prop1@ - @Prop2@"
            };
            var lookupObj = new SecondTestObject {
                Prop1 = "Test1",
                Prop2 = "Test2",
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject, lookupObj);
            //Assert
            Assert.AreEqual("Test1 - Test2", testObject.Prop3);
        }

        [TestMethod]
        public void Should_replace_token_when_token_is_in_child_object() {
            //Arrange
            var testObject = new TestObject {
                Child = new ChildTestObject {
                    Property1 = "@Prop1@"
                }
            };
            var lookupObj = new SecondTestObject {
                Prop1 = "Test1",
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject, lookupObj);
            //Assert
            Assert.AreEqual("Test1", testObject.Child.Property1);
        }

        [TestMethod]
        public void Should_replace_tokens_on_child_object_collections() {
            //Arrange
            var testObject = new TestObject {
                ObjectCollection = new[] {
                    new ChildTestObject { Property1 = "@Prop1@" }
                }
            };
            var lookupObj = new TestObject {
                Prop1 = "Test1"
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject, lookupObj);
            //Assert
            Assert.AreEqual("Test1", testObject.ObjectCollection.FirstOrDefault().Property1);
        }

        [TestMethod]
        public void When_tokens_have_dependencies_or_cascade_should_replace_all_tokens() {
            //Arrange
            var testObject = new TestObject {
                Prop2 = "@prop1@",
                Child = new ChildTestObject {
                    Property1 = "@child.property2@",
                    Property2 = "@prop2@"
                }
            };
            var lookupObj = new SecondTestObject {
                Prop1 = "Test1",
                Prop2 = "@prop1@",
                Child = new ChildTestObject {
                    Property1 = "@child.property2@",
                    Property2 = "@prop2@"
                }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject, lookupObj);
            //Assert
            Assert.AreEqual("Test1", testObject.Prop2);
            Assert.AreEqual("Test1", testObject.Child.Property1);
            Assert.AreEqual("Test1", testObject.Child.Property2);
        }

    }
}

