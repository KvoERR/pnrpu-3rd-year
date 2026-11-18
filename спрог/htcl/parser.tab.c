/* A Bison parser, made by GNU Bison 3.8.2.  */

/* Bison implementation for Yacc-like parsers in C

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

/* C LALR(1) parser skeleton written by Richard Stallman, by
   simplifying the original so-called "semantic" parser.  */

/* DO NOT RELY ON FEATURES THAT ARE NOT DOCUMENTED in the manual,
   especially those whose name start with YY_ or yy_.  They are
   private implementation details that can be changed or removed.  */

/* All symbols defined below should begin with yy or YY, to avoid
   infringing on user name space.  This should be done even for local
   variables, as they might otherwise be expanded by user macros.
   There are some unavoidable exceptions within include files to
   define necessary library symbols; they are noted "INFRINGES ON
   USER NAME SPACE" below.  */

/* Identify Bison output, and Bison version.  */
#define YYBISON 30802

/* Bison version string.  */
#define YYBISON_VERSION "3.8.2"

/* Skeleton name.  */
#define YYSKELETON_NAME "yacc.c"

/* Pure parsers.  */
#define YYPURE 0

/* Push parsers.  */
#define YYPUSH 0

/* Pull parsers.  */
#define YYPULL 1




/* First part of user prologue.  */
#line 1 "parser.y"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include "parser.tab.h"

int yylex(void);
void yyerror(const char *s);

#define MAX_VARS  100
#define MAX_FUNCS 64
#define OUT_SIZE  100000

/* ==================== Символьная таблица ==================== */

typedef struct {
    char *tag;
    char *name;
    char *value;
} sym_entry;

sym_entry sym_table[MAX_VARS];
int sym_count = 0;

char output[OUT_SIZE];
int out_pos = 0;

void emit(const char *fmt, ...) {
    va_list args;
    va_start(args, fmt);
    out_pos += vsnprintf(output + out_pos, OUT_SIZE - out_pos, fmt, args);
    va_end(args);
}

sym_entry *sym_find_by_name(const char *name) {
    for (int i = 0; i < sym_count; i++)
        if (strcmp(sym_table[i].name, name) == 0)
            return &sym_table[i];
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

/* ==================== Таблица функций ==================== */

typedef struct {
    char *name;
    char **ptypes;
    char **pnames;
    int    nparams;
    Node  *body;
} Func;

Func funcs[MAX_FUNCS];
int  func_count = 0;

Func *func_find(const char *name) {
    for (int i = 0; i < func_count; i++)
        if (strcmp(funcs[i].name, name) == 0)
            return &funcs[i];
    return NULL;
}

/* ==================== Выполнение ==================== */

int eval_cond(const char *var, int op, int num) {
    int v = get_num_value(var);
    switch (op) {
      case 0: return v >  num;
      case 1: return v <  num;
      case 2: return v >= num;
      case 3: return v <= num;
      case 4: return v == num;
      case 5: return v != num;
    }
    return 0;
}

static int   g_returned  = 0;
static char *g_ret_value = NULL;

typedef struct {
    char *name;
    char *old_tag;
    char *old_value;
    int   existed;
} SaveSlot;

void exec_node(Node *n);
void exec_nodes(Node *n) {
    for (; n && !g_returned; n = n->next)
        exec_node(n);
}

void exec_node(Node *n) {
    if (!n) return;

    switch (n->kind) {

    case N_ASSIGN:
        sym_set(n->tag, n->name, n->value);
        break;

    case N_MAKE:
        for (int i = 0; i < n->nitems; i++) {
            sym_entry *e = sym_find_by_name(n->items[i]);
            if (e) emit("<%s>%s</%s>\n", e->tag, e->value, e->tag);
            else   fprintf(stderr, "Переменная не найдена: %s\n", n->items[i]);
        }
        break;

    case N_DIV_OPEN:  emit("<div>\n");  break;
    case N_DIV_CLOSE: emit("</div>\n"); break;

    case N_IF:
        if (eval_cond(n->cond_var, n->cond_op, n->cond_num))
            exec_node(n->then_node);
        else
            exec_node(n->else_node);
        break;

    case N_RETURN:
        free(g_ret_value);
        g_ret_value = strdup(n->value);
        g_returned  = 1;
        break;

    case N_CALL: {
        Func *f = func_find(n->name);
        if (!f) { fprintf(stderr, "Нет функции %s\n", n->name); break; }
        if (n->nitems != f->nparams) {
            fprintf(stderr, "Неверное число аргументов у %s (ожидалось %d, передано %d)\n",
                    n->name, f->nparams, n->nitems);
            break;
        }

        SaveSlot saves[32];
        for (int i = 0; i < f->nparams; i++) {
            sym_entry *e = sym_find_by_name(f->pnames[i]);
            saves[i].name      = f->pnames[i];
            saves[i].existed   = (e != NULL);
            saves[i].old_tag   = e ? strdup(e->tag)   : NULL;
            saves[i].old_value = e ? strdup(e->value) : NULL;
        }

        for (int i = 0; i < f->nparams; i++) {
            sym_entry *a = sym_find_by_name(n->items[i]);
            if (!a) {
                fprintf(stderr, "Аргумент не найден: %s\n", n->items[i]);
                sym_set(f->ptypes[i], f->pnames[i], "");
            } else {
                sym_set(f->ptypes[i], f->pnames[i], a->value);
            }
        }

        free(g_ret_value); g_ret_value = NULL;
        g_returned = 0;
        exec_nodes(f->body);
        g_returned = 0;

        if (n->result_name && g_ret_value)
            sym_set(n->result_tag, n->result_name, g_ret_value);

        for (int i = 0; i < f->nparams; i++) {
            if (saves[i].existed)
                sym_set(saves[i].old_tag, saves[i].name, saves[i].old_value);
            else
                sym_set("span", saves[i].name, "");
            free(saves[i].old_tag);
            free(saves[i].old_value);
        }
        break;
    }
    }
}

/* ==================== Сборка программы ==================== */

Node *program = NULL;
Node *program_tail = NULL;

void program_append(Node *n) {
    if (!n) return;
    if (!program) { program = n; program_tail = n; }
    else          { program_tail->next = n; program_tail = n; }
    while (program_tail->next) program_tail = program_tail->next;
}

#line 287 "parser.tab.c"

# ifndef YY_CAST
#  ifdef __cplusplus
#   define YY_CAST(Type, Val) static_cast<Type> (Val)
#   define YY_REINTERPRET_CAST(Type, Val) reinterpret_cast<Type> (Val)
#  else
#   define YY_CAST(Type, Val) ((Type) (Val))
#   define YY_REINTERPRET_CAST(Type, Val) ((Type) (Val))
#  endif
# endif
# ifndef YY_NULLPTR
#  if defined __cplusplus
#   if 201103L <= __cplusplus
#    define YY_NULLPTR nullptr
#   else
#    define YY_NULLPTR 0
#   endif
#  else
#   define YY_NULLPTR ((void*)0)
#  endif
# endif

#include "parser.tab.h"
/* Symbol kind.  */
enum yysymbol_kind_t
{
  YYSYMBOL_YYEMPTY = -2,
  YYSYMBOL_YYEOF = 0,                      /* "end of file"  */
  YYSYMBOL_YYerror = 1,                    /* error  */
  YYSYMBOL_YYUNDEF = 2,                    /* "invalid token"  */
  YYSYMBOL_TOK_IDENT = 3,                  /* TOK_IDENT  */
  YYSYMBOL_TOK_STRING = 4,                 /* TOK_STRING  */
  YYSYMBOL_TOK_NUMBER = 5,                 /* TOK_NUMBER  */
  YYSYMBOL_TOK_IF = 6,                     /* TOK_IF  */
  YYSYMBOL_TOK_ELSE = 7,                   /* TOK_ELSE  */
  YYSYMBOL_TOK_GT = 8,                     /* TOK_GT  */
  YYSYMBOL_TOK_LT = 9,                     /* TOK_LT  */
  YYSYMBOL_TOK_GTE = 10,                   /* TOK_GTE  */
  YYSYMBOL_TOK_LTE = 11,                   /* TOK_LTE  */
  YYSYMBOL_TOK_EQ = 12,                    /* TOK_EQ  */
  YYSYMBOL_TOK_NEQ = 13,                   /* TOK_NEQ  */
  YYSYMBOL_TOK_P = 14,                     /* TOK_P  */
  YYSYMBOL_TOK_H1 = 15,                    /* TOK_H1  */
  YYSYMBOL_TOK_QUA = 16,                   /* TOK_QUA  */
  YYSYMBOL_TOK_MAKE = 17,                  /* TOK_MAKE  */
  YYSYMBOL_TOK_ASSIGN = 18,                /* TOK_ASSIGN  */
  YYSYMBOL_TOK_RULE = 19,                  /* TOK_RULE  */
  YYSYMBOL_TOK_DECL = 20,                  /* TOK_DECL  */
  YYSYMBOL_TOK_LBRACE = 21,                /* TOK_LBRACE  */
  YYSYMBOL_TOK_RBRACE = 22,                /* TOK_RBRACE  */
  YYSYMBOL_23_n_ = 23,                     /* '\n'  */
  YYSYMBOL_24_ = 24,                       /* '('  */
  YYSYMBOL_25_ = 25,                       /* ')'  */
  YYSYMBOL_26_ = 26,                       /* '['  */
  YYSYMBOL_27_ = 27,                       /* ']'  */
  YYSYMBOL_28_ = 28,                       /* ','  */
  YYSYMBOL_29_ = 29,                       /* '{'  */
  YYSYMBOL_30_ = 30,                       /* '}'  */
  YYSYMBOL_YYACCEPT = 31,                  /* $accept  */
  YYSYMBOL_input = 32,                     /* input  */
  YYSYMBOL_line = 33,                      /* line  */
  YYSYMBOL_stmt = 34,                      /* stmt  */
  YYSYMBOL_stmt_node = 35,                 /* stmt_node  */
  YYSYMBOL_stmt_list = 36,                 /* stmt_list  */
  YYSYMBOL_condition = 37,                 /* condition  */
  YYSYMBOL_param_list = 38,                /* param_list  */
  YYSYMBOL_param_item = 39,                /* param_item  */
  YYSYMBOL_type_name = 40,                 /* type_name  */
  YYSYMBOL_call_args = 41,                 /* call_args  */
  YYSYMBOL_name_list = 42,                 /* name_list  */
  YYSYMBOL_name_item = 43                  /* name_item  */
};
typedef enum yysymbol_kind_t yysymbol_kind_t;




#ifdef short
# undef short
#endif

/* On compilers that do not define __PTRDIFF_MAX__ etc., make sure
   <limits.h> and (if available) <stdint.h> are included
   so that the code can choose integer types of a good width.  */

#ifndef __PTRDIFF_MAX__
# include <limits.h> /* INFRINGES ON USER NAME SPACE */
# if defined __STDC_VERSION__ && 199901 <= __STDC_VERSION__
#  include <stdint.h> /* INFRINGES ON USER NAME SPACE */
#  define YY_STDINT_H
# endif
#endif

/* Narrow types that promote to a signed type and that can represent a
   signed or unsigned integer of at least N bits.  In tables they can
   save space and decrease cache pressure.  Promoting to a signed type
   helps avoid bugs in integer arithmetic.  */

#ifdef __INT_LEAST8_MAX__
typedef __INT_LEAST8_TYPE__ yytype_int8;
#elif defined YY_STDINT_H
typedef int_least8_t yytype_int8;
#else
typedef signed char yytype_int8;
#endif

#ifdef __INT_LEAST16_MAX__
typedef __INT_LEAST16_TYPE__ yytype_int16;
#elif defined YY_STDINT_H
typedef int_least16_t yytype_int16;
#else
typedef short yytype_int16;
#endif

/* Work around bug in HP-UX 11.23, which defines these macros
   incorrectly for preprocessor constants.  This workaround can likely
   be removed in 2023, as HPE has promised support for HP-UX 11.23
   (aka HP-UX 11i v2) only through the end of 2022; see Table 2 of
   <https://h20195.www2.hpe.com/V2/getpdf.aspx/4AA4-7673ENW.pdf>.  */
#ifdef __hpux
# undef UINT_LEAST8_MAX
# undef UINT_LEAST16_MAX
# define UINT_LEAST8_MAX 255
# define UINT_LEAST16_MAX 65535
#endif

#if defined __UINT_LEAST8_MAX__ && __UINT_LEAST8_MAX__ <= __INT_MAX__
typedef __UINT_LEAST8_TYPE__ yytype_uint8;
#elif (!defined __UINT_LEAST8_MAX__ && defined YY_STDINT_H \
       && UINT_LEAST8_MAX <= INT_MAX)
