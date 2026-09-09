/**
 * Contains all utilities for bpf module configuration
 * 
 * @author Yekuuun
 */
#include "vmlinux.h"
#include <bpf/bpf_tracing.h>
#include <bpf/bpf_core_read.h>
#include <bpf/bpf_helpers.h>

//short since our targets would not exceed this len.
#ifndef DNAME_MAX
#define DNAME_MAX 128
#endif

#ifndef BPF_CONFIG
#define BPF_CONFIG

#define MAX_PID_ENTRIES 32
#define MAX_BINARIES_CONFIG_CACHE 64

#ifndef TASK_COMM_LEN
#define TASK_COMM_LEN 16
#endif

#ifndef MAX_ENTRIES
#define MAX_ENTRIES 128
#endif


//----------------------------------------------------
// ┌────────────────────────────────────┐
//  SECRETS
// └────────────────────────────────────┘
//----------------------------------------------------
#define SECRET_DIRECTORY_NAME_HIDE "SECRETDIR"

#endif