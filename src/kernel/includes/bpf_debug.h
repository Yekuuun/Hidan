#ifndef BPF_DEBUG_H
#define BPF_DEBUG_H

#include "bpf_config.h"

#define __STR(x) #x
#define STR(x)   __STR(x)

#define PRINT_DEBUG(fmt, ...) bpf_printk("[DBG][%s] " fmt, __func__, ##__VA_ARGS__)

#endif