typedef uint_least8_t yytype_uint8;
#elif !defined __UINT_LEAST8_MAX__ && UCHAR_MAX <= INT_MAX
typedef unsigned char yytype_uint8;
#else
typedef short yytype_uint8;
#endif

#if defined __UINT_LEAST16_MAX__ && __UINT_LEAST16_MAX__ <= __INT_MAX__
typedef __UINT_LEAST16_TYPE__ yytype_uint16;
#elif (!defined __UINT_LEAST16_MAX__ && defined YY_STDINT_H \
       && UINT_LEAST16_MAX <= INT_MAX)
typedef uint_least16_t yytype_uint16;
#elif !defined __UINT_LEAST16_MAX__ && USHRT_MAX <= INT_MAX
typedef unsigned short yytype_uint16;
#else
typedef int yytype_uint16;
#endif

#ifndef YYPTRDIFF_T
# if defined __PTRDIFF_TYPE__ && defined __PTRDIFF_MAX__
#  define YYPTRDIFF_T __PTRDIFF_TYPE__
#  define YYPTRDIFF_MAXIMUM __PTRDIFF_MAX__
# elif defined PTRDIFF_MAX
#  ifndef ptrdiff_t
#   include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  endif
#  define YYPTRDIFF_T ptrdiff_t
#  define YYPTRDIFF_MAXIMUM PTRDIFF_MAX
# else
#  define YYPTRDIFF_T long
#  define YYPTRDIFF_MAXIMUM LONG_MAX
# endif
#endif

#ifndef YYSIZE_T
# ifdef __SIZE_TYPE__
#  define YYSIZE_T __SIZE_TYPE__
# elif defined size_t
#  define YYSIZE_T size_t
# elif defined __STDC_VERSION__ && 199901 <= __STDC_VERSION__
#  include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  define YYSIZE_T size_t
# else
#  define YYSIZE_T unsigned
# endif
#endif

#define YYSIZE_MAXIMUM                                  \
  YY_CAST (YYPTRDIFF_T,                                 \
           (YYPTRDIFF_MAXIMUM < YY_CAST (YYSIZE_T, -1)  \
            ? YYPTRDIFF_MAXIMUM                         \
            : YY_CAST (YYSIZE_T, -1)))

#define YYSIZEOF(X) YY_CAST (YYPTRDIFF_T, sizeof (X))


/* Stored state numbers (used for stacks). */
typedef yytype_int8 yy_state_t;

/* State numbers in computations.  */
typedef int yy_state_fast_t;

#ifndef YY_
# if defined YYENABLE_NLS && YYENABLE_NLS
#  if ENABLE_NLS
#   include <libintl.h> /* INFRINGES ON USER NAME SPACE */
#   define YY_(Msgid) dgettext ("bison-runtime", Msgid)
#  endif
# endif
# ifndef YY_
#  define YY_(Msgid) Msgid
# endif
#endif


#ifndef YY_ATTRIBUTE_PURE
# if defined __GNUC__ && 2 < __GNUC__ + (96 <= __GNUC_MINOR__)
#  define YY_ATTRIBUTE_PURE __attribute__ ((__pure__))
# else
#  define YY_ATTRIBUTE_PURE
# endif
#endif

#ifndef YY_ATTRIBUTE_UNUSED
# if defined __GNUC__ && 2 < __GNUC__ + (7 <= __GNUC_MINOR__)
#  define YY_ATTRIBUTE_UNUSED __attribute__ ((__unused__))
# else
#  define YY_ATTRIBUTE_UNUSED
# endif
#endif

/* Suppress unused-variable warnings by "using" E.  */
#if ! defined lint || defined __GNUC__
# define YY_USE(E) ((void) (E))
#else
# define YY_USE(E) /* empty */
#endif

/* Suppress an incorrect diagnostic about yylval being uninitialized.  */
#if defined __GNUC__ && ! defined __ICC && 406 <= __GNUC__ * 100 + __GNUC_MINOR__
# if __GNUC__ * 100 + __GNUC_MINOR__ < 407
#  define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN                           \
    _Pragma ("GCC diagnostic push")                                     \
    _Pragma ("GCC diagnostic ignored \"-Wuninitialized\"")
