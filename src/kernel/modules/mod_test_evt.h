/**
 * Simple file to test ringbuf event sending.
 * 
 * @author Yekuuun
 */

#ifndef MOD_TEST_EVT_H
#define MOD_TEST_EVT_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ft_utils.h"

/**
 * Fexit on syskill. easy to debug.
 */
SEC("fexit/__x64_sys_kill")
int BPF_PROG(fexit_syskill, pid_t pid, int sig, long ret)
{
    //err.
    if(ret <= 0)
        return ret;

    char comm[TASK_COMM_LEN] = {0};
    if(bpf_get_current_comm(comm, sizeof(comm)) < 0)
        return ret;

    struct task_struct *tsk = bpf_get_current_task_btf();
    if(!tsk)
        return ret;

    ebpf_event *evt = bpf_ringbuf_reserve(&event_output, sizeof(ebpf_event), 0);
    if(!evt)
        return ret;

    __rtl_secure_zero_memory(evt, sizeof(ebpf_event));

    evt->hdr = SET_EVENT_HDR(EVENT_GLOBAL, (__u16)sizeof(ebpf_event), bpf_ktime_get_ns());

    bpf_ringbuf_submit(evt, 0);
    return 0;
}


#endif