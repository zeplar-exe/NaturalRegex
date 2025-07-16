# NaturalRegex

A hobby programming language which allows expression of Regex in more natural terms.

## Setup

When building the library manually, you will need a recent version of ANTLR to generate the necessary C# bindings 
for the parser and lexer.

## Syntax

Constant variables are defined as such:

```
define "my_variable_name" <any expression>
```

Expressions take one of the following forms:

* **Literal sets**: \[abc123\]
* **Raw regex**: 'abc@rbg(.*)'
* **Variable/procedure references**: 
  * (my multi word procedure name 1 'arg2' (some variable))
  * (my variable)

Sequences are technically expressions as well:

```
{
    (start of line)
    'Hello world!'
    (end of line)
}
```

Finally, matching, or otherwise exporting regex from the script, is like so:

```
match (some variable)
match 'abc'
```

A 'script' can contain any number of match statements, and regex will be appended in the order that the match statements
are declared.

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

With the following code, this 'script' generates a nonexhaustive regex for email validation.


```csharp
using NaturalRegex;

var theScript = "..."
var env = new NatRegEnvironment().WithStandardRegexProcedures().WithStandardAsciiSets();
var theRegex = NatReg.GenerateRegex(theScript, env);

Console.WriteLine(theRegex);
// ^(?:[a-zA-Z0-9._%+\-""\\])+@(?:[a-zA-Z0-9.\-""])+\.(?:[a-zA-Z0-9]{1,})$
```

`WithStandardRegexProcedures` and `WithStandardAsciiSets` import common Regex expressions (like "one or more") and 
character sets (equivalent of Regex character sets) such as "any alphanumeric" or "any numeral".

More specifically, WithStandardRegexProcedures implements the following:

- Start of line
  - (end of line)
- End of line
  - (end of line)
- Capture groups
  - (capture group \<exp\>)
  - (group \<exp\>)
- One or more
  - (one or more \<exp\>)
  - (at least one \<exp\>)
  - (at least once \<exp\>)
- Zero or more
  - (zero or more \<exp\>)
  - (any amount \<exp\>)
  - (any amount of times \<exp\>)
- Zero or one
  - (once or none \<exp\>)
  - (at most once \<exp\>)
  - (optional \<exp\>)
  - (optionally \<exp\>)
  - (once at most \<exp\>)
  - (maybe \<exp\>)
- Logical or
  - (one of \<exp1\> \<exp2\> \<exp3\> ... \<expN\>)
  - (or \<exp1\> \<exp2\> \<exp3\> ... \<expN\>)
- Up to n
  - (up to n \<exp\> \<n\>)
- At least n
  - ( n or more \<exp\> \<n\>)
  - ( at least n \<exp\> \<n\>)
- Between n and m
  - (between n and m \<exp\> \<n\> \<m\>)
- Positive lookahead
  - (lookahead \<exp\>)
  - (positive lookahead \<exp\>)
  - (followed by lookahead \<exp\>)
- Negative lookahead
  - (negative lookahead \<exp\>)
  - (not followed by \<exp\>)
  - (not \<exp\>)
- Positive lookbehind
  - (lookbehind \<exp\>)
  - (positive lookbehind \<exp\>)
- Negative lookbehind
  - (negative lookbehind \<exp\>)
- Lazy match (note: in normal regex, this should only be used after a quantifier; the compiler will not provide a 
warning if this is used incorrectly)
  - (lazy \<exp\>)
  - (lazily \<exp\>)

...and WithStandardAsciiSets implements the following:

- (any alphanumeric)
- (any lowercase)
- (any uppercase)
- (any numeral)
- (any digit)
- (any number)
- (any line break)
- (any newline)
- (lowercase alphanumeric)
- (uppercase alphanumeric)
- (any word character)
- (any character)


## Notes

* Literal sets are copied into the final regex directly since there are no transformations to apply to them.
* You can define your own variables/procedures with `NatRegEnvironment.WithVariable` 
by passing a name and a `NatRegExpression`. 
  * The same goes for procedures with `NatRegEnvironment.WithProcedure`. 
  These are C# functions that generate regex based on their arguments.