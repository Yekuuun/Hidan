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
//  GETDENTS
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
    unsigned long fd;
    struct linux_dirent64 *dirent;
    unsigned long count;
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

typedef struct sys_getdents_enter_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    unsigned long fd;
    struct linux_dirent64 *dirent;
    unsigned long count;
} sys_getdents_enter_ctx;

typedef struct sys_getdents_exit_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    long ret;
} sys_getdents_exit_ctx;

//----------------------------------------------------------

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  OPENAT
// └────────────────────────────────────┘
//----------------------------------------------------
typedef struct sys_enter_openat_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    unsigned long dfd;
    const char *filename;
    unsigned long flags;
    umode_t mode;
} sys_enter_openat_ctx;

typedef struct sys_exit_openat_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    long ret;
} sys_exit_openat_ctx;

typedef struct fd_key {
    __u64 pid_tgid;
    __u32 fd;
} fd_key;

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  READ
// └────────────────────────────────────┘
//----------------------------------------------------
typedef struct sys_enter_read_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    unsigned long fd;
    char *buf;
    unsigned long count;
} sys_enter_read_ctx;

typedef struct sys_exit_read_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    long ret;
} sys_exit_read_ctx;

typedef struct sys_enter_cached_val{
    __u32 fd;
    __u32 count;
    char *ubuff_adr; //raw address value for __user *buf
    char path[MAX_PATH];
} sys_enter_cached_val; 

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  EXEC
// └────────────────────────────────────┘
//----------------------------------------------------

/**
 * >> cat /sys/kernel/debug/tracing/events/syscalls/sys_enter_execve/format
 */
typedef struct sys_enter_execve_ctx {
    unsigned short common_type;
    unsigned char common_flags;
    unsigned char common_preempt_count;
    int common_pid;

    int __syscall_nr;
    const char* filename;
    const char* const* argv;
    const char* const* envp;
} sys_enter_execve_ctx;


#endif