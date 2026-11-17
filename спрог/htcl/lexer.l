%{
#include "parser.tab.h"
#include <string.h>
#include <stdlib.h>

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

"$"                 { return TOK_DOLLAR; }
"if"                { return TOK_IF; }
"else"              { return TOK_ELSE; }

/* Идентификаторы (теги, имена переменных, имена атрибутов) */
[a-zA-Z_][a-zA-Z0-9_]* {
                        yylval.str = str_dup(yytext);
                        return TOK_IDENT;
                      }

/* Строки в двойных кавычках (значения атрибутов, текст) */
"\""[^\"]*\""        {
                        size_t len = strlen(yytext) - 2;
                        char *s = malloc(len + 1);
                        if (s) {
                            memcpy(s, yytext + 1, len);
                            s[len] = '\0';
                        }
                        yylval.str = s;
                        return TOK_STRING;
                      }

/* Операторы сравнения */
">="                { return TOK_GTE; }
"<="                { return TOK_LTE; }
"=="                { return TOK_EQ; }
"!="                { return TOK_NEQ; }
">"                 { return TOK_GT; }
"<"                 { return TOK_LT; }

/* Атрибут: name="value" — это комбинация IDENT + "=" + STRING.
   Лексер отдаёт их по отдельности, парсер соберёт в пару. */
"="                 { return TOK_ASSIGN; }

/* Символы скобок и разделителей */
"{"                 { return '{'; }
"}"                 { return '}'; }
"("                 { return '('; }
")"                 { return ')'; }
","                 { return ','; }

/* Угловые скобки для тегов */
"<"                 { return TOK_LT_TAG; }
">"                 { return TOK_GT_TAG; }
"/"                 { return '/'; }

/* Пробелы, табуляции, переносы строк — пропускаем */
[ \t\n\r]+          { /* skip whitespace */ }

/* Любой другой символ — ошибка */
.                   { fprintf(stderr, "Lexer error at line %d: unknown character '%c'\n",
                             yylineno, yytext[0]); }

%%
