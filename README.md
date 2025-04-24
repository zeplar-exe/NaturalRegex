# NaturalRegex

A hobby programming language which allows expression of Regex in more natural terms.

## Setup

When building the library manually, you will need a recent version of ANTLR to generate the necessary C# bindings 
for the parser and lexer.

## Example

```
define "username_character" (any alphanumeric) + [._%+\-""] + [\\]
define "domain_character" (any alphanumeric) + [.\-""]

define "seq_a" {
	(start of line)
	(one or more (username_character))
	'@'
	(one or more (domain_character))
	'.'
	(at least n (any alphanumeric) 1)
	(end of line)
}

match (seq_a)

define "seq_b" {
	(at least n [abcdefg] 5)
	'Hello World'
}

; match (seq_b) this is a comment ;
```

With the following code, this 'script' generates a relatively basic Regex for email validation.


```csharp
var env = new NatRegEnvironment().WithStandardRegexProcedures().WithStandardLatinSets();
var theRegex = NaturalRegex.NaturalRegex.GenerateRegex(testData, env);

// [...]
```

`WithStandardRegexProcedures` and `WithStandardLatinSets` import common Regex expressions (like "one or more") and 
character sets (equivalent of Regex character sets) such as "any alphanumeric" or "any numeral".

## Notes

* Single quotes represent direct Regex expressions to be copied verbatim. 
Double quotes are exclusively used to define identifier names.
* Character sets are effectively copied verbatim as Regex as well.
* You can define your own variables/procedures with `NatRegEnvironment.WithVariable` by passing a name and a 
* `NatRegeExpression`. 