# else
#  define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN                           \
    _Pragma ("GCC diagnostic push")                                     \
    _Pragma ("GCC diagnostic ignored \"-Wuninitialized\"")              \
    _Pragma ("GCC diagnostic ignored \"-Wmaybe-uninitialized\"")
# endif
# define YY_IGNORE_MAYBE_UNINITIALIZED_END      \
    _Pragma ("GCC diagnostic pop")
#else
# define YY_INITIAL_VALUE(Value) Value
#endif
#ifndef YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_END
#endif
#ifndef YY_INITIAL_VALUE
# define YY_INITIAL_VALUE(Value) /* Nothing. */
#endif

#if defined __cplusplus && defined __GNUC__ && ! defined __ICC && 6 <= __GNUC__
# define YY_IGNORE_USELESS_CAST_BEGIN                          \
    _Pragma ("GCC diagnostic push")                            \
    _Pragma ("GCC diagnostic ignored \"-Wuseless-cast\"")
# define YY_IGNORE_USELESS_CAST_END            \
    _Pragma ("GCC diagnostic pop")
#endif
#ifndef YY_IGNORE_USELESS_CAST_BEGIN
# define YY_IGNORE_USELESS_CAST_BEGIN
# define YY_IGNORE_USELESS_CAST_END
#endif


#define YY_ASSERT(E) ((void) (0 && (E)))

#if !defined yyoverflow

/* The parser invokes alloca or malloc; define the necessary symbols.  */

# ifdef YYSTACK_USE_ALLOCA
#  if YYSTACK_USE_ALLOCA
#   ifdef __GNUC__
#    define YYSTACK_ALLOC __builtin_alloca
#   elif defined __BUILTIN_VA_ARG_INCR
#    include <alloca.h> /* INFRINGES ON USER NAME SPACE */
#   elif defined _AIX
#    define YYSTACK_ALLOC __alloca
#   elif defined _MSC_VER
#    include <malloc.h> /* INFRINGES ON USER NAME SPACE */
#    define alloca _alloca
#   else
#    define YYSTACK_ALLOC alloca
#    if ! defined _ALLOCA_H && ! defined EXIT_SUCCESS
#     include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
      /* Use EXIT_SUCCESS as a witness for stdlib.h.  */
#     ifndef EXIT_SUCCESS
#      define EXIT_SUCCESS 0
#     endif
#    endif
#   endif
#  endif
# endif

# ifdef YYSTACK_ALLOC
   /* Pacify GCC's 'empty if-body' warning.  */
#  define YYSTACK_FREE(Ptr) do { /* empty */; } while (0)
#  ifndef YYSTACK_ALLOC_MAXIMUM
    /* The OS might guarantee only one guard page at the bottom of the stack,
       and a page size can be as small as 4096 bytes.  So we cannot safely
       invoke alloca (N) if N exceeds 4096.  Use a slightly smaller number
       to allow for a few compiler-allocated temporary stack slots.  */
#   define YYSTACK_ALLOC_MAXIMUM 4032 /* reasonable circa 2006 */
#  endif
# else
#  define YYSTACK_ALLOC YYMALLOC
#  define YYSTACK_FREE YYFREE
#  ifndef YYSTACK_ALLOC_MAXIMUM
#   define YYSTACK_ALLOC_MAXIMUM YYSIZE_MAXIMUM
#  endif
#  if (defined __cplusplus && ! defined EXIT_SUCCESS \
       && ! ((defined YYMALLOC || defined malloc) \
             && (defined YYFREE || defined free)))
#   include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#   ifndef EXIT_SUCCESS
#    define EXIT_SUCCESS 0
#   endif
#  endif
#  ifndef YYMALLOC
#   define YYMALLOC malloc
#   if ! defined malloc && ! defined EXIT_SUCCESS
void *malloc (YYSIZE_T); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
#  ifndef YYFREE
#   define YYFREE free
#   if ! defined free && ! defined EXIT_SUCCESS
void free (void *); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
# endif
#endif /* !defined yyoverflow */

#if (! defined yyoverflow \
     && (! defined __cplusplus \
         || (defined YYSTYPE_IS_TRIVIAL && YYSTYPE_IS_TRIVIAL)))

/* A type that is properly aligned for any stack member.  */
union yyalloc
{
  yy_state_t yyss_alloc;
  YYSTYPE yyvs_alloc;
};

/* The size of the maximum gap between one aligned stack and the next.  */
# define YYSTACK_GAP_MAXIMUM (YYSIZEOF (union yyalloc) - 1)

/* The size of an array large to enough to hold all stacks, each with
   N elements.  */
# define YYSTACK_BYTES(N) \
     ((N) * (YYSIZEOF (yy_state_t) + YYSIZEOF (YYSTYPE)) \
      + YYSTACK_GAP_MAXIMUM)

# define YYCOPY_NEEDED 1

/* Relocate STACK from its old location to the new one.  The
   local variables YYSIZE and YYSTACKSIZE give the old and new number of
   elements in the stack, and YYPTR gives the new location of the
   stack.  Advance YYPTR to a properly aligned location for the next
   stack.  */
# define YYSTACK_RELOCATE(Stack_alloc, Stack)                           \
    do                                                                  \
      {                                                                 \
        YYPTRDIFF_T yynewbytes;                                         \
        YYCOPY (&yyptr->Stack_alloc, Stack, yysize);                    \
        Stack = &yyptr->Stack_alloc;                                    \
        yynewbytes = yystacksize * YYSIZEOF (*Stack) + YYSTACK_GAP_MAXIMUM; \
        yyptr += yynewbytes / YYSIZEOF (*yyptr);                        \
      }                                                                 \
    while (0)

#endif

#if defined YYCOPY_NEEDED && YYCOPY_NEEDED
/* Copy COUNT objects from SRC to DST.  The source and destination do
   not overlap.  */
# ifndef YYCOPY
#  if defined __GNUC__ && 1 < __GNUC__
#   define YYCOPY(Dst, Src, Count) \
      __builtin_memcpy (Dst, Src, YY_CAST (YYSIZE_T, (Count)) * sizeof (*(Src)))
#  else
#   define YYCOPY(Dst, Src, Count)              \
      do                                        \
        {                                       \
          YYPTRDIFF_T yyi;                      \
          for (yyi = 0; yyi < (Count); yyi++)   \
            (Dst)[yyi] = (Src)[yyi];            \
        }                                       \
      while (0)
#  endif
# endif
#endif /* !YYCOPY_NEEDED */

/* YYFINAL -- State number of the termination state.  */
#define YYFINAL  2
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   119

/* YYNTOKENS -- Number of terminals.  */
#define YYNTOKENS  31
/* YYNNTS -- Number of nonterminals.  */
#define YYNNTS  13
/* YYNRULES -- Number of rules.  */
#define YYNRULES  42
/* YYNSTATES -- Number of states.  */
#define YYNSTATES  94

/* YYMAXUTOK -- Last valid token kind.  */
#define YYMAXUTOK   277


/* YYTRANSLATE(TOKEN-NUM) -- Symbol number corresponding to TOKEN-NUM
   as returned by yylex, with out-of-bounds checking.  */
#define YYTRANSLATE(YYX)                                \
  (0 <= (YYX) && (YYX) <= YYMAXUTOK                     \
   ? YY_CAST (yysymbol_kind_t, yytranslate[YYX])        \
   : YYSYMBOL_YYUNDEF)

/* YYTRANSLATE[TOKEN-NUM] -- Symbol number corresponding to TOKEN-NUM
   as returned by yylex.  */
static const yytype_int8 yytranslate[] =
{
       0,     2,     2,     2,     2,     2,     2,     2,     2,     2,
      23,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
      24,    25,     2,     2,    28,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,    26,     2,    27,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,    29,     2,    30,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     1,     2,     3,     4,
       5,     6,     7,     8,     9,    10,    11,    12,    13,    14,
      15,    16,    17,    18,    19,    20,    21,    22
};

#if YYDEBUG
/* YYRLINE[YYN] -- Source line where rule number YYN was defined.  */
static const yytype_int16 yyrline[] =
{
       0,   285,   285,   287,   291,   292,   299,   305,   311,   318,
     326,   333,   340,   349,   358,   361,   369,   377,   387,   395,
     398,   399,   409,   410,   411,   412,   413,   414,   420,   421,
     422,   434,   445,   446,   447,   453,   454,   455,   466,   467,
     476,   484,   499
};
#endif

