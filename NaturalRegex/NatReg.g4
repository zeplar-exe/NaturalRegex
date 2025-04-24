grammar NatReg;

program: statement+?;

statement
	: DEFINE STRING expression #DefineStatement
	| MATCH expression #MatchStatement;

expression
	: expression op=(PLUS|MINUS) expression #AddExpression
    | reference #ReferenceExpression
	| NUMBER #NumberExpression
	| REGEX #RegexExpression
	| LITERAL_SET #LiteralSetExpression
	| LBRACE expression+? RBRACE #SequenceExpression;

reference: LPAREN WORD+ expression* RPAREN;

COMMENT: ';'.*?';' -> skip;
WHITESPACE: [ \t\r\n]+ -> skip;

LPAREN: '(';
RPAREN: ')';
LBRACKET: '[';
RBRACKET: ']';
LBRACE: '{';
RBRACE: '}';
PLUS: '+';
MINUS: '-';

LITERAL_SET: LBRACKET .+? RBRACKET;
REGEX: ('\'').*?('\'');
STRING: ('"')(.|ESCAPE)*?('"');
ESCAPE: '\\"' | '\\\\';

DEFINE: 'define';
SET: 'set';
MATCH: 'match';

WORD: [a-zA-Z_]+;
NUMBER: [0-9] | [0-9]+'.'[0-9]+;