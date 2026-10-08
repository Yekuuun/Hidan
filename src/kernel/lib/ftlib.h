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

    #pragma unroll
    for (i = 0; i < max; i++)
        if (s[i] == '\0')
            break;

    return i;
}

#endif