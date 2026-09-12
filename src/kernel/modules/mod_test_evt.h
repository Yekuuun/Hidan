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
#include "../lib/ftlib.h"

#define SIG_TEST 65

/**
 * Kprobe on sys_kill for easy debugging.
 */
SEC("kprobe/__x64_sys_kill")
int BPF_KPROBE(kprobe_sys_kill, struct pt_regs *regs)
{
    /**
     * 2 interesting infos to get : 
     * 
     * dx => pid_t pid
     * si => signal sendt
     */
    __u32 pid  = (__u32)PT_REGS_PARM1_CORE(regs);
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

    ebpf_event *evt = bpf_ringbuf_reserve(&event_output, sizeof(ebpf_event), 0);
    if(!evt)
        return 0;

    __rtl_secure_zero_memory(evt, sizeof(ebpf_event));

    evt->hdr = SET_EVENT_HDR(EVENT_GLOBAL, (__u16)sizeof(ebpf_event), bpf_ktime_get_ns());

    bpf_ringbuf_submit(evt, 0);
    return 0;
}


#endif