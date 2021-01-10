# Object Text Tokens

Object text tokens is a library that allows you to do text templating within your .net objects. Use it to provide simple text templating within your application.

Using tokens case insensitive tokens taking the form of @object.property@, you can do text templating within your objects.

### Supports using data from:

* Object Properties (string, int, double, etc.)
* Child Object Properties
* Properties using List or Arrays of strings (output as comma separated lists)
### Supports replacing data in:
* Object Properties (string)
* Dictionary Collections  Dictionary<K,V> where V is string
* IEnumerable<KeyValuePair<string, string>
* IEnumerable<string>
* string[]
* IEnumerable<T> where T is Class

### Also supports

* Cascading token replacement (tokens that depend on other tokens to be replaced first)
### Does not support
* Object Methods
* Private Fields


## Installation

Use the [NuGet](https://www.nuget.org/) package manager to install ObjectTextTokens.

```
Install-Package ObjectTextTokens
```

## Usage

```C#
using ObjectTextTokens;

// Example: Single object with templating and data
var objectSchema = new YourObject { 
   Property1 = "Dog",
   Property2 = "@property1@",
   Child = new ChildObject {
      ChildProp1 = "Cat",
      ChildProp2 = "@Property1@",
      ChildArray = new string[] { "Chicken", "Eagle" }
   },
   Property3 = "@child.childProp@",
   Property4 = "@child.childArray@, @property1@"
};

ObjectTextFieldTokenization.Tokenize(objectSchema);

// Returns 
// objectSchema.Property2 // "Dog";
// objectSchema.Property3 // "Cat";
// objectSchema.Property4 // "Chicken, Eagle, Dog"
// objectSchema.Child.ChildProp2 // "Dog"

// Example: Templated object and separate object with lookup data
var templatedObj = new YourObject { 
   Property2 = "@property1@",
   Property3 = "@child.childProp@"
};

var lookupObj = new YourOtherObject { 
   Property1 = "Dog",
   Child = new ChildObject {
      ChildProp1 = "Cat"
   }
};

ObjectTextFieldTokenization.Tokenize(templatedObj, lookupObj);

// Returns 
// templatedObj.Property2 // "Dog";
// templatedObj.Property3 // "Cat";

```

## Testing
Currently this is a static class. The implication for testing is that your code will run through this class when testing. If this is problematic wrap this code with an interface before using in your project. 

## Roadmap
* Future work might be on providing an instantiatable class and an interface for injecting, along with a static wrapper. 

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