/** Accessing symbol of state STATE.  */
#define YY_ACCESSING_SYMBOL(State) YY_CAST (yysymbol_kind_t, yystos[State])

#if YYDEBUG || 0
/* The user-facing name of the symbol whose (internal) number is
   YYSYMBOL.  No bounds checking.  */
static const char *yysymbol_name (yysymbol_kind_t yysymbol) YY_ATTRIBUTE_UNUSED;

/* YYTNAME[SYMBOL-NUM] -- String name of the symbol SYMBOL-NUM.
   First, the terminals, then, starting at YYNTOKENS, nonterminals.  */
static const char *const yytname[] =
{
  "\"end of file\"", "error", "\"invalid token\"", "TOK_IDENT",
  "TOK_STRING", "TOK_NUMBER", "TOK_IF", "TOK_ELSE", "TOK_GT", "TOK_LT",
  "TOK_GTE", "TOK_LTE", "TOK_EQ", "TOK_NEQ", "TOK_P", "TOK_H1", "TOK_QUA",
  "TOK_MAKE", "TOK_ASSIGN", "TOK_RULE", "TOK_DECL", "TOK_LBRACE",
  "TOK_RBRACE", "'\\n'", "'('", "')'", "'['", "']'", "','", "'{'", "'}'",
  "$accept", "input", "line", "stmt", "stmt_node", "stmt_list",
  "condition", "param_list", "param_item", "type_name", "call_args",
  "name_list", "name_item", YY_NULLPTR
};

static const char *
yysymbol_name (yysymbol_kind_t yysymbol)
{
  return yytname[yysymbol];
}
#endif

#define YYPACT_NINF (-40)

#define yypact_value_is_default(Yyn) \
  ((Yyn) == YYPACT_NINF)

#define YYTABLE_NINF (-1)

#define yytable_value_is_error(Yyn) \
  0

/* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
   STATE-NUM.  */
static const yytype_int8 yypact[] =
{
     -40,    35,   -40,   -13,    17,    54,    65,    70,    22,    71,
      75,   -40,   -40,    56,    76,    78,    34,    55,    64,    66,
      67,     0,    62,    69,   -40,   -40,   -40,    -9,    83,    84,
      86,    87,    88,    89,    50,    59,    68,     4,   -40,     3,
     -40,     5,   -40,    45,    91,   -40,    93,   -40,   -40,   -40,
     -40,   -40,   -40,   -40,    72,    73,   -40,    74,   -40,    77,
     -40,   -40,     7,   -40,   -40,   -40,   -40,   -40,    -4,   -40,
      96,   -40,   -40,    95,    78,    78,    78,   -40,    79,    45,
     -40,    50,    -3,    11,    12,   -40,   -40,   -40,   -40,   -40,
     -40,    -2,   -40,   -40
};

/* YYDEFACT[STATE-NUM] -- Default reduction number in state STATE-NUM.
   Performed when YYTABLE does not specify something else to do.  Zero
   means the default is an error.  */
static const yytype_int8 yydefact[] =
{
       2,     0,     1,     0,     0,     0,     0,     0,     0,     0,
       0,     4,     3,     0,     0,    35,     0,     0,     0,     0,
       0,     0,     0,     0,     5,     9,    36,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,    40,     0,
      15,     0,    38,    28,     0,    13,     0,    22,    23,    24,
      25,    26,    27,    19,     0,     0,     7,     0,     6,     0,
       8,    42,     0,    14,    39,    33,    32,    34,     0,    29,
       0,    18,    37,     0,    35,    35,    35,    41,     0,     0,
      31,     0,     0,     0,     0,    20,    30,    16,    11,    10,
      12,     0,    17,    21
};

/* YYPGOTO[NTERM-NUM].  */
static const yytype_int8 yypgoto[] =
{
     -40,   -40,   -40,    -1,    23,   -40,   -40,   -40,    24,   -40,
       1,    80,   -39
};

/* YYDEFGOTO[NTERM-NUM].  */
static const yytype_int8 yydefgoto[] =
{
       0,     1,    12,    53,    54,    91,    17,    68,    69,    70,
      27,    41,    42
};

/* YYTABLE[YYPACT[STATE-NUM]] -- What to do in state STATE-NUM.  If
   positive, shift that token.  If negative, reduce the rule whose
   number is the opposite.  If YYTABLE_NINF, syntax error.  */
static const yytype_int8 yytable[] =
{
      13,     3,    64,    38,     4,    14,    38,    59,    38,    60,
      38,    15,     5,     6,     7,     8,    45,     9,    10,    46,
      16,    78,    88,    64,    79,    46,    39,    40,    92,    39,
      61,    39,    63,    39,    77,     2,    89,    90,     3,    46,
      46,     4,    28,    29,    30,    31,    32,    33,    21,     5,
       6,     7,     8,     3,     9,    10,     4,    18,    11,    65,
      66,    67,    55,    56,     5,     6,     7,     8,    19,     9,
      10,    57,    58,    20,    22,    82,    83,    84,    23,    24,
      25,    26,    35,    34,    36,    37,    43,    44,    47,    48,
      93,    49,    50,    51,    52,    71,    72,    74,    75,    80,
      73,    76,    81,    86,    87,     0,     0,     0,    85,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,    62
};

static const yytype_int8 yycheck[] =
{
       1,     3,    41,     3,     6,    18,     3,     3,     3,     5,
       3,    24,    14,    15,    16,    17,    25,    19,    20,    28,
       3,    25,    25,    62,    28,    28,    26,    27,    30,    26,
      27,    26,    27,    26,    27,     0,    25,    25,     3,    28,
      28,     6,     8,     9,    10,    11,    12,    13,    26,    14,
      15,    16,    17,     3,    19,    20,     6,     3,    23,    14,
      15,    16,     3,     4,    14,    15,    16,    17,     3,    19,
      20,     3,     4,     3,     3,    74,    75,    76,     3,    23,
       4,     3,    18,    28,    18,    18,    24,    18,     5,     5,
      91,     5,     5,     5,     5,     4,     3,    24,    24,     3,
      28,    24,     7,    79,    81,    -1,    -1,    -1,    29,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    39
};

/* YYSTOS[STATE-NUM] -- The symbol kind of the accessing symbol of
   state STATE-NUM.  */
static const yytype_int8 yystos[] =
{
       0,    32,     0,     3,     6,    14,    15,    16,    17,    19,
      20,    23,    33,    34,    18,    24,     3,    37,     3,     3,
       3,    26,     3,     3,    23,     4,     3,    41,     8,     9,
      10,    11,    12,    13,    28,    18,    18,    18,     3,    26,
      27,    42,    43,    24,    18,    25,    28,     5,     5,     5,
       5,     5,     5,    34,    35,     3,     4,     3,     4,     3,
       5,    27,    42,    27,    43,    14,    15,    16,    38,    39,
      40,     4,     3,    28,    24,    24,    24,    27,    25,    28,
       3,     7,    41,    41,    41,    29,    39,    35,    25,    25,
      25,    36,    30,    34
};

/* YYR1[RULE-NUM] -- Symbol kind of the left-hand side of rule RULE-NUM.  */
static const yytype_int8 yyr1[] =
{
       0,    31,    32,    32,    33,    33,    34,    34,    34,    34,
      34,    34,    34,    34,    34,    34,    34,    34,    34,    35,
      36,    36,    37,    37,    37,    37,    37,    37,    38,    38,
      38,    39,    40,    40,    40,    41,    41,    41,    42,    42,
      43,    43,    43
};

/* YYR2[RULE-NUM] -- Number of symbols on the right-hand side of rule RULE-NUM.  */
static const yytype_int8 yyr2[] =
{
       0,     2,     0,     2,     1,     2,     4,     4,     4,     3,
       7,     7,     7,     4,     4,     3,     7,     8,     4,     1,
       0,     2,     3,     3,     3,     3,     3,     3,     0,     1,
       3,     2,     1,     1,     1,     0,     1,     3,     1,     2,
       1,     3,     2
};


enum { YYENOMEM = -2 };

#define yyerrok         (yyerrstatus = 0)
#define yyclearin       (yychar = YYEMPTY)

#define YYACCEPT        goto yyacceptlab
#define YYABORT         goto yyabortlab
#define YYERROR         goto yyerrorlab
#define YYNOMEM         goto yyexhaustedlab


