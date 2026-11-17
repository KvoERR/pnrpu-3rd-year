%{
#include "parser.tab.h"
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <stdarg.h>

static char *str_dup(const char *s) {
    if (!s) return NULL;
    char *copy = malloc(strlen(s) + 1);
    if (copy) strcpy(copy, s);
    return copy;
}

char *tok_make(const char *fmt, ...) {
    va_list args;
    va_start(args, fmt);
    int len = vsnprintf(NULL, 0, fmt, args) + 1;
    va_end(args);
    
    char *buf = malloc(len);
    va_start(args, fmt);
    vsnprintf(buf, len, fmt, args);
    va_end(args);
    
    return buf;
}
%}

%option noyywrap
%option yylineno

%%

"h1"                { return TOK_H1; }
"make"              { return TOK_MAKE;}

"$"                 { return TOK_DOLLAR; }
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

"="                 { return TOK_ASSIGN; }
"{"                 { return '{'; }
"}"                 { return '}'; }
"("                 { return '('; }
")"                 { return ')'; }
","                 { return ','; }
"/"                 { return '/'; }
"<"                 { return TOK_LT_TAG; }
">"                 { return TOK_GT_TAG; }

[ \t\n\r]+          { /* skip whitespace */ }

.                   { fprintf(stderr, "Lexer error at line %d: unknown character '%c'\n", 
                             yylineno, yytext[0]); }

%%