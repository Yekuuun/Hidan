/**
 * Simple file to test ringbuf event sending.
 * 
 * @author Yekuuun
 */

#ifndef MOD_TEST_EVT_H
#define MOD_TEST_EVT_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

#define SIG_TEST 60

/**
 * Kprobe on sys_kill for test debugging => sending EBPF rinbuffer event on kill -60 <pid> event.
 */
SEC("kprobe/__x64_sys_kill")
int BPF_KPROBE(kprobe_sys_kill, struct pt_regs *regs)
{
    __u32 sig  = (__u32)PT_REGS_PARM2_CORE(regs);

    //only handle dedicated signal for testing.
    if(sig != SIG_TEST)
        return 0;

    char comm[TASK_COMM_LEN] = {0};
    if(bpf_get_current_comm(comm, sizeof(comm)) < 0)
        return 0;

    struct task_struct *tsk = bpf_get_current_task_btf();
    if(!tsk)
        return 0;

    send_event(EVENT_DEBUG, "__x64_sys_kill", "sys_kill event debug testing.");
    return 0;
}


#endif