#define YYRECOVERING()  (!!yyerrstatus)

#define YYBACKUP(Token, Value)                                    \
  do                                                              \
    if (yychar == YYEMPTY)                                        \
      {                                                           \
        yychar = (Token);                                         \
        yylval = (Value);                                         \
        YYPOPSTACK (yylen);                                       \
        yystate = *yyssp;                                         \
        goto yybackup;                                            \
      }                                                           \
    else                                                          \
      {                                                           \
        yyerror (YY_("syntax error: cannot back up")); \
        YYERROR;                                                  \
      }                                                           \
  while (0)

/* Backward compatibility with an undocumented macro.
   Use YYerror or YYUNDEF. */
#define YYERRCODE YYUNDEF


/* Enable debugging if requested.  */
#if YYDEBUG

# ifndef YYFPRINTF
#  include <stdio.h> /* INFRINGES ON USER NAME SPACE */
#  define YYFPRINTF fprintf
# endif

# define YYDPRINTF(Args)                        \
do {                                            \
  if (yydebug)                                  \
    YYFPRINTF Args;                             \
} while (0)




# define YY_SYMBOL_PRINT(Title, Kind, Value, Location)                    \
do {                                                                      \
  if (yydebug)                                                            \
    {                                                                     \
      YYFPRINTF (stderr, "%s ", Title);                                   \
      yy_symbol_print (stderr,                                            \
                  Kind, Value); \
      YYFPRINTF (stderr, "\n");                                           \
    }                                                                     \
} while (0)


/*-----------------------------------.
| Print this symbol's value on YYO.  |
`-----------------------------------*/

static void
yy_symbol_value_print (FILE *yyo,
                       yysymbol_kind_t yykind, YYSTYPE const * const yyvaluep)
{
  FILE *yyoutput = yyo;
  YY_USE (yyoutput);
  if (!yyvaluep)
    return;
  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  YY_USE (yykind);
  YY_IGNORE_MAYBE_UNINITIALIZED_END
}


/*---------------------------.
| Print this symbol on YYO.  |
`---------------------------*/

static void
yy_symbol_print (FILE *yyo,
                 yysymbol_kind_t yykind, YYSTYPE const * const yyvaluep)
{
  YYFPRINTF (yyo, "%s %s (",
             yykind < YYNTOKENS ? "token" : "nterm", yysymbol_name (yykind));

  yy_symbol_value_print (yyo, yykind, yyvaluep);
  YYFPRINTF (yyo, ")");
}

/*------------------------------------------------------------------.
| yy_stack_print -- Print the state stack from its BOTTOM up to its |
| TOP (included).                                                   |
`------------------------------------------------------------------*/

static void
yy_stack_print (yy_state_t *yybottom, yy_state_t *yytop)
{
  YYFPRINTF (stderr, "Stack now");
  for (; yybottom <= yytop; yybottom++)
    {
      int yybot = *yybottom;
      YYFPRINTF (stderr, " %d", yybot);
    }
  YYFPRINTF (stderr, "\n");
}

# define YY_STACK_PRINT(Bottom, Top)                            \
do {                                                            \
  if (yydebug)                                                  \
    yy_stack_print ((Bottom), (Top));                           \
} while (0)


/*------------------------------------------------.
| Report that the YYRULE is going to be reduced.  |
`------------------------------------------------*/

static void
yy_reduce_print (yy_state_t *yyssp, YYSTYPE *yyvsp,
                 int yyrule)
{
  int yylno = yyrline[yyrule];
  int yynrhs = yyr2[yyrule];
  int yyi;
  YYFPRINTF (stderr, "Reducing stack by rule %d (line %d):\n",
             yyrule - 1, yylno);
  /* The symbols being reduced.  */
  for (yyi = 0; yyi < yynrhs; yyi++)
    {
      YYFPRINTF (stderr, "   $%d = ", yyi + 1);
      yy_symbol_print (stderr,
                       YY_ACCESSING_SYMBOL (+yyssp[yyi + 1 - yynrhs]),
                       &yyvsp[(yyi + 1) - (yynrhs)]);
      YYFPRINTF (stderr, "\n");
    }
}

# define YY_REDUCE_PRINT(Rule)          \
do {                                    \
  if (yydebug)                          \
    yy_reduce_print (yyssp, yyvsp, Rule); \
} while (0)

/* Nonzero means print parse trace.  It is left uninitialized so that
   multiple parsers can coexist.  */
int yydebug;
#else /* !YYDEBUG */
# define YYDPRINTF(Args) ((void) 0)
# define YY_SYMBOL_PRINT(Title, Kind, Value, Location)
# define YY_STACK_PRINT(Bottom, Top)
# define YY_REDUCE_PRINT(Rule)
#endif /* !YYDEBUG */


/* YYINITDEPTH -- initial size of the parser's stacks.  */
#ifndef YYINITDEPTH
# define YYINITDEPTH 200
#endif

/* YYMAXDEPTH -- maximum size the stacks can grow to (effective only
   if the built-in stack extension method is used).

   Do not make this value too large; the results are undefined if
   YYSTACK_ALLOC_MAXIMUM < YYSTACK_BYTES (YYMAXDEPTH)
   evaluated with infinite-precision integer arithmetic.  */

#ifndef YYMAXDEPTH
# define YYMAXDEPTH 10000
#endif






/*-----------------------------------------------.
| Release the memory associated to this symbol.  |
`-----------------------------------------------*/

static void
yydestruct (const char *yymsg,
            yysymbol_kind_t yykind, YYSTYPE *yyvaluep)
{
  YY_USE (yyvaluep);
  if (!yymsg)
    yymsg = "Deleting";
  YY_SYMBOL_PRINT (yymsg, yykind, yyvaluep, yylocationp);

  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  YY_USE (yykind);
  YY_IGNORE_MAYBE_UNINITIALIZED_END
}


/* Lookahead token kind.  */
int yychar;

/* The semantic value of the lookahead symbol.  */
YYSTYPE yylval;
/* Number of syntax errors so far.  */
int yynerrs;




/*----------.
| yyparse.  |
`----------*/

