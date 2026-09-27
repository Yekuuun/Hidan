/**
 * This module contains base LSM Ebpg hooks abuse.
 * 
 * Note, i'm basing my ownership based on a custom UID for policy & simple poc testing but consider using more advanced techniques.
 * 
 * Since most of hooks are a bit tricky to play with due the sandboxed environnment. LSM should be a great way to block certains actions on a targetted system.
 * 
 * @ref => https://github.com/torvalds/linux/blob/master/include/linux/lsm_hook_defs.h
 */

#ifndef MOD_LSM_H
#define MOD_LSM_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"

#ifndef EPERM
#define EPERM 1
#endif

//FOR THE dev_user (control policy)
#define ADM_UID 1111

/**
 * Check is the caller as the valid ADM_UID uid.
 */
static int __is_adm_caller(void)
{
    u64 uid_gid = bpf_get_current_uid_gid();

    u32 uid = uid_gid & 0xFFFFFFFF;
    
    return uid == ADM_UID;
}

/**
 * Utility blocking policy
 */
static __always_inline int ptrace_block_policy(pid_t target_candidate_pid, pid_t caller_pid)
{
    if (__is_target_process(target_candidate_pid) && !__is_adm_caller()) {
        char event_str[128] = {0};
        BPF_SNPRINTF(event_str, sizeof(event_str), "blocked caller %d, for pid => %d", caller_pid, target_candidate_pid);

        send_event(EVENT_TRIGGERED, "ls_ptrace_access_check", event_str);
        return -EPERM;
    }

    return 0;
}

SEC("lsm/ptrace_access_check")
int BPF_PROG(handle_ptrace_check, struct task_struct *child, __u32 mode)
{
    struct task_struct *tsk = bpf_get_current_task_btf();
    if (!tsk) 
        return 0;

    // target = child (who wants to be traced), caller = current (tracer)
    return ptrace_block_policy(child->pid, tsk->pid);
}

SEC("lsm/ptrace_traceme")
int BPF_PROG(handle_ptrace_traceme, struct task_struct *parent)
{
    struct task_struct *tsk = bpf_get_current_task_btf();
    if (!tsk) 
        return 0;

    // target = current (whom expose TRACEME), caller = parent
    return ptrace_block_policy(tsk->pid, parent->pid);
}

#endif