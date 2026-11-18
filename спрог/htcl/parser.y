%{
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include "parser.tab.h"

int yylex(void);
void yyerror(const char *s);

#define MAX_VARS 100
#define OUT_SIZE 100000

typedef struct {
    char *tag;
    char *name;
    char *value;
} sym_entry;

sym_entry sym_table[MAX_VARS];
int sym_count = 0;

char output[OUT_SIZE];
int out_pos = 0;

/* Записать в буфер */
void emit(const char *fmt, ...) {
    va_list args;
    va_start(args, fmt);
    out_pos += vsnprintf(output + out_pos, OUT_SIZE - out_pos, fmt, args);
    va_end(args);
}

sym_entry *sym_find_by_name(const char *name) {
    for (int i = 0; i < sym_count; i++) {
        if (strcmp(sym_table[i].name, name) == 0)
            return &sym_table[i];
    }
    return NULL;
}

void sym_set(const char *tag, const char *name, const char *value) {
    for (int i = 0; i < sym_count; i++) {
        if (strcmp(sym_table[i].name, name) == 0) {
            free(sym_table[i].value);
            sym_table[i].value = strdup(value);
            if (tag && strcmp(tag, "") != 0) {
                free(sym_table[i].tag);
                sym_table[i].tag = strdup(tag);
            }
            return;
        }
    }

    if (sym_count >= MAX_VARS) {
        fprintf(stderr, "Слишком много переменных\n");
        return;
    }

    sym_table[sym_count].tag   = strdup(tag && tag[0] ? tag : "span");
    sym_table[sym_count].name  = strdup(name);
    sym_table[sym_count].value = strdup(value);
    sym_count++;
}

int get_num_value(const char *name) {
    sym_entry *e = sym_find_by_name(name);
    if (e) return atoi(e->value);
    return 0;
}
%}

%union {
    char *str;
    int   num;

    struct {
        char *tag;
        char *name;
        char *value;
    } assign;
}

%token <str> TOK_IDENT TOK_STRING
%token <num> TOK_NUMBER

%token TOK_IF TOK_ELSE
%token TOK_GT TOK_LT TOK_GTE TOK_LTE TOK_EQ TOK_NEQ
%token TOK_P TOK_H1 TOK_QUA TOK_MAKE TOK_ASSIGN

%type <num>    condition
%type <assign> assign

%%

input:
    /* пусто */
  | input line
;

line:
    '\n'
  | stmt '\n'
;

stmt:
      assign
      {
          sym_set($1.tag, $1.name, $1.value);
          free($1.tag);
          free($1.name);
          free($1.value);
      }

    | TOK_MAKE name_list
      { }

    | TOK_IF condition ',' assign ',' TOK_ELSE assign
      {
          if ($2) {
              sym_set($4.tag, $4.name, $4.value);
          } else {
              sym_set($7.tag, $7.name, $7.value);
          }

          free($4.tag); free($4.name); free($4.value);
          free($7.tag); free($7.name); free($7.value);
      }
;

assign:
      TOK_H1 TOK_IDENT TOK_ASSIGN TOK_STRING
      {
          $$.tag   = strdup("h1");
          $$.name  = $2;
          $$.value = $4;
      }
    | TOK_P TOK_IDENT TOK_ASSIGN TOK_STRING
      {
          $$.tag   = strdup("p");
          $$.name  = $2;
          $$.value = $4;
      }
    | TOK_QUA TOK_IDENT TOK_ASSIGN TOK_NUMBER
      {
          char buf[64];
          sprintf(buf, "%d", $4);
          $$.tag   = strdup("qua");
          $$.name  = $2;
          $$.value = strdup(buf);
      }
    | TOK_IDENT TOK_ASSIGN TOK_STRING
      {
          $$.tag   = strdup("");
          $$.name  = $1;
          $$.value = $3;
      }
;

condition:
      TOK_IDENT TOK_GT TOK_NUMBER
      {
          $$ = (get_num_value($1) > $3);
          free($1);
      }
    | TOK_IDENT TOK_LT TOK_NUMBER
      {
          $$ = (get_num_value($1) < $3);
          free($1);
      }
    | TOK_IDENT TOK_GTE TOK_NUMBER
      {
          $$ = (get_num_value($1) >= $3);
          free($1);
      }
    | TOK_IDENT TOK_LTE TOK_NUMBER
      {
          $$ = (get_num_value($1) <= $3);
          free($1);
      }
    | TOK_IDENT TOK_EQ TOK_NUMBER
      {
          $$ = (get_num_value($1) == $3);
          free($1);
      }
    | TOK_IDENT TOK_NEQ TOK_NUMBER
      {
          $$ = (get_num_value($1) != $3);
          free($1);
      }
;

name_list:
      name_item
    | name_list name_item
;

name_item:
      TOK_IDENT
      {
          sym_entry *e = sym_find_by_name($1);
          if (e) {
              emit("<%s>%s</%s>\n", e->tag, e->value, e->tag);
          } else {
              fprintf(stderr, "Переменная не найдена: %s\n", $1);
          }
          free($1);
      }
    | '('
      {
          emit("<div>\n");
      }
    | ')'
      {
          emit("</div>\n");
      }
;

%%

void yyerror(const char *s) {
    fprintf(stderr, "Ошибка: %s\n", s);
}

int main(void) {
    // Начинаем HTML-документ
    emit("<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"UTF-8\">\n<title>Generated</title>\n</head>\n<body>\n");

    if (yyparse() == 0) {
        emit("</body>\n</html>\n");

        FILE *f = fopen("output.html", "w");
        if (f) {
            fputs(output, f);
            fclose(f);
            printf("Успешно скомпилировано в output.html\n");
        } else {
            fprintf(stderr, "Не удалось создать output.html\n");
        }
    }
    return 0;
}