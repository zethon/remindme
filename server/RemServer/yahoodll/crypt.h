#ifndef __YAHOO_UTIL_H__
#define __YAHOO_UTIL_H__

# include <stdlib.h>
# include <stdarg.h>

char *yahoo_crypt(char *key, char *salt);

int snprintf(char *str, size_t size, const char *format, ...);
int vsnprintf(char *str, size_t size, const char *format, va_list ap);

#ifndef MIN
#define MIN(x,y) ((x)<(y)?(x):(y))
#endif

#ifndef MAX
#define MAX(x,y) ((x)>(y)?(x):(y))
#endif

# define FREE(x)		if(x) {free(x); x=NULL;}

#endif