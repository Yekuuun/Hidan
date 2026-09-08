/**
 * Some utility declaration for kernel ebpf app.
 */

#ifndef UTILS_H
#define UTILS_H

#include "bpf.h"
#include "vmlinux.h"
#include "config.h"

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
 */
static __always_inline int ft_isnumeric(const char *s){
    size_t i = 0;

    #pragma unroll
    while(*s++ != '\0'){
        if(!ft_isdigit(*s))
            return 0;

        i++;
    }

    return 1;
}

/**
 * Homemade atoi.
 */
static __always_inline int ft_atoi(const char *s){
    int i = 0, res = 0, sign = 1;

    if(!s)
        return 0;

    #pragma unroll
    while((s[i] >= 9 && s[i] <= 13) || s[i] == ' ')
        i++;
    
    if(s[i] == '-'){
        sign = -1;
        i++;
    }

    if(s[i] == '+')
        i++;

    #pragma unroll
    while(ft_isdigit(s[i])){
        res = res * 10  + s[i] - '0';
        i++;
    }

    return res;
}

#endif