#ifndef MOD_HOOK_EXEC
#define MOD_HOOK_EXEC

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

/**
 * For the sake of simplicity, using bash as the main target for the execve exploit.
 */
#define EXEC_TARGET "bash"

SEC("tp/syscalls/sys_enter_execve")
int tp_sys_enter_execve(struct sys_enter_execve_ctx *ctx)
{
    return 0;
}

#endif