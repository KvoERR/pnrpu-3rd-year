%{
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "parser.tab.h"
int yylex(void);
void yyerror(const char *s);

#define MAX_VARS 100

typedef struct {
    char *name;
    char *value;
} sym_entry;

sym_entry sym_table[MAX_VARS];
int sym_count = 0;

char *sym_find(const char *name) {
    for (int i = 0; i < sym_count; i++) {
        if (strcmp(sym_table[i].name, name) == 0) {
            return sym_table[i].value;
        }
    }
    return NULL;
}

void sym_set(const char *name, const char *value) {
    for (int i = 0; i < sym_count; i++) {
        if (strcmp(sym_table[i].name, name) == 0) {
            free(sym_table[i].value);
            sym_table[i].value = strdup(value);
            return;
        }
    }
    sym_table[sym_count].name = strdup(name);
    sym_table[sym_count].value = strdup(value);
    sym_count++;
}
%}

%union {
    char *str;
    int num;
}

/* Сначала объявляем токены */
%token <str> TOK_IDENT TOK_STRING
%token <num> TOK_NUMBER
%token TOK_DOLLAR TOK_IF TOK_ELSE
%token TOK_GTE TOK_LTE TOK_EQ TOK_NEQ
%token TOK_P TOK_H1 TOK_MAKE TOK_ASSIGN TOK_LT_TAG TOK_GT_TAG

%%

/* Здесь начинаются правила грамматики */
input:
    /* пусто */
  | input line
;

line:
    '\n'
  | expr '\n'
;

expr:
    TOK_H1 TOK_IDENT TOK_ASSIGN TOK_STRING  { sym_set($2, $4); printf("<%s>%s</%s>\n", $1, $4, $1); free($1); free($2); free($4); }
  | TOK_MAKE TOK_IDENT                      { char *v = sym_find($2); if (v) printf("<h1>%s</h1>\n", v); else fprintf(stderr, "Не найдена: %s\n", $2); free($2); }
  | TOK_IF '(' TOK_IDENT ')'   { printf("Условие: if(%s)\n", $3); free($3); }
;

%%

/* Функции в самом конце */
void yyerror(const char *s) {
    fprintf(stderr, "Ошибка: %s\n", s);
}

int main(void) {
    return yyparse();
}