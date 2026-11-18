%{
#include "parser.tab.h"
#include <string.h>
#include <stdlib.h>
#include <stdio.h>

static char *str_dup(const char *s) {
    if (!s) return NULL;
    char *copy = malloc(strlen(s) + 1);
    if (copy) strcpy(copy, s);
    return copy;
}
%}

%option noyywrap
%option yylineno

%%

"p"                 { return TOK_P; }
"h1"                { return TOK_H1; }
"qua"               { return TOK_QUA; }
"make"              { return TOK_MAKE; }
"if"                { return TOK_IF; }
"else"              { return TOK_ELSE; }

[a-zA-Z_][a-zA-Z0-9_]* {
                        yylval.str = str_dup(yytext);
                        return TOK_IDENT;
                      }

\"[^\"]*\"          {
                        size_t len = strlen(yytext) - 2;
                        char *s = malloc(len + 1);
                        if (s) {
                            memcpy(s, yytext + 1, len);
                            s[len] = '\0';
                        }
                        yylval.str = s;
                        return TOK_STRING;
                      }

[0-9]+              {
                        yylval.num = atoi(yytext);
                        return TOK_NUMBER;
                      }

">="                { return TOK_GTE; }
"<="                { return TOK_LTE; }
"=="                { return TOK_EQ; }
"!="                { return TOK_NEQ; }
">"                 { return TOK_GT; }
"<"                 { return TOK_LT; }

"="                 { return TOK_ASSIGN; }
","                 { return ','; }
"["                 { return '['; }
"]"                 { return ']'; }

[ \t\r]+            { /* skip spaces and tabs */ }
\n                  { return '\n'; }

.                   {
                        fprintf(stderr, "Lexer error at line %d: unknown character '%c'\n",
                                yylineno, yytext[0]);
                    }

%%