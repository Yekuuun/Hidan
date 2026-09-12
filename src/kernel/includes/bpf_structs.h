/**
 * Utility structs
 * 
 * @author Yekuuun
 */

#ifndef BPF_STRUCTS_H
#define BPF_STRUCTS_H

#include "bpf_config.h"

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  GLOBAL STRUCTS
// └────────────────────────────────────┘
//----------------------------------------------------

/**
 * >> cat /sys/kernel/debug/tracing/events/syscalls/sys_enter_getdents64/format
 */
typedef struct sys_getdents64_enter_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    unsigned int fd;
    struct linux_dirent64 *dirent;
    unsigned int count;
} sys_getdents64_enter_ctx;

/**
 * >> cat /sys/kernel/debug/tracing/events/syscalls/sys_exit_getdents64/format
 */
typedef struct sys_getdents64_exit_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    long ret;
} sys_getdents64_exit_ctx;

#endif