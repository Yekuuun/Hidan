/**
 * Some utility functions
 * 
 * @author Yekuuun
 */

#ifndef FT_LIB_H
#define FT_LIB_H

#include "vmlinux.h"
#include "../includes/bpf_config.h"

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  COMMON FUNCTIONS
// └────────────────────────────────────┘ 
//----------------------------------------------------

#define MAX_STR_LEN 128

/**
 * Check if is c is a valid numeric character.
 */
static __always_inline int ft_isdigit(int c){
    return '0' <= c && c <= '9';
}

/**
 * Check if an str is a valid number.
 * @max => must be know at call.
 */
static __always_inline int ft_isnumeric(const char *s, __u32 max){
    for (__u32 i = 0; i < max; i++) {
        if (s[i] == '\0')
            break;
        if (!ft_isdigit((unsigned char)s[i]))
            return 0;
    }
    return 1;
}

/**
 * Homemade atoi.
 * @max => must be know at call.
 */
static __always_inline int ft_atoi(const char *s, __u32 max){
    __u32 i = 0;
    int res = 0, sign = 1;

    if(!s)
        return 0;

    while (i < max && ((s[i] >= 9 && s[i] <= 13) || s[i] == ' '))
        i++;

    if (i < max && s[i] == '-'){
        sign = -1;
        i++;
    }

    if (i < max && s[i] == '+')
        i++;

    while (i < max && ft_isdigit((unsigned char)s[i])){
        res = res * 10 + (s[i] - '0');
        i++;
    }

    return res * sign;
}

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  STR FUNCTIONS
// └────────────────────────────────────┘ 
//----------------------------------------------------

/**
 * strcmp - Compare two strings
 * @cs: One string
 * @ct: Another string
 * @max: must be know at call (max len)
 *
 * https://elixir.bootlin.com/linux/v6.17.3/source/drivers/firmware/efi/libstub/string.c#L68
 */
static __always_inline int ft_strcmp(const char *cs, const char *ct, __u32 max)
{
    for (__u32 i = 0; i < max; i++) {
        unsigned char c1 = cs[i];
        unsigned char c2 = ct[i];

        if (c1 != c2)
            return c1 < c2 ? -1 : 1;

        if (!c1)
            break;
    }

    return 0;
}

/**
 * Custom strlen function.
 */
static __always_inline size_t ft_strlen(const char *s, __u32 max)
{
    size_t i;

    for (i = 0; i < max; i++)
        if (s[i] == '\0')
            break;

    return i;
}

/**
 * strstr - Find the first substring in a %NUL terminated string
 * @s1: The string to be searched
 * @s2: The string to search for
 */
static __always_inline char *ft_strstr(const char *s1, const char *s2)
{
	__u32 l1, l2, i, j;

	l2 = ft_strlen(s2, MAX_STR_LEN);
	if (!l2)
		return (char *)s1;

	l1 = ft_strlen(s1, MAX_STR_LEN);

	for (i = 0; i < MAX_STR_LEN && i + l2 <= l1; i++) {
        
		for (j = 0; j < l2; j++)
			if (s1[(i + j) & (MAX_STR_LEN - 1)] != s2[j])
				break;

		if (j == l2)
			return (char *)s1 + i;
	}

	return NULL;
}

#endif