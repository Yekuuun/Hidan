/**
 * POC : trying to hook read returns based on dedicated files.
 * 
 * @author Yekuuun
 */

#ifndef MOD_HOOK_READ_H
#define MOD_HOOK_READ_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

/**
 * Hook sys_enter_read.
 */
SEC("tp/syscalls/sys_enter_read")
int trace_sys_enter_read(struct trace_event_raw_sys_enter *ctx)
{
    return 0;
}

#endif