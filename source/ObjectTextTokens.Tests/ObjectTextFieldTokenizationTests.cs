using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectTextTokens.Tests {

    [TestClass]
    public class ObjectTextFieldTokenizationTests {

        [TestMethod]
        public void Should_replace_token_content_on_property() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test",
                Prop2 = "@Prop1@"
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test", testObject.Prop2);
        }

        [TestMethod]
        public void Should_not_replace_token_contents_on_readonly_property() {
            //Arrange
            var testObject = new TestObject();
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("@Prop1@", testObject.ReadonlyProp);
        }

        [TestMethod]
        public void Should_replace_token_content_on_property_regardles_of_token_casing() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test",
                Prop2 = "@pRop1@",
                Prop3 = "@PROP1@"
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test", testObject.Prop2);
            Assert.AreEqual("Test", testObject.Prop3);
        }

        [TestMethod]
        public void Should_replace_multiple_tokens_on_property() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                Prop2 = "Test2",
                Prop3 = "@Prop1@ - @Prop2@"
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test1 - Test2", testObject.Prop3);
        }

        [TestMethod]
        public void Should_replace_multiple_tokens_on_property_when_tokens_are_separated_by_one_character() {
            //Arrange
            var testObject = new TestObject() {
                Prop1 = "Test1",
                Prop2 = "Test2",
                Prop3 = "@Prop1@-@Prop2@"
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test1-Test2", testObject.Prop3);
        }

        [TestMethod]
        public void Should_replace_multiple_tokens_on_property_when_tokens_are_contiguous() {
            //Arrange
            var testObject = new TestObject() {
                Prop1 = "Test1",
                Prop2 = "Test2",
                Prop3 = "@Prop1@@Prop2@"
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test1Test2", testObject.Prop3);
        }

        [TestMethod]
        public void Should_replace_token_when_token_is_in_child_object() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                Child = new ChildTestObject {
                    Property1 = "@Prop1@"
                }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test1", testObject.Child.Property1);
        }

        [TestMethod]
        public void Should_replace_tokens_on_child_object_collections() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                ObjectCollection = new[] {
                    new ChildTestObject { Property1 = "@Prop1@" }
                }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test1", testObject.ObjectCollection.FirstOrDefault().Property1);
        }


        [TestMethod]
        public void Should_replace_tokens_on_grand_child_object() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                Child = new ChildTestObject {
                    GrandChild = new ChildTestObject {
                        Property1 = "@Prop1@"
                    }
                }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test1", testObject.Child.GrandChild.Property1);
        }

        [TestMethod]
        public void When_value_is_defined_in_nested_grandchild_object_should_replace_tokens() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "@child.grandchild.property1@",
                Child = new ChildTestObject {
                    GrandChild = new ChildTestObject {
                        Property1 = "Test"
                    }
                }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test", testObject.Prop1);
        }

        [TestMethod]
        public void Should_replace_token_when_token_points_to_child_object() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "@child.property1@",
                Child = new ChildTestObject {
                    Property1 = "TestChild"
                }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("TestChild", testObject.Prop1);
        }

        [TestMethod]
        public void Should_replace_token_on_keyValPair_key_value_fields() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                KeyValPair = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Key", "Text with @Prop1@") }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Text with Test1", testObject.KeyValPair.First().Value);
        }

        [TestMethod]
        public void Should_replace_token_on_dictionary_fields() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                Dictionary = new Dictionary<string, string>(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Key", "Text with @Prop1@") })
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Text with Test1", testObject.Dictionary.First().Value);
        }

        [TestMethod]
        public void When_throwOnUnfound_is_false_incorrect_token_used_should_return_empty_string() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                Prop2 = "@prop@"
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("", testObject.Prop2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void When_throwOnUnfound_is_true_incorrect_token_used_should_return_empty_string() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                Prop2 = "@prop@"
            };
            //Act
            try {
                ObjectTextFieldTokenization.ThrowOnUnfoundToken = true;

                ObjectTextFieldTokenization.Tokenize(testObject);
            } catch (Exception ex) {
                //Assert
                Assert.AreEqual("@prop@ token path not found in the object, check the token", ex.Message);
                throw;
            }
        }

        [TestMethod]
        public void When_tokens_have_dependencies_or_cascade_should_replace_all_tokens() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "Test1",
                Prop2 = "@prop1@",
                Child = new ChildTestObject {
                    Property1 = "@child.property2@",
                    Property2 = "@prop2@"
                }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("Test1", testObject.Prop2);
            Assert.AreEqual("Test1", testObject.Child.Property1);
            Assert.AreEqual("Test1", testObject.Child.Property2);
        }

        [TestMethod]
        public void When_token_is_enumerable_collection_should_return_comma_separated_strings() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "@collection@",
                Collection = new List<string> { "USA", "CA", "AU" }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("USA, CA, AU", testObject.Prop1);
        }

        [TestMethod]
        public void When_token_is_array_should_return_comma_separated_strings() {
            //Arrange
            var testObject = new TestObject {
                Prop1 = "@arrayStrings@",
                ArrayStrings = new string[] { "USA", "CA", "AU" }
            };
            //Act
            ObjectTextFieldTokenization.Tokenize(testObject);
            //Assert
            Assert.AreEqual("USA, CA, AU", testObject.Prop1);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void When_tokens_are_self_referencing_should_throw_exception() {
            //Arrange
            var testObject = new TestObject {
                Prop2 = "@prop2@"
            };
            //Act
            try {
                ObjectTextFieldTokenization.Tokenize(testObject);
            } catch (Exception ex) {
                //Assert
                Assert.AreEqual("@prop2@ token is self referencing, check the token", ex.Message);
                throw;
            }
        }

    }
}