int
yyparse (void)
{
    yy_state_fast_t yystate = 0;
    /* Number of tokens to shift before error messages enabled.  */
    int yyerrstatus = 0;

    /* Refer to the stacks through separate pointers, to allow yyoverflow
       to reallocate them elsewhere.  */

    /* Their size.  */
    YYPTRDIFF_T yystacksize = YYINITDEPTH;

    /* The state stack: array, bottom, top.  */
    yy_state_t yyssa[YYINITDEPTH];
    yy_state_t *yyss = yyssa;
    yy_state_t *yyssp = yyss;

    /* The semantic value stack: array, bottom, top.  */
    YYSTYPE yyvsa[YYINITDEPTH];
    YYSTYPE *yyvs = yyvsa;
    YYSTYPE *yyvsp = yyvs;

  int yyn;
  /* The return value of yyparse.  */
  int yyresult;
  /* Lookahead symbol kind.  */
  yysymbol_kind_t yytoken = YYSYMBOL_YYEMPTY;
  /* The variables used to return semantic value and location from the
     action routines.  */
  YYSTYPE yyval;



#define YYPOPSTACK(N)   (yyvsp -= (N), yyssp -= (N))

  /* The number of symbols on the RHS of the reduced rule.
     Keep to zero when no symbol should be popped.  */
  int yylen = 0;

  YYDPRINTF ((stderr, "Starting parse\n"));

  yychar = YYEMPTY; /* Cause a token to be read.  */

  goto yysetstate;


/*------------------------------------------------------------.
| yynewstate -- push a new state, which is found in yystate.  |
`------------------------------------------------------------*/
yynewstate:
  /* In all cases, when you get here, the value and location stacks
     have just been pushed.  So pushing a state here evens the stacks.  */
  yyssp++;


/*--------------------------------------------------------------------.
| yysetstate -- set current state (the top of the stack) to yystate.  |
`--------------------------------------------------------------------*/
yysetstate:
  YYDPRINTF ((stderr, "Entering state %d\n", yystate));
  YY_ASSERT (0 <= yystate && yystate < YYNSTATES);
  YY_IGNORE_USELESS_CAST_BEGIN
  *yyssp = YY_CAST (yy_state_t, yystate);
  YY_IGNORE_USELESS_CAST_END
  YY_STACK_PRINT (yyss, yyssp);

  if (yyss + yystacksize - 1 <= yyssp)
#if !defined yyoverflow && !defined YYSTACK_RELOCATE
    YYNOMEM;
#else
    {
      /* Get the current used size of the three stacks, in elements.  */
      YYPTRDIFF_T yysize = yyssp - yyss + 1;

# if defined yyoverflow
      {
        /* Give user a chance to reallocate the stack.  Use copies of
           these so that the &'s don't force the real ones into
           memory.  */
        yy_state_t *yyss1 = yyss;
        YYSTYPE *yyvs1 = yyvs;

        /* Each stack pointer address is followed by the size of the
           data in use in that stack, in bytes.  This used to be a
           conditional around just the two extra args, but that might
           be undefined if yyoverflow is a macro.  */
        yyoverflow (YY_("memory exhausted"),
                    &yyss1, yysize * YYSIZEOF (*yyssp),
                    &yyvs1, yysize * YYSIZEOF (*yyvsp),
                    &yystacksize);
        yyss = yyss1;
        yyvs = yyvs1;
      }
# else /* defined YYSTACK_RELOCATE */
      /* Extend the stack our own way.  */
      if (YYMAXDEPTH <= yystacksize)
        YYNOMEM;
      yystacksize *= 2;
      if (YYMAXDEPTH < yystacksize)
        yystacksize = YYMAXDEPTH;

      {
        yy_state_t *yyss1 = yyss;
        union yyalloc *yyptr =
          YY_CAST (union yyalloc *,
                   YYSTACK_ALLOC (YY_CAST (YYSIZE_T, YYSTACK_BYTES (yystacksize))));
        if (! yyptr)
          YYNOMEM;
        YYSTACK_RELOCATE (yyss_alloc, yyss);
        YYSTACK_RELOCATE (yyvs_alloc, yyvs);
#  undef YYSTACK_RELOCATE
        if (yyss1 != yyssa)
          YYSTACK_FREE (yyss1);
      }
# endif

      yyssp = yyss + yysize - 1;
      yyvsp = yyvs + yysize - 1;

      YY_IGNORE_USELESS_CAST_BEGIN
      YYDPRINTF ((stderr, "Stack size increased to %ld\n",
                  YY_CAST (long, yystacksize)));
      YY_IGNORE_USELESS_CAST_END

      if (yyss + yystacksize - 1 <= yyssp)
        YYABORT;
    }
#endif /* !defined yyoverflow && !defined YYSTACK_RELOCATE */


  if (yystate == YYFINAL)
    YYACCEPT;

  goto yybackup;


/*-----------.
| yybackup.  |
`-----------*/
yybackup:
  /* Do appropriate processing given the current state.  Read a
     lookahead token if we need one and don't already have one.  */

  /* First try to decide what to do without reference to lookahead token.  */
  yyn = yypact[yystate];
  if (yypact_value_is_default (yyn))
    goto yydefault;

  /* Not known => get a lookahead token if don't already have one.  */

  /* YYCHAR is either empty, or end-of-input, or a valid lookahead.  */
  if (yychar == YYEMPTY)
    {
      YYDPRINTF ((stderr, "Reading a token\n"));
      yychar = yylex ();
    }

  if (yychar <= YYEOF)
    {
      yychar = YYEOF;
      yytoken = YYSYMBOL_YYEOF;
      YYDPRINTF ((stderr, "Now at end of input.\n"));
    }
  else if (yychar == YYerror)
    {
      /* The scanner already issued an error message, process directly
         to error recovery.  But do not keep the error token as
         lookahead, it is too special and may lead us to an endless
         loop in error recovery. */
      yychar = YYUNDEF;
      yytoken = YYSYMBOL_YYerror;
      goto yyerrlab1;
    }
  else
    {
      yytoken = YYTRANSLATE (yychar);
      YY_SYMBOL_PRINT ("Next token is", yytoken, &yylval, &yylloc);
    }

  /* If the proper action on seeing token YYTOKEN is to reduce or to
     detect an error, take that action.  */
  yyn += yytoken;
  if (yyn < 0 || YYLAST < yyn || yycheck[yyn] != yytoken)
    goto yydefault;
  yyn = yytable[yyn];
  if (yyn <= 0)
    {
      if (yytable_value_is_error (yyn))
        goto yyerrlab;
      yyn = -yyn;
      goto yyreduce;
    }

  /* Count tokens shifted since error; after three, turn off error
     status.  */
  if (yyerrstatus)
    yyerrstatus--;

  /* Shift the lookahead token.  */
  YY_SYMBOL_PRINT ("Shifting", yytoken, &yylval, &yylloc);
  yystate = yyn;
  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  *++yyvsp = yylval;
  YY_IGNORE_MAYBE_UNINITIALIZED_END

  /* Discard the shifted token.  */
  yychar = YYEMPTY;
  goto yynewstate;


/*-----------------------------------------------------------.
| yydefault -- do the default action for the current state.  |
`-----------------------------------------------------------*/
yydefault:
  yyn = yydefact[yystate];
  if (yyn == 0)
    goto yyerrlab;
  goto yyreduce;


/*-----------------------------.
| yyreduce -- do a reduction.  |
`-----------------------------*/
yyreduce:
  /* yyn is the number of a rule to reduce with.  */
  yylen = yyr2[yyn];

  /* If YYLEN is nonzero, implement the default value of the action:
     '$$ = $1'.

     Otherwise, the following line sets YYVAL to garbage.
     This behavior is undocumented and Bison
     users should not rely upon it.  Assigning to YYVAL
     unconditionally makes the parser a bit smaller, and it avoids a
     GCC warning that YYVAL may be used uninitialized.  */
  yyval = yyvsp[1-yylen];


  YY_REDUCE_PRINT (yyn);
  switch (yyn)
    {
  case 5: /* line: stmt '\n'  */
#line 292 "parser.y"
                       { program_append((yyvsp[-1].node)); }
#line 1374 "parser.tab.c"
    break;

  case 6: /* stmt: TOK_H1 TOK_IDENT TOK_ASSIGN TOK_STRING  */
#line 300 "parser.y"
      {
          Node *n = node_new(N_ASSIGN);
          n->tag = strdup("h1"); n->name = (yyvsp[-2].str); n->value = (yyvsp[0].str);
          (yyval.node) = n;
      }
#line 1384 "parser.tab.c"
    break;

  case 7: /* stmt: TOK_P TOK_IDENT TOK_ASSIGN TOK_STRING  */
#line 306 "parser.y"
      {
          Node *n = node_new(N_ASSIGN);
          n->tag = strdup("p"); n->name = (yyvsp[-2].str); n->value = (yyvsp[0].str);
          (yyval.node) = n;
      }
#line 1394 "parser.tab.c"
    break;

  case 8: /* stmt: TOK_QUA TOK_IDENT TOK_ASSIGN TOK_NUMBER  */
#line 312 "parser.y"
      {
          char buf[64]; sprintf(buf, "%d", (yyvsp[0].num));
          Node *n = node_new(N_ASSIGN);
          n->tag = strdup("qua"); n->name = (yyvsp[-2].str); n->value = strdup(buf);
          (yyval.node) = n;
      }
#line 1405 "parser.tab.c"
    break;

  case 9: /* stmt: TOK_IDENT TOK_ASSIGN TOK_STRING  */
#line 319 "parser.y"
      {
          Node *n = node_new(N_ASSIGN);
          n->tag = strdup(""); n->name = (yyvsp[-2].str); n->value = (yyvsp[0].str);
          (yyval.node) = n;
      }
#line 1415 "parser.tab.c"
    break;

  case 10: /* stmt: TOK_H1 TOK_IDENT TOK_ASSIGN TOK_IDENT '(' call_args ')'  */
#line 327 "parser.y"
      {
          Node *n = node_new(N_CALL);
          n->name = (yyvsp[-3].str); n->items = (yyvsp[-1].call).items; n->nitems = (yyvsp[-1].call).n;
          n->result_tag = strdup("h1"); n->result_name = (yyvsp[-5].str);
          (yyval.node) = n;
      }
#line 1426 "parser.tab.c"
    break;

  case 11: /* stmt: TOK_P TOK_IDENT TOK_ASSIGN TOK_IDENT '(' call_args ')'  */
#line 334 "parser.y"
      {
          Node *n = node_new(N_CALL);
          n->name = (yyvsp[-3].str); n->items = (yyvsp[-1].call).items; n->nitems = (yyvsp[-1].call).n;
          n->result_tag = strdup("p"); n->result_name = (yyvsp[-5].str);
          (yyval.node) = n;
      }
#line 1437 "parser.tab.c"
    break;

  case 12: /* stmt: TOK_QUA TOK_IDENT TOK_ASSIGN TOK_IDENT '(' call_args ')'  */
#line 341 "parser.y"
      {
          Node *n = node_new(N_CALL);
          n->name = (yyvsp[-3].str); n->items = (yyvsp[-1].call).items; n->nitems = (yyvsp[-1].call).n;
          n->result_tag = strdup("qua"); n->result_name = (yyvsp[-5].str);
          (yyval.node) = n;
      }
#line 1448 "parser.tab.c"
    break;

  case 13: /* stmt: TOK_IDENT '(' call_args ')'  */
#line 350 "parser.y"
      {
          Node *n = node_new(N_CALL);
          n->name = (yyvsp[-3].str); n->items = (yyvsp[-1].call).items; n->nitems = (yyvsp[-1].call).n;
          n->result_tag = NULL; n->result_name = NULL;
          (yyval.node) = n;
      }
#line 1459 "parser.tab.c"
    break;

  case 14: /* stmt: TOK_MAKE '[' name_list ']'  */
#line 359 "parser.y"
      { (yyval.node) = (yyvsp[-1].node); }
#line 1465 "parser.tab.c"
    break;

  case 15: /* stmt: TOK_MAKE '[' ']'  */
#line 362 "parser.y"
      {
          Node *open  = node_new(N_DIV_OPEN);
          Node *close = node_new(N_DIV_CLOSE);
          open->next = close;
          (yyval.node) = open;
      }
#line 1476 "parser.tab.c"
    break;

  case 16: /* stmt: TOK_IF condition ',' stmt_node ',' TOK_ELSE stmt_node  */
#line 370 "parser.y"
      {
          Node *n = node_new(N_IF);
          n->cond_var = (yyvsp[-5].cond).var; n->cond_op = (yyvsp[-5].cond).op; n->cond_num = (yyvsp[-5].cond).num;
          n->then_node = (yyvsp[-3].node); n->else_node = (yyvsp[0].node);
          (yyval.node) = n;
      }
#line 1487 "parser.tab.c"
    break;

  case 17: /* stmt: TOK_RULE TOK_IDENT '(' param_list ')' '{' stmt_list '}'  */
#line 378 "parser.y"
      {
          if (func_count < MAX_FUNCS) {
              Func *f = &funcs[func_count++];
              f->name = (yyvsp[-6].str); f->ptypes = (yyvsp[-4].params).types; f->pnames = (yyvsp[-4].params).names;
              f->nparams = (yyvsp[-4].params).n; f->body = (yyvsp[-1].node);
          }
          (yyval.node) = NULL;
      }
#line 1500 "parser.tab.c"
    break;

  case 18: /* stmt: TOK_DECL TOK_IDENT TOK_ASSIGN TOK_STRING  */
#line 388 "parser.y"
      {
          Node *n = node_new(N_RETURN);
          n->name = (yyvsp[-2].str); n->value = (yyvsp[0].str);
          (yyval.node) = n;
      }
#line 1510 "parser.tab.c"
    break;

  case 19: /* stmt_node: stmt  */
#line 395 "parser.y"
                { (yyval.node) = (yyvsp[0].node); }
#line 1516 "parser.tab.c"
    break;

  case 20: /* stmt_list: %empty  */
#line 398 "parser.y"
                       { (yyval.node) = NULL; }
#line 1522 "parser.tab.c"
    break;

  case 21: /* stmt_list: stmt_list stmt  */
#line 400 "parser.y"
      {
          if ((yyvsp[-1].node)) { (yyvsp[-1].node)->next = (yyvsp[0].node); (yyval.node) = (yyvsp[-1].node); }
          else    { (yyval.node) = (yyvsp[0].node); }
      }
#line 1531 "parser.tab.c"
    break;

  case 22: /* condition: TOK_IDENT TOK_GT TOK_NUMBER  */
#line 409 "parser.y"
                                   { (yyval.cond).var=(yyvsp[-2].str); (yyval.cond).op=0; (yyval.cond).num=(yyvsp[0].num); }
#line 1537 "parser.tab.c"
    break;

  case 23: /* condition: TOK_IDENT TOK_LT TOK_NUMBER  */
#line 410 "parser.y"
                                   { (yyval.cond).var=(yyvsp[-2].str); (yyval.cond).op=1; (yyval.cond).num=(yyvsp[0].num); }
#line 1543 "parser.tab.c"
    break;

  case 24: /* condition: TOK_IDENT TOK_GTE TOK_NUMBER  */
#line 411 "parser.y"
                                   { (yyval.cond).var=(yyvsp[-2].str); (yyval.cond).op=2; (yyval.cond).num=(yyvsp[0].num); }
#line 1549 "parser.tab.c"
    break;

  case 25: /* condition: TOK_IDENT TOK_LTE TOK_NUMBER  */
#line 412 "parser.y"
                                   { (yyval.cond).var=(yyvsp[-2].str); (yyval.cond).op=3; (yyval.cond).num=(yyvsp[0].num); }
#line 1555 "parser.tab.c"
    break;

  case 26: /* condition: TOK_IDENT TOK_EQ TOK_NUMBER  */
#line 413 "parser.y"
                                   { (yyval.cond).var=(yyvsp[-2].str); (yyval.cond).op=4; (yyval.cond).num=(yyvsp[0].num); }
#line 1561 "parser.tab.c"
    break;

  case 27: /* condition: TOK_IDENT TOK_NEQ TOK_NUMBER  */
#line 414 "parser.y"
                                   { (yyval.cond).var=(yyvsp[-2].str); (yyval.cond).op=5; (yyval.cond).num=(yyvsp[0].num); }
#line 1567 "parser.tab.c"
    break;

  case 28: /* param_list: %empty  */
#line 420 "parser.y"
                         { (yyval.params).types=NULL; (yyval.params).names=NULL; (yyval.params).n=0; }
#line 1573 "parser.tab.c"
    break;

  case 29: /* param_list: param_item  */
#line 421 "parser.y"
                    { (yyval.params) = (yyvsp[0].params); }
#line 1579 "parser.tab.c"
    break;

  case 30: /* param_list: param_list ',' param_item  */
#line 423 "parser.y"
      {
          (yyval.params) = (yyvsp[-2].params);
          (yyval.params).types = realloc((yyval.params).types, ((yyval.params).n+1) * sizeof(char*));
          (yyval.params).names = realloc((yyval.params).names, ((yyval.params).n+1) * sizeof(char*));
          (yyval.params).types[(yyval.params).n] = (yyvsp[0].params).types[0];
          (yyval.params).names[(yyval.params).n] = (yyvsp[0].params).names[0];
          (yyval.params).n++;
      }
#line 1592 "parser.tab.c"
    break;

  case 31: /* param_item: type_name TOK_IDENT  */
#line 435 "parser.y"
      {
          (yyval.params).types = malloc(sizeof(char*));
          (yyval.params).names = malloc(sizeof(char*));
          (yyval.params).types[0] = (yyvsp[-1].str);
          (yyval.params).names[0] = (yyvsp[0].str);
          (yyval.params).n = 1;
      }
#line 1604 "parser.tab.c"
    break;

  case 32: /* type_name: TOK_H1  */
#line 445 "parser.y"
              { (yyval.str) = strdup("h1"); }
#line 1610 "parser.tab.c"
    break;

  case 33: /* type_name: TOK_P  */
#line 446 "parser.y"
              { (yyval.str) = strdup("p"); }
#line 1616 "parser.tab.c"
    break;

  case 34: /* type_name: TOK_QUA  */
#line 447 "parser.y"
              { (yyval.str) = strdup("qua"); }
#line 1622 "parser.tab.c"
    break;

  case 35: /* call_args: %empty  */
#line 453 "parser.y"
                        { (yyval.call).items=NULL; (yyval.call).n=0; }
#line 1628 "parser.tab.c"
    break;

  case 36: /* call_args: TOK_IDENT  */
#line 454 "parser.y"
                  { (yyval.call).items=malloc(sizeof(char*)); (yyval.call).items[0]=(yyvsp[0].str); (yyval.call).n=1; }
#line 1634 "parser.tab.c"
    break;

  case 37: /* call_args: call_args ',' TOK_IDENT  */
#line 456 "parser.y"
      {
          (yyval.call) = (yyvsp[-2].call);
          (yyval.call).items = realloc((yyval.call).items, ((yyval.call).n+1) * sizeof(char*));
          (yyval.call).items[(yyval.call).n++] = (yyvsp[0].str);
      }
#line 1644 "parser.tab.c"
    break;

  case 38: /* name_list: name_item  */
#line 466 "parser.y"
                             { (yyval.node) = (yyvsp[0].node); }
#line 1650 "parser.tab.c"
    break;

  case 39: /* name_list: name_list name_item  */
#line 467 "parser.y"
                             {
          Node *tail = (yyvsp[-1].node);
          while (tail->next) tail = tail->next;
          tail->next = (yyvsp[0].node);
          (yyval.node) = (yyvsp[-1].node);
      }
#line 1661 "parser.tab.c"
    break;

  case 40: /* name_item: TOK_IDENT  */
#line 477 "parser.y"
      {
          Node *n = node_new(N_MAKE);
          n->items = malloc(sizeof(char*));
          n->items[0] = (yyvsp[0].str);
          n->nitems = 1;
          (yyval.node) = n;
      }
#line 1673 "parser.tab.c"
    break;

  case 41: /* name_item: '[' name_list ']'  */
#line 485 "parser.y"
      {
          Node *open  = node_new(N_DIV_OPEN);
          Node *close = node_new(N_DIV_CLOSE);

          if ((yyvsp[-1].node)) {
              open->next = (yyvsp[-1].node);
              Node *tail = (yyvsp[-1].node);
              while (tail->next) tail = tail->next;
              tail->next = close;
          } else {
              open->next = close;
          }
          (yyval.node) = open;
      }
#line 1692 "parser.tab.c"
    break;

  case 42: /* name_item: '[' ']'  */
#line 500 "parser.y"
      {
          Node *open  = node_new(N_DIV_OPEN);
          Node *close = node_new(N_DIV_CLOSE);
          open->next = close;
          (yyval.node) = open;
      }
#line 1703 "parser.tab.c"
    break;


#line 1707 "parser.tab.c"

      default: break;
    }
  /* User semantic actions sometimes alter yychar, and that requires
     that yytoken be updated with the new translation.  We take the
     approach of translating immediately before every use of yytoken.
     One alternative is translating here after every semantic action,
     but that translation would be missed if the semantic action invokes
     YYABORT, YYACCEPT, or YYERROR immediately after altering yychar or
     if it invokes YYBACKUP.  In the case of YYABORT or YYACCEPT, an
     incorrect destructor might then be invoked immediately.  In the
     case of YYERROR or YYBACKUP, subsequent parser actions might lead
     to an incorrect destructor call or verbose syntax error message
     before the lookahead is translated.  */
  YY_SYMBOL_PRINT ("-> $$ =", YY_CAST (yysymbol_kind_t, yyr1[yyn]), &yyval, &yyloc);

  YYPOPSTACK (yylen);
  yylen = 0;

  *++yyvsp = yyval;

  /* Now 'shift' the result of the reduction.  Determine what state
     that goes to, based on the state we popped back to and the rule
     number reduced by.  */
  {
    const int yylhs = yyr1[yyn] - YYNTOKENS;
    const int yyi = yypgoto[yylhs] + *yyssp;
    yystate = (0 <= yyi && yyi <= YYLAST && yycheck[yyi] == *yyssp
               ? yytable[yyi]
               : yydefgoto[yylhs]);
  }

  goto yynewstate;


/*--------------------------------------.
| yyerrlab -- here on detecting error.  |
`--------------------------------------*/
yyerrlab:
  /* Make sure we have latest lookahead translation.  See comments at
     user semantic actions for why this is necessary.  */
  yytoken = yychar == YYEMPTY ? YYSYMBOL_YYEMPTY : YYTRANSLATE (yychar);
  /* If not already recovering from an error, report this error.  */
  if (!yyerrstatus)
    {
      ++yynerrs;
      yyerror (YY_("syntax error"));
    }

  if (yyerrstatus == 3)
    {
      /* If just tried and failed to reuse lookahead token after an
         error, discard it.  */

      if (yychar <= YYEOF)
        {
          /* Return failure if at end of input.  */
          if (yychar == YYEOF)
            YYABORT;
        }
      else
        {
          yydestruct ("Error: discarding",
                      yytoken, &yylval);
          yychar = YYEMPTY;
        }
    }

  /* Else will try to reuse lookahead token after shifting the error
     token.  */
  goto yyerrlab1;


/*---------------------------------------------------.
| yyerrorlab -- error raised explicitly by YYERROR.  |
`---------------------------------------------------*/
yyerrorlab:
  /* Pacify compilers when the user code never invokes YYERROR and the
     label yyerrorlab therefore never appears in user code.  */
  if (0)
    YYERROR;
  ++yynerrs;

  /* Do not reclaim the symbols of the rule whose action triggered
     this YYERROR.  */
  YYPOPSTACK (yylen);
  yylen = 0;
  YY_STACK_PRINT (yyss, yyssp);
  yystate = *yyssp;
  goto yyerrlab1;


/*-------------------------------------------------------------.
| yyerrlab1 -- common code for both syntax error and YYERROR.  |
`-------------------------------------------------------------*/
yyerrlab1:
  yyerrstatus = 3;      /* Each real token shifted decrements this.  */

  /* Pop stack until we find a state that shifts the error token.  */
  for (;;)
    {
      yyn = yypact[yystate];
      if (!yypact_value_is_default (yyn))
        {
          yyn += YYSYMBOL_YYerror;
          if (0 <= yyn && yyn <= YYLAST && yycheck[yyn] == YYSYMBOL_YYerror)
            {
              yyn = yytable[yyn];
              if (0 < yyn)
                break;
            }
        }

      /* Pop the current state because it cannot handle the error token.  */
      if (yyssp == yyss)
        YYABORT;


      yydestruct ("Error: popping",
                  YY_ACCESSING_SYMBOL (yystate), yyvsp);
      YYPOPSTACK (1);
      yystate = *yyssp;
      YY_STACK_PRINT (yyss, yyssp);
    }

  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  *++yyvsp = yylval;
  YY_IGNORE_MAYBE_UNINITIALIZED_END


  /* Shift the error token.  */
  YY_SYMBOL_PRINT ("Shifting", YY_ACCESSING_SYMBOL (yyn), yyvsp, yylsp);

  yystate = yyn;
  goto yynewstate;


/*-------------------------------------.
| yyacceptlab -- YYACCEPT comes here.  |
`-------------------------------------*/
yyacceptlab:
  yyresult = 0;
  goto yyreturnlab;


/*-----------------------------------.
| yyabortlab -- YYABORT comes here.  |
`-----------------------------------*/
yyabortlab:
  yyresult = 1;
  goto yyreturnlab;


/*-----------------------------------------------------------.
| yyexhaustedlab -- YYNOMEM (memory exhaustion) comes here.  |
`-----------------------------------------------------------*/
yyexhaustedlab:
  yyerror (YY_("memory exhausted"));
  yyresult = 2;
  goto yyreturnlab;


/*----------------------------------------------------------.
| yyreturnlab -- parsing is finished, clean up and return.  |
`----------------------------------------------------------*/
yyreturnlab:
  if (yychar != YYEMPTY)
    {
      /* Make sure we have latest lookahead translation.  See comments at
         user semantic actions for why this is necessary.  */
      yytoken = YYTRANSLATE (yychar);
      yydestruct ("Cleanup: discarding lookahead",
                  yytoken, &yylval);
    }
  /* Do not reclaim the symbols of the rule whose action triggered
     this YYABORT or YYACCEPT.  */
  YYPOPSTACK (yylen);
  YY_STACK_PRINT (yyss, yyssp);
  while (yyssp != yyss)
    {
      yydestruct ("Cleanup: popping",
                  YY_ACCESSING_SYMBOL (+*yyssp), yyvsp);
      YYPOPSTACK (1);
    }
#ifndef yyoverflow
  if (yyss != yyssa)
    YYSTACK_FREE (yyss);
#endif

  return yyresult;
}

#line 508 "parser.y"


/* ==================== Ошибки и точка входа ==================== */

void yyerror(const char *s) {
    fprintf(stderr, "Ошибка: %s\n", s);
}

int main(void) {
    emit("<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"UTF-8\">\n<title>Generated</title>\n</head>\n<body>\n");

    if (yyparse() == 0) {
        exec_nodes(program);
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
