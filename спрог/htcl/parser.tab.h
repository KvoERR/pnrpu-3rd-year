/* A Bison parser, made by GNU Bison 3.8.2.  */

/* Bison interface for Yacc-like parsers in C

   Copyright (C) 1984, 1989-1990, 2000-2015, 2018-2021 Free Software Foundation,
   Inc.

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <https://www.gnu.org/licenses/>.  */

/* As a special exception, you may create a larger work that contains
   part or all of the Bison parser skeleton and distribute that work
   under terms of your choice, so long as that work isn't itself a
   parser generator using the skeleton or a modified version thereof
   as a parser skeleton.  Alternatively, if you modify or redistribute
   the parser skeleton itself, you may (at your option) remove this
   special exception, which will cause the skeleton and the resulting
   Bison output files to be licensed under the GNU General Public
   License without this special exception.

   This special exception was added by the Free Software Foundation in
   version 2.2 of Bison.  */

/* DO NOT RELY ON FEATURES THAT ARE NOT DOCUMENTED in the manual,
   especially those whose name start with YY_ or yy_.  They are
   private implementation details that can be changed or removed.  */

#ifndef YY_YY_PARSER_TAB_H_INCLUDED
# define YY_YY_PARSER_TAB_H_INCLUDED
/* Debug traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif
#if YYDEBUG
extern int yydebug;
#endif
/* "%code requires" blocks.  */
#line 219 "parser.y"

    typedef enum {
        N_ASSIGN,
        N_MAKE,
        N_DIV_OPEN,
        N_DIV_CLOSE,
        N_IF,
        N_CALL,
        N_RETURN
    } node_kind;

    typedef struct Node {
        node_kind kind;

        /* assign / return */
        char *tag, *name, *value;

        /* make */
        char **items;
        int    nitems;

        /* if */
        char *cond_var;
        int   cond_op;
        int   cond_num;
        struct Node *then_node, *else_node;

        /* call */
        char *result_tag, *result_name;

        /* список */
        struct Node *next;
    } Node;

#line 84 "parser.tab.h"

/* Token kinds.  */
#ifndef YYTOKENTYPE
# define YYTOKENTYPE
  enum yytokentype
  {
    YYEMPTY = -2,
    YYEOF = 0,                     /* "end of file"  */
    YYerror = 256,                 /* error  */
    YYUNDEF = 257,                 /* "invalid token"  */
    TOK_IDENT = 258,               /* TOK_IDENT  */
    TOK_STRING = 259,              /* TOK_STRING  */
    TOK_NUMBER = 260,              /* TOK_NUMBER  */
    TOK_IF = 261,                  /* TOK_IF  */
    TOK_ELSE = 262,                /* TOK_ELSE  */
    TOK_GT = 263,                  /* TOK_GT  */
    TOK_LT = 264,                  /* TOK_LT  */
    TOK_GTE = 265,                 /* TOK_GTE  */
    TOK_LTE = 266,                 /* TOK_LTE  */
    TOK_EQ = 267,                  /* TOK_EQ  */
    TOK_NEQ = 268,                 /* TOK_NEQ  */
    TOK_P = 269,                   /* TOK_P  */
    TOK_H1 = 270,                  /* TOK_H1  */
    TOK_QUA = 271,                 /* TOK_QUA  */
    TOK_MAKE = 272,                /* TOK_MAKE  */
    TOK_ASSIGN = 273,              /* TOK_ASSIGN  */
    TOK_RULE = 274,                /* TOK_RULE  */
    TOK_DECL = 275,                /* TOK_DECL  */
    TOK_LBRACE = 276,              /* TOK_LBRACE  */
    TOK_RBRACE = 277               /* TOK_RBRACE  */
  };
  typedef enum yytokentype yytoken_kind_t;
#endif

/* Value type.  */
#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
union YYSTYPE
{
#line 256 "parser.y"

    char *str;
    int   num;

    struct { char *var; int op; int num; }         cond;
    Node *node;
    struct { char **types; char **names; int n; }  params;
    struct { char *name; char **items; int n; }    call;

#line 133 "parser.tab.h"

};
typedef union YYSTYPE YYSTYPE;
# define YYSTYPE_IS_TRIVIAL 1
# define YYSTYPE_IS_DECLARED 1
#endif


extern YYSTYPE yylval;


int yyparse (void);


#endif /* !YY_YY_PARSER_TAB_H_INCLUDED  */
