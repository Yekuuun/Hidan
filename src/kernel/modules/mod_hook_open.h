/**
 * Hooking open & openat for storing fd of potiential target files.
 */

#ifndef MOD_HOOK_OPEN_H
#define MOD_HOOK_OPEN_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

/**
 * Hook sys_entry_openat
 */
SEC("tp/syscall/sys_enter_openat")
int tp_sys_enter_openat(struct sys_enter_openat_ctx *ctx)
{
    if(!__is_target_bin())
        return 0;

    __u64 pid_tgid = bpf_get_current_pid_tgid(); //64 bits value returned.

    return 0;
}

/**
 * Hook sys_exit_openat
 */
SEC("tp/syscalls/sys_exit_openat")
int tp_sys_exit_openat(struct sys_exit_openat_ctx *ctx)
{
    if(!__is_target_bin())
        return 0;

    __u64 pid_tgid = bpf_get_current_pid_tgid(); //64 bits value returned.


    return 0;
}

#endif