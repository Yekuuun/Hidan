/**
 * Contains all utilities for bpf module configuration
 * 
 * @author Yekuuun
 */
#include "vmlinux.h"
#include <bpf/bpf_tracing.h>
#include <bpf/bpf_core_read.h>
#include <bpf/bpf_helpers.h>

#ifndef BPF_CONFIG
#define BPF_CONFIG

//----------------------------------------------------

#define MAX_PID_ENTRIES 32
#define MAX_BINARIES_CONFIG_CACHE 64

#ifndef TASK_COMM_LEN
#define TASK_COMM_LEN 16
#endif

#ifndef MAX_PATH
#define MAX_PATH    256
#endif

#ifndef MAX_ENTRIES
#define MAX_ENTRIES 128
#endif

#define PID_STR_MAX 16 

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  DIR UTILITIES
// └────────────────────────────────────┘
//----------------------------------------------------
#ifndef DNAME_MAX
#define DNAME_MAX   128
#endif

#ifndef DT_DIR
#define DT_DIR      4
#endif 

#ifndef DT_REG
#define DT_REG      8
#endif

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  CAT HOOK UTILITIES
// └────────────────────────────────────┘
//----------------------------------------------------

#define CAT_BIN "cat"

#endif