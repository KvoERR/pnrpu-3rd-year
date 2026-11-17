#include <stdio.h>
#include "parser.tab.h"

extern int yylex(void);
extern int yylineno;
extern char *yytext;

int main(int argc, char **argv) {
    int token;
    while ((token = yylex()) != 0) {
        switch (token) {
            case TOK_LT_TAG:   printf("TOK_LT_TAG ('<')\n"); break;
            case TOK_GT_TAG:   printf("TOK_GT_TAG ('>')\n"); break;
            case TOK_IDENT:    printf("TOK_IDENT ('%s')\n", yytext); break;
            case TOK_STRING:   printf("TOK_STRING ('%s')\n", yytext); break;
            case TOK_ASSIGN:   printf("TOK_ASSIGN ('=')\n"); break;
            case TOK_DOLLAR:   printf("TOK_DOLLAR ('$')\n"); break;
            case TOK_IF:       printf("TOK_IF ('if')\n"); break;
            case TOK_ELSE:     printf("TOK_ELSE ('else')\n"); break;
            case '{':          printf("TOKEN '{'\n"); break;
            case '}':          printf("TOKEN '}'\n"); break;
            case '/':          printf("TOKEN '/'\n"); break;
            default:           printf("TOKEN %d\n", token); break;
        }
    }
    return 0;
}
